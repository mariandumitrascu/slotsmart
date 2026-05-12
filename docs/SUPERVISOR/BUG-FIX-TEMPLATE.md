# Bug Fix Documentation Template

> **Purpose**: Document bug fixes comprehensively for future reference, knowledge sharing, and prevention of similar issues.
> **Save as**: `SESSION-SUMMARIES/BUG-[COMPONENT]-[DESCRIPTION]-FIX.md`

---

## Bug Identification

### Bug ID/Name
**BUG-[COMPONENT]-[DESCRIPTION]** 
Example: `BUG-BOOKING-DOUBLE-BOOKING`, `BUG-AUTH-TOKEN-EXPIRY`

### Severity Level
- 🔴 **CRITICAL**: System down, data loss, security breach
- 🟡 **HIGH**: Feature broken, major functionality impaired
- 🟢 **MEDIUM**: Feature partially working, workaround available
- 🔵 **LOW**: Minor issue, cosmetic, edge case

### Discovery Information
- **Discovered By**: User report | Testing | Code review | Production monitoring
- **Discovery Date**: YYYY-MM-DD
- **Environment**: Production | Staging | Development | Testing
- **Affected Versions**: vX.Y.Z to vA.B.C

---

## Bug Description

### Summary
[One-sentence description of the bug]

### Detailed Description
[Comprehensive description of what's wrong, what should happen vs. what actually happens]

### Impact
**Users Affected**: [All | Specific user group | Edge case] 
**Business Impact**: [How this affects business operations/user experience] 
**Frequency**: [How often does this occur? Always | Sometimes | Rare]

### Reproduction Steps
1. [Step 1 - be specific]
2. [Step 2 - be specific]
3. [Step 3 - be specific]
4. **Result**: [What happens]
5. **Expected**: [What should happen]

### Evidence
- **Error Messages**: 
  ```
  [Paste exact error message]
  ```
- **Logs**:
  ```
  [Paste relevant log entries]
  ```

---

## Root Cause Analysis

### Investigation Process

1. **Initial Hypothesis**: [What you first thought]
2. **Investigation Steps**:
   - [What you checked first]
   - [Key findings]

### Root Cause
**Primary Cause**: [The fundamental reason this bug exists]

**Contributing Factors**:
1. [Factor 1 - e.g., Missing validation]
2. [Factor 2 - e.g., Incorrect assumption]

### Why It Wasn't Caught Earlier
- [ ] Missing test coverage
- [ ] Test had incorrect assumption
- [ ] Edge case not considered
- [ ] Integration gap between components
- [ ] Other: [Explanation]

---

## The Fix

### Solution Approach
**Strategy**: [High-level approach to fixing]

**Alternatives Considered**:
1. **Option A**: [Description] - ❌ Rejected because [reason]
2. **Option B**: [Description] - ✅ **Selected** because [reason]

### Implementation Details

#### Files Modified
```
path/to/file1.ext
path/to/file2.ext
path/to/test_file.ext
```

#### Code Changes Summary
1. **File**: `path/to/file1.ext`
   - **Change**: [What was changed]
   - **Reason**: [Why this change fixes it]

#### Key Code Snippets

**Before** (Buggy Code):
```csharp
[Paste problematic code]
```

**After** (Fixed Code):
```csharp
[Paste corrected code]
```

**Explanation**: [Why this fixes the issue]

---

## Testing & Verification

### Regression Test Added
**Test File**: `path/to/test_file.ext` 
**Test Name**: `[specific_bug_scenario]_ShouldReturnExpected`

```csharp
[Paste test code or pseudocode]
```

**Test Coverage**:
- ✅ Reproduces original bug (fails on old code)
- ✅ Passes with fix
- ✅ Covers edge cases

### Manual Verification
**Verification Steps**:
1. [Step to verify fix works]
2. [Step to verify no regression]

**Verification Result**: ✅ PASS | ❌ FAIL (with details)

---

## Prevention Strategy

### Immediate Actions
- [ ] Add regression test (Done above)
- [ ] Update related tests
- [ ] Fix similar issues in codebase
- [ ] Update error handling

### Long-Term Improvements
1. **Documentation**:
   - [ ] Update code comments
   - [ ] Update API documentation
   - [ ] Add architecture notes

2. **Process Improvements**:
   - [ ] Add to code review checklist
   - [ ] Update testing guidelines

---

## Timeline

| Date | Event |
|------|-------|
| YYYY-MM-DD | Bug discovered |
| YYYY-MM-DD | Investigation started |
| YYYY-MM-DD | Root cause identified |
| YYYY-MM-DD | Fix implemented |
| YYYY-MM-DD | Tests added |
| YYYY-MM-DD | Fix deployed |
| YYYY-MM-DD | Verified in production |

**Total Time**: X hours investigation + Y hours implementation = Z hours total

---

## Deployment Notes

### Deployment Checklist
- [ ] All tests passing
- [ ] Code reviewed
- [ ] Changelog updated
- [ ] Deployed to staging
- [ ] Verified in staging
- [ ] Deployed to production
- [ ] Verified in production

---

## Metadata

**Author**: [Who fixed it] 
**Date**: YYYY-MM-DD 
**Version Fixed**: vX.Y.Z 
**Framework Version**: 1.2.0 
**Template Version**: 1.0

**Tags**: `bug`, `[component]`, `[severity]`, `[category]`

---

**Last Updated**: YYYY-MM-DD 
**Status**: ✅ FIXED | 🚧 IN PROGRESS | ⏸️ DEFERRED | ❌ WONTFIX
