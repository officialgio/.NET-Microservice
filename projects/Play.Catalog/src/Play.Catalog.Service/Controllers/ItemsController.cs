using System;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Repositories;
using static Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")] // https://localhost5001/items
public class ItemsController : ControllerBase
{
	/// <summary>
	/// Use this to talk to the Data Layer of the service.
	/// </summary>
	private readonly IRepository<Item> itemsRepository;

	public ItemsController(IRepository<Item> itemsRepository)
	{
		this.itemsRepository = itemsRepository;
	}

	[HttpGet]
	public async Task<IEnumerable<ItemDto>> GetAsync()
	{
		var items = (await itemsRepository.GetAllAsync())
			.Select(item => item.AsDto());

		return items;
	}

	[HttpGet("{id}")]
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
	public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
	{
		var item = new Item
		{
			Name = createItemDto.Name,
			Description = createItemDto.Description,
			Price = createItemDto.Price,
			CreatedAt = DateTimeOffset.UtcNow
		};

		await itemsRepository.CreateAsync(item);

		return CreatedAtAction(nameof(GetByIdAsync), new { id = item.id }, item);
	}

	[HttpPut]
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

		await itemsRepository.UpdateAsync(existingItem);

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteAsync(Guid id)
	{
		var item =  await itemsRepository.GetAsync(id); 

		if (item is null)
		{
			return NotFound(); 
		}

		await itemsRepository.RemoveAsync(item.id);

		return NoContent();
	}
}
