using System;

namespace Play.Catalog.Contracts;

/// <summary>
/// This class serves as contracts for only information the separate service consumer needs.
/// Services who needs to retrieves events within the message broker (e.g RabbitMQ) will receive
/// these records.
/// </summary>
public class Contracts
{
    public record CatalogItemCreated(
        Guid ItemId,
        string Name,
        string Description,
        decimal Price
        );
    public record CatalogItemUpdated(
        Guid ItemId,
        string Name,
        string Description,
        decimal Price);

    public record CatalogItemDeleted(Guid ItemId);
}
