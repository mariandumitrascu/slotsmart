namespace SlotSmart.Domain.Common;

/// <summary>
/// Base class for every persistent entity in the SlotSmart domain.
/// </summary>
/// <remarks>
/// Implements the dual-key identity pattern (ADR-007 / docs/plan/00-architecture/entity-identity.md):
/// <list type="bullet">
///   <item><see cref="EntityId"/> — public, stable, externally shareable identifier (UUIDv7).
///         The only id the domain layer knows about.</item>
///   <item>The storage primary key is a hidden <c>bigint</c> surrogate added by EF Core as a shadow
///         property. Domain code must never reference the surrogate; equality is by EntityId only.</item>
/// </list>
/// </remarks>
public abstract class Entity
{
    /// <summary>Public domain identifier — UUIDv7. Set in the constructor / factory; never mutated.</summary>
    public Guid EntityId { get; protected init; }

    public override bool Equals(object? obj) =>
        obj is Entity other && GetType() == other.GetType() && EntityId == other.EntityId;

    public override int GetHashCode() => HashCode.Combine(GetType(), EntityId);

    public static bool operator ==(Entity? left, Entity? right) => Equals(left, right);
    public static bool operator !=(Entity? left, Entity? right) => !Equals(left, right);
}
