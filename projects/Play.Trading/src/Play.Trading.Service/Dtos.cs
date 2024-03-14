using System;
using System.ComponentModel.DataAnnotations;

namespace Play.Trading.Service;

/// <summary>
/// Record that describes the POST request
/// </summary>
public record SubmitPurchaseDto(
    [Required] Guid? ItemId,
    [Range(1, 100)] int Quantity
);

/// <summary>
/// Record that describes the GET request
/// </summary>
public record PurchaseDto(
    Guid UserId,
    Guid ItemId,
    decimal? PurchaseTotal,
    int Quantity,
    string State,
    string Reason,
    DateTimeOffset Received,
    DateTimeOffset LastUpdated
);