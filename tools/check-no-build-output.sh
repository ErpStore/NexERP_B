#!/usr/bin/env bash
# tools/check-no-build-output.sh
#
# CI guard for task M0-08 (docs/kb/execution/tasks/M0-08.md). Exits non-zero
# if any build output, IDE state, or dependency directory is TRACKED by git
# -- not merely present on disk, which is normal and expected (they are
# correctly .gitignore'd; see docs/kb/risks/technical-debt-register.md R-14,
# corrected 2026-08-14).
#
# Requires no arguments and no dependency beyond git and a POSIX shell.
# Run from anywhere inside the repository; it resolves the repo root itself.
#
# Exit code: 0 if the tracked set is clean, 1 (with every offending path
# listed) otherwise.

set -uo pipefail

repo_root="$(git rev-parse --show-toplevel 2>/dev/null)"
if [ -z "$repo_root" ]; then
    echo "check-no-build-output.sh: not inside a git repository." >&2
    exit 2
fi
cd "$repo_root" || exit 2

# Same pattern used to audit the tracked set during M0-08's own investigation
# -- kept identical here so "the audit passed" and "the guard passes" are
# provably the same check, not two similar-looking ones that could drift
# apart.
pattern='(^|/)(bin|obj|dist|node_modules|\.angular|\.vs|out-tsc|bazel-out|packages)/|\.(user|suo|userosscache|rsuser)$|\.db-lock$|(^|/)TestResults/|\.vsidx$'

offenders="$(git ls-files | grep -E -i "$pattern" || true)"

if [ -n "$offenders" ]; then
    echo "check-no-build-output.sh: FAIL -- build output / IDE state / a dependency directory is tracked by git:" >&2
    echo "$offenders" | sed 's/^/  /' >&2
    echo "" >&2
    echo "Untrack each with: git rm --cached -r <path>   (NEVER a bare 'git rm' -- the file content must stay on disk)." >&2
    echo "Do not rewrite git history to remove a past commit of these paths -- that is task M0-05's scope, not this guard's." >&2
    exit 1
fi

echo "check-no-build-output.sh: OK -- no build output, IDE state, or dependency directory is tracked."
exit 0
