---
doc_id: KB-102
title: Stored-Procedure Inventory — Reference vs. Scripted DDL Reconciliation
module: architecture
source_files:
  - Existing Store Procedures/StoredProcedures/
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ReportExecutor.cs
  - V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs
  - db/stored-procedures/manifest.csv
  - db/tools/sp-inventory.sh
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-13
dependencies: [KB-011, KB-012, KB-060, ADR-005]
---

# Stored-Procedure Inventory — Reference vs. Scripted DDL Reconciliation

**Produced by task M0-01-01.** This document, `db/stored-procedures/manifest.csv`,
`db/stored-procedures/README.md` and `db/tools/sp-inventory.sh` are M0-01-01's four
deliverables. This task requires **no database access** — everything in it is derived from
the repository as it stands on 2026-08-13.

## doc_id note

The task's own execution prompt names `KB-085` for this document. That id is **already
allocated** — to `docs/kb/execution/M0-00-baseline-decisions.md`
(`docs/kb/INDEX.md:61`, `docs/kb/investigation-registry.md`) — so using it here would
collide with an existing document. Per `docs/kb/INDEX.md`'s own allocation table
(`docs/kb/INDEX.md:63`), the next free id in the `KB-100+` artefact range is **KB-102**,
which is what this document claims. `docs/kb/INDEX.md` is updated accordingly. Recorded as
a deviation in the task's final report, not corrected silently.

## Methodology

Two starting counts were re-derived in this session rather than trusted from any prior
document (both **Confirmed**, 2026-08-13):

```bash
grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor \
  --exclude-dir=obj --exclude-dir=bin V.SMART | sort -u | wc -l
# -> 94
```

```bash
ls "Existing Store Procedures/StoredProcedures/" | wc -l
# -> 13
```

Both matched the expected values, so no repository drift was found and the task proceeded.

The **unscoped** form of the first command,
`grep -rhoE "Sp_[A-Za-z0-9_]+" | sort -u` (recorded by INV-009), also matches procedure
names quoted inside the 13 `.sql` files and inside `docs/kb/` itself — including this
document's own prose. It is a superset, not the referenced set, and must not be used to
"correct" 94 upward. **Confirmed** by re-running it and observing the extra matches
disappear once the `--include`/`--exclude-dir` scoping and the `V.SMART` root are applied.

**Reference side.** `db/tools/sp-inventory.sh` (created by this task) re-runs the scoped
grep, resolves for every match whether the containing line is commented out (a first-non-
whitespace-character heuristic — see the script's header comment for its exact, stated
limitations), and emits `procedure_name<TAB>path:line<TAB>commented(yes|no)`, sorted. Every
one of the 94 names has at least one **live** (non-commented) reference; none is
commented-out-only. Two lines are genuinely commented out —
`Sp_Print_PurchasePo` at
`V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseQuote_Pages/PurchaseQuoteDetails.razor:463`
and `Sp_Print_Salary` at
`V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/Payroll_Pages/SalaryDetails.razor:309`
— both verified by reading the surrounding lines; both procedures have other, live
references, so neither is a "references are all commented out" candidate.

**Scripted side.** The declared name in each of the 13 files was read from its
`CREATE`/`ALTER PROCEDURE` statement, never from the file name:

```bash
grep -rniE "(CREATE|ALTER)[[:space:]]+(OR[[:space:]]+ALTER[[:space:]]+)?PROC(EDURE)?[[:space:]]+[^[:space:]]+" \
  "Existing Store Procedures/StoredProcedures/"
```

