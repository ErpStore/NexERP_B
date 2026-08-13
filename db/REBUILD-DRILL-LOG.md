# Rebuild drill log — tenant database from source control alone (task M0-01-03)

**Status: SKELETON — not yet executed.** Every field below is `TBD` until a named person
runs `db/RUNBOOK-rebuild-tenant-database.md` end to end and fills this in. Do not report
task M0-01-03 Completed, and do not tick any G0 box anywhere, on the strength of this file
alone while it still reads `TBD`.

**Never paste a connection string, hostname, IP literal, or tenant name anywhere in this
file.** Identify the throwaway database by the name you chose in the runbook's §4, never by
its connection string — same rule as `db/stored-procedures/CAPTURE-STATUS.md`.

---

## 1. Preconditions (runbook §1)

| Item | Value |
|---|---|
| Operator (named person) | TBD |
| Date | TBD |
| SQL Server instance — version/edition (`SELECT @@VERSION;`) | TBD |
| Host OS this drill ran on | TBD |
| `dotnet --version` | TBD |
| `dotnet ef --version` (before/after install, if it had to be installed) | TBD |
| `SqlServer` PowerShell module version (`Get-Module -ListAvailable -Name SqlServer`) | TBD |
| Was this a fresh non-production instance, confirmed empty before starting? | TBD |

## 2. Install `dotnet-ef` (runbook §2)

| Field | Value |
|---|---|
| Was `dotnet-ef` already installed? | TBD |
| Version installed | TBD |
| Any version-mismatch issue against the 9.0.5 package references? | TBD |

## 3. Master database (runbook §3)

| Field | Value |
|---|---|
| Command run (redact the connection string) | TBD |
| Outcome | TBD |
| `Tenants` table present afterward, correct shape? | TBD |
| Wall-clock time | TBD |
| First-time failures and fixes (if any) | TBD |

## 4. `Tenants` row (runbook §4)

| Field | Value |
|---|---|
| Tenant `Name` chosen (never the connection string) | TBD |
| `Hostname` value used | TBD |
| Row inserted successfully? | TBD |
| First-time failures and fixes (if any) | TBD |

## 5. Tenant database + EF migrations (runbook §5)

| Field | Value |
|---|---|
| Exact command run (redact the connection string) | TBD |
| Outcome | TBD |
| Wall-clock time (218 migrations is expected to take a while — record the real number) | TBD |
| `SELECT COUNT(*) FROM sys.tables;` result | TBD |
| `SELECT COUNT(*) FROM Screens;` result (expect 152) | TBD |
| `Administrator` user present (`UserId = 1`)? | TBD |
| First-time failures and fixes (if any) | TBD |
| **Did this expose anything new about Q-02** (how migrations are rolled out per tenant in production)? Record it here AND propagate to `docs/kb/open-questions.md` if so. | TBD |

## 6. Stored-procedure deployment (runbook §6, `db/deploy-stored-procedures.ps1`)

| Field | Value |
|---|---|
| Command run (redact credentials) | TBD |
| Completeness check result (gaps reported, if any) | TBD |
| Applied / skipped / failed counts from the script's own summary | TBD |
| `SELECT COUNT(*) FROM sys.procedures WHERE name LIKE 'Sp[_]%';` result (expect 90, plus `Sp_Print_PurchaseOrder` = 91 total `Sp_*`-pattern procedures deployed by this script) | TBD |
| Wall-clock time | TBD |
| First-time failures and fixes (if any) — **name the exact file(s)** | TBD |
| **Did the drill reveal an ordering dependency** (contradicting the deferred-name-resolution assumption in the deploy script's header comment)? If yes, this is a required update to `db/deploy-stored-procedures.ps1` and to R-04 in `docs/kb/risks/technical-debt-register.md` — do not leave it only in this log. | TBD |

## 7. Smoke test (runbook §7)

| Field | Value |
|---|---|
| `V.SMART.Web` started successfully against the throwaway master DB? | TBD |
| Login method used (existing known password vs. hash reset — not the password itself) | TBD |
| Login succeeded? | TBD |
| List screen opened (which one) | TBD |
| Report run through `ReportExecutor` (`/StockLedger` / `Sp_StockLedger`) — succeeded, empty-but-no-error, or failed? | TBD |
| Sales Enquiry record created for the print test — succeeded? | TBD |
| Document printed through `ReportService.Generate_Report` (`Sp_Print_CompanyDetails` + `Sp_Print_EnquirySales`) — succeeded, or exact error? | TBD |
| **This is the Sp_Print_CompanyDetails proof — record the actual outcome, not an assumption** | TBD |

## 8. Build regression guard (runbook §8)

| Field | Value |
|---|---|
| `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` — errors / warnings | TBD |

---

## 9. Overall result

| Field | Value |
|---|---|
| Did every step above succeed (first attempt or after a recorded fix)? | TBD |
| **Is G0 exit criterion 1 met?** ("a fresh, empty SQL Server can be brought to a working tenant database from source control alone … and the app runs against it") | TBD — **this file records evidence; the gate itself is assessed at the milestone review (`docs/kb/execution/README.md` §22, KB-084), not ticked here.** |
| Open findings with named owners (if any step failed and was not fixed in this drill) | TBD |

## 10. Every failure encountered, in the order it happened

This section is the most valuable part of this document — a second operator needs it to
succeed faster than the first one did. One entry per failure, however small.

| # | Step | What failed (exact error/message) | Root cause (if known) | Fix applied | Where the fix landed (script / runbook / KB / still open) |
|---|---|---|---|---|---|
| TBD | | | | | |
