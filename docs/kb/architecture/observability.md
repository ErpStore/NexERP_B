---
doc_id: KB-113
title: Observability — Health Checks, Structured Logging and the Audit Trail
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Services/ILoggingService.cs
  - V.SMART/V.SMART.Shared/Services/StructuredLoggingService.cs
  - V.SMART/V.SMART.Shared/Services/SensitiveDataRedactor.cs
  - V.SMART/V.SMART.Shared/Services/FileLoggingService.cs
  - V.SMART/V.SMART.Api/HealthChecks/TenantDatabaseHealthCheck.cs
  - V.SMART/V.SMART.Api/HealthChecks/MasterDatabaseHealthCheck.cs
  - V.SMART/V.SMART.Api/HealthChecks/HealthResponseWriter.cs
  - V.SMART/V.SMART.Api/HealthChecks/HealthProbeOptions.cs
  - V.SMART/V.SMART.Api/Logging/TenantInfoDestructuringPolicy.cs
  - V.SMART/V.SMART.Api/Logging/AuditLoggingOptions.cs
  - V.SMART/V.SMART.Api/Program.cs
entities: [TenantInfo]
api_endpoints:
  - "GET /health/live"
  - "GET /health/ready"
database_tables: []
business_rules: []
status: active
confidence: confirmed
last_verified: 2026-08-21
dependencies: [KB-060, KB-041, KB-014, KB-011, KB-105]
---

# Observability — health checks, structured logging and the audit trail

Delivered by **M2-B11**. Closes **R-23** for `V.SMART.Api` and the health-check half of
**KB-041 item C2**.

> **Read §6 before changing any `ILoggingService` registration.** Which host resolves which
> implementation is a deliberate decision, not an oversight, and reversing it silently deletes
> a live audit trail.

---

## 1. What changed, in one table

| | Before (R-23) | After (this document) |
|---|---|---|
| Audit sink | `App_Data/Logs/UserLogs/{date}_User_{UserName}.txt`, one file **per user per day**, `\|`-delimited free text | `App_Data/Logs/audit-{date}.json`, compact JSON, one file per day, six named properties |
| Diagnostic sink | `App_Data/Logs/DeveloperLogs/{date}_ErrorLog.txt`, **one shared file per day** | `App_Data/Logs/diagnostics-{date}.json`, compact JSON |
| Retention | none — `File.AppendAllTextAsync`, no rotation, no cap | audit **3650 days**, diagnostics **14 days**, 64 MB size cap, both configurable |
| Errors | `"EXCEPTION: {msg}\nSTACKTRACE: {stack}"` — multi-line text in a line-oriented file | the `Exception` object, rendered by the sink |
| Correlation | none | the M2-A06 correlation id on every event in a request |
| Health | none | `GET /health/live`, `GET /health/ready` |
| Which hosts | all three | **`V.SMART.Api` only** — see §6 |

---

## 2. The health-check contract

```
GET /health/live    200  — the process is up. Runs NO check. Touches NO database.
GET /health/ready   200 / 503 — master DB reachable, AND the probed tenant DBs reachable.
```

Both live **outside `/api/v1`**: they are infrastructure, not part of the versioned API
contract, and an orchestrator should not have to track an API version to ask whether a pod is
alive. Both are **anonymous** — an orchestrator cannot present a JWT.

### Liveness runs no checks, and that is the design

`Program.cs` maps `/health/live` with `Predicate = _ => false`, so the health service executes
none of the registered checks. A liveness probe that touches the database gets the container
**killed** whenever the database is slow, turning a database incident into a cluster-wide crash
loop — and removing the very instances that would have recovered when the database came back.

### Readiness: master, plus a configurable subset of tenants

This is a database-per-tenant system (KB-014). A healthy master proves nothing about a tenant,
so `/health/ready` runs two checks and reports each individually:

| Check name | What it does |
|---|---|
| `master-db` | `MasterDbContext.Database.CanConnectAsync()` |
| `tenant-db` | reads `MasterDbContext.Tenants`, opens a `SqlConnection` per probed tenant **directly**, runs `SELECT 1` |

