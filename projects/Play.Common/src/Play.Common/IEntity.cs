using System;

namespace Play.Common;

/// <summary>
/// For generic Models usage
/// </summary>
public interface IEntity
{
    Guid Id { get; set; }
}
