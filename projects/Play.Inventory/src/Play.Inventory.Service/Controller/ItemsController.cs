using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Play.Common;
using Play.Inventory.Service.Clients;
using Play.Inventory.Service.Entities;
using static Play.Inventory.Service.Dtos;

namespace Play.Inventory.Service.Controller;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    /// <summary>
    /// This is a referenece to the MongoDatabase Collection
    /// </summary>
    private readonly IRepository<InventoryItem> itemsRepository;

    /// <summary>
    /// Reference to talk to the Catalog Microservice
    /// </summary>
    private readonly CatalogClient catalogClient;

	public ItemsController(IRepository<InventoryItem> itemsRepository, CatalogClient catalogClient)
	{
		this.itemsRepository = itemsRepository;
		this.catalogClient = catalogClient;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
	{
		if (userId == Guid.Empty)
		{
			return BadRequest();
		}

		// Get Catalog Items from the Catalog Service 1 st
		var catalogItems = await catalogClient.GetCatalogItemsAsync();

		Expression<Func<InventoryItem, bool>> filterFunc = item => item.UserId == userId;

		// The item UserId must be the same from the param userId within our Inventory Db collection
		// If so, grab the item only if it the catalogItem Id matches with inventory CatalogItemId.
		var inventoryItemEntities = await itemsRepository.GetAllAsync(filterFunc);
		var inventoryItemDtos = inventoryItemEntities.Select(inventoryItem =>
		{
			var catalogItem = catalogItems.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);
			return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
		});

		return Ok(inventoryItemDtos); 
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