**The tenant check cannot use `ITenantProvider`, and this is not a shortcut.**
`TenantProvider.GetCurrentTenant()` resolves from
`_httpContextAccessor?.HttpContext?.User?.FindFirst("TenantId")`
(`V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantProvider.cs:33-34`), falling back
to the request host, and `TenantDbContextFactory` depends on it
(`TenantDbContextFactory.cs:7-12`). **A probe has no `HttpContext`, no JWT and no user.**
Reading `MasterDbContext.Tenants` (`MasterDbContext.cs:8`) and connecting directly is the only
correct shape. (Confirmed — the same note is added to KB-014.)

### Configuration — the `Health` section

| Key | Default | Meaning |
|---|---|---|
| `TenantProbeCount` | `1` | how many tenants to probe, **lowest `TenantInfo.Id` first** |
| `TenantHostnames` | `[]` | optionally pin the probe to named tenants instead |
| `ProbeTimeoutSeconds` | `5` | clamps `Connect Timeout` and the command timeout |
| `TenantFailureIsFatal` | `true` | an unreachable tenant ⇒ `Unhealthy` (503); `false` ⇒ `Degraded` (200) |
| `RequireAtLeastOneTenant` | `true` | a master with no tenant rows fails readiness |

**Why one tenant, not all of them.** A readiness probe runs on every orchestrator poll.
Probing N tenants turns one poll into N+1 connections, does not scale, and lets one sick tenant
take the whole service out of the load balancer. One tenant is enough to answer the question a
readiness probe is actually asking: *does the tenant path work at all?*

**Why one unreachable tenant makes the service unready by default — the decision, made
explicitly.** With the default probe set of one, an unreachable tenant is not "one customer is
down". It is the only evidence this instance has that its tenant-connection path works, and a
request routed here would fail. *Ready* means *send me traffic*, and the instance cannot
honestly say that. An operator running a large probe set, where one tenant genuinely is one
customer, should set `TenantFailureIsFatal: false`; the per-tenant detail names the failure
either way.

---

## 3. What the health endpoints deliberately do **not** disclose

Both endpoints are anonymous, so the response body is reachable by anyone who can reach the
port. `HealthResponseWriter` emits an **allow-list**, never a projection of `HealthReport`:

| Emitted | Never emitted |
|---|---|
| overall `status` | `entry.Description` |
| `totalDurationMs` | `entry.Exception` (type, message or stack trace) |
| per check: `name`, `status`, `durationMs` | `entry.Tags` |
| for `tenant-db` only: `detail` — keys of the form `tenant-{Id}`, values `"Healthy"`/`"Unhealthy"` | connection strings, server names, database names, tenant `Name`, tenant `Hostname` |

The tenant is named by its **opaque integer id**, which is enough for an operator to act on and
is not a customer name. `MasterDatabaseHealthCheck` and `TenantDatabaseHealthCheck` both
swallow their exceptions rather than surfacing them — the one place in this repository where
swallowing is correct, because the exception detail is exactly what must not cross the wire,
and because a probe running every few seconds against a down database would otherwise fill the
diagnostic sink with identical stack traces. **The status is the signal.**

Tested by `tests/V.SMART.Api.Tests/HealthChecks/HealthResponseWriterTests.cs`, which builds a
report deliberately stuffed with a connection string, a `SqlException`-shaped exception and
tags, and asserts none of it reaches the body.

### Observed responses (2026-08-21, `dotnet run`, local SQL Express)

```
GET /health/live,  master DB unreachable, no token  → 200
  {"status":"Healthy","totalDurationMs":1,"checks":[]}

GET /health/ready, master DB unreachable, no token  → 503
  {"status":"Unhealthy","totalDurationMs":2673,"checks":[
    {"name":"master-db","status":"Unhealthy","durationMs":2403},
    {"name":"tenant-db","status":"Unhealthy","durationMs":2456,
     "detail":{"tenant-source":"unavailable"}}]}

GET /health/ready, real master + real tenant, no token → 200
  {"status":"Healthy","totalDurationMs":682,"checks":[
    {"name":"master-db","status":"Healthy","durationMs":493},
    {"name":"tenant-db","status":"Healthy","durationMs":585,
     "detail":{"tenant-1":"Healthy"}}]}
```

