---
name: team-planner
description: This skill should be used when the user asks to "can this plan be split", "can this be worked on by a team", "agent team for this plan", "parallelize this implementation", "teammates for this work", "create a team for this plan", or questions about whether a plan can be worked on independently by multiple teammates. Provides guidance for analyzing plans and creating appropriate agent team structures.
allowed-tools: "Grep,Read,AskUserQuestion"
---

# Team Planner Skill

## Purpose

Use agent teams when a plan can be broken into independent work units that multiple teammates can execute simultaneously. Teams are most effective when:

- Tasks are naturally separated by file, module, or architectural boundary
- Different perspectives are needed (security, performance, readability)
- Multiple competing proposals would benefit exploration
- Work scales beyond what single-threaded execution can efficiently handle

**Do not use teams for:**
- Single-file changes with tight coupling
- Sequential tasks where step N depends on step N-1
- Simple refactors that benefit from global context
- Tasks requiring consistency across a unified vision

## Analysis Framework

Evaluate plan parallelizability using these dimensions:

### 1. File/Module Separation
Can the work be divided by file, directory, or module?

**Look for:**
- Distinct components (e.g., frontend vs backend vs tests)
- Feature boundaries (vertical slices)
- Independent services or modules

**Ask:** "Do different teammates need to edit the same files?"

### 2. Task Independence
Can tasks be performed simultaneously without coordination?

**Look for:**
- Absence of sequential dependencies
- Clear interfaces between components
- Minimal merge conflict risk

**Ask:** "Does task A block task B, or can they run in parallel?"

### 3. Perspective Multiplicity
Would different lenses add value?

**Look for:**
- Code review: security, performance, test coverage, readability
- Research: different sources, approaches, technologies
- Architecture: scalability, maintainability, simplicity

**Ask:** "Would multiple viewpoints produce better outcomes?"

### 4. Proposal Competition
Would competing approaches be valuable?

**Look for:**
- Open-ended problems with multiple valid solutions
- Technology selection decisions
- Design explorations

**Ask:** "Would seeing different proposals help the user choose?"

## Clarifying Question Patterns

**IMPORTANT: Use the `AskUserQuestion` tool for all clarifying questions.** Present options as structured choices rather than open-ended questions.

Always propose specific options based on context:

### For File/Module Separation

*"I see this plan touches [list files/modules]. These could be split by:*
- *Option A: [grouping 1]*
- *Option B: [grouping 2]*
- *Option C: [grouping 3]*
*Which grouping makes the most sense?"*

### For Task Independence

*"Looking at the dependencies:*
- *Task [X] requires [Y] first*
- *Task [A] and [B] seem independent*
*Should I create a team for the independent tasks, or would the sequential nature make coordination costly?"*

### For Perspective Multiplicity

*"This work could benefit from different lenses:*
- *Security focus*
- *Performance focus*
- *Test coverage focus*
*Should I assign different teammates to each perspective, or would a single comprehensive review be more efficient?"*

### For Brainstorming/Proposals

*"This seems like an open problem where multiple approaches could work:*
- *Approach A: [description]*
- *Approach B: [description]*
- *Approach C: [description]*
*Would you like multiple teammates to explore different approaches in parallel?"*

## Team Structure Patterns

### Research Team

**Use when:** Multiple independent research threads are needed

**Structure:**
- Each teammate researches a specific source/technology
- No coordination needed during research phase
- Results synthesized at the end

**Example breakdown:**
- Teammate 1: Documentation research
- Teammate 2: GitHub issues/solutions research
- Teammate 3: Alternative approaches research

### Code Review Team

**Use when:** Multiple perspectives on the same code would add value

**Structure:**
- Each teammate focuses on a specific lens
- Same code base, different evaluation criteria
- Synthesis of findings for comprehensive review

**Example breakdown:**
- Teammate 1: Security vulnerabilities
- Teammate 2: Performance bottlenecks
- Teammate 3: Test coverage gaps
- Teammate 4: Code readability/maintainability

### Implementation Team

**Use when:** Work can be cleanly divided by component/file

