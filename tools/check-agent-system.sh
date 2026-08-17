#!/usr/bin/env bash
# tools/check-agent-system.sh
#
# Validates the autonomous agent execution system -- the orchestrator, the four
# worker agents, the slash commands, and the model-routing / retry / escalation
# policy they all defer to. Every check corresponds to a guarantee that system
# makes; a failure means some future autonomous run will misroute work, re-derive
# state that should be on disk, retry forever, or silently do something the
# repository forbids.
#
# See docs/kb/execution/autonomous-runner.md (KB-091) for what is guaranteed, and
# the TEST CASE MAP below for which check covers which guarantee.
#
# Companion to tools/check-kb-execution-framework.sh, which validates the
# repository-driven framework this system is built on. Same precedent as that
# script and tools/check-no-build-output.sh (M0-08): a standalone guard needing
# nothing but git, awk, sed, grep and a POSIX shell, that M0-07 can wire into CI
# with a single line. Deliberately NOT a .NET test project -- creating one is
# M0-12-01's scope (INV-023).
#
#   ./tools/check-agent-system.sh              run every check
#   ./tools/check-agent-system.sh --verbose    also print each PASS
#   ./tools/check-agent-system.sh --self-test  prove the checks can fail
#
# Exit code: 0 if every check passes, 1 if any fails, 2 on a harness error.
#
# ---------------------------------------------------------------------------
# TEST CASE MAP -- guarantee -> the check that covers it
# ---------------------------------------------------------------------------
#   the five agents exist and are well-formed      A01 A02 A03
#   model routing uses aliases, never dated ids    A04 A05
#   the investigator/validator are read-only       A06
#   validation is never routed to the cheapest     A07
#   the orchestrator can actually delegate         A08
#   policy lives in exactly one place (KB-091)     A09 A10 A11 A12
#   the state machine is fully defined             A13
#   retries are bounded and escalation is defined  A14
#   run state persists on disk, not in a chat      A15 A16
#   the failure log exists and is registered       A17
#   entry points exist and defer to the policy     A18 A19
#   every path an agent names actually exists      A20
#   the completed tasks were not disturbed         A21
#   an actual orchestration loop exists            W01 W02
#   the loop drives the real agents                W03
#   script routing matches the KB-091 spec         W04 W05
#   retries are bounded in the executable path     W06
#   stop conditions are implemented, not just documented  W07
#   run state is persisted and registered          W08 W09
#   start and stop entry points exist              W10
# ---------------------------------------------------------------------------

set -uo pipefail

VERBOSE=0
SELF_TEST=0
for arg in "$@"; do
    case "$arg" in
        --verbose) VERBOSE=1 ;;
        --self-test) SELF_TEST=1 ;;
        *) echo "unknown argument: $arg" >&2; exit 2 ;;
    esac
done

repo_root="$(git rev-parse --show-toplevel 2>/dev/null)"
if [ -z "$repo_root" ]; then
    echo "check-agent-system.sh: not inside a git repository." >&2
    exit 2
fi
cd "$repo_root" || exit 2

AGENTS=".claude/agents"
COMMANDS=".claude/commands"
EXEC="docs/kb/execution"
RUNNER="$EXEC/autonomous-runner.md"
FAILLOG="$EXEC/failure-log.md"
CURRENT="$EXEC/current-task.md"
INDEX="docs/kb/INDEX.md"

WORKERS="migration-investigator migration-implementer migration-validator migration-debugger"
ALL_AGENTS="migration-orchestrator $WORKERS"

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

# --------------------------------------------------------------------------
# Generic checks -- take a file argument so --self-test can run them on
# fixtures. A check that cannot fail is not a check.
# --------------------------------------------------------------------------

# Print the YAML frontmatter block of a markdown file (between the first two ---).
frontmatter() {
    awk 'NR==1 && $0 != "---" { exit }
         NR==1 { next }
         /^---[ \t]*$/ { exit }
         { print }' "$1"
}

