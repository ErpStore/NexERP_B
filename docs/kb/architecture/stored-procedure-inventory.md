---
doc_id: KB-102
title: Stored-Procedure Reference/DDL Reconciliation
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

# Stored-Procedure Reference/DDL Reconciliation

**Task:** [M0-01-01](../execution/tasks/M0-01-01.md). **Produced:** 2026-08-13.
**This document requires no database access** — everything in it is derived from the
repository.

## Methodology

Two sets were built independently, then compared:

1. **Referenced names** — every distinct `Sp_*` token that appears in `.cs`/`.razor` under
   `V.SMART/`, excluding build output:
   ```bash
   grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor \
     --exclude-dir=obj --exclude-dir=bin V.SMART | sort -u | wc -l
   ```
   Result: **94** (re-derived 2026-08-13; matches INV-009/INV-029's Confirmed figure).

   Run **unscoped** from the repository root (`grep -rhoE "Sp_[A-Za-z0-9_]+" | sort -u`), this
   returns **111** — it also matches procedure names quoted inside the 13 `.sql` files
   themselves and inside `docs/kb/` prose, including this document. That is a *superset*, not
   the referenced set. Use the scoped form above; do not "correct" 94 upward on the strength
   of the unscoped count.

2. **Declared names** — the identifier in the `CREATE`/`ALTER PROCEDURE` statement of each of
   the 13 `.sql` files in `Existing Store Procedures/StoredProcedures/`, never the file name:
   ```bash
   grep -rniE "(CREATE|ALTER)[[:space:]]+(OR[[:space:]]+ALTER[[:space:]]+)?PROC(EDURE)?[[:space:]]+[^[:space:]]+" \
     "Existing Store Procedures/StoredProcedures/"
   ```
   Result: **13** lines, one per file (re-derived 2026-08-13; matches INV-029).

3. **Reference index** — [`db/tools/sp-inventory.sh`](../../../db/tools/sp-inventory.sh) walks
   the same `.cs`/`.razor` scope and emits, for every `(procedure_name, path:line)` pair, a
   `commented(yes|no)` flag. The commented/live distinction is a **line-level heuristic**, not
   a parser: a match is "commented" if the text preceding it on the same line contains `//` or
   an unclosed `@*`, or the line (after trimming) begins with `*`. It does **not** track a
   `/* ... */` or `@* ... *@` block that opens on an earlier line and closes after the match's
   line. Verify any "live" classification you rely on for a load-bearing decision by opening
   the file — this script found exactly two commented occurrences (both also live-referenced
   elsewhere; see *Findings*), and both were spot-checked against the source file by hand.

4. **Classification**, case-insensitive per identifier (SQL Server's default collation is
   case-insensitive — **Inferred**, no live tenant collation has been observed this session):

   | Status | Definition |
   |---|---|
   | `scripted` | Referenced from code **and** declared in one of the 13 files, exact match |
   | `case_mismatch` | Referenced and declared, but the two spellings differ only in case |
   | `missing` | Referenced from code, no DDL anywhere in the repository |
   | `unreferenced` | Declared in a `.sql` file but referenced nowhere in `.cs`/`.razor` |

## Counts and arithmetic closure

| Status | Count |
|---|---|
| `scripted` | 11 |
| `case_mismatch` | 1 |
| `missing` | 82 |
| `unreferenced` | 1 |
| **Total distinct names (referenced ∪ declared)** | **95** |

Closure (both must hold, and do):

```
scripted + case_mismatch + missing      = 11 + 1 + 82 = 94  = referenced-name count  ✓
scripted + case_mismatch + unreferenced = 11 + 1 + 1  = 13  = declared-name count    ✓
```

**The corrected missing-count is 82, not the original 81** — see *Findings* below for why.

## Full reconciliation table

`scripted_file` shows the file name only; all 13 live under
`Existing Store Procedures/StoredProcedures/`. Full detail (`reference_count`,
`live_reference_count`, `notes`) is in
[`db/stored-procedures/manifest.csv`](../../../db/stored-procedures/manifest.csv), one row
per name here.

<!-- BEGIN reconciliation table -->
| Procedure name | Status | Scripted file | First reference |
|---|---|---|---|
| `Sp_InvDetailsLabelPrint` | scripted | Sp_InvDetailsLabelPrint.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourInvoice_Pages/LabourInvoiceDetails.razor:570 |
| `Sp_Print_CompanyDetails` | scripted | Sp_Print_CompanyDetails.sql | V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs:200 |
| `Sp_Print_GRNDetailsLabelPrint` | scripted | Sp_Print_GRNDetailsLabelPrint.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourSCN_Pages/LabourSCNDetails.razor:345 |
| `Sp_Print_LabourDC` | scripted | Sp_Print_LabourDC.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourDcOut_Pages/LabourDcOutgoingDetails.razor:431 |
| `Sp_Print_LabourGRN` | scripted | Sp_Print_LabourGRN.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourGRN_Pages/LabourGRNDetails.razor:291 |
| `Sp_Print_LabourInv` | scripted | Sp_Print_LabourInv.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourInvoice_Pages/LabourInvoiceDetails.razor:538 |
| `Sp_Print_LabourSCN` | scripted | Sp_Print_LabourSCN.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourSCN_Pages/LabourSCNDetails.razor:314 |
| `Sp_Print_MfgInv` | scripted | Sp_Print_MfgInv.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/MfgInv_Pages/MfgInvDetails.razor:558 |
| `Sp_Print_MfgQuote` | scripted | Sp_Print_MfgQuote.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/MfgQuote_Pages/MfgQuoteDetails.razor:493 |
| `Sp_Print_PerformaInvoice` | scripted | Sp_Print_PerformaInvoice.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/PerformaInvoice_pages/PerformaDetails.razor:457 |
| `Sp_Print_SubConDcOut` | scripted | Sp_Print_SubConDcOut.sql | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/SubContractDCOut_Pages/SubContractDCOutDetails.razor:272 |
| `Sp_Print_MfgDC` | case_mismatch | Sp_Print_MFGDC.sql | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesDc_Pages/MfgDcDetails.razor:395 |
| `Sp_AttendanceReport` | missing | — | V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/Attendance_Pages/AttendanceDetails.razor:282 |
| `Sp_BomAnalysis` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/JobOrderAnalysisService.cs:48 |
| `Sp_CreditDebitNoteSummaryReport` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/AccountsService/CreditDebitSummaryService.cs:77 |
| `Sp_GetCreditNoteList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/CreditNoteService.cs:1409 |
| `Sp_GetDebitNoteList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/DebitNote_Service/DebitNoteService.cs:1248 |
| `Sp_GetExportInvoiceStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/ExpInvService.cs:1662 |
| `Sp_GetHSNSummaryReport` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/AccountsService/HSNSummaryService.cs:77 |
| `Sp_GetItemModificationReport` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/ItemModificationReportServices.cs:20 |
| `Sp_GetJobOrderAssemblySubAssemblyList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/PlanningService/JobOrderService.cs:1337 |
| `Sp_GetLabourDcInOutTrack` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ViewTallyDCInOutTrackService.cs:184 |
| `Sp_GetLabourDcOutgoingStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourDcOutgoingService.cs:5929 |
| `Sp_GetLabourGRNStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourGRNService.cs:1879 |
| `Sp_GetLabourInvStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourInvoiceService.cs:1541 |
| `Sp_GetLabourSCNStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/LabourServices/LabourSCNService.cs:1737 |
| `Sp_GetManufacturingInvoiceStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgInvService.cs:2029 |
| `Sp_GetMaterialReq` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/MaterialRequisitionService/MaterialReqService.cs:1731 |
| `Sp_GetMfgPOPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/PoPendingServices.cs:117 |
| `Sp_GetMfgPosPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs:1830 |
| `Sp_GetProductionIssueAssyStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionIssueAssyService.cs:1675 |
| `Sp_GetProductionIssueCompStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionIssueCompService.cs:1991 |
| `Sp_GetProductionReturnAssyStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionReturnAssyService.cs:2287 |
| `Sp_GetProductionReturnCompStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionReturnCompService.cs:3599 |
| `Sp_GetProductionSCNAssyStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionSCNAssyService.cs:966 |
| `Sp_GetProductionSCNCompStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ProductionService/ProductionSCNCompService.cs:1378 |
| `Sp_GetPurchandSubQuotePendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchOrSubConQuoteService/PurchaseQuoteService.cs:1321 |
| `Sp_GetPurchaseGRNsPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchaseGRN_Service/PurchaseGRNService.cs:1688 |
| `Sp_GetPurchaseInvoiceStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/Purchase_Invoice_Service/PurchaseInvoiceService.cs:1596 |
| `Sp_GetPurchasePosPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchOrSubConPoService/PuchPoService.cs:2520 |
| `Sp_GetPurchaseSCNsPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchaseSCN_Service/PurchaseSCNService.cs:1808 |
| `Sp_GetPurchasesandSubcontractEnquiryPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/PurchOrSubConEnquiryService/EnquiryPurchaseService.cs:1412 |
| `Sp_GetSalesandLabQuotePendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgQuotationService.cs:1542 |
| `Sp_GetSalesandlabourPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/EnquirySalesService.cs:985 |
| `Sp_GetSubContractDcGRNPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractGRNService/SubConGRNService.cs:5603 |
| `Sp_GetSubContractDcInOutTrack` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/ViewTallyDCInOutTrackService.cs:213 |
| `Sp_GetSubContractDcoutPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractDcOutService/SubConDcOutService.cs:2931 |
| `Sp_GetSubContractInvoicePendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractInvoiceService/SubConInvService.cs:1276 |
| `Sp_GetSubContractScnPendingList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/OutSourcingService/SubContractSCNService/SubConSCNService.cs:1786 |
| `Sp_GetTaxDetailsReport` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TaxDetailsService/TaxDetailsService.cs:79 |
| `Sp_GetToolCribIssueNoteStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/ToolCribIssueService.cs:765 |
| `Sp_GetToolCribReturnNoteStatusList` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/ToolCribReturnService.cs:834 |
| `Sp_ItemWiseHistoryReport` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/History_Service/ItemWiseReportService.cs:34 |
| `Sp_JobOrderTrack` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/JobOrderAnalysisService.cs:77 |
| `Sp_LabourPendingReport` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/LabourPendingService.cs:58 |
| `Sp_Labour_Track` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/LabourTrackReportService.cs:257 |
| `Sp_Print_AppointmentLetter` | missing | — | V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/AppointmentLetter_Pages/AppointmentLetterDetails.razor:214 |
| `Sp_Print_CREDITNOTE` | missing | — | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/CreditNote_Pages/CreditNoteDetails.razor:387 |
| `Sp_Print_DebitNote` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/DebitNote_pages/DebitNoteDetails.razor:388 |
| `Sp_Print_EnquiryFeasibility` | missing | — | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/Fesibility_Pages/EnquiryFeasibilityListDetails.razor:247 |
| `Sp_Print_EnquirySales` | missing | — | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesEnquiry_Pages/EnquiryDetails.razor:227 |
| `Sp_Print_Estimation` | missing | — | V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/Estimation_Pages/EstimationDetails.razor:436 |
| `Sp_Print_InterStoreTransfer` | missing | — | V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/StoreInterTrans_Pages/StoreInterTransDetails.razor:203 |
| `Sp_Print_JobOrder` | missing | — | V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/JobOrder_Pages/JobOrderDetails.razor:290 |
| `Sp_Print_LeaveApplication` | missing | — | V.SMART/V.SMART.Shared/Pages/Master_Module_pages/LeaveApplication_Pages/LeaveDetails.razor:181 |
| `Sp_Print_MaterialIssueNoteReduction` | missing | — | V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/MaterialIssueNote_pages/MaterialIssueDetails.razor:283 |
| `Sp_Print_MaterialReq` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/MaterialRequisition_pages/MaterialReqDetails.razor:314 |
| `Sp_Print_OfferLetter` | missing | — | V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/OfferLetter_Pages/OffferLetterDetails.razor:214 |
| `Sp_Print_Payments` | missing | — | V.SMART/V.SMART.Shared/Pages/CashFlow_Pages/Payments_Pages/PaymentDetails.razor:189 |
| `Sp_Print_ProdAssyGRN` | missing | — | V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionReturnAssy_Pages/ProductionReturnAssyUpsert.razor:1660 |
| `Sp_Print_ProdAssySCN` | missing | — | V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionAssySCN_Pages/ProductionAssySCNDetails.razor:290 |
| `Sp_Print_ProdCompSCN` | missing | — | V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionSCNComp_Pages/ProductionSCNCompDetails.razor:253 |
| `Sp_Print_ProductionCompGRN` | missing | — | V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionCompReturn_Pages/ProductionReturnCompDetails.razor:258 |
| `Sp_Print_ProductionIssueAss` | missing | — | V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionIssueAssy_Pages/ProductionIssueAssyDetails.razor:290 |
| `Sp_Print_ProductionIssueComp` | missing | — | V.SMART/V.SMART.Shared/Pages/ProductionModule_pages/ProductionComp_Pages/ProductionIssueCompDetails.razor:276 |
| `Sp_Print_PurchaseEnquiry` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConEnquiry_Pages/EnquiryPurchaseUpsert.razor:2310 |
| `Sp_Print_PurchaseGRN` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseGRN_Pages/PurchaseGRNDetails.razor:277 |
| `Sp_Print_PurchaseInvoice` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseInvoice_Pages/PurchaseInvoiceDetails.razor:450 |
| `Sp_Print_PurchasePo` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConPO_Pages/PurchPOUpsert.razor:4596 |
| `Sp_Print_PurchaseSCN` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseSCN_Pages/PurchaseSCNDetails.razor:316 |
| `Sp_Print_Receipts` | missing | — | V.SMART/V.SMART.Shared/Pages/CashFlow_Pages/Receipt_Pages/ReceiptDetails.razor:189 |
| `Sp_Print_RouteCard` | missing | — | V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/RouteCard_Pages/RouteCardDetails.razor:327 |
| `Sp_Print_Salary` | missing | — | V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/Payroll_Pages/SalaryDetails.razor:310 |
| `Sp_Print_SingleProcessInspection` | missing | — | V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/LabourInvoice_Pages/LabourInvoiceDetails.razor:596 |
| `Sp_Print_SubconSCN` | missing | — | V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/SubContractSCN_pages/SubConSCNDetails.razor:307 |
| `Sp_Print_ToolCribIssueNote` | missing | — | V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/ToolCribIssue_Pages/ToolCribIssueDetails.razor:208 |
| `Sp_Print_ToolCribReturn` | missing | — | V.SMART/V.SMART.Shared/Pages/Inventory(Stock)_Module_Pages/ToolCribReturn_Pages/ToolCribReturnDetails.razor:267 |
| `Sp_PurchaseSalesTrack` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/PurchaseSalesTrackReportServices.cs:48 |
| `Sp_RouteCardAnalysisDetails` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/RouteCardAnalysisService.cs:71 |
| `Sp_RouteCardAnalysisSummary` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/RouteCardAnalysisService.cs:55 |
| `Sp_StockAnalysis` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/StockAnalysisService.cs:139 |
| `Sp_StockLedger` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/AnalysisReportService/StockLedgerReportService.cs:52 |
| `Sp_TDSReport` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/AccountsService/TDSSummaryService.cs:76 |
| `Sp_VendorPRRating` | missing | — | V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/ReportService/Rating_Services/PrPoRatingService.cs:106 |
| `Sp_Print_PurchaseOrder` | unreferenced | Sp_Print_PurchaseOrder.sql | — |
<!-- END reconciliation table -->

## Findings

**Finding 1 — the headline number is 82, not 81 (Confirmed).**
`Sp_Print_PurchaseOrder.sql:1` declares `[dbo].[Sp_Print_PurchaseOrder]`, a name referenced
**nowhere** in `.cs`/`.razor` — it is dead DDL, classified `unreferenced` above. The
application instead calls **`Sp_Print_PurchasePo`**
(`V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConPO_Pages/PurchasePoDetails.razor:306`,
`.../PurchPOUpsert.razor:4596`,
`V.SMART/V.SMART.Shared/Pages/Planning_Module_Pages/Authorization_Pages/Authorization.razor:723`),
which has **no DDL at all** — classified `missing` above. One scripted file is dead and one
live print path is unscripted; the naive "13 scripted therefore 81 missing" arithmetic
undercounts by one. **Evidence:** manifest rows `Sp_Print_PurchaseOrder` (`unreferenced`) and
`Sp_Print_PurchasePo` (`missing`). **Confidence:** Confirmed.

**Finding 2 — one case-only mismatch (Confirmed the spelling difference; Inferred the
resolution).**
`Sp_Print_MFGDC.sql:1` declares `[dbo].[Sp_Print_MFGDC]`; the application calls
`Sp_Print_MfgDC` (`V.SMART/V.SMART.Shared/Pages/SalesAndLabour_pages/SalesDc_Pages/MfgDcDetails.razor:395`).
The two spellings differ only in case. SQL Server's default collation
(`SQL_Latin1_General_CP1_CI_AS`) is case-insensitive, so the two would resolve to the same
object — but **no live tenant's actual collation has been observed this session**, so the
resolution itself is **Inferred**, not Confirmed. M0-01-02 should confirm the live collation
before treating this as fully closed.

**Finding 3 — two references exist only in a comment, but neither is orphaned.**
`Sp_Print_PurchasePo` has a fourth occurrence at
`V.SMART/V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchaseQuote_Pages/PurchaseQuoteDetails.razor:463`,
entirely inside a `//`-commented line. `Sp_Print_Salary` has a commented occurrence at
`V.SMART/V.SMART.Shared/Pages/HumanResource_Pages/Payroll_Pages/SalaryDetails.razor:309`
immediately followed by a live, uncommented call to the same procedure on line 310 (the
commented line is stale dead code from an earlier `Generate_Report` call path, superseded by
`GenerateSalarySlipReport` on the next line). Both names already have a live reference
elsewhere, so neither changes classification — this is recorded because a reviewer might
otherwise wonder whether the commented line was the *only* evidence for `missing` status.
**No name in the 94-name referenced set has only commented references** (verified: `comm -23`
between the full and live-only name sets returns empty). **Confidence:** Confirmed.

**Finding 4 — `Sp_Print_CompanyDetails` is a hard dependency of every printed document, not
one report among many.**
`ReportService.Generate_Report` throws before resolving the document's own procedure if the
`Sp_Print_CompanyDetails` data source is absent
(`V.SMART/V.SMART.Shared/Services/ReportViewer/ReportService.cs:74-77`:
`if (report.GetDataSource("Sp_Print_CompanyDetails") is not TableDataSource dsCompany) throw …`).
The same requirement repeats in `GenerateSalarySlipReport` (`:200-202`) and
`Generate_Attendance_Report` (`:328-331`). It is one of the 11 correctly `scripted` names —
losing it (or its DDL drifting) breaks **all** printing at once, not just one document type.
**Confidence:** Confirmed.

**Finding 5 — mixed DDL form across the 13 scripted files (relevant to M0-01-02/M0-01-03).**
- **Casing:** `CREATE OR ALTER PROCEDURE` (2 files), `Create Or ALTER Procedure` (8 files),
  `CREATE OR ALTER Procedure` (1 file), `CREATE OR ALTER procedure` (1 file),
  `Create or ALTER PROCEDURE` (1 file) — re-derived from the step-2 grep, verbatim per file.
- **Bracketing:** 7 of 13 declare `[dbo].[Sp_Name]`; 6 declare the bare name with no schema
  or bracket qualifier.
- **BOM:** 6 of 13 files (`Sp_InvDetailsLabelPrint.sql`, `Sp_Print_CompanyDetails.sql`,
  `Sp_Print_LabourDC.sql`, `Sp_Print_LabourGRN.sql`, `Sp_Print_LabourInv.sql`,
  `Sp_Print_LabourSCN.sql`) begin with a UTF-8 BOM (`EF BB BF`); the other 7 do not.
  All three facts are stated so M0-01-03's deployment script normalises them rather than
  being surprised by them mid-rollout. **Confidence:** Confirmed (checked file-by-file with
  `od -An -tx1`, not sampled).

## What this method cannot see

Stated explicitly, per the task's requirement — do not read a "not found" below as "does not
exist":

- **FastReport `.frx` data-source bindings.** 104 `.frx` templates exist under
  `V.SMART/V.SMART.Shared/wwwroot/templates/` (confirmed by `find`, count matches INV-009). A
  text `grep` for `Sp_Print_PurchaseOrder` across all 104 files returned no match, which is
  **weak negative evidence at best** — `.frx` is an XML format that may reference a data
  source by an internal alias rather than the literal procedure string, and this session did
  not parse the format. The `unreferenced` classification for `Sp_Print_PurchaseOrder` rests
  on the `.cs`/`.razor` grep only; do **not** conclude "dead code" from that grep alone, as
  the task instructs.
- **Procedure-to-procedure calls.** A stored procedure calling another stored procedure is
  invisible to a C#/Razor grep. None of the 13 scripted `.sql` files was inspected for
  `EXEC`/`sp_executesql` calls to other procedures as part of this task (out of scope — no
  database access, and the file contents were read only for the declaration line).
- **Dynamically composed names.** Checked with a heuristic grep for string interpolation or
  concatenation touching an `Sp_` literal
  (`grep -rnE '\$"[^"]*Sp_|"Sp_[A-Za-z0-9_]*"\s*\+|\+\s*"Sp_"'` across `.cs`/`.razor`,
  excluding the known `$"EXEC dbo.{procedureName}"` pattern in `ReportExecutor.cs:27`, which
  interpolates a *parameter*, not a literal). **Result: no output — negative result,
  Confirmed** for this heuristic's scope. This does not prove no name is ever assembled from
  parts elsewhere in a way the heuristic's regex does not match; it is a bounded check, not an
  exhaustive one.

## Manifest and tooling

- [`db/stored-procedures/manifest.csv`](../../../db/stored-procedures/manifest.csv) — 95 data
  rows, the schema documented in
  [`db/stored-procedures/README.md`](../../../db/stored-procedures/README.md). This is the
  literal worklist M0-01-02 executes: every `missing` row needs its DDL scripted from a live
  tenant database; the `case_mismatch` row needs its collation-resolution confirmed; the
  `unreferenced` row needs a human decision (delete vs. keep as historical DDL) that this task
  does not make.
- [`db/tools/sp-inventory.sh`](../../../db/tools/sp-inventory.sh) — regenerates the reference
  side of the comparison. Re-run it after any future code change touching a `Sp_*` call site
  to detect drift from this document.

## Reproducing this document

```bash
grep -rhoE "Sp_[A-Za-z0-9_]+" --include=*.cs --include=*.razor --exclude-dir=obj --exclude-dir=bin V.SMART | sort -u | wc -l   # 94
ls "Existing Store Procedures/StoredProcedures/" | wc -l                                                                       # 13
grep -rniE "(CREATE|ALTER)[[:space:]]+(OR[[:space:]]+ALTER[[:space:]]+)?PROC(EDURE)?[[:space:]]+[^[:space:]]+" "Existing Store Procedures/StoredProcedures/"
db/tools/sp-inventory.sh > /tmp/sp-refs.tsv   # 146 (procedure_name, path:line, commented) rows, 94 distinct names
```

If any of these counts differ from a re-run, the repository has moved since 2026-08-13 and
every downstream number in this document (and in R-04, INV-027) must be regenerated, not
patched.
