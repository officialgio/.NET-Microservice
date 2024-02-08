using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service.Controller;

[ApiController]
[Route("items")] // base route
public class ItemsController : ControllerBase
{
	private readonly IRepository<InventoryItem> itemsRepository;

	public ItemsController(IRepository<InventoryItem> itemsRepository)
	{
		this.itemsRepository = itemsRepository;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
	{
		if (userId == Guid.Empty)
		{
			return BadRequest();
		}

		Expression<Func<InventoryItem, bool>> filterFunc = item => item.UserId == userId;

		var items = (await itemsRepository.GetAllAsync(filterFunc)).Select(item => item.AsDto());

		return Ok(items); 
	}

	[HttpPost]
	public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
	{
		// Check if it exist, if it it's null create a new InventoryItem
		// Otherwise, increment the item and update it.
		Expression<Func<InventoryItem, bool>> filter =
			item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId;

		var inventoryItem = await itemsRepository.GetAsync(filter);

		if (inventoryItem is null)
		{
			inventoryItem = new InventoryItem()
			{
				CatalogItemId = grantItemsDto.CatalogItemId,
				UserId = grantItemsDto.UserId,
				Quantity = grantItemsDto.Quantity,
				AcquiredDate = DateTimeOffset.UtcNow,
			};

			await itemsRepository.CreateAsync(inventoryItem);
		}
		else
		{
			inventoryItem.Quantity += grantItemsDto.Quantity;
			await itemsRepository.UpdateAsync(inventoryItem);
		}
		return Ok();
	}
}
