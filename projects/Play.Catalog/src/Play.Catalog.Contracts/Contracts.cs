namespace Play.Catalog.Contracts;

/// <summary>
/// This class serves as contracts for only information the seperate service consumer needs.
/// Services who needs to retrieves events within the message broker (e.g RabbitMQ) will receive
/// these records.
/// </summary>
public class Contracts
{
    public record CatalogItemCreated(Guid Id, string Name, string Description);
    public record CatalogItemUpdated(Guid Id, string Name, string Description);
    public record CatalogItemDeleted(Guid Id);
}
