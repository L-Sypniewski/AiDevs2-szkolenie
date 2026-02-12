#!/usr/bin/env bash
# Git Worktree Helper Script
# Provides utility functions for worktree management

set -euo pipefail

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get git repository root
get_git_root() {
    git rev-parse --show-toplevel 2>/dev/null || {
        echo "Error: Not a Git repository" >&2
        exit 1
    }
}

# Get parent directory of git repo
get_parent_dir() {
    local git_root
    git_root="$(get_git_root)"
    dirname "$git_root"
}

# Check if worktree exists
worktree_exists() {
    local name="$1"
    local git_root
    git_root="$(get_git_root)"
    [[ -d "${git_root}/../${name}" ]]
}

# Check if branch exists locally
branch_exists_local() {
    git rev-parse --verify "$1" >/dev/null 2>&1
}

# Check if branch exists remotely
branch_exists_remote() {
    local branch="$1"
    git ls-remote --heads origin "${branch}" 2>/dev/null | grep -q "${branch}"
}

# Create worktree in sibling folder
create_worktree() {
    local name="$1"
    local git_root
    local parent_dir

    git_root="$(get_git_root)"
    parent_dir="$(dirname "$git_root")"

    echo -e "${GREEN}Creating worktree: ${name}${NC}"

    if worktree_exists "$name"; then
        echo -e "${YELLOW}Warning: Worktree directory already exists at ${parent_dir}/${name}${NC}"
        return 1
    fi

    if branch_exists_local "$name"; then
        echo -e "${YELLOW}Notice: Branch '${name}' already exists locally. Checking out existing branch.${NC}"
        git -C "$git_root" worktree add "${parent_dir}/${name}" "$name"
    else
        git -C "$git_root" worktree add "${parent_dir}/${name}" -b "$name"
    fi

    echo -e "${GREEN}Success: Worktree created at ${parent_dir}/${name}${NC}"
    echo "Branch: ${name}"
}

# Remove worktree and cleanup
remove_worktree() {
    local name="$1"
    local delete_branch="${2:-false}"
    local git_root
    local parent_dir

    git_root="$(get_git_root)"
    parent_dir="$(dirname "$git_root")"

    echo -e "${GREEN}Removing worktree: ${name}${NC}"

    if ! worktree_exists "$name"; then
        echo -e "${YELLOW}Warning: Worktree directory not found at ${parent_dir}/${name}${NC}"
    else
        # Check for uncommitted changes
        if [[ -n "$(git -C "${parent_dir}/${name}" status --porcelain 2>/dev/null)" ]]; then
            echo -e "${RED}Error: Worktree has uncommitted changes. Commit or stash them first.${NC}"
            return 1
        fi

        git -C "$git_root" worktree remove "${parent_dir}/${name}"
        echo -e "${GREEN}Removed worktree directory${NC}"
    fi

    # Prune stale worktree metadata
    git -C "$git_root" worktree prune
    echo -e "${GREEN}Pruned worktree metadata${NC}"

    # Delete branch if requested
    if [[ "$delete_branch" == "true" ]]; then
        if branch_exists_local "$name"; then
            git -C "$git_root" branch -D "$name"
            echo -e "${GREEN}Deleted branch: ${name}${NC}"
        else
            echo -e "${YELLOW}Branch '${name}' does not exist locally${NC}"
        fi
    fi
}

# List all worktrees
list_worktrees() {
    local git_root
    git_root="$(get_git_root)"

    echo -e "${GREEN}Git worktrees:${NC}"
    git -C "$git_root" worktree list
}

# Main - handle subcommands
case "${1:-}" in
    create)
        if [[ -z "${2:-}" ]]; then
            echo "Usage: $0 create <name>" >&2
            exit 1
        fi
        create_worktree "$2"
        ;;
    remove)
        if [[ -z "${2:-}" ]]; then
            echo "Usage: $0 remove <name> [--delete-branch]" >&2
            exit 1
        fi
        delete_branch="false"
        [[ "${3:-}" == "--delete-branch" ]] && delete_branch="true"
        remove_worktree "$2" "$delete_branch"
        ;;
    list)
        list_worktrees
        ;;
    *)
        echo "Usage: $0 {create|remove|list} [args]" >&2
        echo "  create <name>       Create worktree in sibling folder" >&2
        echo "  remove <name>       Remove worktree (add --delete-branch to also delete branch)" >&2
        echo "  list                List all worktrees" >&2
        exit 1
        ;;
esac
