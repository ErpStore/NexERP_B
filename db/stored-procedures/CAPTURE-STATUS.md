# Capture status — M0-01-02 stored-procedure DDL capture

This file is the provenance record for every `.sql` file under `db/stored-procedures/`. It is
filled in by the DBA performing the capture (half B of
[M0-01-02](../../docs/kb/execution/tasks/M0-01-02.md)) and reconciled by the verifying AI
session afterwards (half C). Until it is filled in, any `.sql` file found in this directory
has **unknown provenance** and must not be trusted.

**Do not put a connection string, password, host name, or IP address anywhere in this file.**
Record the tenant by name or id only — see the runbook's *secrets rule*
(`../RUNBOOK-capture-stored-procedures.md`).

## Capture metadata

| Field | Value |
|---|---|
| Source tenant (name/id only — never a connection string) | TBD |
| Source server (hostname/instance only — never a full connection string) | TBD |
| Capture date | TBD |
| Operator | TBD |
| Capture method (`Export-StoredProcedures.ps1` / manual SSMS) | TBD |

## Per-procedure outcome

One row per `missing` name in [`manifest.csv`](manifest.csv), populated by the DBA (or by
`Export-StoredProcedures.ps1`'s console output, transcribed here). Outcome is one of:
`captured` · `not_found` · `permission_denied` · `multiple_matched` · `non_dbo_schema` ·
`empty_definition` (likely `WITH ENCRYPTION`) · `flagged_for_review` (captured, but something
about it needs a human decision — describe it in Notes).

| Procedure name | Outcome | Notes |
|---|---|---|
| `Sp_AttendanceReport` | TBD | |
| `Sp_BomAnalysis` | TBD | |
| `Sp_CreditDebitNoteSummaryReport` | TBD | |
| `Sp_GetCreditNoteList` | TBD | |
| `Sp_GetDebitNoteList` | TBD | |
| `Sp_GetExportInvoiceStatusList` | TBD | |
| `Sp_GetHSNSummaryReport` | TBD | |
| `Sp_GetItemModificationReport` | TBD | |
| `Sp_GetJobOrderAssemblySubAssemblyList` | TBD | |
| `Sp_GetLabourDcInOutTrack` | TBD | |
| `Sp_GetLabourDcOutgoingStatusList` | TBD | |
| `Sp_GetLabourGRNStatusList` | TBD | |
| `Sp_GetLabourInvStatusList` | TBD | |
| `Sp_GetLabourSCNStatusList` | TBD | |
| `Sp_GetManufacturingInvoiceStatusList` | TBD | |
| `Sp_GetMaterialReq` | TBD | |
| `Sp_GetMfgPOPendingList` | TBD | |
| `Sp_GetMfgPosPendingList` | TBD | |
| `Sp_GetProductionIssueAssyStatusList` | TBD | |
| `Sp_GetProductionIssueCompStatusList` | TBD | |
| `Sp_GetProductionReturnAssyStatusList` | TBD | |
| `Sp_GetProductionReturnCompStatusList` | TBD | |
| `Sp_GetProductionSCNAssyStatusList` | TBD | |
| `Sp_GetProductionSCNCompStatusList` | TBD | |
| `Sp_GetPurchandSubQuotePendingList` | TBD | |
| `Sp_GetPurchaseGRNsPendingList` | TBD | |
| `Sp_GetPurchaseInvoiceStatusList` | TBD | |
| `Sp_GetPurchasePosPendingList` | TBD | |
| `Sp_GetPurchaseSCNsPendingList` | TBD | |
| `Sp_GetPurchasesandSubcontractEnquiryPendingList` | TBD | |
| `Sp_GetSalesandLabQuotePendingList` | TBD | |
| `Sp_GetSalesandlabourPendingList` | TBD | |
| `Sp_GetSubContractDcGRNPendingList` | TBD | |
| `Sp_GetSubContractDcInOutTrack` | TBD | |
| `Sp_GetSubContractDcoutPendingList` | TBD | |
| `Sp_GetSubContractInvoicePendingList` | TBD | |
| `Sp_GetSubContractScnPendingList` | TBD | |
| `Sp_GetTaxDetailsReport` | TBD | |
| `Sp_GetToolCribIssueNoteStatusList` | TBD | |
| `Sp_GetToolCribReturnNoteStatusList` | TBD | |
| `Sp_ItemWiseHistoryReport` | TBD | |
| `Sp_JobOrderTrack` | TBD | |
| `Sp_LabourPendingReport` | TBD | |
| `Sp_Labour_Track` | TBD | |
| `Sp_Print_AppointmentLetter` | TBD | |
| `Sp_Print_CREDITNOTE` | TBD | |
| `Sp_Print_DebitNote` | TBD | |
| `Sp_Print_EnquiryFeasibility` | TBD | |
| `Sp_Print_EnquirySales` | TBD | |
| `Sp_Print_Estimation` | TBD | |
| `Sp_Print_InterStoreTransfer` | TBD | |
| `Sp_Print_JobOrder` | TBD | |
| `Sp_Print_LeaveApplication` | TBD | |
| `Sp_Print_MaterialIssueNoteReduction` | TBD | |
| `Sp_Print_MaterialReq` | TBD | |
| `Sp_Print_OfferLetter` | TBD | |
| `Sp_Print_Payments` | TBD | |
| `Sp_Print_ProdAssyGRN` | TBD | |
| `Sp_Print_ProdAssySCN` | TBD | |
| `Sp_Print_ProdCompSCN` | TBD | |
| `Sp_Print_ProductionCompGRN` | TBD | |
| `Sp_Print_ProductionIssueAss` | TBD | |
| `Sp_Print_ProductionIssueComp` | TBD | |
| `Sp_Print_PurchaseEnquiry` | TBD | |
| `Sp_Print_PurchaseGRN` | TBD | |
| `Sp_Print_PurchaseInvoice` | TBD | |
| `Sp_Print_PurchasePo` | TBD | |
| `Sp_Print_PurchaseSCN` | TBD | |
| `Sp_Print_Receipts` | TBD | |
| `Sp_Print_RouteCard` | TBD | |
| `Sp_Print_Salary` | TBD | |
| `Sp_Print_SingleProcessInspection` | TBD | |
| `Sp_Print_SubconSCN` | TBD | |
| `Sp_Print_ToolCribIssueNote` | TBD | |
| `Sp_Print_ToolCribReturn` | TBD | |
| `Sp_PurchaseSalesTrack` | TBD | |
| `Sp_RouteCardAnalysisDetails` | TBD | |
| `Sp_RouteCardAnalysisSummary` | TBD | |
| `Sp_StockAnalysis` | TBD | |
| `Sp_StockLedger` | TBD | |
| `Sp_TDSReport` | TBD | |
| `Sp_VendorPRRating` | TBD | |

## Discrepancies referred to a human

Anything `verify-capture.sh` cannot resolve mechanically — a procedure genuinely absent from
the source tenant, a name matched to multiple objects, a schema other than `dbo`, an encrypted
module — gets a named owner and a decision recorded here, not silently dropped.

| Procedure | Issue | Owner | Decision |
|---|---|---|---|
| *(none yet — half B has not run)* | | | |
