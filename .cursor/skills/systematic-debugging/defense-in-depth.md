# Defense in Depth

A pattern for adding validation at multiple layers after finding a root cause.

## When to Use

After finding a root cause bug, consider whether validation should be added at multiple layers to prevent similar issues in the future.

## The Pattern

Defense in depth means you don't rely on a single layer to catch all problems. Instead, each layer defends itself:

```
API Layer     → Validate request shape (DTOs, model binding)
Use Case      → Validate business rules
Domain        → Enforce invariants (domain model always valid)
Repository    → Trust domain model, but handle DB constraints
```

## Example (SlotSmart Context)

**Bug Found**: Booking created for a training that's at capacity

**Root Cause Fix**: Add capacity check in `BookingService.CreateAsync()`

**Defense in Depth**:
1. **API Layer**: Return 409 Conflict if training full (caught early)
2. **Application Layer**: Check capacity before creating booking
3. **Domain Layer**: `Training.AddBooking()` throws if at capacity (invariant)
4. **Database**: Unique constraint or check constraint on booking count (last resort)

## Why Multiple Layers?

- Domain model is reusable (not always called through API)
- Database constraints are the ultimate safety net
- Early validation improves API response clarity
- Defense in depth prevents data corruption

## Template

When fixing a bug, ask:
1. Where should this have been caught EARLIEST?
2. Where is it MOST IMPORTANT to catch it?
3. What's the LAST LINE OF DEFENSE in the database?

Then add appropriate validation at each relevant layer.