# check_model_alias <file> -> 1 if the model: value is not a supported alias.
# Dated ids (claude-sonnet-4-5-20250929) go stale and silently pin an old model.
check_model_alias() {
    local m
    m="$(frontmatter "$1" | grep -E '^model:' | head -1 | sed 's/^model:[ \t]*//; s/[ \t]*$//')"
    if [ -z "$m" ]; then
        echo "no model: key"
        return 1
    fi
    case "$m" in
        haiku|sonnet|opus|inherit) return 0 ;;
        *) echo "not a supported alias: '$m'"; return 1 ;;
    esac
}

# check_no_write_tools <file> -> 1 if the agent may write. Read-only agents must
# not be able to modify what they are describing or judging.
check_no_write_tools() {
    local t
    t="$(frontmatter "$1" | grep -E '^tools:' | head -1)"
    if [ -z "$t" ]; then
        echo "no tools: key -- an unrestricted agent inherits write access"
        return 1
    fi
    if echo "$t" | grep -qE '(^|[ ,:])(Write|Edit|NotebookEdit)([ ,]|$)'; then
        echo "grants a write tool: $t"
        return 1
    fi
    return 0
}

# check_kb_paths_exist <file> -> prints each docs/kb path named in the file that
# does not exist on disk. An agent pointed at a missing file wastes a whole run.
check_kb_paths_exist() {
    local root="${2:-.}" p missing=0
    grep -oE '(docs/kb|tools)/[A-Za-z0-9._/-]+\.(md|sh)' "$1" 2>/dev/null \
    | sed 's/[.,)]*$//' | sort -u \
    | while IFS= read -r p; do
        [ -e "$root/$p" ] || printf '%s\n' "$p"
    done
}

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

# --------------------------------------------------------------------------
# --self-test
# --------------------------------------------------------------------------
if [ "$SELF_TEST" -eq 1 ]; then
    echo "self-test: verifying each generic check rejects a known-bad input"
    st_fail=0
    fx="$TMP/fixture"
    mkdir -p "$fx"

    printf -- '---\nname: x\ntools: Read, Grep\nmodel: claude-sonnet-4-5-20250929\n---\nbody\n' > "$fx/a.md"
    if check_model_alias "$fx/a.md" >/dev/null; then
        echo "SELF-TEST FAIL  S1  check_model_alias accepted a dated model id" >&2; st_fail=1
    else
        echo "  ok  S1  check_model_alias rejects a dated model id"
    fi

    printf -- '---\nname: x\ntools: Read, Grep\nmodel: sonnet\n---\nbody\n' > "$fx/b.md"
    if check_model_alias "$fx/b.md" >/dev/null; then
        echo "  ok  S1b check_model_alias accepts a supported alias"
    else
        echo "SELF-TEST FAIL  S1b check_model_alias rejected a valid alias" >&2; st_fail=1
    fi

    printf -- '---\nname: x\ntools: Read, Write, Grep\nmodel: sonnet\n---\nbody\n' > "$fx/c.md"
    if check_no_write_tools "$fx/c.md" >/dev/null; then
        echo "SELF-TEST FAIL  S2  check_no_write_tools accepted a write tool" >&2; st_fail=1
    else
        echo "  ok  S2  check_no_write_tools rejects a write tool"
    fi

    if check_no_write_tools "$fx/b.md" >/dev/null; then
        echo "  ok  S2b check_no_write_tools accepts a read-only tool list"
    else
        echo "SELF-TEST FAIL  S2b check_no_write_tools rejected a read-only list" >&2; st_fail=1
    fi

    mkdir -p "$fx/docs/kb"
    printf 'See docs/kb/does-not-exist.md for detail.\n' > "$fx/d.md"
    if [ -n "$(check_kb_paths_exist "$fx/d.md" "$fx")" ]; then
        echo "  ok  S3  check_kb_paths_exist reports a missing path"
    else
        echo "SELF-TEST FAIL  S3  check_kb_paths_exist missed a dangling path" >&2; st_fail=1
    fi

    if [ "$st_fail" -ne 0 ]; then
        echo "self-test: FAILED -- the checks themselves are broken." >&2
        exit 1
    fi
    echo "self-test: OK -- every generic check rejects its known-bad input."
    echo ""
fi

echo "check-agent-system.sh: validating the autonomous agent system"
echo ""

# ==========================================================================
# The agents exist and are well-formed
# ==========================================================================

