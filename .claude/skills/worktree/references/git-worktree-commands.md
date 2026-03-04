# Git Worktree Commands Reference

Complete reference for Git worktree commands used by this skill.

## Primary Commands

### `git worktree add <path> [-b <branch>]`

Create a new worktree.

**Syntax:**
```bash
git worktree add <path> [<branch>]
git worktree add <path> -b <new-branch>
```

**Options:**
- `<path>` - Where to create the worktree (required)
- `-b <branch>` - Create a new branch with this name
- `--detach` - Create worktree with detached HEAD
- `--checkout` - Populate the new worktree (default)
- `--no-checkout` - Create worktree but don't check out files

**Examples:**
```bash
# Create worktree for existing branch
git worktree add ../fix-login-bug fix-login-bug

# Create worktree with new branch
git worktree add ../new-feature -b new-feature

# Create worktree in specific location
git worktree add /tmp/worktree-temp -b temp-branch
```

### `git worktree remove <path>`

Remove a worktree.

**Syntax:**
```bash
git worktree remove <path>
```

**Behavior:**
- Removes the worktree directory
- Does NOT delete the branch
- Requires clean working directory (no uncommitted changes)
- Fails if worktree has uncommitted changes

**Examples:**
```bash
git worktree remove ../fix-login-bug
```

### `git worktree prune`

Clean up stale worktree metadata.

**Syntax:**
```bash
git worktree prune
```

**Behavior:**
- Removes administrative files for deleted worktrees
- Safe to run anytime
- Recommended after manual worktree deletion

**When to use:**
- After manually deleting a worktree directory
- After `git worktree remove`
- Periodically to clean up stale metadata

### `git worktree list`

List all worktrees.

**Syntax:**
```bash
git worktree list
```

**Output format:**
```
/path/to/main-repo              abcd123 [master]
/path/to/sibling/feature-branch 5678ef9 [feature-branch]
```

## Advanced Commands

### `git worktree move <old> <new>`

Move a worktree to a new location.

**Syntax:**
```bash
git worktree move <old-path> <new-path>
```

### `git worktree lock [--reason <text>] <path>`

Prevent a worktree from being removed or pruned.

**Syntax:**
```bash
git worktree lock ../feature-branch
git worktree lock --reason "In use by CI" ../feature-branch
```

### `git worktree unlock <path>`

Remove the lock from a worktree.

**Syntax:**
```bash
git worktree unlock ../feature-branch
```

## Configuration

### `git worktree.<options>`

Configure worktree behavior.

**Settings:**
```bash
# Warn when adding worktree with sparse checkout
git config worktree.warnForSparseBranch true

# Set default worktree location
# (Not directly supported, use scripts or aliases)
```

## Troubleshooting

### Worktree Already Exists

**Error:** `worktree '<path>' already exists`

**Solution:**
```bash
# List existing worktrees
git worktree list

# Remove existing worktree first
git worktree remove <path>
```

### Uncommitted Changes

**Error:** `This worktree contains uncommitted changes`

**Solution:**
```bash
# Commit or stash changes in the worktree
cd ../worktree-name
git commit -am "WIP"
# or
git stash

# Then remove the worktree
cd -
git worktree remove ../worktree-name
```

### Branch Already Exists

**Scenario:** Want to create worktree but branch already exists

**Solution:**
```bash
# Use existing branch without -b flag
git worktree add ../worktree-name existing-branch-name
```

### Stale Worktree Metadata

**Symptoms:** Ghost worktree entries, errors after manual deletion

**Solution:**
```bash
git worktree prune
```

## Worktree Directory Structure

Each worktree has:
- `.git` file (points to main repo's `.git/worktrees/<name>/`)
- Working directory files
- Separate index and working directory

Main repository `.git/worktrees/` contains:
- `<name>/gitdir` - Path to worktree's `.git` file
- `<name>/commondir` - Path to shared git directory
- `<name>/HEAD` - Worktree's HEAD reference
- `<name>/logs/` - Ref logs for worktree
