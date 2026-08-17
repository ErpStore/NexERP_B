#!/usr/bin/env bash
# tools/check-kb-execution-framework.sh
#
# Validates the repository-driven execution framework -- the mechanism that lets
# a fresh AI session continue the migration with nothing pasted in. Every check
# corresponds to a guarantee that framework makes; a failure means some future
# session will either be unable to start, or will act on a broken reference.
#
# See docs/kb/execution/workflow.md (KB-088) for what is being guaranteed, and
# the TEST CASE MAP below for which check covers which change.
#
# Deliberately NOT a .NET test project: there is no test project in the solution
# and creating one is task M0-12-01's scope (INV-023). Deliberately NOT a CI
# workflow file: .github/workflows/ is task M0-07's scope. This follows the
# precedent set by tools/check-no-build-output.sh (M0-08) -- a standalone guard
# M0-07 can wire into CI with one line.
#
# Scope note: only CLAUDE.md and docs/kb/ are inspected. The repository contains
# ~1,300 further .md files under frontend/vsmart-erp/node_modules/ which are
# vendor content and none of this framework's business.
#
# Requires no arguments and nothing beyond git, awk, sed, grep and a POSIX shell.
# Run from anywhere inside the repository; it resolves the repo root itself.
#
#   ./tools/check-kb-execution-framework.sh              run every check
#   ./tools/check-kb-execution-framework.sh --verbose    also print each PASS
#   ./tools/check-kb-execution-framework.sh --self-test  prove the checks can fail
#
# Exit code: 0 if every check passes, 1 if any fails, 2 on a harness error.
#
# ---------------------------------------------------------------------------
# TEST CASE MAP -- change -> covering check
# ---------------------------------------------------------------------------
#   CLAUDE.md (repo root, new)          T01 T02 T03 T22
#   CLAUDE.md (wrapper dir, new)        T04
#   current-task.md (KB-089, new)       T05 T06 T07 T08 T09 T10 T22
#   workflow.md (KB-088, new)           T11 T12 T22
#   task-template.md (KB-090, new)      T13
#   INDEX.md registry + allocation      T14 T15 T16
#   task-tracker.md (KB-081)            T09 T17
#   dependency-graph.md (KB-082)        T18
#   prompt-template.md (KB-083)         T19 T21
#   review-templates.md (KB-084)        T20
#   whole KB (links, anchors, ids)      T15 T23 T24
#   context budget                      T10 T22
# ---------------------------------------------------------------------------

set -uo pipefail

VERBOSE=0
SELF_TEST=0
for arg in "$@"; do
    case "$arg" in
        --verbose)   VERBOSE=1 ;;
        --self-test) SELF_TEST=1 ;;
        *) echo "unknown argument: $arg" >&2; exit 2 ;;
    esac
done

repo_root="$(git rev-parse --show-toplevel 2>/dev/null)"
if [ -z "$repo_root" ]; then
    echo "check-kb-execution-framework.sh: not inside a git repository." >&2
    exit 2
fi
cd "$repo_root" || exit 2

KB="docs/kb"
EXEC="$KB/execution"

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

pass_count=0
fail_count=0

pass() {
    pass_count=$((pass_count + 1))
    [ "$VERBOSE" -eq 1 ] && printf '  PASS  %s  %s\n' "$1" "$2"
    return 0
}

fail() {
    fail_count=$((fail_count + 1))
    printf 'FAIL  %s  %s\n' "$1" "$2" >&2
    [ -n "${3:-}" ] && printf '        -> %s\n' "$3" >&2
    return 0
}

require_file() {  # <id> <path> <description>
    if [ -f "$2" ]; then pass "$1" "$3"; else fail "$1" "$3" "missing file: $2"; fi
}

require_grep() {  # <id> <file> <ERE> <description> <remedy>
    if [ ! -f "$2" ]; then
        fail "$1" "$4" "missing file: $2"
    elif grep -Eq "$3" "$2"; then
        pass "$1" "$4"
    else
        fail "$1" "$4" "$5"
    fi
}

# --------------------------------------------------------------------------
# Helpers -- written to avoid per-link subshells, which are punishingly slow
# on Windows. Each helper makes ONE pass with ONE process.
# --------------------------------------------------------------------------