| File | Declared name | Bracketed? | Casing of `CREATE`/`ALTER` | BOM? |
|---|---|---|---|---|
| `Sp_InvDetailsLabelPrint.sql` | `Sp_InvDetailsLabelPrint` | `[dbo].[…]` | `CREATE OR ALTER procedure` | yes |
| `Sp_Print_CompanyDetails.sql` | `Sp_Print_CompanyDetails` | bare | `CREATE OR ALTER PROCEDURE` | yes |
| `Sp_Print_GRNDetailsLabelPrint.sql` | `Sp_Print_GRNDetailsLabelPrint` | bare | `CREATE OR ALTER PROCEDURE` | no |
| `Sp_Print_LabourDC.sql` | `Sp_Print_LabourDC` | bare | `CREATE OR ALTER Procedure` | yes |
| `Sp_Print_LabourGRN.sql` | `Sp_Print_LabourGRN` | bare | `CREATE OR ALTER Procedure` | yes |
| `Sp_Print_LabourInv.sql` | `Sp_Print_LabourInv` | bare | `CREATE OR ALTER Procedure` | yes |
| `Sp_Print_LabourSCN.sql` | `Sp_Print_LabourSCN` | bare | `CREATE OR ALTER Procedure` | yes |
| `Sp_Print_MFGDC.sql` | `Sp_Print_MFGDC` | `[dbo].[…]` | `Create Or ALTER Procedure` | no |
| `Sp_Print_MfgInv.sql` | `Sp_Print_MfgInv` | `[dbo].[…]` | `Create Or ALTER Procedure` | no |
| `Sp_Print_MfgQuote.sql` | `Sp_Print_MfgQuote` | `[dbo].[…]` | `Create Or ALTER Procedure` | no |
| `Sp_Print_PerformaInvoice.sql` | `Sp_Print_PerformaInvoice` | `[dbo].[…]` | `Create Or ALTER Procedure` | no |
| `Sp_Print_PurchaseOrder.sql` | `Sp_Print_PurchaseOrder` | `[dbo].[…]` | `Create or ALTER PROCEDURE` | no |
| `Sp_Print_SubConDcOut.sql` | `Sp_Print_SubConDcOut` | `[dbo].[…]` | `Create Or ALTER Procedure` | no |

Casing, bracketing and BOM presence are all mixed, exactly as expected going in — recorded
here for M0-01-03's deployment script, which must tolerate all three forms on read and
normalise on write.

**Classification.** Each of the 94 referenced names was compared against the 13 declared
names, case-sensitively first and then case-insensitively for the remainder:

```
comm -12 <(sort referenced_94.txt) <(sort declared_13.txt)          # exact matches -> scripted
comm -23 <(sort declared_13.txt) <(sort referenced_94.txt)          # declared, no exact match
  | while read d; do grep -ix "$d" referenced_94.txt; done          # case-insensitive retry
```

SQL Server's default collation is case-insensitive, so a name differing only in case
resolves to the same object at runtime. **This is Inferred, not Confirmed** — no live
tenant's actual collation has been observed in this session; M0-01-02 or M0-02 should
confirm it against a real server.

## Counts

| | Count |
|---|---|
| Referenced from `.cs`/`.razor` (scoped grep) | **94** |
| Declared in the 13 existing `.sql` files | **13** |
| `scripted` (exact match) | **11** |
| `case_mismatch` (differ only in case) | **1** |
| `missing` (referenced, no DDL anywhere) | **82** |
| `unreferenced` (declared, never referenced) | **1** |

Arithmetic, both directions, verified in this session:

```
scripted + case_mismatch + missing      = 11 + 1 + 82 = 94  == referenced count       ✓
scripted + case_mismatch + unreferenced = 11 + 1 +  1 = 13  == declared count          ✓
```

The corrected missing-count is **82**, not the naive `94 − 13 = 81`, because one of the 13
scripted files (`Sp_Print_PurchaseOrder.sql`) declares a name the application never calls,
while the name it *does* call for that same document (`Sp_Print_PurchasePo`) has no DDL at
all — see Findings below. `82` is `94 − 11 (scripted) − 1 (case_mismatch)`.

## Findings

> **Finding:** `Sp_Print_PurchaseOrder.sql` declares `[dbo].[Sp_Print_PurchaseOrder]`, but
> that name is referenced nowhere in `.cs`/`.razor` under `V.SMART/`. The application calls
> `Sp_Print_PurchasePo` instead, for which no DDL exists anywhere in the repository.
> **Evidence:** `Existing Store Procedures/StoredProcedures/Sp_Print_PurchaseOrder.sql:1`;
> `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConPO_Pages/PurchasePoDetails.razor:306`;
> `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConPO_Pages/PurchPOUpsert.razor:4596`;
> `V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/Authorization_Pages/Authorization.razor:723`;
> also referenced (commented out) at
> `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseQuote_Pages/PurchaseQuoteDetails.razor:463`.
> **Business rule:** n/a.
> **Confidence:** Confirmed.
> **Last verified:** 2026-08-13.
>
> This is the single highest-consequence finding in this reconciliation. `Sp_Print_PurchaseOrder`
> is dead DDL — a candidate for deletion, but that decision belongs to a human, not to this
> task. `Sp_Print_PurchasePo` is a live latent defect: the Purchase Order print screen throws
> the moment a user opens it in any environment rebuilt from source control alone, and would
> do so today against any tenant that happens to lack the procedure. Manifest rows:
> `Sp_Print_PurchaseOrder` (`unreferenced`), `Sp_Print_PurchasePo` (`missing`).

