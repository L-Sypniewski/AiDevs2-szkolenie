# Research Team Example

## Scenario Context

User asks: "I need to understand how to implement real-time updates in a Blazor application. Can this be researched by a team?"

The task involves researching real-time update patterns specifically for Blazor applications across multiple dimensions:
- Built-in Blazor mechanisms (SignalR integration)
- Third-party libraries and alternatives
- Performance best practices
- Common pitfalls and solutions

## Clarifying Questions Asked

**Analysis:** "This research task has clear independent threads:
- Official Blazor documentation and Microsoft Learn
- Community solutions (GitHub, Stack Overflow, blogs)
- Alternative approaches (WebSockets directly, SSE, polling)
- Performance/scaling considerations

**Proposals:**
- Option A: 3 teammates - one for official docs, one for community solutions, one for alternatives
- Option B: 2 teammates - one for Blazor-specific research, one for general real-time patterns
- Option C: Single researcher covering all topics sequentially

Which approach would you prefer?"

## Proposed Team Structure

**Confirmed:** Option A with 3 teammates

### Team Configuration

**Team name:** `blazor-realtime-research`

**Team members:**
- **docs-researcher:** Focus on official Microsoft Learn, Blazor documentation, SignalR docs
- **community-researcher:** Focus on GitHub repos, Stack Overflow, blog posts, community patterns
- **alternatives-researcher:** Focus on WebSockets, SSE, polling, and comparison approaches

## Resulting Task Breakdown

### Task 1: Official Documentation Research
**Owner:** docs-researcher

1. Search Microsoft Learn for Blazor real-time documentation
2. Review official SignalR integration patterns
3. Document built-in Blazor mechanisms for real-time updates
4. Identify supported scenarios and limitations
5. Extract code examples from official sources

**Deliverable:** Summary of official approaches with code examples and links to documentation

### Task 2: Community Solutions Research
**Owner:** community-researcher

1. Search GitHub for popular Blazor real-time libraries
2. Review Stack Overflow for common issues and solutions
3. Identify blog posts and community articles
4. Document real-world usage patterns
5. Compile list of popular open-source solutions

**Deliverable:** Curated list of community solutions with usage patterns and links

### Task 3: Alternative Approaches Research
**Owner:** alternatives-researcher

1. Research direct WebSocket implementation in .NET
2. Investigate Server-Sent Events (SSE) patterns
3. Compare polling vs push-based approaches
4. Analyze tradeoffs between approaches
5. Document when alternatives might be preferred

**Deliverable:** Comparison matrix of real-time approaches with pros/cons

### Task 4: Synthesis (Team Lead)
**Owner:** Team lead

1. Compile findings from all researchers
2. Create unified summary
3. Provide recommendations based on use cases
4. Identify gaps or conflicts in information
5. Create decision guide for choosing approach

**Deliverable:** Comprehensive research summary with actionable recommendations

## Execution Flow

1. All three researchers work in parallel (no dependencies)
2. Team lead synthesizes after research completes
3. Final deliverable combines all threads with recommendations

## Result

User receives comprehensive coverage from three independent angles, allowing informed decision-making about which real-time approach best fits their specific requirements.