# Sets RESOLVED to <base>/<rel> with . and .. collapsed. No fork: assigning to a
# global instead of echoing into $(...) is what keeps this check seconds not
# minutes.
RESOLVED=""
resolve_path() {
    local base="$1" rel="$2" combined out="" seg oldIFS
    case "$rel" in
        /*) combined="${rel#/}" ;;
        *)  if [ -z "$base" ]; then combined="$rel"; else combined="$base/$rel"; fi ;;
    esac
    oldIFS="$IFS"; IFS='/'
    set -f
    for seg in $combined; do
        case "$seg" in
            ''|'.') ;;
            '..') case "$out" in */*) out="${out%/*}" ;; *) out="" ;; esac ;;
            *)    if [ -z "$out" ]; then out="$seg"; else out="$out/$seg"; fi ;;
        esac
    done
    set +f
    IFS="$oldIFS"
    RESOLVED="$out"
}

# The markdown that constitutes the framework + knowledge base, relative to root.
framework_md_list() {  # <root>
    local root="$1"
    (
        cd "$root" 2>/dev/null || return 0
        if [ -d docs/kb ]; then
            [ -f CLAUDE.md ] && echo CLAUDE.md
            find docs/kb -type f -name '*.md' | sed 's|^\./||'
        else
            # fixture layout used by --self-test
            find . -type f -name '*.md' | sed 's|^\./||'
        fi
    ) | sort -u
}

# "file<TAB>linktarget" for every markdown link, in one grep.
link_pairs() {  # <root>
    local root="$1"
    (
        cd "$root" 2>/dev/null || return 0
        local -a targets=()
        if [ -d docs/kb ]; then
            targets+=(docs/kb)
            [ -f CLAUDE.md ] && targets+=(CLAUDE.md)
        else
            targets+=(.)
        fi
        grep -rHoE '\]\([^)]+\)' --include='*.md' "${targets[@]}" 2>/dev/null
    ) | sed 's|:\](|\t|; s|)$||' | sed 's|^\./||'
}

# "file<TAB>anchor" for every heading, skipping fenced code blocks so an example
# heading inside ``` does not register as a real anchor. Duplicate headings get
# GitHub's -1/-2 suffixes. One awk over every file.
emit_all_anchors() {  # <root>
    local root="$1"
    framework_md_list "$root" > "$TMP/mdlist"
    [ -s "$TMP/mdlist" ] || return 0
    ( cd "$root" && tr '\n' '\0' < "$TMP/mdlist" | xargs -0 awk '
        FNR == 1 { fence = 0 }
        /^[ \t]*```/ { fence = !fence; next }
        fence { next }
        /^#{1,6}[ \t]/ {
            line = $0
            sub(/^#+[ \t]+/, "", line)
            sub(/[ \t]+$/, "", line)
            slug = tolower(line)
            gsub(/[^a-z0-9 _-]/, "", slug)
            gsub(/ /, "-", slug)
            if (slug == "") next
            key = FILENAME "\t" slug
            n = seen[key]++
            if (n > 0) slug = slug "-" n
            printf "%s\t%s\n", FILENAME, slug
        }
    ' ) | sed 's|^\./||'
}

# --------------------------------------------------------------------------
# Generic checks -- parameterised by root so --self-test can feed them fixtures
# --------------------------------------------------------------------------

# Prints every broken relative link; returns 1 if any.
check_links() {
    local root="$1" line f target rc=0
    while IFS="$(printf '\t')" read -r f target; do
        [ -z "${target:-}" ] && continue
        case "$target" in http:*|https:*|mailto:*|'#'*) continue ;; esac
        target="${target%%#*}"
        [ -z "$target" ] && continue
        local dir="$f"
        case "$f" in */*) dir="${f%/*}" ;; *) dir="" ;; esac
        resolve_path "$dir" "$target"
        if [ ! -e "$root/$RESOLVED" ]; then
            printf '%s -> %s (resolved: %s)\n' "$f" "$target" "$RESOLVED"
            rc=1
        fi
    done < <(link_pairs "$root")
    return $rc
}

# Prints every #anchor that does not match a real heading; returns 1 if any.
check_anchors() {
    local root="$1" f target anchor rc=0 resolved
    emit_all_anchors "$root" | sort -u > "$TMP/anchors"
    while IFS="$(printf '\t')" read -r f target; do
        [ -z "${target:-}" ] && continue
        case "$target" in http:*|https:*|mailto:*) continue ;; esac
        case "$target" in *'#'*) ;; *) continue ;; esac
        anchor="${target#*#}"
        [ -z "$anchor" ] && continue
        target="${target%%#*}"
        if [ -z "$target" ]; then
            resolved="$f"
        else
            local dir="$f"
            case "$f" in */*) dir="${f%/*}" ;; *) dir="" ;; esac
            resolve_path "$dir" "$target"
            resolved="$RESOLVED"
        fi
        # A missing file is T23's finding, not this one.
        [ -f "$root/$resolved" ] || continue
        if ! grep -qxF "$(printf '%s\t%s' "$resolved" "$anchor")" "$TMP/anchors"; then
            printf '%s -> %s#%s\n' "$f" "$target" "$anchor"
            rc=1
        fi
    done < <(link_pairs "$root")
    return $rc
}

# Returns 1 if an unfilled <placeholder> survives outside a code fence.
check_placeholders() {  # <file>
    awk '
        /^[ \t]*```/ { fence = !fence; next }
        fence { next }
        {
            line = $0
            gsub(/`[^`]*`/, "", line)          # backticked text is illustrative
            if (line ~ /<[a-z][a-z0-9 _.\/-]*>/) {
                print FILENAME ":" FNR ": " $0
                found = 1
            }
        }
        END { exit(found ? 1 : 0) }
    ' "$1"
}

# Returns 1 if the Status row is absent or not a lifecycle state.
check_status_valid() {  # <file>
    local status
    status="$(grep -oE '^\| \*\*Status\*\* \| `[A-Z_]+`' "$1" 2>/dev/null \
              | grep -oE '`[A-Z_]+`' | tr -d '`' | head -1)"
    if [ -z "$status" ]; then
        echo "no '| **Status** | \`STATE\` |' row found"
        return 1
    fi
    case "$status" in
        PLANNED|READY|IN_PROGRESS|IMPLEMENTATION|TESTING|REVIEW|COMPLETED|BLOCKED) return 0 ;;
        *) echo "invalid lifecycle state: $status"; return 1 ;;
    esac
}

