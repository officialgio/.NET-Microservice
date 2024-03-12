using System.Linq.Expressions;
using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Inventory.Service.Dtos;
using static Play.Inventory.Contracts;

namespace Play.Inventory.Service.Consumers;

public class GrantItemsConsumer : IConsumer<GrantItems>
{
    /// <summary>
    /// This is a referenece to the MongoDatabase Collection
    /// </summary>
    private readonly IRepository<InventoryItem> inventoryItemsRepository;

    /// <summary>
    /// This is a referenece to the MongoDatabase Collection
    /// </summary>
    private readonly IRepository<CatalogItem> catalogItemsRepository;

    public GrantItemsConsumer(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepository)
	{
        this.inventoryItemsRepository = inventoryItemsRepository;
        this.catalogItemsRepository = catalogItemsRepository;
	}

    // First, check if the item exist, if so, get the item and assign it to a user.
    public async Task Consume(ConsumeContext<GrantItems> context)
    {
        var message = context.Message;

        var item = await catalogItemsRepository.GetAsync(message.CatalogItemId);

        if (item is null)
        {
            throw new UnknownItemException(message.CatalogItemId);
        }

        // Check if it exist, if it it's null create a new InventoryItem
        // Otherwise, increment the item and update it.
        Expression<Func<InventoryItem, bool>> filter =
            item => item.UserId == message.UserId && item.CatalogItemId == message.CatalogItemId;

        var inventoryItem = await inventoryItemsRepository.GetAsync(filter);

        if (inventoryItem is null)
        {
            inventoryItem = new InventoryItem()
            {
                CatalogItemId = message.CatalogItemId,
                UserId = message.UserId,
                Quantity = message.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow,
            };

            await inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += message.Quantity;
            await inventoryItemsRepository.UpdateAsync(inventoryItem);
        };

        await context.Publish(new InventoryItemsGranted(message.CorrelationId));
    }
}

