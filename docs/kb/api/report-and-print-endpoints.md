---
doc_id: KB-110
title: Report and Print Endpoint Contract
module: api
source_files:
  - V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs
  - V.SMART/V.SMART.Api/Reporting/ReportRegistry.cs
  - V.SMART/V.SMART.Api/Reporting/PrintRegistry.cs
  - V.SMART/V.SMART.Api/Reporting/ApiPathProvider.cs
entities: [Screens]
api_endpoints:
  - "GET /api/v1/{resource}/{id:int}/print"
  - "GET /api/v1/reports"
  - "GET /api/v1/reports/{slug}"
database_tables: []
business_rules: []
status: active
confidence: confirmed
last_verified: 2026-08-27
dependencies: [ADR-005, ADR-002, ADR-004, KB-041, KB-060, KB-114]
---

# Report and Print Endpoint Contract (M2-B08)

Implements [ADR-005](../decisions/ADR-005-reporting-and-printing.md): exposes the two existing
server-side reporting engines — FastReport (`ReportService`) and raw-SQL analytical reports
(`ReportExecutor`) — over HTTP, unchanged. Neither engine, no `.frx` template and no stored
procedure was modified by this task.

## Print map

Every entry below is transcribed from a real Blazor call site, not invented — see `INV-060`
for the full sampling method and the two dead-reference findings it surfaced. **Seeded with 3
of the task's allowed "at most 5"** — see *Scope note* below.

| Resource | Route | Screen | Template | SQL parameter | Procedure | Generator | Source |
|---|---|---|---|---|---|---|---|
| `purchase-pos` | `GET /api/v1/purchase-pos/{id:int}/print` | `Purchase Order` | `PurchaseOrder.frx` | `PoId` | `Sp_Print_PurchasePo` | `Generate_Report` | `PurchasePoDetails.razor:306` |
| `job-orders` | `GET /api/v1/job-orders/{id:int}/print` | `Job Order` | `JobOrder.frx` | `JobId` | `Sp_Print_JobOrder` | `Generate_Report` | `JobOrderDetails.razor:290` |
| `salary-slips` | `GET /api/v1/salary-slips/{id:int}/print` | `Salary` | `SalarySlip.frx` | `RowId` | `Sp_Print_Salary` | `GenerateSalarySlipReport` | `SalaryDetails.razor:310` |

`Sp_Print_CompanyDetails` is a hard dependency of every one of the three above (and of every
print path in the product) — `ReportService.cs:74-77`. Confirmed present and executable in the
local tenant database (`INV-060`).

## Report map

| Slug | Route | Screen | Procedure | Parameters | Result type | Source |
|---|---|---|---|---|---|---|
| `hsn-summary` | `GET /api/v1/reports/hsn-summary` | `HSNSummary Report` | `Sp_GetHSNSummaryReport` | `reportType` (string, required), `fromDate`, `toDate` | `HSNSummaryVM` | `HSNSummaryService.cs:77` |
| `sales-track` | `GET /api/v1/reports/sales-track` | `Sales Track Report` | `sp_Sales_Track` | `fromDate`, `toDate`, `customerId` | `SalesTrackVM` | `SalesTrackReportService.cs:67` |
| `vendor-pr-rating` | `GET /api/v1/reports/vendor-pr-rating` | `PR PO Rating Report` | `Sp_VendorPRRating` | `fromDate`, `toDate`, `vendorCode` | `PrPoratingVM` | `PrPoRatingService.cs:106` |

`GET /api/v1/reports` is the catalogue — one row per entry above, machine-readable, with each
row's `screenName` and parameter shapes. It executes no procedure itself (`[NoScreenRight]`,
justified inline and in `ExemptEndpointAllowList`).

**Paging is in-memory for every entry, not server-side, and this is stated in the catalogue
response itself (`Paging: "in-memory"`).** None of the three underlying procedures accept
`@Skip`/`@Take` — `ReportExecutor.cs:48-86` is a commented-out attempt at exactly that,
abandoned before this task and not revived by it (adding those parameters to a procedure is a
schema change, out of scope). `ExecuteAsync<T>` returns the full result set; the controller
applies `Skip`/`Take` to the in-memory list before returning `PagedResult<object>`.

