using System;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Entities; 
using Play.Common;
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

	private static int requestCounter = 0;

	public ItemsController(IRepository<Item> itemsRepository)
	{
		this.itemsRepository = itemsRepository;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
	{
		requestCounter++;

		Console.WriteLine($"request: {requestCounter} - Starting...");

		if (requestCounter <= 2)
		{
            Console.WriteLine($"request: {requestCounter} - Delaying...");
			await Task.Delay(TimeSpan.FromSeconds(10)) ;
        }

        if (requestCounter <= 4)
        {
            Console.WriteLine($"request: {requestCounter} - 500 (Internal Server Error)...");
			return StatusCode(500);
        }

        var items = (await itemsRepository.GetAllAsync())
			.Select(item => item.AsDto());

        Console.WriteLine($"request: {requestCounter} - 200 (Ok)");
        return Ok(items);
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

		return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item);
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

		await itemsRepository.RemoveAsync(item.Id);

		return NoContent();
	}
}
