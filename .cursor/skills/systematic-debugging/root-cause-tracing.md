# Root Cause Tracing

A technique for tracing bugs backward through a call stack to find the original trigger.

## When to Use

Use when:
- Error is thrown deep in a call stack
- You see the symptom but not the cause
- Multiple layers are involved (API → Service → Repository → Database)
- Data is wrong but you don't know where it went wrong

## The Backward Tracing Technique

### Step 1: Start at the Symptom

Note the exact error, line, and location where the symptom occurs.

### Step 2: Ask "What Called This?"

For each layer, ask:
- What code called the failing code?
- What data was passed in?
- Was the data already wrong when it arrived?

### Step 3: Trace Upward Until Data is Correct

Keep moving up the call stack until you find the point where:
- Data that should be valid is first made invalid, OR
- A missing check/validation should have caught the issue, OR
- The wrong value was first introduced

### Step 4: Fix at the Source

Fix where the data goes wrong, not where the symptom manifests.

## Example (SlotSmart Context)

**Symptom**: `NullReferenceException` in `BookingService.CreateAsync()`

**Wrong approach**: Add null check in `BookingService`

**Right approach**: Trace backward:
1. `BookingService.CreateAsync()` received null `trainingId`
2. `BookingsController.Post()` passed null `trainingId` from DTO
3. DTO deserialization failed silently (nullable Guid in DTO)
4. **Fix at source**: Make DTO field required (`[Required]`), or validate before passing

## Template

```
Error: [exact error]
Location: [file:line]

Trace:
  Layer 4 (symptom): [what failed and why]
    ↑ called by
  Layer 3: [what data was passed here]
    ↑ called by
  Layer 2: [where did data become wrong?]
    ↑ called by
  Layer 1 (source): [root cause - fix HERE]
```

## Key Rules

- Fix at the SOURCE, not the symptom
- Adding null checks at symptom location is a band-aid, not a fix
- Every layer should be able to assume its inputs are valid (validated upstream)
- Prefer validation at entry points (API controllers, use case boundaries)
