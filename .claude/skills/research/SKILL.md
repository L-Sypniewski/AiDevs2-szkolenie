---
name: research
description: Use this skill PROACTIVELY before development tasks that require external knowledge, latest library versions, API documentation, or best practices. Also use when user asks to "research", "find information about", "look up", "investigate", "what's the latest on", "before implementing", or "check the docs". Ensures fact-anchored development with proper citations.
allowed-tools: "mcp__sequential-thinking__*,mcp__context7__*,mcp__microsoft-docs__*,mcp__web-reader__*,mcp__web-search-prime__*,mcp__zread__*,mcp__gh-repos__*,mcp__gh-issues__*,mcp__gh-pull__*,TodoWrite,Read,Glob,Grep,Write"
context: fork
---

# Research Skill

Comprehensive, fact-anchored research skill that synthesizes information from multiple authoritative sources before development tasks.

## When to Use This Skill

**Proactive Usage (Invoke automatically before development):**
- When implementing a new library or package integration
- Before using unfamiliar APIs or interfaces
- When checking for latest version compatibility
- When implementing security-sensitive features
- Before architectural decisions that impact multiple components

**Explicit Usage (User requests research):**
- Research a technology, library, or framework before implementation
- Find the latest documentation or best practices
- Investigate known issues, bugs, or solutions
- Understand complex topics through multiple sources
- Gather information for architectural decisions

## Research Workflow

### Step 1: Query Analysis

Before searching, analyze the research query to identify:

1. **Domain Classification**
   - Microsoft/.NET ecosystem → prioritize `microsoft-docs`
   - Third-party library → prioritize `context7`
   - GitHub project → prioritize `zread`, `gh-repos`
   - General web content → prioritize `web-search-prime`

2. **Freshness Requirements**
   - Bleeding edge (last week) → web search + recent GitHub issues
   - Recent (last month) → official docs + release notes
   - Stable → library docs + established patterns

3. **Source Types Needed**
   - API reference → documentation tools
   - Code examples → GitHub repos, code search
   - Tutorials/guides → web search + blogs
   - Known issues → GitHub issues, PRs

### Step 2: Source Selection

Map your topic to the appropriate research tools:

| Topic Type | Primary Tools | Secondary Tools |
|------------|---------------|-----------------|
| .NET/C#/Azure | `microsoft-docs` | `context7`, `gh-repos` |
| JavaScript/Node | `context7` | `web-search-prime`, `npm docs` |
| Python | `context7` | `web-search-prime`, `pypi` |
| Specific Library | `context7`, `zread` | `gh-repos`, `gh-issues` |
| Architecture/Patterns | `web-search-prime` | `gh-repos`, `context7` |
| Bug Investigation | `gh-issues`, `gh-pull` | `gh-repos`, `web-search-prime` |

### Step 3: Research Execution

Execute searches in parallel when possible:

```
┌─────────────────────────────────────────────────────────────┐
│                    PARALLEL SEARCH                          │
├─────────────────┬─────────────────┬─────────────────────────┤
│ Documentation   │ GitHub          │ Web                     │
│ (context7,      │ (zread,         │ (web-search-prime,      │
│  microsoft-docs)│  gh-repos)      │  web-reader)            │
└─────────────────┴─────────────────┴─────────────────────────┘
                           │
                           ▼
              ┌────────────────────────┐
              │   Synthesize with      │
              │   sequential-thinking  │
              └────────────────────────┘
```

**Search Strategy:**

1. Start with 2-3 parallel searches targeting different source types
2. Track all source URLs for citations
3. Use `web-reader` to extract full content from promising results
4. For complex topics, use `sequential-thinking` to analyze findings

### Step 4: Synthesis

Use `sequential-thinking` when:
- Findings from different sources conflict
- The topic requires multi-step reasoning
- You need to compare approaches or alternatives
- Complex relationships need to be mapped

