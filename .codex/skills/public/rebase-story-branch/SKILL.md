---
name: rebase-story-branch
description: Rebase the feature branch for a named story onto the latest base branch, resolve straightforward merge conflicts, and pause for user input when conflicts are too complex to resolve safely.
---

# Rebase Story Branch

## When to use

Use this skill when the user asks to rebase the branch for a specific story or ticket, including prompts like:
- `rebase the FMH-5 branch`
- `rebase story FMH-12 onto main`
- `rebase the branch for ticket ABC-123`

## Workflow

1. Confirm the current branch is the branch for the story the user named.
2. Fetch the latest remote state for the target base branch and the feature branch.
3. Rebase the story branch onto the latest base branch.
4. Resolve simple textual merge conflicts directly.
5. If conflicts are broad, ambiguous, or require product decisions, stop and ask the user for guidance before continuing.
6. Run the smallest relevant validation needed to confirm the rebase result.
7. If the repository uses a remote branch, update it with a force push only when needed and only after the rebase is clean.

## Conflict handling

Treat a conflict as too complex to resolve when any of these are true:
- the same change appears in multiple files with unclear intent
- the resolution would change behavior rather than reconcile syntax
- tests or source context do not make the correct resolution obvious

When that happens, report:
- the files in conflict
- the decision needed from the user
- the smallest next action required to continue