# Returns 1 if any non-TASK doc_id is claimed twice.
check_duplicate_doc_ids() {  # <root>
    # NB: separate `local` statements on purpose -- bash expands every word of a
    # single `local a=1 b=$a` before performing any assignment, so `b` would see
    # the *outer* scope's `a` (unset, and fatal under `set -u`).
    local root="$1"
    local dupes
    local searchdir="$root"
    [ -d "$root/docs/kb" ] && searchdir="$root/docs/kb"
    dupes="$(grep -rhoE '^doc_id: [A-Za-z0-9-]+' --include='*.md' "$searchdir" 2>/dev/null \
             | awk '{print $2}' | grep -vE '^TASK-' | sort | uniq -d)"
    if [ -n "$dupes" ]; then echo "$dupes"; return 1; fi
    return 0
}

# ==========================================================================
# --self-test: prove each generic check rejects a known-bad input.
# A check that cannot fail is not a check.
# ==========================================================================
if [ "$SELF_TEST" -eq 1 ]; then
    echo "self-test: verifying each generic check rejects a known-bad input"
    st_fail=0
    fx="$TMP/fixture"
    mkdir -p "$fx/sub"

    # S1 -- a broken relative link must be caught
    printf 'See [x](does-not-exist.md).\n' > "$fx/a.md"
    if check_links "$fx" >/dev/null 2>&1; then
        echo "SELF-TEST FAIL  S1   check_links accepted a broken link" >&2; st_fail=1
    else
        echo "  ok  S1   check_links rejects a broken link"
    fi
    # ...and a link that does resolve, including through ../, must be accepted
    printf 'ok [y](../a.md)\n' > "$fx/sub/b.md"
    if check_links "$fx" >/dev/null 2>&1; then
        echo "SELF-TEST FAIL  S1b  check_links should still fail (a.md is broken)" >&2; st_fail=1
    else
        echo "  ok  S1b  check_links resolves ../ without false alarm"
    fi
    rm -f "$fx/a.md" "$fx/sub/b.md"
    printf '# T\n' > "$fx/target.md"
    printf 'See [x](target.md).\n' > "$fx/a.md"
    if check_links "$fx" >/dev/null 2>&1; then
        echo "  ok  S1c  check_links accepts a resolving link"
    else
        echo "SELF-TEST FAIL  S1c  check_links rejected a valid link" >&2; st_fail=1
    fi

    # S2 -- a dangling #anchor must be caught
    printf '# Real Heading\n\nSee [x](target.md#no-such-heading).\n' > "$fx/a.md"
    if check_anchors "$fx" >/dev/null 2>&1; then
        echo "SELF-TEST FAIL  S2   check_anchors accepted a dangling anchor" >&2; st_fail=1
    else
        echo "  ok  S2   check_anchors rejects a dangling anchor"
    fi
    printf '# T\n\n## Real Heading\n' > "$fx/target.md"
    printf 'See [x](target.md#real-heading).\n' > "$fx/a.md"
    if check_anchors "$fx" >/dev/null 2>&1; then
        echo "  ok  S2b  check_anchors accepts a resolving anchor"
    else
        echo "SELF-TEST FAIL  S2b  check_anchors rejected a valid anchor" >&2; st_fail=1
    fi

    # S3 -- an unfilled placeholder must be caught, but not inside a fence
    printf '# T\n\nObjective: <task objective>\n' > "$fx/c.md"
    if check_placeholders "$fx/c.md" >/dev/null; then
        echo "SELF-TEST FAIL  S3   check_placeholders accepted an unfilled placeholder" >&2; st_fail=1
    else
        echo "  ok  S3   check_placeholders rejects an unfilled placeholder"
    fi
    printf '# T\n\n```\nObjective: <task objective>\n```\n' > "$fx/c.md"
    if check_placeholders "$fx/c.md" >/dev/null; then
        echo "  ok  S3b  check_placeholders ignores fenced template placeholders"
    else
        echo "SELF-TEST FAIL  S3b  check_placeholders fired inside a code fence" >&2; st_fail=1
    fi

    # S4 -- a bogus lifecycle state must be caught
    printf '| **Status** | `DONE-ISH` |\n' > "$fx/d.md"
    if check_status_valid "$fx/d.md" >/dev/null; then
        echo "SELF-TEST FAIL  S4   check_status_valid accepted a bogus state" >&2; st_fail=1
    else
        echo "  ok  S4   check_status_valid rejects a non-lifecycle state"
    fi
    printf '| **Status** | `READY` |\n' > "$fx/d.md"
    if check_status_valid "$fx/d.md" >/dev/null; then
        echo "  ok  S4b  check_status_valid accepts a valid state"
    else
        echo "SELF-TEST FAIL  S4b  check_status_valid rejected a valid state" >&2; st_fail=1
    fi

    # S5 -- a duplicated doc_id must be caught
    printf 'doc_id: KB-999\n' > "$fx/e1.md"
    printf 'doc_id: KB-999\n' > "$fx/e2.md"
    if check_duplicate_doc_ids "$fx" >/dev/null; then
        echo "SELF-TEST FAIL  S5   check_duplicate_doc_ids accepted a duplicate" >&2; st_fail=1
    else
        echo "  ok  S5   check_duplicate_doc_ids rejects a duplicated id"
    fi
    printf 'doc_id: KB-998\n' > "$fx/e2.md"
    if check_duplicate_doc_ids "$fx" >/dev/null; then
        echo "  ok  S5b  check_duplicate_doc_ids accepts distinct ids"
    else
        echo "SELF-TEST FAIL  S5b  check_duplicate_doc_ids rejected distinct ids" >&2; st_fail=1
    fi

    rm -rf "$fx"
    if [ "$st_fail" -ne 0 ]; then
        echo "self-test: FAILED -- the checks themselves are broken." >&2
        exit 1
    fi
    echo "self-test: OK -- every generic check rejects its known-bad input."
    echo ""