### One recorded risk

If a deployment cannot restrict `/health/*` at the network layer, the endpoints remain an
unauthenticated surface disclosing *that* a database is down and *how many* tenants are
probed. That is accepted: the alternative — an authenticated probe — cannot be used by an
orchestrator. Restrict at the ingress.

---

## 4. The sink decision is **deferred**, and these are the criteria

**Q-16 (deployment topology) is unanswered.** Choosing a log aggregation platform without
knowing where this runs, what already collects logs there, and who pays for retention would be
a guess. Per M2-B11's own instruction the deferral is recorded rather than resolved.

**What ships instead:** Serilog writing **compact JSON** to two rolling files, plus
`ReadFrom.Configuration(...)` reading the `Serilog` section of `appsettings.json`, so a
deployment adds a second sink — Seq, Elasticsearch, an OTLP collector, a file share — with a
configuration change and no rebuild. Structured JSON in a file is already a large improvement
on `|`-delimited free text.

**A sink must meet all five before it is chosen:**

1. **Audit retention** measured in years, not days, and configurable **independently** of
   diagnostic retention. An ERP audit trail is a business record, not a rolling buffer.
2. **Queryable by `UserName`, by `Screen` and by date range** — the three predicates of §5.
3. **Survives container restart**, i.e. not backed by ephemeral container storage.
4. **The audit stream is separable** from diagnostics — a distinct index, stream or sink — so
   audit retention is not silently governed by the diagnostic budget.
5. **Cost at the measured event volume**, which nobody has measured yet.

---

## 5. The audit-event schema

`ILoggingService.LogUserAction` is **not diagnostics**. It is a business record of who did what,
on which screen, from which machine and IP. `StructuredLoggingService` emits it as named
properties:

| `LogUserAction` argument | Structured property |
|---|---|
| `UserName` | `UserName` |
| `Machine` | `Machine` |
| `IP_Address` | `IpAddress` |
| `screen` | `Screen` |
| `action` | `Action` |
| `additionalInfo` | `AdditionalInfo` (passed through `SensitiveDataRedactor`) |
| — | `EventType` = `"UserAction"` — the discriminator |
| — | `CorrelationId` — the M2-A06 join key |
| — | `@t` — the timestamp, supplied by Serilog |

That is the entire point of the migration: `Screen = "Sales Order"` is a query;
`"| Screen: Sales Order |"` was a substring search.

### Separability and retention

`Program.cs` splits the pipeline into two sub-loggers filtered on `EventType`:

| Stream | File | Retention | Filter |
|---|---|---|---|
| Audit | `audit-{date}.json` | `Observability:Logging:AuditRetentionDays`, **default 3650** (10 years) | `EventType == "UserAction"` |
| Diagnostics | `diagnostics-{date}.json` | `Observability:Logging:DiagnosticRetentionDays`, **default 14** | everything else |

**Precision, because the option names say "days" and the sink does not.** Both values are
passed to Serilog's `retainedFileCountLimit` (`V.SMART/V.SMART.Api/Program.cs:88`, `:100`),
which is a **retained file count**, not a span of days. With one file per day the two
coincide, which is the intent — but `rollOnFileSizeLimit: true` is also set, so a day whose
volume exceeds the 64 MB cap produces more than one file and the *effective* retention span
falls below the nominal number. Sizing a deployment on the ten-year figure must account for
this, or raise `FileSizeLimitBytes`.

**Startup fails if `AuditRetentionDays <= DiagnosticRetentionDays`.** R-23 requires audit
retention to be independent of and longer than diagnostic retention, and configuration can
express the opposite; refusing at startup is cheaper than discovering it when someone asks what
a user did last March.

### The three queries, and how to run them