missing=""
for a in $ALL_AGENTS; do
    [ -f "$AGENTS/$a.md" ] || missing="$missing $a"
done
if [ -z "$missing" ]; then
    pass A01 "all five migration agents exist under $AGENTS"
else
    fail A01 "all five migration agents exist under $AGENTS" "missing:$missing"
fi

bad=""
for a in $ALL_AGENTS; do
    f="$AGENTS/$a.md"
    [ -f "$f" ] || continue
    for key in name description tools model; do
        frontmatter "$f" | grep -qE "^$key:" || bad="$bad $a:no-$key"
    done
done
if [ -z "$bad" ]; then
    pass A02 "every agent declares name, description, tools and model"
else
    fail A02 "every agent declares name, description, tools and model" "missing:$bad"
fi

bad=""
for a in $ALL_AGENTS; do
    f="$AGENTS/$a.md"
    [ -f "$f" ] || continue
    declared="$(frontmatter "$f" | grep -E '^name:' | head -1 | sed 's/^name:[ \t]*//; s/[ \t]*$//')"
    [ "$declared" = "$a" ] || bad="$bad $a(declares '$declared')"
done
if [ -z "$bad" ]; then
    pass A03 "every agent's name matches its filename"
else
    fail A03 "every agent's name matches its filename" \
        "mismatched:$bad -- Claude Code dispatches by name, so a mismatch silently never runs"
fi

# ==========================================================================
# Model routing
# ==========================================================================

bad=""
for a in $ALL_AGENTS; do
    f="$AGENTS/$a.md"
    [ -f "$f" ] || continue
    if err="$(check_model_alias "$f")"; then :; else bad="$bad $a($err)"; fi
done
if [ -z "$bad" ]; then
    pass A04 "every agent's model is a supported alias, not a dated id"
else
    fail A04 "every agent's model is a supported alias, not a dated id" \
        "$bad -- KB-091 section 5 requires aliases; dated ids pin a model that goes stale"
fi

# A dated model id anywhere in the system is a hard-coded routing decision.
dated="$(grep -rlE 'claude-[a-z]+-[0-9]+(-[0-9]+)?-[0-9]{8}' "$AGENTS" "$COMMANDS" "$RUNNER" 2>/dev/null)"
if [ -z "$dated" ]; then
    pass A05 "no dated model id is hard-coded anywhere in the agent system"
else
    fail A05 "no dated model id is hard-coded anywhere in the agent system" \
        "found in: $(echo "$dated" | tr '\n' ' ')"
fi

for a in migration-investigator migration-validator; do
    f="$AGENTS/$a.md"
    if [ ! -f "$f" ]; then
        fail A06 "$a is read-only" "missing file: $f"
    elif err="$(check_no_write_tools "$f")"; then
        pass A06 "$a is read-only"
    else
        fail A06 "$a is read-only" \
            "$err -- an investigator that edits code cannot be trusted to describe it, and a validator that edits code is not independent"
    fi
done

vf="$AGENTS/migration-validator.md"
if [ -f "$vf" ]; then
    vm="$(frontmatter "$vf" | grep -E '^model:' | head -1 | sed 's/^model:[ \t]*//; s/[ \t]*$//')"
    if [ "$vm" = "haiku" ]; then
        fail A07 "validation is never routed to the cheapest model" \
            "migration-validator declares model: haiku -- KB-091 section 5.2 floor 1 forbids it"
    else
        pass A07 "validation is never routed to the cheapest model (declares '$vm')"
    fi
fi

of="$AGENTS/migration-orchestrator.md"
if [ -f "$of" ]; then
    if frontmatter "$of" | grep -E '^tools:' | grep -q 'Agent'; then
        pass A08 "the orchestrator can dispatch worker agents"
    else
        fail A08 "the orchestrator can dispatch worker agents" \
            "its tools: line does not grant Agent -- it would have to do the work itself, losing independent validation"
    fi
fi

# ==========================================================================
# The policy document -- one place, fully specified
# ==========================================================================

if [ -f "$RUNNER" ]; then
    pass A09 "the routing/retry policy document exists (KB-091)"
else
    fail A09 "the routing/retry policy document exists (KB-091)" "missing file: $RUNNER"
fi

