namespace SlotSmart.Domain.Common;

/// <summary>
/// Marker interface identifying an aggregate root. Aggregate roots are the only entities loaded
/// directly via the DbContext; all other entities are reached through a root.
/// </summary>
public interface IAggregateRoot
{
}
