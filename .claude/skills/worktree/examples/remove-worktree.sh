#!/usr/bin/env bash
# Example: Removing a worktree and cleaning up
# Usage: ./examples/remove-worktree.sh <branch-name> [--delete-branch]

set -euo pipefail

if [[ -z "${1:-}" ]]; then
    echo "Usage: $0 <branch-name> [--delete-branch]" >&2
    exit 1
fi

NAME="$1"
DELETE_BRANCH="${2:-}"
GIT_ROOT="$(git rev-parse --show-toplevel)"
PARENT_DIR="$(dirname "$GIT_ROOT")"
WORKTREE_PATH="$PARENT_DIR/$NAME"

echo "Removing worktree: $NAME"
echo "Repository root: $GIT_ROOT"
echo "Worktree location: $WORKTREE_PATH"

# Check for uncommitted changes
if [[ -d "$WORKTREE_PATH" ]]; then
    if [[ -n "$(git -C "$WORKTREE_PATH" status --porcelain 2>/dev/null)" ]]; then
        echo "Warning: Worktree has uncommitted changes!"
        git -C "$WORKTREE_PATH" status
        read -p "Continue anyway? (y/N) " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            echo "Aborted."
            exit 1
        fi
    fi

    # Remove the worktree
    git worktree remove "$WORKTREE_PATH"
    echo "Worktree directory removed."
else
    echo "Warning: Worktree directory not found at $WORKTREE_PATH"
fi

# Prune stale worktree metadata
git worktree prune
echo "Pruned worktree metadata."

# Delete branch if requested
if [[ "$DELETE_BRANCH" == "--delete-branch" ]]; then
    if git rev-parse --verify "$NAME" >/dev/null 2>&1; then
        git branch -D "$NAME"
        echo "Branch '$NAME' deleted."
    else
        echo "Branch '$NAME' does not exist locally."
    fi
fi

echo "Cleanup complete!"