if [ -f "$RUNNER" ]; then
    id="$(grep -oE '^doc_id: [A-Za-z0-9-]+' "$RUNNER" | awk '{print $2}' | head -1)"
    if [ -n "$id" ] && grep -q "| $id |" "$INDEX"; then
        pass A10 "KB-091 is registered in INDEX.md as $id"
    else
        fail A10 "KB-091 is registered in INDEX.md" \
            "doc_id '$id' has no row in $INDEX -- an unregistered document is one no session will find"
    fi

    missing=""
    for role in Investigate Implement Validate Diagnose; do
        grep -q "| $role" "$RUNNER" || missing="$missing $role"
    done
    if [ -z "$missing" ]; then
        pass A11 "the routing table covers every role"
    else
        fail A11 "the routing table covers every role" "no routing row for:$missing"
    fi

    missing=""
    for key in max_tasks_per_run continue_on_complete max_retries \
               escalate_after_failures allow_merge allow_push allow_schema_change; do
        grep -q "$key" "$RUNNER" || missing="$missing $key"
    done
    if [ -z "$missing" ]; then
        pass A12 "the runner configuration block defines every required key"
    else
        fail A12 "the runner configuration block defines every required key" "missing:$missing"
    fi

    missing=""
    for state in PLANNED READY IN_PROGRESS INVESTIGATING IMPLEMENTING VALIDATING \
                 FAILED DIAGNOSING RETRYING ESCALATED REVIEW COMPLETED BLOCKED; do
        grep -q "$state" "$RUNNER" || missing="$missing $state"
    done
    if [ -z "$missing" ]; then
        pass A13 "the state machine defines every runner state"
    else
        fail A13 "the state machine defines every runner state" "undefined:$missing"
    fi

    # Retries must be bounded, and the bound must be a real number.
    if grep -qE 'max_retries:[ \t]*[0-9]+' "$RUNNER"; then
        pass A14 "the retry budget is bounded by an explicit number"
    else
        fail A14 "the retry budget is bounded by an explicit number" \
            "no 'max_retries: <n>' in $RUNNER -- an unbounded retry loop is the failure mode this policy exists to prevent"
    fi
fi

# ==========================================================================
# Persistent state
# ==========================================================================

if [ -f "$CURRENT" ] && grep -qE '^## Run State' "$CURRENT"; then
    pass A15 "current-task.md carries a Run State section"
else
    fail A15 "current-task.md carries a Run State section" \
        "without it an interrupted run cannot resume from disk, and the conversation becomes the source of truth"
fi

# Run State must record the attempt count -- the retry budget is meaningless
# if the count only exists in a chat window.
if [ -f "$CURRENT" ] && grep -qiE '^\| \*\*Attempt\*\*' "$CURRENT"; then
    pass A16 "Run State records the attempt count"
else
    fail A16 "Run State records the attempt count" \
        "no '| **Attempt** |' row in $CURRENT"
fi

if [ -f "$FAILLOG" ]; then
    id="$(grep -oE '^doc_id: [A-Za-z0-9-]+' "$FAILLOG" | awk '{print $2}' | head -1)"
    if [ -n "$id" ] && grep -q "| $id |" "$INDEX"; then
        pass A17 "the failure log exists and is registered as $id"
    else
        fail A17 "the failure log exists and is registered" "doc_id '$id' has no row in $INDEX"
    fi
else
    fail A17 "the failure log exists and is registered" "missing file: $FAILLOG"
fi

# ==========================================================================
# Entry points
# ==========================================================================

missing=""
for c in migration-run migration-status; do
    [ -f "$COMMANDS/$c.md" ] || missing="$missing $c"
done
if [ -z "$missing" ]; then
    pass A18 "both slash commands exist under $COMMANDS"
else
    fail A18 "both slash commands exist under $COMMANDS" "missing:$missing"
fi

# The commands and the orchestrator must defer to KB-091 rather than restating
# policy. A second copy of a retry count is a second copy that drifts.
bad=""
for f in "$COMMANDS/migration-run.md" "$AGENTS/migration-orchestrator.md"; do
    [ -f "$f" ] || continue
    grep -q 'autonomous-runner.md' "$f" || bad="$bad $f"
