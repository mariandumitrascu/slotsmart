using SlotSmart.Application.Common.Abstractions;

namespace SlotSmart.Infrastructure.Time;

/// <summary>
/// Default <see cref="IClock"/>: returns the OS UTC time. Registered as singleton.
/// Tests substitute a controllable fake.
/// </summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
