---
name: create-pr-to-task
description: Create a pull request from current branch to master with AI-generated title and description based on diff. Use when user wants to create a PR to the task branch.
disable-model-invocation: true
allowed-tools: "Bash(git:*),Bash(gh:*),mcp__gh-pull__*,Read,Grep,Task"
---

# Create PR to Task Branch

Create a pull request from the current branch to the `master` branch with AI-generated title and description based on the diff.

## What it does

1. Gets the current branch name
2. Retrieves the diff between current branch and `master`
3. Checks for changes in `tasks.md` to understand completed work
4. Analyzes the changes to generate a concise, human-readable title and description
5. Creates a PR targeting the `master` branch
6. Returns the PR URL

## Usage

```
/create-pr-to-task
```

The skill will:
- Automatically detect your current branch
- Compare against `master` (the base branch)
- Analyze tasks.md changes to understand which tasks were completed
- Generate a focused title based on code and task changes (e.g., "Add user authentication to login flow")
- Create a brief, targeted description highlighting the key changes and completed tasks
- Open the new PR

## Implementation Details

The skill uses GitHub interaction tools:

**For branch operations:**
```bash
# Check current branch
git rev-parse --abbrev-ref HEAD

# Get diff
git diff master...HEAD

# Push if needed
git push -u origin <branch-name>
```

**For PR creation:**
The skill uses the **GitHub MCP tools** (`mcp__gh-pull__create_pull_request`) to create the PR:
```
mcp__gh-pull__create_pull_request:
  owner: "TwentyFiveDev-L-Sypniewski"
  repo: "samochodowy-pl-scraper"
  title: "<AI-generated title>"
  head: "<current-branch>"
  base: "master"
  body: "<AI-generated description>"
```

Alternatively, it can use **GitHub CLI** (`gh`) for immediate feedback:
```bash
gh pr create --base master --title "..." --body "..."
```

## Notes

- The PR always targets `master` as the base branch
- Title and description are generated from both code diff and tasks.md changes
- Tasks marked as completed in tasks.md are reflected in the PR description
- If the branch is already pushed, it will create the PR immediately
- If the branch is not pushed, it will push it first with `-u` flag
- The AI analysis focuses on what changed (features, fixes, tests) not implementation details
- GitHub interactions use either MCP tools or GitHub CLI (`gh`) - never raw HTTP requests