## Template-coverage matrix

Sampled across the print map's 13 call sites (10 print-map + 2 wrapper-resolved + 1 salary
slip — see `INV-060`): **11 of 13** `.frx` names exist under `default` (with 0–4 tenant-folder
overrides layered on top; none of the sampled names exist *only* in a tenant folder, so no
"tenant-only, no default fallback" case was found in this sample — see caveat below). **2 are
broken references, not registered here:**

| `.frx` | Referenced from | Exists in `default`? | Exists in any tenant folder? |
|---|---|---|---|
| `PurchaseInvoice.frx` | `PurchaseInvoiceDetails.razor:450` | No | No |
| `Estimation.frx` | `EstimationDetails.razor:436` | No | No |

Both would throw `InvalidOperationException` at `ReportService.cs:57` today, in the live
Blazor app, independently of this task — pre-existing defects, not introduced or fixed here.
Flagged for a human decision (fix the missing template, or find the correct one) rather than
silently worked around by pointing the registry at a different file.

**Caveat on completeness:** this matrix covers the 13 sampled call sites, not all ~79 real
`Generate_Report`/`GenerateSalarySlipReport` callers. A tenant-only template with no `default`
fallback may exist among the ~66 unsampled sites; this document does not claim otherwise.

## Scope note — 3 of the allowed 5 entries per registry

The task allows seeding "at most 5" print entries and 5 report entries — a ceiling, not a
target. This close-out seeded 3 of each, chosen to prove every structural branch a reviewer
would need to see:

- **Both `PrintGenerator` members** — `purchase-pos`/`job-orders` use `Generate_Report`;
  `salary-slips` uses `GenerateSalarySlipReport`, the second, less common entry point.
- **Three genuinely different report parameter shapes** — one required string plus two dates
  (`hsn-summary`); two dates plus a customer id (`sales-track`); two dates plus a vendor code
  (`vendor-pr-rating`).
- **The tenant/default template fallback mechanism** — proven once, structurally, by
  `ApiPathProvider` and exercised by every print entry identically; a 4th or 5th entry would
  not exercise a new branch of that mechanism.

Filling in the remaining ~76 print call sites and ~37 report call sites is module-wave work
(M3/M4), not M2 — the same boundary [M2-B08's own task file](../execution/tasks/M2-B08.md)
draws for the full 94/42-procedure surface.

## Request-level timeout, measured

Not independently re-measured against a live Kestrel instance under load in this close-out —
the service-level timeout (`ReportExecutor.cs:25`, `SetCommandTimeout(300)`) is unchanged and
was directly observed via `sqlcmd` to complete in well under a second for all three seeded
report procedures against the local tenant database (empty/near-empty dev data). Whether
Kestrel's own default request timeout or a reverse-proxy timeout would cut off a genuinely
slow 300-second report first is **not measured here** — recorded as an open item for whoever
next runs a report against a production-scale dataset, not assumed either way.

## What this task did not do

- Did not touch `ReportService.cs`, `ReportExecutor.cs`, any `.frx` file, or any stored
  procedure.
- Did not fix the two broken template references found (`PurchaseInvoice.frx`,
  `Estimation.frx`) — flagged above for a human decision.
- Did not register `LabourPendingService.cs`'s `Sp_LabourPendingReport` (one procedure, two
  result types depending on an `IsDetails` flag) — noted in `INV-060` for whoever extends the
  registry next.
- Did not obtain a live login token to exercise the full 200-success path over real HTTP with
  real report data — the local environment's only account is the seeded `Administrator`
  (`UserId = 1`), whose password hash this session declined to alter for testing purposes (a
  DB-credential mutation, correctly refused by this session's own permission boundary). What
  *was* verified live: clean host startup with no DI or `ScreenRightStartupValidator` failure,
  `401` for every seeded endpoint called with no token, and all 7 referenced stored procedures
  executing without error against the real local tenant database via direct SQL. The
  authorization *mechanism* itself (403-without-rights, 200-with-rights, per action) is
  verified by the existing `PermissionMatrix` fixture-based harness, which discovered and
  exercised all 6 newly gated actions automatically (106 → 154 harness tests) — the established
  methodology this codebase already uses in place of live-login testing (`INV-049`).
