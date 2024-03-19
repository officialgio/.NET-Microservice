using System;
using Automatonymous;
using MassTransit;
using Play.Identity.Contracts;
using Play.Inventory.Contracts;
using Play.Trading.Service.Activities;

namespace Play.Trading.Service.StateMachines;

/// <summary>
/// This class is the actual concrete State Machine.
/// </summary>
public class PurchaseStateMachine : MassTransitStateMachine<PurchaseState>
{
	public State Accepted { get; }
	public State ItemsGranted { get; }
	public State Completed { get; }
	public State Faulted { get; }

	public Event<PurchaseRequested> PurchaseRequested { get; }

	public Event<GetPurchaseState> GetPurchaseState { get; }

	public Event<InventoryItemsGranted> InventoryItemsGranted { get; }

	public Event<GilDebited> GilDebited { get; }


	// Event with other services fails doing the operations (will be used)
	public Event<Fault<GrantItems>> GrantItemsFaulted { get; }

	public Event<Fault<DebitGil>> DebitGilFaulted { get; }


	public PurchaseStateMachine()
	{
		// Track the state
		InstanceState(state => state.CurrentState);
		ConfigureEvents();
		ConfigureInitialState();
		ConfigureAny();
		ConfigureAccepted();
		ConfigureItemsGranted();
		ConfigureFaulted();
	}

	// MassTransit will make use of any necessary events in this method.
	private void ConfigureEvents()
	{
		Event(() => PurchaseRequested);
		Event(() => GetPurchaseState);
		Event(() => InventoryItemsGranted);
		Event(() => GilDebited);

		// Failure Events
		Event(() => GrantItemsFaulted,
			x => x.CorrelateById(context => context.Message.Message.CorrelationId));
		Event(() => DebitGilFaulted,
			x => x.CorrelateById(context => context.Message.Message.CorrelationId));
	}

	private void ConfigureInitialState()
	{
		// When receiving a PurchaseRequested event, then we must obtain all of these properties.
		Initially(
			When(PurchaseRequested) // 1. Obtain the props
				.Then(context =>
				{
					// For clarification, instance = this class instance and Data = Incoming Request (i.e PurchaseRequested)
					context.Instance.UserId = context.Data.UserId;
					context.Instance.ItemId = context.Data.ItemId;
					context.Instance.Quantity = context.Data.Quantity;
					context.Instance.Received = DateTimeOffset.Now;
					context.Instance.LastUpdated = DateTimeOffset.Now;
				})
				.Activity(x => x.OfType<CalculatePurchaseTotalActivity>()) // 2. Perform the calculation
				.Send(context => new GrantItems( // 3. Send the GrantItems request to Inventory microservice
					context.Instance.UserId,
					context.Instance.ItemId,
					context.Instance.Quantity,
					context.Instance.CorrelationId))
				.TransitionTo(Accepted) // 4. Happy Case!
				.Catch<Exception>(ex => ex
					.Then(context =>
					{
						// Store the exception (if any) into the instance state
						context.Instance.ErrorMessage = context.Exception.Message;
						context.Instance.LastUpdated = DateTimeOffset.Now;
					})
					.TransitionTo(Faulted))
		);
	}

	private void ConfigureAccepted()
	{
		// During the accepted state, 
		During(Accepted,
			When(InventoryItemsGranted)
				.Then(context =>
				{
					context.Instance.LastUpdated = DateTimeOffset.Now;
				})
				.Send(context => new DebitGil( // Send to Identity service
					context.Instance.UserId,
					context.Instance.PurchaseTotal,
					context.Instance.CorrelationId
				))
				.TransitionTo(ItemsGranted),
			When(GrantItemsFaulted) // This section is for when things fail in some other service
				.Then(context =>
				{
					context.Instance.ErrorMessage = context.Data.Exceptions[0].Message;
					context.Instance.LastUpdated = DateTimeOffset.Now;
				})
				.TransitionTo(Faulted)
			);
	}

	private void ConfigureItemsGranted()
	{
		During(ItemsGranted,
			When(GilDebited) // Identity sends this event back
				.Then(context =>
				{
					context.Instance.LastUpdated = DateTimeOffset.Now;
				})
				.TransitionTo(Completed), // Once Identity sends GilDebit successfully = Done!
			When(DebitGilFaulted) // If something goes wrong, we must revert/tell the service to subtract
				.Send(context => new SubtractItems(
					context.Instance.UserId,
					context.Instance.ItemId,
					context.Instance.Quantity,
					context.Instance.CorrelationId
				))
				.Then(context =>
				{
					context.Instance.ErrorMessage = context.Data.Exceptions[0].Message;
					context.Instance.LastUpdated = DateTimeOffset.Now;
				})
				.TransitionTo(Faulted)
		);
	}

	private void ConfigureAny()
	{
		DuringAny(
			When(GetPurchaseState)
				.Respond(x => x.Instance)
		);
	}

	private void ConfigureFaulted()
	{
		// If there's an error, ignore any of these events
		During(Faulted,
			Ignore(PurchaseRequested),
			Ignore(InventoryItemsGranted),
			Ignore(GilDebited)
		);
	}
}
