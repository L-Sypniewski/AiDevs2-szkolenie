---
name: address-pr-comments
description: Fetch and address pull request review comments by delegating to parallel senior engineer subagents, then inline-reply to each comment once resolved
argument-hint: "[pr-number] or [owner/repo pr-number]"
disable-model-invocation: true
allowed-tools: mcp__gh-pull__*, Bash(gh:*), Task
---

# Address PR Comments

Address all unresolved review comments on pull request $ARGUMENTS.

## Workflow

### 1. Parse arguments

Extract owner, repo, and PR number from $ARGUMENTS:

- Format: `123` (uses current repo from git)
- Format: `owner/repo 123`

If no arguments provided, ask user for PR number.

### 2. Fetch PR review comments

Use GitHub MCP to get review threads:

```
mcp__gh-pull__pull_request_read:
  method: "get_review_comments"
  owner: <owner>
  repo: <repo>
  pullNumber: <pr-number>
```

This returns `reviewThreads[]` with:

- `ID` - Thread identifier
- `IsResolved` - Skip if true
- `IsOutdated` - Note if true
- `Comments.Nodes[]` - Array of comments in thread
  - `ID` - Comment ID (for replies)
  - `Body` - Comment text
  - `Path` - File path
  - `Line` - Line number
  - `Author.Login` - Who wrote it

### 3. Filter and group comments

- Skip resolved threads (IsResolved: true)
- Skip comments already addressed by user
- Group by file/path for related issues
- Create list of unique actionable comments

### 4. Delegate in parallel

For each actionable comment, launch a senior-engineer subagent using the Task tool:

```
Task:
  subagent_type: "development-workflow:senior-engineer"
  description: "Address PR comment on {Path}:{Line}"
  prompt: |
    Address this PR review comment:

    File: {Path}:{Line}
    Comment: {Body}
    Author: {Author}

    Make necessary code changes or provide explanation why the suggestion isn't applicable.
    When done, report what you did.
```

Launch ALL subagents in parallel (single message with multiple Task tool calls).

### 5. Wait for completion

Monitor subagent outputs. Each should report what they did.

### 6. Reply inline to each comment

For each addressed comment, post inline reply using curl + GitHub API:

```bash
GITHUB_TOKEN=$(gh auth token)

curl -s -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/{owner}/{repo}/pulls/{pr-number}/comments/{comment_id}/replies \
  -d '{"body":"Done - <brief description of change> (commit <sha>)"}'
```

**Important notes:**

- Extract numeric `comment_id` from comment data (not the Node ID)
- Escape quotes and newlines in JSON body
- Keep replies concise (1-2 sentences)
- Include commit SHA if code changed
- If no code change needed, explain why

### 7. Report summary

After all replies sent:

```
✅ Addressed PR #123 review comments:
- Fixed: 8 comments (code changes)
- Explained: 3 comments (no changes needed)
- Clarified: 1 comment (needs follow-up)

All inline replies posted.
```

## Reply templates

**For code changes:**

```
Done - replaced DateTime with DateTimeOffset throughout (commit a1b2c3d)
```

**For explanations:**

```
Id is the primary key, OfferId is the foreign key to parent.
Both are GUIDs but serve different purposes per EF Core conventions.
```

**For clarifications needed:**

```
Good point. Which approach do you prefer:
1. Extract to shared TestDatabaseFixture
2. Use test setup attribute
```

**For future work:**

```
Will address in follow-up PR - this needs broader refactoring of the scraper pipeline
```

## Notes

- Use git status/log to find latest commit SHA for replies
- Skip outdated comments but mention them in summary
- If a comment is unclear, the subagent should ask before proceeding
- Respect GitHub API rate limits (built-in throttling)

## Additional Resources

See [reference.md](reference.md) for detailed GitHub API documentation and troubleshooting tips.
