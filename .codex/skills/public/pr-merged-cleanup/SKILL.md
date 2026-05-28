---
name: pr-merged-cleanup
description: Check whether a ticket-scoped pull request is merged, delete the branch if it still exists, switch to main, pull the latest main branch, and move the ticket to Done. Use it for prompts like "PR for FMH-5 is now merged".
---

# PR Merged Cleanup

## Overview

Use this skill when the user says a ticketed PR is merged, for example: "PR for FMH-5 is now merged".
The skill should confirm the PR is merged, clean up the branch locally and remotely if it still exists, then switch to `main` and pull the latest changes.

## Workflow

1. Identify the ticket or branch associated with the merged PR.
2. Confirm the PR is merged.
3. If the branch still exists locally or remotely, delete it.
4. Switch to `main`.
5. Pull the latest changes for `main`.
6. Update the ticket status to `Done`.
7. Report back what was deleted and whether `main` was updated.

## Checks

- If the PR is not merged, do not delete the branch or switch branches.
- If the branch is already gone, report that it did not need deletion.
- If `main` cannot be updated, report the reason and stop.
- If the ticket cannot be moved to `Done`, report the reason and stop.

## Reporting

When reporting back, be direct:
- say whether the PR was merged
- say whether the branch was deleted locally and remotely
- say whether `main` was updated
- say whether the ticket was moved to `Done`