**Structure:**
- Each teammate owns distinct files/modules
- Minimal merge conflict risk
- Clear ownership boundaries

**Example breakdown:**
- Teammate 1: Frontend components
- Teammate 2: Backend API
- Teammate 3: Database layer
- Teammate 4: Tests

### Brainstorm Team

**Use when:** Competing proposals would help decision-making

**Structure:**
- Each teammate explores a different approach
- Cross-pollination of ideas
- Comparison/selection at the end

**Example breakdown:**
- Teammate 1: Approach using framework A
- Teammate 2: Approach using framework B
- Teammate 3: Hybrid/alternative approach

## Tool-Specific Considerations

### GitHub CLI/MCP Tools

**Always use separate agents for GitHub tools** - even for simple tasks. GitHub CLI/MCP responses are verbose and cause rapid context window pollution.

**Granular delegation patterns:**

| Task | Agent Breakdown |
|------|-----------------|
| List issues → read issue details | Agent 1: list issues, Agent 2: read specific issue |
| PR review + comments | Agent 1: fetch PR diff, Agent 2: review files, Agent 3: post comments |
| Multi-repo operations | Separate agent per repository |
| Search + fetch | Agent 1: search, Agent 2: fetch detailed results |

**Rationale:** Each GitHub tool call can return hundreds of tokens. Isolating these calls to separate agents keeps the main context clean and allows context compression to work more effectively.

## Interactive Workflow

Follow this sequence when the skill is triggered:

### 1. Analyze the Plan

Read the current plan or task context. Identify:
- What work is being done
- Natural boundaries (files, modules, perspectives)
- Dependencies between components

### 2. Propose Team Structure

Present your analysis with specific proposals:

*"Based on this plan, I can see [X main components]. This could be split as:*
- *Option 1: [team structure with brief rationale]*
- *Option 2: [alternative structure with rationale]*
- *Option 3: [alternative structure with rationale]*
*Which approach would you prefer?"*

### 3. Confirm Before Creating

Always get explicit confirmation:

*"I recommend creating a team with [N] teammates:*
- *Teammate A: [role]*
- *Teammate B: [role]*
- *Teammate C: [role]*
*Shall I proceed with creating this team?"*

### 4. Create the Team

If confirmed:
1. Use TeamCreate to initialize the team
2. Create tasks for each teammate
3. Assign tasks to appropriate teammates
4. Coordinate execution and synthesis

If rejected:
- Ask what concerns they have
- Propose alternative structure
- Explain why single-agent approach may be better

### 5. Explain When Not Parallelizable

If the plan isn't suitable for teams, explain why:

*"This plan doesn't seem well-suited for a team because:*
- *All tasks depend on [common factor]*
- *Changes are concentrated in [single file]*
- *Consistency requires unified vision*
- *Coordination overhead would exceed benefits*
*A single agent would be more efficient here."*

## Resource References

**Example scenarios:** See `examples/` directory for detailed walkthroughs:
- `research-team.md` - Parallel research exploration
- `code-review-team.md` - Multi-perspective code review
- `implementation-team.md` - Component-based implementation
- `brainstorm-team.md` - Competing proposals

**Related patterns:**
- Task tool delegation for specialized work
- TeamCreate/TeamDelete for team lifecycle
- SendMessage for team coordination

## Decision Tree Summary

```
Can work be split by file/module? → YES → Consider Implementation Team
                                    ↓ NO
Can different perspectives add value? → YES → Consider Code Review Team
                                    ↓ NO
Would competing proposals help? → YES → Consider Brainstorm Team
                              ↓ NO
Research multiple threads? → YES → Consider Research Team
                      ↓ NO
Use single agent (not suitable for team)
```

## Key Principles

1. **Propose, don't assume** - Always present options based on context
2. **Get confirmation** - Never create a team without explicit approval
3. **Explain tradeoffs** - Help user understand why teams help (or don't)
4. **Keep it simple** - Don't over-engineer team structures for small tasks
5. **Synthesis matters** - Plan for how results will be combined