fi

echo "check-kb-execution-framework.sh: validating the execution framework"
echo ""

# ==========================================================================
# CLAUDE.md -- the session entry point
# ==========================================================================

require_file T01 "CLAUDE.md" "CLAUDE.md exists at the repository root"

require_grep T02 "CLAUDE.md" 'docs/kb/execution/current-task\.md' \
    "CLAUDE.md points at current-task.md" \
    "the entry point must name the active-task file, or a session cannot find its task"

require_grep T03 "CLAUDE.md" 'nested' \
    "CLAUDE.md warns about the nested repository root" \
    "the wrapper-vs-repo-root trap silently breaks every relative path"

if [ -f "../CLAUDE.md" ]; then
    if grep -q 'NexGen-ERP---2025-master/CLAUDE.md' "../CLAUDE.md"; then
        pass T04 "wrapper CLAUDE.md redirects to the repository CLAUDE.md"
    else
        fail T04 "wrapper CLAUDE.md redirects to the repository CLAUDE.md" \
            "../CLAUDE.md exists but does not point at NexGen-ERP---2025-master/CLAUDE.md"
    fi
else
    pass T04 "wrapper CLAUDE.md absent (lives outside the repo; skipped)"
fi

# ==========================================================================
# current-task.md -- the one active task
# ==========================================================================

