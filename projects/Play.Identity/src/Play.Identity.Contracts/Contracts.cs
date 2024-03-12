namespace Play.Identity.Contracts;

/// <summary>
/// Event that Debits Gil accordingly to the the user
/// </summary>
public record DebitGil(
    Guid UserId,
    decimal Gil,
    Guid CorrelationId);

/// <summary>
/// Event response for debitted gil.
/// </summary>
public record GilDebited(Guid CorrelationId);
