﻿using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Trading.Service.StateMachines;

namespace Play.Trading.Service.Controllers;

[ApiController]
[Route("purchase")]
[Authorize]
public class PurchaseController : ControllerBase
{
	/// <summary>
	/// Instance to publish messages.
	/// </summary>
	private readonly IPublishEndpoint publishEndpoint;

	/// <summary>
	/// To send or publish requests and wait for a response. The request client is asynchronous.
	/// </summary>
	private readonly IRequestClient<GetPurchaseState> purchaseClient;

	public PurchaseController(IPublishEndpoint publishEndpoint, IRequestClient<GetPurchaseState> purchaseClient)
	{
		this.publishEndpoint = publishEndpoint;
		this.purchaseClient = purchaseClient;
	}

	[HttpGet("status/{correlationId}")]
	public async Task<ActionResult<PurchaseDto>> GetStatusAsync(Guid correlationId)
	{
		var response = await purchaseClient.GetResponse<PurchaseState>(new GetPurchaseState(correlationId));
		var purchaseState = response.Message;

		var purchase = new PurchaseDto(
			purchaseState.UserId,
			purchaseState.ItemId,
			purchaseState.PurchaseTotal,
			purchaseState.Quantity,
			purchaseState.CurrentState,
			purchaseState.ErrorMessage,
			purchaseState.Received,
			purchaseState.LastUpdated
		);

		return Ok(purchase);
	}

	[HttpPost]
	public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
	{
		var userId = User.FindFirst("sub").Value; // Identity Service Provider
		var correlationId = Guid.NewGuid();

		var message = new PurchaseRequested(
			Guid.Parse(userId),
			purchase.ItemId.Value,
			purchase.Quantity,
			correlationId
			);

		await publishEndpoint.Publish(message);
		return AcceptedAtAction(nameof(GetStatusAsync), new { correlationId }, new { correlationId });
	}
}
