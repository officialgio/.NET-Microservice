using Play.Common;

namespace Play.Catalog.Service.Entities;

/// <summary>
/// The model is used to create an Item.
/// </summary>
public class Item : IEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