```bash
# by user
jq -c 'select(.UserName == "vivek")'                 App_Data/Logs/audit-*.json
# by screen
jq -c 'select(.Screen == "Sales Order")'             App_Data/Logs/audit-*.json
# by date range — the daily roll makes this a filename glob
jq -c .                                              App_Data/Logs/audit-202603*.json
# all three
jq -c 'select(.UserName=="vivek" and .Screen=="Sales Order")' App_Data/Logs/audit-202603*.json
```

Asserted in code by
`tests/V.SMART.Api.Tests/Logging/StructuredLoggingServiceTests.cs` →
`LogUserAction_is_queryable_by_user_by_screen_and_by_date_range`.

### `LogDeveloperError` logs the exception, not a string

`FileLoggingService.cs:60` built `"EXCEPTION: {ex.Message}\nSTACKTRACE: {ex.StackTrace}"` — a
multi-line entry in a line-oriented file, which is one of the reasons the old format is
unparseable. The exception now travels as an `Exception`; every sink renders its type, message
and stack trace better than a string does.

---

## 6. Which host resolves which implementation — the decision

| Host | `ILoggingService` | Registered at |
|---|---|---|
| `V.SMART.Api` | **`StructuredLoggingService`** | `V.SMART/V.SMART.Api/Program.cs`, after `AddVSmartDomain()` — last registration wins |
| `V.SMART.Web` (Blazor) | `FileLoggingService` — **unchanged** | `AddVSmartDomain()` (`ServiceCollectionExtensions.cs`) |
| `V.SMART` (MAUI) | `FileLoggingService` — **unchanged** | `AddVSmartDomain()` |

**Why the registration was not simply changed inside `AddVSmartDomain()`,** which is where
M2-B11's task file points by default: that extension is the composition root for **all three**
hosts. Neither the Blazor host nor the MAUI head configures a Serilog sink. Changing it there
would route a live audit trail — **494 `LogUserAction` call sites across 202 files** — into
whatever `ILogger` providers those hosts happen to have, which is a console and a debug window.
That **deletes** the audit trail instead of restructuring it, and R-23's action item is
explicitly *preserve* it. The Blazor host therefore keeps writing
`App_Data/Logs/UserLogs/*.txt` exactly as before and remains the durable audit record for its
own traffic until it is retired.

**Consequence, stated plainly: R-23 is closed for `V.SMART.Api` and remains open for the other
two hosts.** KB-060 records it that way. Closing it for the Blazor host means giving that host a
Serilog sink and is a separate task.

### The `IHttpContextAccessor` question, resolved

`StructuredLoggingService` takes `IHttpContextAccessor?` as an **optional** constructor
argument — the same pattern already used by `TenantProvider.cs:18`. The MAUI host has no
`IHttpContextAccessor` at all, so a required dependency would fail at resolution time. Optional
means the class is safe to register in the MAUI host the day that host gains a durable sink;
with no accessor the correlation id falls back to `Activity.Current?.Id` alone. Asserted by
`It_works_with_no_IHttpContextAccessor_at_all_which_is_the_MAUI_case`.

### Correlation, and one accepted duplication

The correlation id is `Activity.Current?.Id ?? HttpContext.TraceIdentifier` — the M2-A06
definition at `V.SMART/V.SMART.Api/Middleware/CorrelationId.cs:41`. `Program.cs` pushes it into
Serilog's `LogContext` for the whole request, so every event emitted while handling a request
carries the same `CorrelationId`, including `UseSerilogRequestLogging`'s summary. An inbound
`X-Correlation-Id` is still ignored (Q-35).

`StructuredLoggingService` **restates** that expression rather than calling `CorrelationId.For`,
because `CorrelationId` lives in `V.SMART.Api` and `V.SMART.Shared` cannot reference it without
inverting the project dependency. **This is a drift risk and is recorded as one** — if M2-A06's
definition ever changes, two places must change. The cheap fix, if it ever matters, is to move
the definition into `V.SMART.Shared` and have `CorrelationId.For` delegate to it; that was out
of M2-B11's scope.

