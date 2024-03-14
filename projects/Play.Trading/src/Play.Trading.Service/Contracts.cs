using System;

namespace Play.Trading.Service;

/// <summary>
/// This record will only be used within this service.
/// </summary>
public record PurchaseRequested(
    Guid UserId,
    Guid ItemId,
    int Quantity,
    Guid CorrelationId);
