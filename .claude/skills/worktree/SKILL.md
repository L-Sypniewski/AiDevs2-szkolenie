---
name: worktree
description: This skill should be used when the user needs to work on multiple branches simultaneously without context switching, wants to create isolated development environments for different features, or needs to manage Git worktrees in sibling folders. Triggered by commands like `/worktree create feature-branch`, `/worktree remove old-branch`, or requests to "create a worktree for", "add a worktree", "set up parallel branch work".
version: 1.0.0
disable-model-invocation: true
user-invocable: true
---

# Git Worktree Management

Manages Git worktrees by creating and removing them in sibling folders to the main repository.

## Purpose

Create and remove Git worktrees efficiently. Worktrees allow working on multiple branches simultaneously without switching branches in a single checkout. Each worktree gets its own folder at the same level as the main repository.

## Dynamic Context Injection

This skill uses dynamic context injection to automatically detect the Git repository root. The following variables are available when the skill loads:

```yaml
git:
  root: "/absolute/path/to/git/repo/root"
  branch: "current-branch-name"
  remote: "origin" (if configured)
```

Use `{{git.root}}` to reference the repository root path in all commands.

## Usage

### Invocation

Invoke manually via `/worktree [action] [name]`

- **action**: `create` or `remove` (infer from context if omitted)
- **name**: Branch/folder name (required)

### Examples

- `/worktree create fix-login-bug` - Create worktree for new branch
- `/worktree fix-login-bug` - Create worktree (action inferred)
- `/worktree remove fix-login-bug` - Remove worktree and clean up
- `/worktree remove` - Remove worktree (lists all and asks which one)
- `/worktree` - No arguments: use AskUserQuestion to prompt for details

## Creating Worktrees

### Process

1. **Detect repository root**: Use `{{git.root}}` from dynamic context
2. **Determine parent directory**: Navigate one level up from repository root
3. **Confirm action**: Always use AskUserQuestion before executing
4. **Create worktree**: Run `git worktree add` command
5. **Report result**: Show the created worktree path

### Command Structure

```bash
# From repository root, create worktree in sibling folder
cd {{git.root}}
git worktree add ../<name> -b <name>
```

### Example Output

```
Creating worktree 'fix-login-bug'...
Location: /path/to/sibling/fix-login-bug
Branch: fix-login-bug (new)
```

### Before Creating

Always confirm with AskUserQuestion:
- **Question**: "Create worktree for '<name>' in sibling folder?"
- **Options**:
  - "Create" - Proceed with worktree creation
  - "Cancel" - Abort operation

## Removing Worktrees

### Process

1. **Detect repository root**: Use `{{git.root}}` from dynamic context
2. **List worktrees if name ambiguous**: Run `git worktree list` and present to user
3. **Confirm action**: Always use AskUserQuestion before executing
4. **Remove worktree**: Run `git worktree remove` command
5. **Prune worktree metadata**: Run `git worktree prune`
6. **Delete branch**: Optionally delete the branch after removal
7. **Report result**: Show cleanup status

### When Worktree Name is Ambiguous or Missing

If the user invokes `/worktree remove` without specifying a name, or the name is unclear:

1. Run `git worktree list` to get all worktrees
2. Present the list to the user with folder names and branches
3. Use AskUserQuestion to clarify which worktree to remove

Example:
```
Existing worktrees:
  /path/to/main-repo      [master]
  /path/to/fix-login      [fix-login]
  /path/to/add-feature    [add-feature]

Which worktree would you like to remove?
```

AskUserQuestion options:
- "fix-login" - Remove this worktree
- "add-feature" - Remove this worktree
- "Cancel" - Abort operation

### Command Structure

```bash
cd {{git.root}}
git worktree remove ../<name>
git worktree prune
git branch -D <name>  # Optional: delete branch
```

### Before Removing (when name is specified)

Always confirm with AskUserQuestion:
- **Question**: "Remove worktree '<name>'? This will delete the folder and branch."
- **Options**:
  - "Remove folder only" - Remove worktree, keep branch
  - "Remove folder and branch" - Complete cleanup
  - "Cancel" - Abort operation

## Inferring Action from Context

When action is omitted, infer from context:

| Context Pattern | Inferred Action |
|-----------------|-----------------|
| "new worktree", "create", "add" | create |
| "remove worktree", "delete", "clean up" | remove |
| Branch name exists | remove |
| Branch name doesn't exist | create |
| Ambiguous | AskUserQuestion |

## Error Handling

Handle these common scenarios:

- **Not a Git repository**: Detect via `git rev-parse --git-dir` and inform user
- **Worktree exists**: Inform user and offer to remove/recreate
- **Branch exists**: When creating, inform if branch already exists locally or remotely
- **No parent directory**: If repo is at filesystem root, cannot create sibling folder
- **Uncommitted changes**: Warn if worktree has uncommitted changes before removal

## Listing Existing Worktrees

To show all worktrees, use:

```bash
cd {{git.root}}
git worktree list
```

## Additional Resources

### Reference Files

- **`references/git-worktree-commands.md`** - Complete Git worktree command reference with syntax, options, and troubleshooting

### Scripts

- **`scripts/worktree-helper.sh`** - Bash utility for worktree operations (create, remove, list)

### Examples

- **`examples/create-worktree.sh`** - Example worktree creation
- **`examples/remove-worktree.sh`** - Example worktree removal
