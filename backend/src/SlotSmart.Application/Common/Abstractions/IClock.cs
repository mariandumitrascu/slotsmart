namespace SlotSmart.Application.Common.Abstractions;

/// <summary>
/// Time source abstraction. The whole codebase asks for "now" through this — never
/// directly via <c>DateTimeOffset.UtcNow</c> — so deterministic time can be injected in tests.
/// </summary>
/// <remarks>
/// We always carry <see cref="DateTimeOffset"/>, never <see cref="DateTime"/>:
/// <see cref="DateTime"/> drops the offset and is a frequent source of "off by N hours" bugs
/// across DST boundaries (relevant for booking windows, training schedules, audit timestamps).
/// </remarks>
public interface IClock
{
    /// <summary>The current UTC instant, with offset = +00:00.</summary>
    DateTimeOffset UtcNow { get; }
}
