using System;
using Microsoft.AspNetCore.Mvc;
using static Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("items")] // https://localhost5001/items
public class ItemsController : ControllerBase
{
	private static readonly List<ItemDto> items = new()
	{
		new ItemDto(Guid.NewGuid(), "Potion", "Restores all amount of HP", 5, DateTimeOffset.UtcNow),
		new ItemDto(Guid.NewGuid(), "Antidote", "Cures poison", 7, DateTimeOffset.UtcNow),
		new ItemDto(Guid.NewGuid(), "Bronze Sword", "Deals a small amount of damage", 20, DateTimeOffset.UtcNow)
	};

	[HttpGet]
	public IEnumerable<ItemDto> Get()
	{
		return items;
	}

	[HttpGet("{id}")] // GET /items/{id}
	public ActionResult<ItemDto> GetById(Guid id)
	{
        var item = items.Where(item => item.id == id).SingleOrDefault();

		if (item is null)
		{
			return NotFound();
		}

		return item;
	}

	[HttpPost]
	public ActionResult<ItemDto> Post(CreateItemDto createItemDto)
	{
		var item = new ItemDto(
			Guid.NewGuid(),
			createItemDto.Name,
			createItemDto.Description,
			createItemDto.Price,
			DateTimeOffset.Now
			);

		items.Add(item);

		return CreatedAtAction(nameof(GetById), new { id = item.id }, item);
	}

	[HttpPut]
	public IActionResult Put(Guid id, UpdateItemDto updateItemDto)
	{
		var existingItem = items.Where(item => item.id == id).SingleOrDefault();

        if (existingItem is null)
        {
            return NotFound();
        }

        var updatedItem = existingItem with
		{
			Name = updateItemDto.Name,
			Description = updateItemDto.Description,
			Price = updateItemDto.Price,
		};

        var index = items.FindIndex(existingItem => existingItem.id == id);
		items[index] = updatedItem;

		return NoContent();
	}

	[HttpDelete("{id}")]
	public IActionResult Delete(Guid id)
	{
		var index = items.FindIndex(existingItem => existingItem.id == id);

		if (index < 0)
		{
			return NotFound(); 
		}

		items.RemoveAt(index);
		return NoContent();
	}
}
