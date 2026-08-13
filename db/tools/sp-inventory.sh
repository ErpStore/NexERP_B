#!/usr/bin/env bash
# db/tools/sp-inventory.sh
#
# Regenerates the REFERENCE side of the M0-01-01 stored-procedure
# reconciliation (docs/kb/architecture/stored-procedure-inventory.md,
# db/stored-procedures/manifest.csv). It does not touch a database and
# carries no connection string, host name or credential.
#
# Usage:
#   bash db/tools/sp-inventory.sh
#
# Takes no arguments. Must be run with the repository root as the current
# working directory (it does not `cd` itself, so relative paths in the
# output are repo-relative).
#
# Output (stdout, sorted, tab-separated, no header):
#   procedure_name<TAB>path:line<TAB>commented(yes|no)
#
# One line per occurrence of an `Sp_*` identifier in a .cs or .razor file
# under V.SMART/ (excluding obj/ and bin/ build output). A name referenced
# on 3 lines produces 3 output lines.
#
# "commented" is a heuristic, not a parser: a line is marked "yes" when its
# first non-whitespace characters open a comment for that file's language
# (.cs -> // or /* ; .razor -> // or /* or @*). This catches the common
# whole-line-commented-out case. It does NOT detect:
#   - a reference sitting on its own line inside a multi-line block comment
#     whose opening /* or @* is on an earlier line;
#   - an inline trailing // (or @* *@) comment following real code on the
#     same line — such a line is marked "no" because it also contains a
#     live reference earlier on the line.
# Known limitation, recorded here and in the KB document rather than
# silently assumed away.
#
# Tooling: grep, sort, sed only, so this runs unmodified in Git Bash on
# Windows and in a Linux CI container.

set -euo pipefail

grep -rnoE "Sp_[A-Za-z0-9_]+" \
  --include=*.cs --include=*.razor \
  --exclude-dir=obj --exclude-dir=bin \
  V.SMART \
| while IFS=: read -r sp_path sp_line sp_match; do
    content=$(sed -n "${sp_line}p" "$sp_path" 2>/dev/null || true)
    # Strip leading whitespace to find the first real characters on the line.
    trimmed="${content#"${content%%[![:space:]]*}"}"
    case "$sp_path" in
      *.razor)
        case "$trimmed" in
          '//'*|'/*'*|'@*'*) commented=yes ;;
          *)                 commented=no ;;
        esac
        ;;
      *)
        case "$trimmed" in
          '//'*|'/*'*) commented=yes ;;
          *)           commented=no ;;
        esac
        ;;
    esac
    printf '%s\t%s:%s\t%s\n' "$sp_match" "$sp_path" "$sp_line" "$commented"
  done \
| sort
