---
name: jira-ready-check
description: Check whether a Jira issue is ready to be moved to READY, using JiraTool to inspect the ticket and transition it only when the issue has enough detail, acceptance criteria, and no obvious blockers. When the issue is ready, create a feature branch, make the code changes, and open a PR. When creating branch commits, use commit author/committer metadata to mark changes as Codex-authored and do not change git config.
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
3. If it is not ready, stop, move it back to `Backlog`, append a short note about what is missing, and report the gap briefly.
4. If it is ready, run `JiraTool ready <issueKey>`.
5. Create a new branch in the repository using the pattern `feature/<issue-key>-<shortdescription>`.
6. Make the code changes described in the ticket on that branch.
7. Create commits with Codex author/committer metadata instead of changing local git config.
8. Push the branch and open a PR to the main repository when GitHub access is available.

## Readiness Criteria

Treat a Jira issue as ready when it has enough detail for implementation without needing extra clarification.
Prefer moving to `READY` only when the issue states what must be built, how success will be judged, and whether there are dependencies or constraints.

## Output

When reporting back, be direct:
- say whether the issue is ready
- if ready, say it was moved to `READY`
- if not ready, list the missing pieces briefly
- if code was changed, note that commit metadata was used to distinguish Codex-authored commits
- if a branch or PR was created, name it explicitly
