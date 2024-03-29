﻿using System;
using Play.Common;

namespace Play.Inventory.Service.Entities;

/// <summary>
/// This class contructs the InventoryItem entity. Several of its memebers (i.e UserId, CatalogItemId) will be used
/// for reference purpose to check its existance on the current and seperate Microservices.
/// </summary>
public class InventoryItem : IEntity
{
    /// <summary>
    /// Id of the IventoryItem.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Id of the UserId to be reference to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Id of the CatalogItem to be reference to.
    /// </summary>
    public Guid CatalogItemId { get; set; }

    /// <summary>
    /// Total quantity of items.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Date of purchased.
    /// </summary>
    public DateTimeOffset AcquiredDate { get; set; }

    /// <summary>
    /// Represents unique identifiers for the messages being consumed
    /// </summary>
    public HashSet<Guid> MessageIds { get; set; } = new();
}
