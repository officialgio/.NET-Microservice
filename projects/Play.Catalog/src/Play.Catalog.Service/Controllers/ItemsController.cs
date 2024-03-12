using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using Play.Common;
using static Play.Catalog.Contracts.Contracts;
using static Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")] // https://localhost5001/items
public class ItemsController : ControllerBase
{
	/// <summary>
	/// Anyone hitting this API must have an admin claim.
	/// </summary>
	private const string AdminRole = "Admin";

	/// <summary>
	/// Use this to talk to the Data Layer of the service.
	/// </summary>
	private readonly IRepository<Item> itemsRepository;

	/// <summary>
	/// Use this to send messages to some location. 
	/// </summary>
	private readonly IPublishEndpoint publishEndPoint;

	public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndPoint)
	{
		this.itemsRepository = itemsRepository;
		this.publishEndPoint = publishEndPoint;
	}

	[HttpGet]
	[Authorize(Policies.Read)]
	public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
	{
		var items = (await itemsRepository.GetAllAsync())
			.Select(item => item.AsDto());

		return Ok(items);
	}

	[HttpGet("{id}")]
	[Authorize(Policies.Read)]
	public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
	{
		var item = await itemsRepository.GetAsync(id);

		if (item is null)
		{
			return NotFound();
		}

		return item.AsDto();
	}

	[HttpPost]
	[Authorize(Policies.Write)]
	public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
	{
		var item = new Item
		{
			Name = createItemDto.Name,
			Description = createItemDto.Description,
			Price = createItemDto.Price,
			CreatedAt = DateTimeOffset.UtcNow
		};

		// Create and publish the message for consumer usage 
		await itemsRepository.CreateAsync(item);
		await publishEndPoint.Publish(new CatalogItemCreated(
			item.Id,
			item.Name,
			item.Description,
			item.Price));

		return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
	}

	[HttpPut("{id}")]
    [Authorize(Policies.Write)]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
	{
		var existingItem = await itemsRepository.GetAsync(id);

		if (existingItem is null)
		{
			return NotFound();
		}

		existingItem.Name = updateItemDto.Name;
		existingItem.Description = updateItemDto.Description;
		existingItem.Price = updateItemDto.Price;

        // Create and publish the message for consumer usage
        await itemsRepository.UpdateAsync(existingItem);
		await publishEndPoint.Publish(new CatalogItemUpdated(
			existingItem.Id,
			existingItem.Name,
			existingItem.Description,
			existingItem.Price));

		return NoContent();
	}

	[HttpDelete("{id}")]
    [Authorize(Policies.Write)]
    public async Task<IActionResult> DeleteAsync(Guid id)
	{
		var item = await itemsRepository.GetAsync(id);

		if (item is null)
		{
			return NotFound();
		}

        // Delete and publish the message for consumer usage
        await itemsRepository.RemoveAsync(item.Id);
		await publishEndPoint.Publish(new CatalogItemDeleted(id));

		return NoContent();
	}
}
