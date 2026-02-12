# Agent Team Planner Plugin

A user-invokable skill for planning and creating agent teams to work on implementation plans in parallel.

## Installation

### Local Installation

Copy the plugin to your project:

```bash
# From your project root
cp -r .claude-plugin ~/.claude/plugins/agent-team-planner
```

### Per-Project Installation

The plugin is already in `.claude-plugin/` directory. Run Claude Code from this directory:

```bash
cc --plugin-dir .claude-plugin
```

## Usage

### Trigger Phrases

Use any of these phrases to invoke the skill:

- "Can this plan be split into independent work?"
- "Can this be worked on by a team?"
- "Create an agent team for this plan"
- "Parallelize this implementation"
- "Should I use teammates for this work?"
- "Is this suitable for a team?"

### How It Works

1. **Analysis**: The skill analyzes your plan to identify parallelization opportunities
2. **Proposals**: Context-aware proposals for team structure based on:
   - File/module boundaries
   - Task dependencies
   - Different perspectives needed
   - Proposal competition scenarios
3. **Confirmation**: Always asks for approval before creating teams
4. **Execution**: If confirmed, creates the team and coordinates work

### When Teams Help

| Scenario | Team Type | Benefit |
|----------|-----------|---------|
| Multiple files/modules | Implementation Team | Parallel component development |
| Code review needed | Code Review Team | Security, performance, test coverage lenses |
| Research needed | Research Team | Parallel exploration of sources |
| Open-ended problem | Brainstorm Team | Competing proposals for comparison |

### When Teams Don't Help

- Single-file changes with tight coupling
- Sequential tasks with hard dependencies
- Simple refactors requiring global consistency
- Small tasks where coordination overhead exceeds benefits

## Examples

The plugin includes four detailed examples:

1. **Research Team** (`examples/research-team.md`) - Parallel research exploration
2. **Code Review Team** (`examples/code-review-team.md`) - Multi-perspective review
3. **Implementation Team** (`examples/implementation-team.md`) - Component-based development
4. **Brainstorm Team** (`examples/brainstorm-team.md`) - Competing proposals

See the `examples/` directory for complete walkthroughs.

## Plugin Structure

```
.claude-plugin/
├── plugin.json                    # Plugin manifest
├── README.md                      # This file
└── skills/
    └── team-planner/
        ├── SKILL.md               # Main skill (1,500-2,000 words)
        └── examples/              # Example scenarios
            ├── research-team.md
            ├── code-review-team.md
            ├── implementation-team.md
            └── brainstorm-team.md
```

## Verification

Test that the skill loads correctly:

```bash
# Run Claude Code with the plugin
cc --plugin-dir .claude-plugin

# Within Claude Code, verify the skill loads
# The skill should appear in available skills list
```

## Development

### Skill Design

- **Frontmatter**: Third-person description with specific triggers
- **Body**: Imperative form, concise (1,500-2,000 words)
- **Examples**: Progressive disclosure - core skill references examples

### Quality Checklist

- [x] Plugin manifest (plugin.json) with name, version, description
- [x] SKILL.md with proper frontmatter and triggers
- [x] Four example scenarios covering main use cases
- [x] README with installation and usage instructions
- [x] Skill follows plugin-dev best practices

## Version

0.1.0 - Initial release
