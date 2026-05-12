# Session Handover Template

> **Purpose**: Use this template to create clear handover documentation when ending a work session or transitioning between SUPERVISOR and WORKER roles.
> **Save as**: `SESSION-SUMMARIES/HANDOVER-[DATE]-[TOPIC].md`

---

## Session Summary

- **Date**: YYYY-MM-DD
- **Role**: SUPERVISOR | WORKER | HYBRID
- **Duration**: X hours
- **Primary Goal**: [What you set out to accomplish this session]
- **Status**: ✅ COMPLETED | 🚧 IN PROGRESS | ⏸️ PAUSED | ❌ BLOCKED

---

## Work Completed ✅

### Features/Tasks Delivered
1. **[Feature/Task Name]**
   - Location: `path/to/files`
   - Status: Completed/Tested/Deployed
   - Tests: X tests added, Y% coverage
   
2. **[Feature/Task Name]**
   - Location: `path/to/files`
   - Status: Completed/Tested/Deployed
   - Tests: X tests added, Y% coverage

### Bugs Fixed 🐛
1. **[Bug Name/ID]**
   - Root Cause: [Brief description]
   - Fix: [What was changed]
   - Test: [Regression test added]

---

## Work In Progress 🚧

### Incomplete Tasks
1. **[Task Name]**
   - Progress: X% complete
   - Next Step: [Specific next action]
   - Blocker: [None | Description of blocker]
   - Files: `path/to/files`

---

## Decisions Made 📋

### Technical Decisions
1. **[Decision Title]**
   - **Decision**: [What was decided]
   - **Rationale**: [Why this approach]
   - **Impact**: [What this affects]
   - **Documented In**: DECISIONS-LOG.md

### Strategic Decisions
1. **[Decision Title]**
   - **Decision**: [What was decided]
   - **Rationale**: [Why this approach]
   - **Impact**: [What this affects]
   - **Documented In**: THINKING-LOG.md

---

## Files Modified 📝

### New Files Created
- `path/to/new/file.ext` - [Purpose]
- `path/to/new/test.ext` - [Test file]

### Files Modified
- `path/to/modified/file.ext` - [What changed]
- `path/to/modified/file.ext` - [What changed]

---

## Test Status 🧪

### Tests Added
- **Unit Tests**: X added (Y total, Z% coverage)
- **Integration Tests**: X added (Y total)

### Test Results
- ✅ All tests passing (X/X)
- ⚠️ Some tests failing (X/Y) - [Details]
- ❌ Tests broken - [Action required]

---

## Known Issues ⚠️

### Blockers (Critical)
1. **[Issue Description]**
   - Impact: [What's blocked]
   - Workaround: [Temporary solution if any]
   - Action Required: [Who needs to do what]
   - ETA: [When can this be resolved]

### Non-Critical Issues
1. **[Issue Description]**
   - Impact: [Minor impact description]
   - Plan: [How/when to address]
   - Priority: LOW | MEDIUM

---

## Next Steps ➡️

### Immediate (Next Session)
1. **[Task]** - [Brief description, estimated effort]
2. **[Task]** - [Brief description, estimated effort]
3. **[Task]** - [Brief description, estimated effort]

### Short-Term (This Week)
1. **[Task]** - [Brief description]
2. **[Task]** - [Brief description]

---

## Questions for Next Session ❓

### Decisions Needed
1. **[Question/Decision Point]**
   - Context: [Why this needs deciding]
   - Options: [A, B, C]
   - Impact: [What depends on this]

---

## Context for Next AI Session 🤖

### If Continuing as SUPERVISOR
- Review DELEGATION-TRACKER.md for task status
- Check CURRENT-STATUS.md for project health
- Validate completed work from this session
- Plan next delegations

### If Continuing as WORKER
- Check for new delegation prompt in [location]
- Review relevant files: [list key files]
- Understand context: [key points to know]

### Key Context to Remember
- [Important context point 1]
- [Important context point 2]
- [Important context point 3]

---

## Updated Documents 📚

Mark which documentation was updated:

- [ ] CURRENT-STATUS.md - [What changed]
- [ ] DELEGATION-TRACKER.md - [Tasks added/updated/completed]
- [ ] CHANGELOG.md - [Entries added]
- [ ] THINKING-LOG.md - [Strategic decisions]
- [ ] DECISIONS-LOG.md - [Technical decisions]
- [ ] README.md - [Documentation updates]
- [ ] [Other] - [Description]

---

## Handover Checklist ✅

Before ending session, ensure:

- [ ] All changes committed locally
- [ ] Tests pass (or failures documented)
- [ ] No linter errors (or acceptable warnings documented)
- [ ] CURRENT-STATUS.md updated
- [ ] CHANGELOG.md updated (if code changed)
- [ ] DELEGATION-TRACKER.md updated (if tasks changed)
- [ ] Decision logs updated (if applicable)
- [ ] Known issues documented
- [ ] Next steps clearly defined
- [ ] This handover document completed

---

**Handover Created By**: [SUPERVISOR | WORKER | HYBRID] 
**Next Session Should Start With**: [Read X, Review Y, Continue Z] 
**Estimated Pickup Time**: [X minutes with this handover]

---

**Template Version**: 1.0 
**Framework Version**: 1.3.1 
**Last Updated**: May 2026
