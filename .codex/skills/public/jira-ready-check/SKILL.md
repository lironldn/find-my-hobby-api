---
name: jira-ready-check
description: Check whether a Jira issue is ready to be moved to READY, using JiraTool to inspect the ticket and transition it only when the issue has enough detail, acceptance criteria, and no obvious blockers. Use "check whether <issue> is ready" for the readiness transition only, and "pick up <issue>" for the full branch, implementation, and PR workflow. When creating branch commits, use commit author/committer metadata to mark changes as Codex-authored and do not change git config.
---

# Jira Ready Check

## Overview

Use this skill when the user asks to check whether a Jira issue is ready or to pick up a Jira issue for implementation.
Examples:
- "check whether FMH-5 is ready"
- "pick up FMH-5"

The skill should inspect the issue with `JiraTool`, decide whether it is actionable, and either move it to `READY`, move it back to `Backlog`, or proceed with the branch and implementation workflow.

## Workflow

### Check readiness only

Use this path when the user asks whether an issue is ready.

1. Read the issue with `JiraTool show <issueKey>`.
2. Check whether the ticket has:
   - a clear summary and description
   - concrete acceptance criteria
   - implementation notes or a bounded scope
   - no unresolved blockers or missing dependencies
3. If it is not ready, stop, move it back to `Backlog`, append a short note about what is missing, and report the gap briefly.
4. If it is ready, run `JiraTool ready <issueKey>`.

### Pick up the ticket

Use this path when the user asks to pick up an issue.

1. Read the issue with `JiraTool show <issueKey>`.
2. Apply the same readiness check as above.
3. If it is not ready, move it back to `Backlog`, append a short note explaining why it cannot be picked up, and stop.
4. If it is ready, run `JiraTool ready <issueKey>`.
5. Update the ticket to `In Progress` before starting work.
6. Create a new branch in the repository using the pattern `feature/<issue-key>-<shortdescription>`.
7. Make the code changes described in the ticket on that branch.
8. Create commits with Codex author/committer metadata instead of changing local git config.
9. Push the branch and open a PR to the main repository when GitHub access is available.
10. After opening the PR, update the ticket status to `Review`.

## Readiness Criteria

Treat a Jira issue as ready when it has enough detail for implementation without needing extra clarification.
Prefer moving to `READY` only when the issue states what must be built, how success will be judged, and whether there are dependencies or constraints.

## Output

When reporting back, be direct:
- say whether the issue is ready
- if ready, say it was moved to `READY`
- if not ready, list the missing pieces briefly
- if the user asked to pick up the ticket, say whether the branch/PR workflow started
- if code was changed, note that commit metadata was used to distinguish Codex-authored commits
- if a branch or PR was created, name it explicitly
