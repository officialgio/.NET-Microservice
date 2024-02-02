using System;
namespace Play.Catalog.Service;

public class Dtos
{
	public record ItemDto(Guid id, string Name, string Description, decimal Price, DateTimeOffset createdDate);
	public record CreateItemDto(string Name, string Description, decimal Price);
	public record UpdateItemDto(string Name, string Description, decimal Price);
}

