using System;
using Automatonymous;
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

	public PurchaseStateMachine()
	{
		// Track the state
		InstanceState(state => state.CurrentState);
		ConfigureEvents();
		ConfigureInitialState();
		ConfigureAny();
		ConfigureAccepted();
	}

	// MassTransit will make use of any necessary events in this method.
	private void ConfigureEvents()
	{
		Event(() => PurchaseRequested);
		Event(() => GetPurchaseState);
		Event(() => InventoryItemsGranted);
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
				.TransitionTo(ItemsGranted));
	}

	private void ConfigureAny()
	{
		DuringAny(
			When(GetPurchaseState)
				.Respond(x => x.Instance)
		);
	}
}
