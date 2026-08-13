#!/usr/bin/env bash
# tools/check-no-build-output.sh
#
# M0-08 CI guard. Exits non-zero if any build output, IDE state, or
# dependency directory is ever tracked by git — the property R-14 (as
# corrected) says is currently true and must stay true.
#
# Dependency-free: git and a POSIX shell, nothing else. Runs from the
# repository root with no arguments. Intended for M0-07 to call as a CI
# step; also runnable manually or in a pre-push hook.
#
# See docs/kb/risks/technical-debt-register.md (R-14) and
# docs/kb/execution/tasks/M0-08.md for the full rationale.

set -uo pipefail

if [ ! -d "V.SMART" ]; then
  echo "error: run this script from the repository root (V.SMART/ not found)" >&2
  exit 2
fi

PATTERN='(^|/)(bin|obj|dist|node_modules|\.angular|\.vs|out-tsc|bazel-out|packages)/|\.(user|suo|userosscache|rsuser)$|\.db-lock$|(^|/)TestResults/|\.vsidx$'

offenders=$(git ls-files | grep -E -i "$PATTERN" || true)

if [ -n "$offenders" ]; then
  echo "FAIL: build output, IDE state, or a dependency directory is tracked by git:" >&2
  echo "$offenders" | sed 's/^/  /' >&2
  echo "" >&2
  echo "Untrack with 'git rm --cached -r <path>' (never a bare 'git rm' — the file" >&2
  echo "content must stay on disk) and commit. Do not rewrite history for this;" >&2
  echo "that is M0-05's job and this check does not require it." >&2
  exit 1
fi

echo "PASS: no build output, IDE state, or dependency directory is tracked by git."
exit 0
