using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

	public PurchaseController(IPublishEndpoint publishEndpoint)
	{
		this.publishEndpoint = publishEndpoint;
	}

	[HttpPost]
	public async Task<IActionResult> PostAsync(SubmitPurchaseDto purchase)
	{
		var userId = User.FindFirst("sub").ToString();
		var correlationId = Guid.NewGuid();

		var message = new PurchaseRequested(
			Guid.Parse(userId),
			purchase.ItemId.Value,
			purchase.Quantity,
			correlationId
			);

		await publishEndpoint.Publish(message);
		return Accepted();
	}
}