> **Finding:** `Sp_Print_MFGDC.sql` declares `[dbo].[Sp_Print_MFGDC]`; the application calls
> `Sp_Print_MfgDC` — identical except for case.
> **Evidence:** `Existing Store Procedures/StoredProcedures/Sp_Print_MFGDC.sql:1`;
> `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesDc_Pages/MfgDcDetails.razor:395`;
> `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesDc_Pages/MfgDcUpsert.razor:3351`
> (second live reference, confirmed by `db/tools/sp-inventory.sh`'s output for this name).
> **Business rule:** n/a.
> **Confidence:** Confirmed for the spelling mismatch; Inferred that it resolves correctly at
> runtime (depends on the live tenant's collation, not observed this session).
> **Last verified:** 2026-08-13.
>
> Manifest row: `Sp_Print_MfgDC` (`case_mismatch`, `scripted_file` points at
> `Sp_Print_MFGDC.sql`). M0-01-02 does not need to capture this name — it is already
> scripted, under a different case — but the mismatch itself should be flagged to a human;
> silently "fixing" the case in either the C# call site or the `.sql` file is out of scope
> for this task and is not this task's decision to make.

> **Finding:** `Sp_Print_CompanyDetails` — one of the 11 exact `scripted` matches — is a hard
> dependency of **every** printed FastReport document, not one report among many.
> **Evidence:** `V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs:74-77`
> (`Generate_Report` throws before resolving the document's own procedure if this data
> source is absent), repeated in `GenerateSalarySlipReport` (`ReportService.cs:200-202`) and
> `Generate_Attendance_Report` (`ReportService.cs:328-331`).
> **Business rule:** n/a.
> **Confidence:** Confirmed.
> **Last verified:** 2026-08-13.
>
> Losing this one procedure breaks all printing at once. It is already scripted
> (`Existing Store Procedures/StoredProcedures/Sp_Print_CompanyDetails.sql`), so no capture
> action is needed, but its criticality should weigh on any future change to that file.

> **Finding (negative result):** no evidence of a run-time-composed procedure name (string
> concatenation or interpolation building an `Sp_*` identifier at call time) anywhere the
> scoped grep can see.
> **Evidence:**
> `grep -rnE '\$"Sp_|"Sp_"\s*\+|\+\s*"Sp_'` and
> `grep -rnE '\$"Sp_[A-Za-z0-9_]*\{'`, both run against `--include=*.cs --include=*.razor
> --exclude-dir=obj --exclude-dir=bin V.SMART` — both return no matches. Every call site that
> passes a `procedureName` argument to `ReportExecutor` or `ReportService` passes a literal
> string, which is exactly what the scoped `Sp_[A-Za-z0-9_]+` grep captures.
> **Business rule:** n/a.
> **Confidence:** Confirmed (for what the grep can see — see *Method limitations* below).
> **Last verified:** 2026-08-13.

> **Finding (negative result):** no procedure name is referenced only from commented-out
> code.
> **Evidence:** `db/tools/sp-inventory.sh`'s output covers all 94 names; every name has at
> least one line flagged `commented=no`. The two lines that are flagged `commented=yes`
> (`Sp_Print_PurchasePo` at `PurchaseQuoteDetails.razor:463`; `Sp_Print_Salary` at
> `SalaryDetails.razor:309`) each belong to a name with other live references.
> **Confidence:** Confirmed, subject to the heuristic's stated limitations (see below).
> **Last verified:** 2026-08-13.

## What this method cannot see

Stated explicitly, per the task's own requirement, not assumed away:

- **FastReport `.frx` data-source bindings.** The 104 templates under
  `V.SMART/V.SMART.Shared/wwwroot/templates/` bind data sources by procedure name inside the
  `.frx` XML, not inside `.cs`/`.razor`. A grep over C#/Razor cannot see those bindings, so
  `Sp_Print_PurchaseOrder`'s `unreferenced` status means "not called from application code" —
  it does **not** rule out a live `.frx` template still binding to it. Confirming that is out
  of scope for this task (it would require opening or parsing 104 template files) and is
  explicitly **not** concluded here as "dead code."
- **Procedure-to-procedure calls.** If any of the 94 (or any undeclared name) is invoked from
  inside another stored procedure's body via `EXEC`, this method — which only scans `.cs`
  and `.razor` — cannot detect it. None of the 13 existing `.sql` files was found to `EXEC`
  another procedure (spot-checked while reading each file's DDL for its declared name), but
  the 82 `missing` procedures' bodies do not exist in this repository to check.
- **Dynamically composed names.** Covered above as a checked negative result for the
  application layer. Not checked, and not checkable from this repository, for the *inside*
  of any stored procedure — the same limitation as the previous point.
- **The comment-detection heuristic** in `db/tools/sp-inventory.sh` classifies a line by its
  first non-whitespace characters. It does not track multi-line block-comment state across
  lines, so a reference sitting alone on a line *inside* a `/* … */` or `@* … *@` block whose
  opener is on an earlier line would be misclassified as live. A manual scan of the two
  flagged `commented=yes` lines (above) confirmed both are correctly classified, but this is
  not a guarantee for every one of the 146 total occurrences — only that none of the 94 names
  depends on the classification to reach a `missing`/`scripted`/`case_mismatch`/`unreferenced`
  verdict (the verdict comes from comparing name sets, not from the commented flag).

## Full reconciliation table

95 rows: the 94 referenced names, plus the 1 declared-but-unreferenced name
(`Sp_Print_PurchaseOrder`). Machine-readable form, with `reference_count` and
`live_reference_count`, is `db/stored-procedures/manifest.csv`.

| procedure_name | status | scripted_file | first_reference |
|---|---|---|---|
| `Sp_AttendanceReport` | missing | — | `V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/Attendance_Pages/AttendanceDetails.razor:282` |
| `Sp_BomAnalysis` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/JobOrderAnalysisService.cs:48` |
| `Sp_CreditDebitNoteSummaryReport` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/AccountsService/CreditDebitSummaryService.cs:77` |
| `Sp_GetCreditNoteList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/CreditNoteService.cs:1409` |
| `Sp_GetDebitNoteList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/DebitNote_Service/DebitNoteService.cs:1248` |
| `Sp_GetExportInvoiceStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/ExpInvService.cs:1662` |
| `Sp_GetHSNSummaryReport` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/AccountsService/HSNSummaryService.cs:77` |
| `Sp_GetItemModificationReport` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/ItemModificationReportServices.cs:20` |
| `Sp_GetJobOrderAssemblySubAssemblyList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/PlanningService/JobOrderService.cs:1337` |
| `Sp_GetLabourDcInOutTrack` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ViewTallyDCInOutTrackService.cs:184` |
| `Sp_GetLabourDcOutgoingStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourDcOutgoingService.cs:5929` |
| `Sp_GetLabourGRNStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourGRNService.cs:1879` |
| `Sp_GetLabourInvStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourInvoiceService.cs:1541` |
| `Sp_GetLabourSCNStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourSCNService.cs:1737` |
| `Sp_GetManufacturingInvoiceStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgInvService.cs:2029` |
| `Sp_GetMaterialReq` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/MaterialRequisitionService/MaterialReqService.cs:1731` |
| `Sp_GetMfgPOPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/PoPendingServices.cs:117` |
| `Sp_GetMfgPosPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs:1830` |
| `Sp_GetProductionIssueAssyStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionIssueAssyService.cs:1675` |
| `Sp_GetProductionIssueCompStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionIssueCompService.cs:1991` |
| `Sp_GetProductionReturnAssyStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionReturnAssyService.cs:2287` |
| `Sp_GetProductionReturnCompStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionReturnCompService.cs:3599` |
| `Sp_GetProductionSCNAssyStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionSCNAssyService.cs:966` |
| `Sp_GetProductionSCNCompStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionSCNCompService.cs:1378` |
| `Sp_GetPurchandSubQuotePendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs:1321` |
| `Sp_GetPurchaseGRNsPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs:1688` |
| `Sp_GetPurchaseInvoiceStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/Purchase_Invoice_Service/PurchaseInvoiceService.cs:1596` |
| `Sp_GetPurchasePosPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchOrSubConPoService/PuchPoService.cs:2520` |
| `Sp_GetPurchaseSCNsPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs:1808` |
| `Sp_GetPurchasesandSubcontractEnquiryPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs:1412` |
| `Sp_GetSalesandLabQuotePendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgQuotationService.cs:1542` |
| `Sp_GetSalesandlabourPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/EnquirySalesService.cs:985` |
| `Sp_GetSubContractDcGRNPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractGRNService/SubConGRNService.cs:5603` |
| `Sp_GetSubContractDcInOutTrack` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ViewTallyDCInOutTrackService.cs:213` |
| `Sp_GetSubContractDcoutPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:2931` |
| `Sp_GetSubContractInvoicePendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractInvoiceService/SubConInvService.cs:1276` |
| `Sp_GetSubContractScnPendingList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractSCNService/SubConSCNService.cs:1786` |
| `Sp_GetTaxDetailsReport` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TaxDetailsService/TaxDetailsService.cs:79` |
| `Sp_GetToolCribIssueNoteStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/ToolCribIssueService.cs:765` |
| `Sp_GetToolCribReturnNoteStatusList` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/ToolCribReturnService.cs:834` |
| `Sp_InvDetailsLabelPrint` | scripted | `Existing Store Procedures/StoredProcedures/Sp_InvDetailsLabelPrint.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourInvoice_Pages/LabourInvoiceDetails.razor:570` |
| `Sp_ItemWiseHistoryReport` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/History_Service/ItemWiseReportService.cs:34` |
| `Sp_JobOrderTrack` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/JobOrderAnalysisService.cs:77` |
| `Sp_LabourPendingReport` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/LabourPendingService.cs:58` |
| `Sp_Labour_Track` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/LabourTrackReportService.cs:257` |
| `Sp_Print_AppointmentLetter` | missing | — | `V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/AppointmentLetter_Pages/AppointmentLetterDetails.razor:214` |
| `Sp_Print_CREDITNOTE` | missing | — | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/CreditNote_Pages/CreditNoteDetails.razor:387` |
| `Sp_Print_CompanyDetails` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_CompanyDetails.sql` | `V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs:200` |
| `Sp_Print_DebitNote` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/DebitNote_pages/DebitNoteDetails.razor:388` |
| `Sp_Print_EnquiryFeasibility` | missing | — | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/Fesibility_Pages/EnquiryFeasibilityListDetails.razor:247` |
| `Sp_Print_EnquirySales` | missing | — | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesEnquiry_Pages/EnquiryDetails.razor:227` |
| `Sp_Print_Estimation` | missing | — | `V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/Estimation_Pages/EstimationDetails.razor:436` |
| `Sp_Print_GRNDetailsLabelPrint` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_GRNDetailsLabelPrint.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourSCN_Pages/LabourSCNDetails.razor:345` |
| `Sp_Print_InterStoreTransfer` | missing | — | `V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/StoreInterTrans_Pages/StoreInterTransDetails.razor:203` |
| `Sp_Print_JobOrder` | missing | — | `V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/JobOrder_Pages/JobOrderDetails.razor:290` |
| `Sp_Print_LabourDC` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_LabourDC.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourDcOut_Pages/LabourDcOutgoingDetails.razor:431` |
| `Sp_Print_LabourGRN` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_LabourGRN.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourGRN_Pages/LabourGRNDetails.razor:291` |
| `Sp_Print_LabourInv` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_LabourInv.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourInvoice_Pages/LabourInvoiceDetails.razor:538` |
| `Sp_Print_LabourSCN` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_LabourSCN.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourSCN_Pages/LabourSCNDetails.razor:314` |
| `Sp_Print_LeaveApplication` | missing | — | `V.SMART/V.SMART.Shared/Pages/Master_Module_pages/LeaveApplication_Pages/LeaveDetails.razor:181` |
| `Sp_Print_MaterialIssueNoteReduction` | missing | — | `V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/MaterialIssueNote_pages/MaterialIssueDetails.razor:283` |
| `Sp_Print_MaterialReq` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/MaterialRequisition_pages/MaterialReqDetails.razor:314` |
| `Sp_Print_MfgDC` | case_mismatch | `Existing Store Procedures/StoredProcedures/Sp_Print_MFGDC.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesDc_Pages/MfgDcDetails.razor:395` |
| `Sp_Print_MfgInv` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_MfgInv.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/MfgInv_Pages/MfgInvDetails.razor:558` |
| `Sp_Print_MfgQuote` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_MfgQuote.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/MfgQuote_Pages/MfgQuoteDetails.razor:493` |
| `Sp_Print_OfferLetter` | missing | — | `V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/OfferLetter_Pages/OffferLetterDetails.razor:214` |
| `Sp_Print_Payments` | missing | — | `V.SMART/V.SMART.Shared/Pages/CashFlow_Pages/Payments_Pages/PaymentDetails.razor:189` |
| `Sp_Print_PerformaInvoice` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_PerformaInvoice.sql` | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/PerformaInvoice_pages/PerformaDetails.razor:457` |
| `Sp_Print_ProdAssyGRN` | missing | — | `V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionReturnAssy_Pages/ProductionReturnAssyUpsert.razor:1660` |
| `Sp_Print_ProdAssySCN` | missing | — | `V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionAssySCN_Pages/ProductionAssySCNDetails.razor:290` |
| `Sp_Print_ProdCompSCN` | missing | — | `V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionSCNComp_Pages/ProductionSCNCompDetails.razor:253` |
| `Sp_Print_ProductionCompGRN` | missing | — | `V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionCompReturn_Pages/ProductionReturnCompDetails.razor:258` |
| `Sp_Print_ProductionIssueAss` | missing | — | `V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionIssueAssy_Pages/ProductionIssueAssyDetails.razor:290` |
| `Sp_Print_ProductionIssueComp` | missing | — | `V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionComp_Pages/ProductionIssueCompDetails.razor:276` |
| `Sp_Print_PurchaseEnquiry` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConEnquiry_Pages/EnquiryPurchaseUpsert.razor:2310` |
| `Sp_Print_PurchaseGRN` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseGRN_Pages/PurchaseGRNDetails.razor:277` |
| `Sp_Print_PurchaseInvoice` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseInvoice_Pages/PurchaseInvoiceDetails.razor:450` |
| `Sp_Print_PurchaseOrder` | **unreferenced** | `Existing Store Procedures/StoredProcedures/Sp_Print_PurchaseOrder.sql` | — (unreferenced; see Findings) |
| `Sp_Print_PurchasePo` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConPO_Pages/PurchPOUpsert.razor:4596` |
| `Sp_Print_PurchaseSCN` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseSCN_Pages/PurchaseSCNDetails.razor:316` |
| `Sp_Print_Receipts` | missing | — | `V.SMART/V.SMART.Shared/Pages/CashFlow_Pages/Receipt_Pages/ReceiptDetails.razor:189` |
| `Sp_Print_RouteCard` | missing | — | `V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/RouteCard_Pages/RouteCardDetails.razor:327` |
| `Sp_Print_Salary` | missing | — | `V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/Payroll_Pages/SalaryDetails.razor:310` |
| `Sp_Print_SingleProcessInspection` | missing | — | `V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourInvoice_Pages/LabourInvoiceDetails.razor:596` |
| `Sp_Print_SubConDcOut` | scripted | `Existing Store Procedures/StoredProcedures/Sp_Print_SubConDcOut.sql` | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/SubContractDCOut_Pages/SubContractDCOutDetails.razor:272` |
| `Sp_Print_SubconSCN` | missing | — | `V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/SubContractSCN_pages/SubConSCNDetails.razor:307` |
| `Sp_Print_ToolCribIssueNote` | missing | — | `V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/ToolCribIssue_Pages/ToolCribIssueDetails.razor:208` |
| `Sp_Print_ToolCribReturn` | missing | — | `V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/ToolCribReturn_Pages/ToolCribReturnDetails.razor:267` |
| `Sp_PurchaseSalesTrack` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/PurchaseSalesTrackReportServices.cs:48` |
| `Sp_RouteCardAnalysisDetails` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/RouteCardAnalysisService.cs:71` |
| `Sp_RouteCardAnalysisSummary` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/RouteCardAnalysisService.cs:55` |
| `Sp_StockAnalysis` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/StockAnalysisService.cs:139` |
| `Sp_StockLedger` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/StockLedgerReportService.cs:52` |
| `Sp_TDSReport` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/AccountsService/TDSSummaryService.cs:76` |
| `Sp_VendorPRRating` | missing | — | `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/Rating_Services/PrPoRatingService.cs:106` |

## M0-01-02 capture outcome (2026-08-13 update)

The table above is **unchanged** — it is M0-01-01's reference-vs-scripted classification
(what's declared in `Existing Store Procedures/StoredProcedures/` vs. what's called from
C#/Razor), and that classification is still true regardless of what has since been
captured into a *different* location. This section records what M0-01-02 did with the
82 `missing` rows; it does not retroactively rewrite the `status` column above.

**Result: 78 of the 82 `missing` procedures now have captured DDL** in
`db/stored-procedures/` (operator PavanKunar, `db/tools/Export-StoredProcedures.ps1`,
2026-08-13; verified by `db/tools/verify-capture.sh`, 0 hard failures). **4 remain
genuinely absent** and are escalated as findings, not silently dropped:
`Sp_BomAnalysis`, `Sp_Print_Estimation`, `Sp_Print_Receipts`,
`Sp_Print_SingleProcessInspection`.

**Source tenant — read the caveat, this is not a clean single-tenant capture.** The query
ran against `NexGenErpDb` (local, `DESKTOP-FIIBE97\SQLEXPRESS`), but that database was
empty of procedures until manually seeded from a script (`AllSp.sql`, local file, not
committed) originally generated from a **different** database, `IQSMARTDEMO_DB_2025-26` —
a demo/reference tenant reachable via the connection string already commented out in
`ApplicationDbContextFactory.cs`. So the DDL's actual origin is `IQSMARTDEMO_DB_2025-26`,
relayed through `NexGenErpDb`. Full chain of evidence: `db/stored-procedures/
CAPTURE-STATUS.md`, "Provenance caveat". This is now the concrete input to **Q-14**
(`docs/kb/open-questions.md`, owned by M0-02): whether a demo tenant's procedure set is
representative of a production tenant's is unanswered, and this document does not answer
it.

**A tooling defect was found and fixed during this capture**, not a data-quality problem:
3 of the 78 (`Sp_CreditDebitNoteSummaryReport`, `Sp_GetHSNSummaryReport`, `Sp_TDSReport`)
initially failed because their deployed definitions carry a leading `-- ====...` comment
before `CREATE PROCEDURE`, which `Export-StoredProcedures.ps1`'s regex did not originally
tolerate. Fixed same day (see git history); re-captured cleanly, no procedure body
altered.

**`Sp_Print_CompanyDetails` scope clarification.** One of this task's own acceptance
criteria states it should be "present in `db/stored-procedures/`" — but per the table
above it is `scripted`, not `missing`, so it was correctly left untouched (it already
lives in `Existing Store Procedures/StoredProcedures/`; M0-01-03 relocates the 13
existing files, not M0-01-02). Flagged as a documented deviation, not silently resolved
either direction — see `CAPTURE-STATUS.md`, Findings.

Full per-procedure outcome, verification output and finding detail:
`db/stored-procedures/CAPTURE-STATUS.md`. Investigation status: `INV-027` is now
**Complete** (`docs/kb/investigation-registry.md`).

## Downstream consumers

- **M0-01-02** (script the missing procedures from a live tenant DB) reads
  `db/stored-procedures/manifest.csv`, filters `status == missing`, and captures exactly
  those 82 names — no more, no fewer.
- **M0-02** (confirm stored-procedure drift across tenants, Q-14) reuses this manifest's name
  list as the set to diff against other tenants' deployed procedures.
- **M0-01-03** (deployment script + rebuild runbook) reuses the casing/bracketing/BOM survey
  above when normalising the 13 existing files plus the ones M0-01-02 captures.