Synthesis should:
- Identify consensus across sources
- Note contradictions and their contexts
- Prioritize official/authoritative sources
- Flag outdated information

### Step 5: Output

Save research findings to a timestamped markdown file:

**Location:** `/home/gombka/Documents/Developer/AiDevs2-szkolenie/research-notes/`

**Filename format:** `{YYYY-MM-DD}-{topic-slug}.md`

**Template:**

```markdown
# Research: {Topic}

**Date**: {YYYY-MM-DD HH:MM}
**Query**: {original user query}

## Summary

{2-3 paragraphs synthesizing the key findings. Lead with the most important
conclusion, then supporting context, then limitations or caveats.}

## Key Findings

- **{Finding Category}**: {Specific finding with actionable insight}
  - Source: [{Title}]({url})
  - Context: {Why this matters for the task}

- **{Finding Category}**: {Another finding}
  - Source: [{Title}]({url})
  - Context: {Relevance explanation}

## Code Examples

{Optional: Include relevant code snippets found during research}

```{language}
// Code example with attribution
```

## Open Questions

{Topics that need further investigation or clarification}

## Sources

### Primary Sources
1. [{Title}]({url}) - {Brief description of content and authority level}

### Secondary Sources
1. [{Title}]({url}) - {Brief description}

## Related Topics

- {Suggested follow-up research area 1}
- {Suggested follow-up research area 2}

---
*Generated by Claude Code Research Skill*
```

## Tool Reference

### Documentation Tools

**microsoft-docs** (Microsoft/Azure/.NET)
- `microsoft_docs_search`: Quick search returning 10 content chunks
- `microsoft_docs_fetch`: Get full page content
- `microsoft_code_sample_search`: Find code examples

**context7** (Any library/framework)
- `mcp__context7__resolve-library-id`: Find the library ID first
- `mcp__context7__query-docs`: Query specific documentation

### GitHub Tools

**zread** (Repo exploration)
- `mcp__zread__get_repo_structure`: Directory listing
- `mcp__zread__read_file`: Read specific file
- `mcp__zread__search_doc`: Search docs/issues/commits

**gh-repos** (Direct API access)
- `mcp__gh-repos__search_code`: Search code across repos
- `mcp__gh-repos__get_file_contents`: Get file/directory contents

**gh-issues** (Issue tracking)
- `mcp__gh-issues__search_issues`: Search issues
- `mcp__gh-issues__issue_read`: Get issue details/comments

**gh-pull** (Pull requests)
- `mcp__gh-pull__search_pull_requests`: Search PRs
- `mcp__gh-pull__pull_request_read`: Get PR details

### Web Tools

**web-search-prime**
- `mcp__web-search-prime__webSearchPrime`: General web search

**web-reader**
- `mcp__web-reader__webReader`: Fetch and convert URL content

### Analysis Tools

**sequential-thinking**
- `mcp__sequential-thinking__sequentialthinking`: Multi-step reasoning

## Best Practices

1. **Always cite sources** - Every claim should have a source URL
2. **Prioritize official docs** - Official documentation over blog posts
3. **Check dates** - Note when information was published/updated
4. **Verify across sources** - Cross-reference important claims
5. **Save context** - Include enough context to understand findings later
6. **Note gaps** - Document what you couldn't find or verify

## Example Usage

```
/research Microsoft.Extensions.AI IChatClient interface

# Will execute:
1. microsoft_docs_search for "IChatClient interface"
2. context7 query for Microsoft.Extensions.AI
3. gh-repos search for IChatClient implementations
4. Synthesize findings
5. Save to research-notes/2024-01-15-microsoft-extensions-ai-ichatclient.md
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| No results from context7 | Try alternative library names, check if library exists |
| Microsoft docs incomplete | Use `microsoft_docs_fetch` on promising URLs |
| Conflicting information | Use `sequential-thinking` to analyze contradictions |
| Outdated information | Check GitHub issues/PRs for recent discussions |
| Too many results | Narrow with specific version numbers or exact terms |
