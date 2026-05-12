# Emergency Response Protocol

> **Purpose**: Define procedures for handling critical situations, blockers, and emergencies during AI-assisted development.

**Framework Version**: 1.2.0 
**Last Updated**: May 2026

---

## When to Use This Protocol

Use this protocol when encountering:
- 🔴 **Production outages** or critical bugs
- 🔴 **Security vulnerabilities** discovered
- 🟡 **Major blockers** preventing all progress
- 🟡 **Architecture pivots** requiring significant rework
- 🟡 **Data loss** or corruption issues
- 🟡 **Deadline-critical** situations

---

## Emergency Severity Levels

### 🔴 SEV1 - Critical Emergency
**Definition**: System down, data loss, security breach, or production outage 
**Response Time**: Immediate (within 15 minutes) 
**Escalation**: Required

### 🟡 SEV2 - High Urgency
**Definition**: Major functionality impaired, significant user impact 
**Response Time**: Within 1 hour 
**Escalation**: If not resolved in 2 hours

### 🟢 SEV3 - Medium Urgency
**Definition**: Moderate impact, workaround available 
**Response Time**: Within 4 hours 
**Escalation**: If not resolved in 1 day

---

## 🔴 SEV1 - Critical Emergency Response

### Immediate Actions (First 15 Minutes)

1. **🛑 STOP All Other Work**
   - Halt all feature development
   - Focus 100% on emergency

2. **📢 Declare Emergency**
   - Update `CURRENT-STATUS.md` with 🚨 EMERGENCY banner
   - Create `EMERGENCY-[DATE]-[ISSUE].md` document in SESSION-SUMMARIES/

3. **🔍 Quick Assessment**
   - What is broken?
   - Who is affected?
   - When did it start?
   - Is it getting worse?

4. **🛡️ Damage Control**
   - Can you rollback? (Do it immediately if safe)
   - Can you disable the feature?
   - Can you redirect traffic?
   - Can you isolate the problem?

### Emergency Document Template

Create `SESSION-SUMMARIES/EMERGENCY-[DATE]-[ISSUE].md`:

```markdown
# 🚨 EMERGENCY: [Issue Title]

## Status: 🔴 ACTIVE | 🟡 INVESTIGATING | 🟢 RESOLVED

**Started**: YYYY-MM-DD HH:MM UTC  
**Resolved**: [Pending]  
**Duration**: [X minutes/hours]

## Impact
- **Severity**: SEV1
- **Affected**: [All users | X% of users | Specific feature]
- **Business Impact**: [Revenue loss | User data | Reputation]

## Timeline
| Time | Event |
|------|-------|
| HH:MM | Issue first detected |
| HH:MM | Emergency declared |
| HH:MM | Root cause identified |
| HH:MM | Fix deployed |
| HH:MM | Verified resolved |

## Root Cause
[What caused this - fill in as discovered]

## Actions Taken
1. [Action 1 - timestamp]
2. [Action 2 - timestamp]

## Next Steps
1. [Immediate next action]
```

### Resolution Phase

1. **🔧 Implement Fix**
   - Make minimal changes to fix issue
   - Test thoroughly (but quickly)
   - Document every change

2. **✅ Verification**
   - Verify fix in staging (if available)
   - Test affected functionality
   - Check for side effects

3. **🚀 Deploy**
   - Deploy to production immediately
   - Monitor closely for 30 minutes
   - Keep rollback plan ready

4. **📊 Confirm Resolution**
   - Verify issue is resolved
   - Check metrics/logs
   - Monitor for 1-2 hours

### Post-Emergency (After Resolution)

1. **📝 Post-Mortem Required**
   - Create detailed incident report
   - Document lessons learned
   - Update prevention strategies

2. **🔄 Follow-Up Actions**
   - Add regression tests
   - Fix related issues
   - Update monitoring/alerts

---

## 🟡 SEV2 - High Urgency Response

### Immediate Actions (First Hour)

1. **⏸️ Pause Non-Critical Work**
   - Continue only P0 tasks
   - Defer all P2/P3 work

2. **📊 Document Blocker**
   - Update `DELEGATION-TRACKER.md` with ⚠️ BLOCKED status
   - Update `CURRENT-STATUS.md` with blocker details

3. **🎯 Assess Options**
   - Can you work around it?
   - Can you pivot to different approach?
   - What's needed to unblock?

---

## 🟢 SEV3 - Medium Urgency Response

### Standard Blocker Handling

1. **📝 Document**
   - Update task status to ⚠️ BLOCKED
   - Document blocker clearly
   - Note attempted solutions

2. **🔄 Pivot**
   - Switch to unblocked tasks
   - Make progress elsewhere
   - Return when unblocked

3. **⏰ Set Follow-Up**
   - Schedule check-in (4-24 hours)
   - Set reminder to revisit
   - Track in DELEGATION-TRACKER

---

## Major Pivot Protocol

### When Architecture/Approach Must Change

Create `SESSION-SUMMARIES/PIVOT-[DATE]-[REASON].md`:

```markdown
# Major Pivot: [Reason]

## Original Plan
[What was originally planned]

## Why Pivot Required
[Detailed reasoning]

## Current Progress
- [X] Completed: [List]
- [ ] Incomplete: [List]

## New Approach
[What will be done instead]

## Work to Salvage
[What can be reused]

## Estimated Impact
- Time: +/- X hours/days
- Scope: [Changes]
- Risk: [Assessment]
```

---

## Communication Templates

### Emergency Notification

```markdown
🚨 EMERGENCY: [Issue Title]

Severity: SEV1 | SEV2 | SEV3
Impact: [Brief description]
Status: [Current status]

Actions Taken:
1. [Action]

Next Steps:
1. [Next step]

ETA: [Expected resolution time]
```

### Blocker Escalation

```markdown
⚠️ BLOCKED: Need Help

Task: [What you're trying to do]
Blocker: [What's blocking you]

Tried:
- [Solution attempt 1]
- [Solution attempt 2]

Need:
[Specifically what you need to unblock]

Impact:
[What can't proceed without this]

Urgency: [Why this matters now]
```

---

## Recovery Checklist

After any emergency, complete this checklist:

### Immediate (Within 24 Hours)
- [ ] Issue fully resolved
- [ ] Emergency documentation complete
- [ ] CHANGELOG updated
- [ ] Stakeholders notified of resolution

### Short-Term (Within 1 Week)
- [ ] Post-mortem completed
- [ ] Root cause analysis documented
- [ ] Regression tests added
- [ ] Monitoring/alerting improved

---

**Remember**: 
- Stay calm
- Document everything
- Communicate clearly
- Fix first, analyze later
- Learn and improve

---

**Template Version**: 1.0 
**Framework Version**: 1.2.0 
**Last Updated**: May 2026
