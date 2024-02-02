using System;
using System.ComponentModel.DataAnnotations;

namespace Play.Catalog.Service;

/// <summary>
/// Record representations of Item Dtos
/// </summary>
public class Dtos
{
	public record ItemDto(Guid id, string Name, string Description, decimal Price, DateTimeOffset createdDate);
	public record CreateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);
	public record UpdateItemDto([Required] string Name, string Description, [Range(0, 1000)] decimal Price);
}

