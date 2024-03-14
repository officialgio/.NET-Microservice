using System;
using Automatonymous;

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

	public PurchaseStateMachine()
	{
		// Track the state
		InstanceState(state => state.CurrentState);
		ConfigureEvents();
		ConfigureInitialState();
		ConfigureAny();
	}

	// MassTransit will make use of any necessary events in this method.
	private void ConfigureEvents()
	{
		Event(() => PurchaseRequested);
		Event(() => GetPurchaseState);
	}

	private void ConfigureInitialState()
	{
		// When receiving a PurchaseRequested event, then we must obtain all of these properties.
		Initially(
			When(PurchaseRequested)
				.Then(context =>
				{
					// For clarification, instance = this class instance and Data = Incoming Request (i.e PurchaseRequested)
					context.Instance.UserId = context.Data.UserId;
					context.Instance.ItemId = context.Data.ItemId;
					context.Instance.Quantity = context.Data.Quantity;
					context.Instance.Received = DateTimeOffset.Now;
					context.Instance.LastUpdated = DateTimeOffset.Now;
				})
				.TransitionTo(Accepted)
		);
	}

	private void ConfigureAny()
	{
		DuringAny(
			When(GetPurchaseState)
				.Respond(x => x.Instance)
		);
	}
}