require_file T05 "$EXEC/current-task.md" "current-task.md exists"

if [ -f "$EXEC/current-task.md" ]; then
    CT="$EXEC/current-task.md"

    missing=""
    for section in Objective Dependencies "Relevant Documentation" \
                   "Relevant Existing Code" "Business Rules" \
                   "Acceptance Criteria" "Testing Requirements" \
                   "Documentation Updates" "Completion Conditions"; do
        grep -Eq "^## .*${section}" "$CT" || missing="$missing '$section'"
    done
    if [ -z "$missing" ]; then
        pass T06 "current-task.md has every required section"
    else
        fail T06 "current-task.md has every required section" \
            "missing:$missing -- see docs/kb/execution/task-template.md (KB-090)"
    fi

    if err="$(check_status_valid "$CT")"; then
        pass T07 "current-task.md declares a valid lifecycle state"
    else
        fail T07 "current-task.md declares a valid lifecycle state" \
            "$err -- valid states are in docs/kb/execution/workflow.md section 1"
    fi

    if out="$(check_placeholders "$CT")"; then
        pass T08 "current-task.md contains no unfilled placeholders"
    else
        fail T08 "current-task.md contains no unfilled placeholders" \
            "$(echo "$out" | head -3 | tr '\n' ' ')"
    fi

    task_id="$(grep -oE '^\| \*\*Task ID\*\* \| \*\*[A-Za-z0-9-]+\*\*' "$CT" \
               | sed 's/.*\*\* | \*\*//; s/\*\*.*//' | head -1)"
    if [ -z "$task_id" ]; then
        fail T09 "current-task.md's Task ID resolves to a real, tracked task" \
            "could not parse a '| **Task ID** | **<ID>** |' row"
    elif [ ! -f "$EXEC/tasks/$task_id.md" ]; then
        fail T09 "current-task.md's Task ID resolves to a real, tracked task" \
            "no spec file: $EXEC/tasks/$task_id.md"
    elif ! grep -q "| $task_id |" "$EXEC/task-tracker.md"; then
        fail T09 "current-task.md's Task ID resolves to a real, tracked task" \
            "$task_id has no row in task-tracker.md (KB-081 is authoritative on status)"
    else
        pass T09 "current-task.md's Task ID ($task_id) resolves to a real, tracked task"
    fi

    ct_bytes="$(wc -c < "$CT" | tr -d ' ')"
    if [ "$ct_bytes" -le 20000 ]; then
        pass T10 "current-task.md is a pointer, not a copy (${ct_bytes} bytes <= 20000)"
    else
        fail T10 "current-task.md is a pointer, not a copy" \
            "${ct_bytes} bytes exceeds 20000 -- it is duplicating the KB rather than referencing it"
    fi
fi

# ==========================================================================
# workflow.md -- the procedure
# ==========================================================================

require_file T11 "$EXEC/workflow.md" "workflow.md exists"

if [ -f "$EXEC/workflow.md" ]; then
    missing=""
    for state in PLANNED READY IN_PROGRESS IMPLEMENTATION TESTING REVIEW COMPLETED BLOCKED; do
        grep -q "$state" "$EXEC/workflow.md" || missing="$missing $state"
    done
    if [ -z "$missing" ]; then
        pass T12 "workflow.md defines every lifecycle state"
    else
        fail T12 "workflow.md defines every lifecycle state" "undefined:$missing"
    fi
fi

# ==========================================================================
# task-template.md -- the shape of a new task
# ==========================================================================

if [ -f "$EXEC/task-template.md" ]; then
    missing=""
    for section in Objective Scope "Out of Scope" Dependencies \
                   "Relevant Documentation" "Relevant Existing Code" \
                   "Business Rules" "Implementation Requirements" \
                   "Acceptance Criteria" "Testing Requirements" \
                   "Documentation Updates" "Completion Conditions"; do
        grep -Fq "## $section" "$EXEC/task-template.md" || missing="$missing '$section'"
    done
    if [ -z "$missing" ]; then
        pass T13 "task-template.md carries every required task section"
    else
        fail T13 "task-template.md carries every required task section" "missing:$missing"
    fi
