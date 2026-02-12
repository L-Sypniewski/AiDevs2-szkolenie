# GitHub API Reference for PR Comments

Detailed API documentation for working with pull request review comments.

## GitHub API Endpoints

### List Review Comments
```
GET /repos/{owner}/{repo}/pulls/{pull_number}/comments
```

**Using GitHub MCP (Recommended):**
```
mcp__gh-pull__pull_request_read:
  method: "get_review_comments"
  owner: "owner-name"
  repo: "repo-name"
  pullNumber: 123
```

**Using GitHub CLI:**
```bash
gh api repos/{owner}/{repo}/pulls/{pr-number}/comments
```

**Response structure:**
```json
{
  "reviewThreads": [
    {
      "ID": "PRRT_kwDORAhPnc6BsKQP",
      "IsResolved": false,
      "IsOutdated": false,
      "IsCollapsed": false,
      "Comments": {
        "Nodes": [
          {
            "ID": "PRRC_kwDORAhPnc6ie4Q5",
            "DatabaseId": 2726003769,
            "Body": "Let's always use DateTimeOffset instead of DateTime",
            "Path": "src/Features/Scraping/LogEntry.cs",
            "Line": 15,
            "Author": {
              "Login": "reviewer-username"
            },
            "URL": "https://github.com/owner/repo/pull/1#discussion_r2726003769"
          }
        ]
      }
    }
  ]
}
```

### Reply to Review Comment
```
POST /repos/{owner}/{repo}/pulls/{pull_number}/comments/{comment_id}/replies
```

**Using curl:**
```bash
# Get auth token
GITHUB_TOKEN=$(gh auth token)

# Reply to comment
curl -s -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/{owner}/{repo}/pulls/{pr-number}/comments/{comment_id}/replies \
  -d '{"body":"Your reply here"}'
```

**Request body:**
```json
{
  "body": "Done - replaced with DateTimeOffset (commit abc123)"
}
```

**Important:**
- Use `DatabaseId` (numeric), not `ID` (Node ID string) in the URL
- Body must be valid JSON (escape quotes: `\"`, newlines: `\n`)
- Returns 201 on success

## Comment ID Types

GitHub uses two ID formats:

1. **Node ID** (string): `"PRRC_kwDORAhPnc6ie4Q5"`
   - GraphQL identifier
   - Used in some GraphQL API calls
   - Present in MCP response as `ID`

2. **Database ID** (number): `2726003769`
   - REST API identifier
   - Used in `/comments/{comment_id}/replies` endpoint
   - Present in MCP response as `DatabaseId`
   - Also visible in comment URLs: `#discussion_r2726003769`

**For reply API, always use DatabaseId.**

## Error Handling

### Rate Limiting
```json
{
  "message": "API rate limit exceeded",
  "documentation_url": "https://docs.github.com/rest/overview/resources-in-the-rest-api#rate-limiting"
}
```

**Solution:** Wait and retry. GitHub CLI handles this automatically.

### Invalid Comment ID
```json
{
  "message": "Not Found",
  "documentation_url": "https://docs.github.com/rest/pulls/comments#create-a-reply-for-a-review-comment"
}
```

**Cause:** Using Node ID instead of Database ID, or comment doesn't exist.

### Authentication Failed
```json
{
  "message": "Bad credentials",
  "documentation_url": "https://docs.github.com/rest"
}
```

**Solution:** Verify `gh auth token` returns valid token. Re-authenticate with `gh auth login`.

## Escaping JSON Bodies

**Problem:** Multi-line replies or quotes break JSON.

**Solution:** Use jq to properly escape:
```bash
REPLY_TEXT="Done - fixed the following:
1. Replaced DateTime with DateTimeOffset
2. Updated all references"

curl -s -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/{owner}/{repo}/pulls/{pr}/comments/{id}/replies \
  -d "$(jq -n --arg body "$REPLY_TEXT" '{body: $body}')"
```

**Or use heredoc:**
```bash
curl -s -X POST \
  -H "Authorization: Bearer $GITHUB_TOKEN" \
  -H "Accept: application/vnd.github+json" \
  https://api.github.com/repos/{owner}/{repo}/pulls/{pr}/comments/{id}/replies \
  -d @- <<EOF
{
  "body": "Done - replaced with DateTimeOffset (commit abc123)"
}
EOF
```

## Testing

**Test comment reply locally:**
```bash
# List comments first
gh api repos/owner/repo/pulls/123/comments | jq '.[] | {id: .id, body: .body}'

# Reply to a comment (get id from above)
gh api -X POST \
  repos/owner/repo/pulls/123/comments/2726003769/replies \
  -f body="Test reply from CLI"
```

## Additional Resources

- [GitHub REST API - Review Comments](https://docs.github.com/en/rest/pulls/comments)
- [GitHub GraphQL API - Review Threads](https://docs.github.com/en/graphql/reference/objects#pullrequestreviewthread)
- [GitHub CLI Manual](https://cli.github.com/manual/)
