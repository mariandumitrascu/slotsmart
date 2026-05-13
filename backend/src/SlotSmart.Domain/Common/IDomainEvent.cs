namespace SlotSmart.Domain.Common;

/// <summary>
/// Marker interface for domain events emitted by aggregates. Dispatched after <c>SaveChanges</c>
/// via the EF interceptor wired up in P1-T02 / P5-T05 (outbox pattern).
/// </summary>
public interface IDomainEvent
{
    /// <summary>UTC timestamp of when the event was raised in the domain.</summary>
    DateTimeOffset OccurredOnUtc { get; }
}