else
    fail T13 "task-template.md carries every required task section" \
        "missing file: $EXEC/task-template.md"
fi

# ==========================================================================
# INDEX.md -- doc_id registry and allocation
# ==========================================================================

unregistered=""
for f in "$EXEC/workflow.md" "$EXEC/current-task.md" "$EXEC/task-template.md"; do
    [ -f "$f" ] || continue
    id="$(grep -oE '^doc_id: [A-Za-z0-9-]+' "$f" | awk '{print $2}' | head -1)"
    if [ -z "$id" ]; then
        unregistered="$unregistered $(basename "$f"):no-doc_id"
    elif ! grep -q "| $id |" "$KB/INDEX.md"; then
        unregistered="$unregistered $id"
    fi
done
if [ -z "$unregistered" ]; then
    pass T14 "every new framework document is registered in INDEX.md"
else
    fail T14 "every new framework document is registered in INDEX.md" \
        "unregistered:$unregistered -- add a row to KB-005's document registry"
fi

if dupes="$(check_duplicate_doc_ids ".")"; then
    pass T15 "no doc_id is claimed twice across the knowledge base"
else
    fail T15 "no doc_id is claimed twice across the knowledge base" \
        "duplicated: $(echo "$dupes" | tr '\n' ' ')"
fi

require_grep T16 "$KB/INDEX.md" 'Next free' \
    "INDEX.md still records the next free doc_id" \
    "the allocation table must state the next free id or concurrent sessions collide"

# ==========================================================================
# The documents the framework depends on
# ==========================================================================

require_grep T17 "$EXEC/task-tracker.md" 'current-task\.md' \
    "task-tracker.md points at current-task.md" \
    "KB-081 must route a reader to the active task"

require_grep T18 "$EXEC/dependency-graph.md" '^## Ready-task selection rule' \
    "dependency-graph.md defines the next-task selection rule" \
    "current-task.md and workflow.md both link to this anchor"

require_grep T19 "$EXEC/prompt-template.md" '^## Verified repository commands' \
    "prompt-template.md still owns the verified-commands table" \
    "this table is the single source of truth for build/test commands"

require_grep T20 "$EXEC/review-templates.md" 'Session close-out' \
    "review-templates.md carries the session close-out checklist" \
    "without it, findings die in the conversation instead of reaching the repository"

vc_count="$(grep -rlE '^## Verified repository commands' --include='*.md' "$KB" 2>/dev/null | wc -l | tr -d ' ')"
if [ "$vc_count" -eq 1 ]; then
    pass T21 "the verified-commands table exists in exactly one file"
else
    fail T21 "the verified-commands table exists in exactly one file" \
        "found in $vc_count files -- duplicated tables drift apart and one becomes a lie"
fi

# ==========================================================================
# Context budget -- the point of the refactor
# ==========================================================================

bootstrap_bytes=0
for f in CLAUDE.md "$EXEC/current-task.md" "$EXEC/workflow.md"; do
    [ -f "$f" ] && bootstrap_bytes=$((bootstrap_bytes + $(wc -c < "$f" | tr -d ' ')))
done
if [ "$bootstrap_bytes" -le 45000 ]; then
    pass T22 "session bootstrap set is within budget (${bootstrap_bytes} bytes <= 45000)"
else
    fail T22 "session bootstrap set is within budget" \
        "${bootstrap_bytes} bytes exceeds 45000 -- every session pays this before doing any work"
fi

# ==========================================================================
# Whole-KB integrity
# ==========================================================================

if broken="$(check_links ".")"; then
    pass T23 "every relative markdown link in the KB resolves"
else
    fail T23 "every relative markdown link in the KB resolves" \
        "$(echo "$broken" | head -5 | tr '\n' '; ')"
fi

if bad="$(check_anchors ".")"; then
    pass T24 "every markdown #anchor resolves to a real heading"
else
    fail T24 "every markdown #anchor resolves to a real heading" \
        "$(echo "$bad" | head -5 | tr '\n' '; ')"
fi

# ==========================================================================

echo ""
if [ "$fail_count" -eq 0 ]; then
    echo "check-kb-execution-framework.sh: OK -- $pass_count checks passed."
    exit 0
fi
echo "check-kb-execution-framework.sh: FAIL -- $fail_count failed, $pass_count passed." >&2
echo "The execution framework is what lets a fresh session start with no pasted context." >&2
echo "A failure here means some future session will not be able to." >&2
exit 1