done
if [ -z "$bad" ]; then
    pass A19 "the entry points defer to KB-091 for policy"
else
    fail A19 "the entry points defer to KB-091 for policy" \
        "no reference to autonomous-runner.md in:$bad"
fi

# ==========================================================================
# Every path the system names must exist
# ==========================================================================

: > "$TMP/dangling"
for f in "$AGENTS"/*.md "$COMMANDS"/*.md "$RUNNER" "$FAILLOG"; do
    [ -f "$f" ] || continue
    while IFS= read -r p; do
        [ -n "$p" ] && printf '%s -> %s\n' "$f" "$p" >> "$TMP/dangling"
    done < <(check_kb_paths_exist "$f" ".")
done
if [ ! -s "$TMP/dangling" ]; then
    pass A20 "every docs/kb and tools path named by an agent or command exists"
else
    fail A20 "every docs/kb and tools path named by an agent or command exists" \
        "$(head -5 "$TMP/dangling" | tr '\n' ';')"
fi

# ==========================================================================
# Regression guard -- the completed work stays completed
# ==========================================================================

TRACKER="$EXEC/task-tracker.md"
bad=""
for t in M0-00 M0-01-01 M0-01-02; do
    if [ ! -f "$TRACKER" ]; then
        bad="$bad no-tracker"; break
    fi
    row="$(grep -E "^\| $t \|" "$TRACKER" | head -1)"
    if [ -z "$row" ]; then
        bad="$bad $t(no row)"
    elif ! echo "$row" | grep -q 'Completed'; then
        bad="$bad $t(not Completed)"
    fi
done
if [ -z "$bad" ]; then
    pass A21 "the three completed tasks are still recorded Completed"
else
    fail A21 "the three completed tasks are still recorded Completed" \
        "$bad -- the agent system must never re-open or restart finished work"
fi

# ==========================================================================
# The orchestration loop -- the mechanism, not the agent definitions
#
# Agent files are declarative: five of them create five workers and no loop.
# These checks assert that something actually DRIVES them, and that the driver
# has not drifted from the specification it implements. A workflow script has
# no filesystem access, so it cannot read KB-091 at runtime -- the routing
# table, retry budget and stop conditions exist as literals in the script and
# as prose in the document. That duplication is deliberate and is kept honest
# here rather than by trust.
# ==========================================================================

WORKFLOW=".claude/workflows/migration-runner.js"
STATE="$EXEC/runner-state.md"

if [ -f "$WORKFLOW" ]; then
    pass W01 "an orchestration loop exists ($WORKFLOW)"
else
    fail W01 "an orchestration loop exists" \
        "missing file: $WORKFLOW -- agent definitions alone select no task, route no model and retry nothing"
fi

if [ -f "$WORKFLOW" ]; then
    # A workflow must declare meta.name matching how it is invoked.
    if grep -qE "name:[ \t]*'migration-runner'" "$WORKFLOW"; then
        pass W02 "the workflow declares the name it is invoked by"
    else
        fail W02 "the workflow declares the name it is invoked by" \
            "no \"name: 'migration-runner'\" in meta -- Workflow({name:...}) would not resolve it"
    fi

    # The loop must delegate to the real agents, by the names that exist on disk.
    missing=""
    for a in $WORKERS; do
        grep -q "agentType: '$a'" "$WORKFLOW" || missing="$missing $a"
    done
    if [ -z "$missing" ]; then
        pass W03 "the loop dispatches every worker agent by agentType"
    else
        fail W03 "the loop dispatches every worker agent by agentType" \
            "not dispatched:$missing -- a worker the loop never calls is dead configuration"
    fi

    # Routing must be present for every role, and must use aliases only.
    missing=""
    for role in investigate implement validate diagnose; do
        grep -qE "^[ \t]*$role:[ \t]*\{" "$WORKFLOW" || missing="$missing $role"
    done
    if [ -z "$missing" ]; then
        pass W04 "the script routes a model for every role"
    else
        fail W04 "the script routes a model for every role" "no ROUTING entry for:$missing"
    fi

    # Every model literal in the script must be a supported alias.
    badmodels="$(grep -oE "(model|MODEL):[ \t]*'[a-z0-9.-]+'" "$WORKFLOW" \
                 | grep -oE "'[a-z0-9.-]+'" | tr -d "'" | sort -u \
                 | grep -vE '^(haiku|sonnet|opus|fable|inherit)$')"
    if [ -z "$badmodels" ]; then
        pass W05 "every model literal in the script is a supported alias"
    else
        fail W05 "every model literal in the script is a supported alias" \
            "unsupported: $(echo "$badmodels" | tr '\n' ' ')"
    fi

    # The retry cap must be a real number in the executable path, not just prose.
    if grep -qE 'maxRetries:[ \t]*[0-9]+' "$WORKFLOW"; then
        pass W06 "the retry budget is bounded in the executable path"
    else
        fail W06 "the retry budget is bounded in the executable path" \
            "no numeric maxRetries default in $WORKFLOW -- an unbounded loop is the failure mode this exists to prevent"
    fi

    # Stop conditions must be implemented, not merely documented in KB-091.
    missing=""
    for cond in stopReason budgetReserve safetyStop requiresHuman maxEscalations; do
        grep -q "$cond" "$WORKFLOW" || missing="$missing $cond"
    done
    if [ -z "$missing" ]; then
        pass W07 "the documented stop conditions are implemented in the loop"
    else
        fail W07 "the documented stop conditions are implemented in the loop" \
            "not implemented:$missing -- a stop condition that exists only in prose does not stop anything"
    fi

    # The loop must never be able to authorise merge/push/schema change by default.
    unsafe=""
    for flag in allowMerge allowPush allowSchemaChange allowDestructiveDb; do
        if grep -qE "$flag:[ \t]*true" "$WORKFLOW"; then unsafe="$unsafe $flag"; fi
    done
    if [ -z "$unsafe" ]; then
        pass W08 "no unsafe capability is enabled by default in the loop"
    else
        fail W08 "no unsafe capability is enabled by default in the loop" \
            "defaults to true:$unsafe -- these are lifted only by an explicit human instruction"
    fi
fi

# Persistent run state must exist, be registered, and record a stop reason.
if [ -f "$STATE" ]; then
    id="$(grep -oE '^doc_id: [A-Za-z0-9-]+' "$STATE" | awk '{print $2}' | head -1)"
    if [ -n "$id" ] && grep -q "| $id |" "$INDEX"; then
        pass W09 "runner state exists and is registered as $id"
    else
        fail W09 "runner state exists and is registered" "doc_id '$id' has no row in $INDEX"
    fi

    missing=""
    for field in Status "Stop reason" "Current task" Attempt; do
        grep -qF "$field" "$STATE" || missing="$missing '$field'"
    done
    if [ -z "$missing" ]; then
        pass W10 "runner state records status, stop reason, current task and attempt"
    else
        fail W10 "runner state records status, stop reason, current task and attempt" \
            "missing:$missing -- a run that stops without recording why costs the next session the diagnosis again"
    fi
else
    fail W09 "runner state exists and is registered" "missing file: $STATE"
fi

# Both entry points must exist: starting a run and stopping one safely.
missing=""
for c in migration-run migration-stop migration-status; do
    [ -f "$COMMANDS/$c.md" ] || missing="$missing $c"
done
if [ -z "$missing" ]; then
    pass W11 "start, stop and status entry points all exist"
else
    fail W11 "start, stop and status entry points all exist" "missing:$missing"
fi

# The start command must actually launch the workflow, not re-improvise the loop.
if [ -f "$COMMANDS/migration-run.md" ]; then
    if grep -q 'migration-runner' "$COMMANDS/migration-run.md"; then
        pass W12 "the start command launches the orchestration workflow"
    else
        fail W12 "the start command launches the orchestration workflow" \
            "migration-run.md never names migration-runner -- it would improvise the loop instead of running it"
    fi
fi

# ==========================================================================

echo ""
if [ "$fail_count" -eq 0 ]; then
    echo "check-agent-system.sh: OK -- $pass_count checks passed."
    exit 0
fi
echo "check-agent-system.sh: FAIL -- $fail_count failed, $pass_count passed." >&2
echo "The agent system decides which model does what, when to retry, and when to stop." >&2
echo "A failure here means an autonomous run will misroute, loop, or act unsafely." >&2
exit 1
