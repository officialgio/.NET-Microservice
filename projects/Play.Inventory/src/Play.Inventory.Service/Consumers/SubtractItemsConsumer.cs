using System.Linq.Expressions;
using MassTransit;
using Play.Common;
using Play.Inventory.Service.Entities;
using static Play.Inventory.Service.Dtos;
using static Play.Inventory.Contracts;

namespace Play.Inventory.Service.Consumers;

public class SubtractItemsConsumer : IConsumer<SubtractItems>
{
    /// <summary>
    /// This is a referenece to the MongoDatabase Collection
    /// </summary>
    private readonly IRepository<InventoryItem> inventoryItemsRepository;

    /// <summary>
    /// This is a referenece to the MongoDatabase Collection
    /// </summary>
    private readonly IRepository<CatalogItem> catalogItemsRepository;

    public SubtractItemsConsumer(IRepository<InventoryItem> inventoryItemsRepository, IRepository<CatalogItem> catalogItemsRepository)
	{
        this.inventoryItemsRepository = inventoryItemsRepository;
        this.catalogItemsRepository = catalogItemsRepository;
	}

    // First, check if the item exists in the db, if so, grab the item and ubstract the quantity that was requested initially.
    public async Task Consume(ConsumeContext<SubtractItems> context)
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

        if (inventoryItem is not null)
        {
            inventoryItem.Quantity -= message.Quantity;
            await inventoryItemsRepository.UpdateAsync(inventoryItem);
        }

        await context.Publish(new InventoryItemsSubtracted(message.CorrelationId));
    }
}
