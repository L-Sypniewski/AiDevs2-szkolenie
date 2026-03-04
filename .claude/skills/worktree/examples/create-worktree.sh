#!/usr/bin/env bash
# Example: Creating a worktree for a new feature
# Usage: ./examples/create-worktree.sh <branch-name>

set -euo pipefail

if [[ -z "${1:-}" ]]; then
    echo "Usage: $0 <branch-name>" >&2
    exit 1
fi

NAME="$1"
GIT_ROOT="$(git rev-parse --show-toplevel)"
PARENT_DIR="$(dirname "$GIT_ROOT")"

echo "Creating worktree: $NAME"
echo "Repository root: $GIT_ROOT"
echo "Worktree location: $PARENT_DIR/$NAME"

# Create the worktree with a new branch
git worktree add "$PARENT_DIR/$NAME" -b "$NAME"

echo "Worktree created successfully!"
echo "Branch: $NAME"
echo "Path: $PARENT_DIR/$NAME"