Observed 2026-08-21: `X-Correlation-Id: 00-c4cf1e18…-74955d5b…-00` on the HTTP response and
`"CorrelationId":"00-c4cf1e18…-74955d5b…-00"` on the corresponding `diagnostics-*.json` line —
the request and its log line join.

---

## 7. Credential redaction — two layers

Structured logging **serialises objects**. `TenantInfo` is
`{ Id, Name, Hostname, ConnectionString }` (`TenantInfo.cs:5-8`), one field of which is a live
credential-bearing connection string. A single `Log.Information("{@Tenant}", tenant)` anywhere
in this codebase, now or in five years, would publish database credentials into a searchable,
retained, possibly third-party sink — **strictly worse than the flat files R-23 replaces**,
which never held it.

| Layer | Where | Covers |
|---|---|---|
| `TenantInfoDestructuringPolicy` | `V.SMART/V.SMART.Api/Logging/`, installed by `Program.cs` | any `TenantInfo` reaching any sink — replaced by `{ TenantId, Hostname }`; `Name` and `ConnectionString` are never emitted |
| `SensitiveDataRedactor` | `V.SMART/V.SMART.Shared/Services/`, applied to `additionalInfo` and diagnostic messages | free text no destructuring policy inspects — `password`/`pwd`/`user id`/`uid`/`token`/`secret`/`apikey` and the locator keywords `server`/`data source`/`initial catalog`/`database`/`address` |

The redactor is **pattern-based and deliberately conservative**. It cannot prove a string is
credential-free; it removes the shapes that actually occur. It is defence in depth, never a
reason it is safe to hand a connection string to a logger.

Proven by `tests/V.SMART.Api.Tests/Logging/TenantCredentialRedactionTests.cs` (7 cases) and, as
a live check on 2026-08-21, by grepping the diagnostics file produced by a run whose master
**and** tenant connections both failed — the leak path exercised — for `NotReal123`,
`Password`, `TenantInfo`, `SQLEXPRESS` and `NexGenErpDb`: **0 hits each**.

---

## 8. The historical flat files — where they are, and why they stay

**Nothing is deleted and nothing is migrated.** The pre-M2-B11 files are the only record of
what happened before this task:

| Stream | Path, relative to the host's `AppContext.BaseDirectory` |
|---|---|
| User actions | `App_Data/Logs/UserLogs/{yyyy-MM-dd}_User_{UserName}.txt` |
| Developer logs | `App_Data/Logs/DeveloperLogs/{yyyy-MM-dd}_ErrorLog.txt` |
| Logging's own failures | `%TEMP%/LoggingFailure.txt` |

`AppContext.BaseDirectory` is the **application directory** — typically
`…/V.SMART.Web/bin/{config}/net9.0/App_Data/Logs/`. The Blazor host keeps writing there. To
read the historical audit trail, grep those files; the format is documented in `FileLoggingService.cs:33-34`.

**`FileLoggingService` is deprecated, not deleted.** It is the fallback, it is still the Blazor
and MAUI registration, and it is the only reference for the historical format.

### Where the new files go — and the one R-23 impact this task does not fix

`Observability:Logging:Directory`, defaulting to `{ContentRoot}/App_Data/Logs`. That default
reproduces the old location, which is honest but **not adequate**: "lost on container restart"
is one of R-23's four recorded impacts and it is the one that cannot be fixed in code, because
it is a deployment property. **Set `Observability:Logging:Directory` to a mounted volume in any
containerised deployment**, or the ten-year audit retention configured above is fiction.

---

## 9. Related

- **KB-060 R-23** — the risk, its four impacts and the three corrections M2-B11 recorded.
- **KB-041 item C2** — health checks + structured logging sink.
- **KB-014** — database-per-tenant, and why one health check is not enough.
- **KB-011** — backend architecture, logging section.
- **KB-105 §7.1** — the correlation-id definition M2-A06 fixed.
- **INV-046** — the audit-coverage map and the `#if WINDOWS` finding
  (`docs/kb/investigation-registry.md`).
