# Condition-Based Waiting

Replace arbitrary `await Task.Delay()` timeouts with condition polling.

## The Problem with Arbitrary Timeouts

```csharp
// BAD: Arbitrary timeout
await Task.Delay(5000); // Hope 5 seconds is enough
var result = await service.GetStatusAsync();
```

Problems:
- Too short: flaky tests, race conditions
- Too long: slow tests, wasted CI time
- Non-deterministic: works on fast machines, fails on slow ones

## The Solution: Condition-Based Waiting

Poll for a condition to be true, with a timeout as a safety net:

```csharp
// GOOD: Condition-based waiting
await WaitForConditionAsync(
    condition: async () => {
        var status = await service.GetStatusAsync();
        return status == ExpectedStatus.Ready;
    },
    timeout: TimeSpan.FromSeconds(10),
    pollInterval: TimeSpan.FromMilliseconds(100)
);
```

## Helper Implementation

```csharp
public static async Task WaitForConditionAsync(
    Func<Task<bool>> condition,
    TimeSpan timeout,
    TimeSpan? pollInterval = null)
{
    var interval = pollInterval ?? TimeSpan.FromMilliseconds(100);
    var deadline = DateTime.UtcNow + timeout;
    
    while (DateTime.UtcNow < deadline)
    {
        if (await condition())
            return;
            
        await Task.Delay(interval);
    }
    
    throw new TimeoutException($"Condition not met within {timeout.TotalSeconds}s");
}
```

## When to Use

- Integration tests waiting for async operations
- Tests waiting for background jobs to complete
- Tests waiting for eventual consistency
- Any code that needs to wait for an external state change

## Key Rules

- Always have a timeout (even condition-based waiting needs a safety net)
- Keep poll intervals short (100-500ms for most cases)
- Make timeout long enough to handle slow CI (2-10x normal expected time)
- Throw a meaningful error if timeout expires
