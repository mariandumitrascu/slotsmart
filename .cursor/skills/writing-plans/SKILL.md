---
name: writing-plans
description: Use when you have a spec or requirements for a multi-step task, before touching code
---

# Writing Plans

## Overview

Write comprehensive implementation plans assuming the engineer has zero context for our codebase and questionable taste. Document everything they need to know: which files to touch for each task, code, testing, docs they might need to check, how to test it. Give them the whole plan as bite-sized tasks. DRY. YAGNI. TDD. Frequent commits.

Assume they are a skilled developer, but know almost nothing about our toolset or problem domain. Assume they don't know good test design very well.

**Announce at start:** "I'm using the writing-plans skill to create the implementation plan."

**Save plans to:** `docs/SUPERVISOR/plans/YYYY-MM-DD-[name].md`

## Scope Check

If the spec covers multiple independent subsystems, suggest breaking this into separate plans — one per subsystem. Each plan should produce working, testable software on its own.

## File Structure

Before defining tasks, map out which files will be created or modified and what each one is responsible for.

- Design units with clear boundaries and well-defined interfaces
- Prefer smaller, focused files over large ones that do too much
- Files that change together should live together
- In existing codebases, follow established patterns

## Bite-Sized Task Granularity

**Each step is one action (2-5 minutes):**
- "Write the failing test" - step
- "Run it to make sure it fails" - step
- "Implement the minimal code to make the test pass" - step
- "Run the tests and make sure they pass" - step
- "Commit" - step

## Plan Document Header

**Every plan MUST start with this header:**

```markdown
# [Feature Name] Implementation Plan

**Goal:** [One sentence describing what this builds]

**Architecture:** [2-3 sentences about approach]

**Tech Stack:** [Key technologies/libraries]

---
```

## Task Structure

````markdown
### Task N: [Component Name]

**Files:**
- Create: `exact/path/to/file.cs`
- Modify: `exact/path/to/existing.cs`
- Test: `tests/exact/path/to/TestClass.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Test]
public void SpecificBehavior_ShouldReturnExpected()
{
    // Arrange
    // Act
    // Assert
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "SpecificBehavior_ShouldReturnExpected"`
Expected: FAIL with "method not found" or similar

- [ ] **Step 3: Write minimal implementation**

```csharp
// minimal code here
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "SpecificBehavior_ShouldReturnExpected"`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add .
git commit -m "feat: add specific feature"
```
````

## Remember
- Exact file paths always
- Complete code in plan (not "add validation")
- Exact commands with expected output
- DRY, YAGNI, TDD, frequent commits

## Execution Handoff

After saving the plan:

**"Plan complete and saved to `docs/SUPERVISOR/plans/[name].md`. Ready to execute?"**
