---
name: jira-ready-check
description: Check whether a Jira issue is ready to be moved to READY, using JiraTool to inspect the ticket and transition it only when the issue has enough detail, acceptance criteria, and no obvious blockers.
---

# Jira Ready Check

## Overview

Use this skill when the user asks to check whether a Jira issue is ready, for example: "check if FMH-5 is ready".
The skill should inspect the issue with `JiraTool`, decide whether it is actionable, and either move it to `READY` or explain why it is not ready.

## Workflow

1. Read the issue with `JiraTool show <issueKey>`.
2. Check whether the ticket has:
   - a clear summary and description
   - concrete acceptance criteria
   - implementation notes or a bounded scope
   - no unresolved blockers or missing dependencies
3. If the issue is ready, run `JiraTool ready <issueKey>`.
4. If it is not ready, do not transition it. Report the missing details briefly.

## Readiness Criteria

Treat a Jira issue as ready when it has enough detail for implementation without needing extra clarification.
Prefer moving to `READY` only when the issue states what must be built, how success will be judged, and whether there are dependencies or constraints.

## Output

When reporting back, be direct:
- say whether the issue is ready
- if ready, say it was moved to `READY`
- if not ready, list the missing pieces briefly
