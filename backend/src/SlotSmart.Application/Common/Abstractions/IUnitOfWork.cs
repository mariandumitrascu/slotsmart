namespace SlotSmart.Application.Common.Abstractions;

/// <summary>
/// Single transactional boundary for a use case. Hides the EF Core <c>SaveChangesAsync</c>
/// call from Application handlers so they don't need to depend on the DbContext directly.
/// </summary>
/// <remarks>
/// The Infrastructure-layer implementation forwards to <c>SlotSmartDbContext.SaveChangesAsync</c>.
/// Cross-aggregate transactions (rare in this domain) use <c>BeginTransactionAsync</c>.
/// </remarks>
public interface IUnitOfWork
{
    /// <summary>Commits all pending changes in the current scope. Returns the count of affected entities.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
