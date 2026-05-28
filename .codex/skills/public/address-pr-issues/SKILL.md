---
name: address-pr-issues
description: Read comments on a ticket-scoped pull request, identify actionable review feedback, implement the requested fixes on the existing branch, and report back what was addressed. Use it for prompts like "address FMH-5 PR comments" or "address PR comments for FMH-5".
---

# Address PR Issues

## Overview

Use this skill when the user asks to address PR comments for a specific ticket, for example: "address FMH-5 PR comments".
The skill should inspect the pull request comments and review feedback for that ticket, identify what needs code changes, and apply the fixes on the current branch.

## Workflow

1. Read the pull request comments and review feedback.
2. Separate actionable issues from non-actionable remarks.
3. For actionable feedback, make the code changes on the existing branch.
4. Run the relevant checks for the changed code.
5. Push the updated branch and report back which review items were addressed.

## What Counts As Actionable

- correctness bugs
- missing validation
- failing tests
- reviewer-requested refactors tied to behavior
- documentation updates that are directly requested in the PR

## What To Ignore Unless Requested

- style-only nits that do not affect correctness
- subjective preferences
- comments that are already resolved in the current branch
- feedback unrelated to the changed code

## Reporting

When reporting back, be direct:
- say which PR comments were addressed
- say which comments were left unresolved and why
- mention if code changes were pushed
