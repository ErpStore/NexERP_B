---
doc_id: KB-052
title: Existing Feature → New Angular Screen Mapping (Proposal)
module: frontend-new
source_files:
  - V.SMART/V.SMART.Shared/Pages/
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/
  - V.SMART/V.SMART.Shared/Layout/NavMenu.razor
status: proposal
confidence: n/a
last_verified: 2026-08-28
dependencies: [KB-020, KB-041, KB-050]
---

# Existing Feature → New Angular Screen Mapping

> **Proposal.** "Existing API/logic" names the **service** that must be exposed — none of
> these endpoints exist yet except Currency (see [`api/api-overview.md`](../api/api-overview.md)).

## Complexity scale

| Rating | Meaning | Typical driver |
|---|---|---|
| **Low** | Simple CRUD, < 500 LOC page, no upstream pickers | master data |
| **Medium** | CRUD + child collection, or a report with parameters | |
| **High** | Document editor with line grid, 1–2 upstream pickers, workflow commands | |
| **Very High** | ≥ 3,000 LOC page, multiple pickers, stock/tax/e-invoice side effects, approval | |

Complexity here means **rebuild cost**, dominated by how much logic must first be extracted
out of the Razor `@code` block (see
[`architecture/frontend-architecture-existing.md`](../architecture/frontend-architecture-existing.md)).

---

## Masters

| Existing feature | Existing routes | Existing service | New Angular screen | New components | Complexity |
|---|---|---|---|---|---|
| Currency Master | `/currency`, `/currency/create`, `/currency/update/{id}` | `ICurrencyService` — **API already exists** | **Delivered `M2-D01` (2026-08-28)** — folder `features/masters/currency/`, routes `/masters/currencies[/new\|/{id}]` (KB-053). The folder name (singular, `currency`) and the route (plural, `currencies`) are deliberately different — the folder groups the *feature*, the route names the *collection* it lists, matching every other list/detail resource in this API. Not a naming bug to reconcile. | DataGrid, FormLayout, Drawer | **Low** ⭐ start here |
| Expense / Income / Bank / Cost Centre / Project Type | `/expense`, `/income`, `/bank`, `/costcenter`, `/project-type-master` | `MasterService/AccountsService` | one generic `MasterCrudPage` config per entity | DataGrid, FormLayout | **Low** |
| UOM / Category / State / Factors / Grouping | `/uom`, `/category`, `/state`, `/factorlist`, `/groupingList` | `MasterService/InventoryService`, `GeneralService` | same generic page | DataGrid, FormLayout | **Low** |
| HSN/SAC Master | `/hsnMaster` | `IHSNService` | List + form | DataGrid, FormLayout | **Low** |
| Terms & Conditions | `/terms-and-conditions` | `ITermsAndConditionsService` | List + rich-text form | RichTextEditor | **Low** |
| Store / Store Mapping | `/stores`, `/store-map` | `IStoreService` | List + mapping matrix | DataGrid, TransferList | Medium |
| Machine Master | `/machineList` | `IMachineService` | List + form | DataGrid, FormLayout | **Low** |
| **Customer Master** | `/customer`, `/customer/create`, `/customer/update/{id}` | `ICustomerService` | List + tabbed detail (General · Contacts · Addresses · Commercial) | DataGrid, Tabs, FormLayout, child grids | **Medium** ⭐ 2nd |
| **Vendor Master** | `/vendor`, `/vendor/create`, `/vendor/update/{id}` | `IVendorService` | mirror of Customer | same | **Medium** |
| Raw Material Master | `/rawMaterialList` | `IRawMaterialService` | List + form | DataGrid, FormLayout | **Low** |
| **Item Master** | `/itemList`, `/item/create`, `/item/update/{id}` | `IItemService` — page is **4,731 LOC** | Tabbed detail (General · Tax · Stock · Purchase · Sales · Assembly · Process · History) | Tabs, FormLayout, child grids, ImageUpload | **High** |
| **BOM Master** | `/bomList`, `/bom/create`, `/bom/update/{id}`, `/bom/details/{id}` | `IAssemblyDefService` | Tree editor + component grid | **TreeGrid** (new), LineItemGrid | **High** |
| BOM Labour | `/bomLabourList`, `/bomLabour/*` | `IAssemblyDefLabourService` (1,839 LOC) | Operation/labour cost grid | LineItemGrid | **High** |
| Process Master / Flow Chart | `/processList`, process flow | `IProcessService` | List + sequence editor | SortableList | Medium |
| Item Price Update | `/ItemRate_Updation` | `IItemService` (bulk `ExecuteSqlRaw`) | Bulk-edit grid | Editable DataGrid | Medium |
| Master Upload (Excel) | `/master-upload`, `/bom-upload`, `/bom-labour-upload` | `IExcelTemplateService` | Import wizard (template → upload → preview → commit) | ExcelImportDialog, Stepper | Medium |
| User Master | `/user`, `/user/create`, `/user/update/{id}` | `IUserService` | List + form incl. QR, device, email settings | DataGrid, Tabs, FormLayout | Medium |
| **User Rights** | `/userRights` | `IUserRightsService` | 152-screen × 5-right permission matrix | **PermissionMatrix** (new), virtualised | **High** |
| Authority Manage | `/userLevelAuthorization` | `IApprovalService`/`IUserAuthority` | 12 doc-type × level matrix | PermissionMatrix | Medium |
| Screen Management | `/screenManagement` | `IScreenManagementService` | List + form | DataGrid | **Low** |
| General Settings | `/generalSettings` | `ISettingsService` | Sectioned settings page | FormLayout, Tabs | Medium |
| Print Management | `/print-management` | `IPrintSettingService` | Per-screen print config grid | DataGrid | Medium |
| HR Masters (Staff, Candidate, Leave Type, Leave Balance, Holiday, Shift) | `/Staff`, `/candidate`, `/leaveType`, `/employeeLeaveBalance`, `/hrMaster/holidayList`, `/shiftAllocation` | `MasterService/HRMasterService` | List + form each; Staff is tabbed | DataGrid, Tabs | Medium |
| My Company | `/myCompany` | `ICompanyService` | Settings page + logo upload | FormLayout, FileUpload | **Low** |

## Sales

| Existing feature | Existing routes | Existing service | New Angular screen | New components | Complexity |
|---|---|---|---|---|---|
| Leads | `/Leads`, `/Leads-details/*` | `ILeadService` | List + form | DataGrid, FormLayout | **Low** |
| Sales Enquiry | `/enquirySales*`, `/enquiryDetails/{id}` | `IEnquirySalesService` | DocumentEditor | DocumentEditor, LineItemGrid | **High** |
| Enquiry Feasibility | `/enquiryFeasibility*`, `/enquiryfeasibiltyDetails/{id}` | `IEnquiryFesibilityService` | DocumentEditor + picker from Enquiry | DocumentEditor, RecordPicker | **High** |
| Quotation | `/mfgQuoteList`, `/MfgQuote/*`, `/quote/details/{id}` | `IMfgQuotationService` | DocumentEditor + revisions + approval | DocumentEditor, ApprovalActions, RevisionHistory | **Very High** |
| **Sales Order** | `/mfgPOList`, `/MfgPO/create[/{CustId}]`, `/MfgPO/update/{PoId}`, `/mfgPO/details/{PoId}` | `IMfgPoService` — page **4,383 LOC** | **DocumentEditor reference implementation** | DocumentEditor, LineItemGrid, RecordPicker (Quotation), TotalsPanel, ConfirmDialog(reason), ApprovalActions | **Very High** ⭐ pilot |
| Contract Review | `/contractReviewCheckList*` | `IContractReviewService` | Checklist form bound to a Sales Order | ChecklistForm | Medium |
| Proforma Invoice | `/PerformaInvList`, `/PerformaInv/*` | `IPerformaInvoiceService` | DocumentEditor + picker from Sales Order | DocumentEditor, RecordPicker | **High** |
| Sales DC | `/mfgDcList`, `/MfgDc/*` | `IMfgDcService` (2,103 LOC) | DocumentEditor + stock + e-Way Bill | DocumentEditor, EwayPanel | **Very High** |
| Domestic Tax Invoice | `/mfgInvList`, `/mfgInv/*` | `IMfgInvService` (2,074 LOC); page 4,431 LOC | DocumentEditor + e-Invoice (IRN/QR) | DocumentEditor, EInvoicePanel, TotalsPanel | **Very High** |
| Export Invoice | `/expInvList`, `/expInv/*` | `IExpInvService`; page 4,100 LOC | DocumentEditor + FX + export docs | DocumentEditor, CurrencyPanel | **Very High** |
| Credit Note | `/creditNoteList`, `/creditNote/*` | `ICreditNoteService` | DocumentEditor + picker from Invoice | DocumentEditor, RecordPicker | **High** |

## Labour Work

| Existing feature | Existing routes | Existing service | New Angular screen | Complexity |
|---|---|---|---|---|
| Labour GRN (cum DC) | `/labourGRNList`, `/labourGRN/*` | `ILabourGRNService` (1,924 LOC) | DocumentEditor + stock | **Very High** |
| Labour SCN | `/labourSCNList`, `/labourSCN/*` | `ILabourSCNService` | DocumentEditor + `IStockManagerService` | **Very High** |
| **Labour DC Outgoing** | `/labourDcoutgoingList`, `/labourDcOutgoing/*` | `ILabourDcOutgoingService` **(6,112 LOC)**; page **6,528 LOC** | DocumentEditor + component tracking + e-Way | **Very High** — largest single item |
| Labour Invoice | `/LabourInvoiceList`, `/LabourInvoice/*` | `ILabourInvoiceService`; page 3,602 LOC | DocumentEditor + e-Invoice | **Very High** |

## Out Sourcing / Purchase / Sub Contract

| Existing feature | Existing routes | Existing service | New Angular screen | Complexity |
|---|---|---|---|---|
| Material Requisition | `/materialRequisitionList`, `/materialRequisition/*` | `IMaterialReqService` | DocumentEditor + approval | **High** |
| Purchase Enquiry | `/EnquiryPurchase`, `/enquiryPurchase/*` | `IEnquiryPurchaseService` | DocumentEditor + multi-vendor assign | **High** |
| Vendor Quotation | `/purchaseQuoteList`, `/PurchaseQuote/*` | `IPurchaseQuoteService`; page 3,076 LOC | DocumentEditor + picker from Enquiry | **High** |
| Price Comparison | `/rate-comparison` | `IPurchaseQuoteService` | Comparison matrix (vendors × items) | **High** (bespoke) |
| **Purchase Order** | `/PurchPOList`, `/PurchPO/create[/{VendorCode}]`, `/PurchPO/multi-vendor-create`, `/PurchPO/update/{PoId}` | `IPurchPoService` (2,700 LOC); page **4,620 LOC** | DocumentEditor + multi-vendor mode + approval | **Very High** |
| Purchase GRN | `/purchaseGRNList`, `/purchaseGRN/*` | `IPurchaseGRNService` | DocumentEditor + inspection trigger | **Very High** |
| Purchase SCN | `/purchaseSCNList`, `/purchaseSCN/*` | `IPurchaseSCNService` (1,941 LOC) | DocumentEditor + **stock addition** | **Very High** |
| Purchase Invoice | `/purchaseInvoiceList`, `/purchaseInvoice/*` | `IPurchaseInvoiceService` | DocumentEditor + TDS | **High** |
| SubCon DC-Out | `/subConDcOutList`, `/DcSubConOutgoing/*` | `ISubConDcOutService` (2,943 LOC); page 3,911 LOC | DocumentEditor + **stock issue** + e-Way | **Very High** |
| SubCon GRN | `/subConGrnList`, `/DcSubConIncoming/*` | `ISubConGRNService` **(5,631 LOC)**; page 3,768 LOC | DocumentEditor + reconciliation (`SubConGRNTrack`) | **Very High** |
| SubCon SCN | `/subConScnList`, `/DcSubConSCN/*` | `ISubConSCNService` (1,814 LOC) | DocumentEditor + stock | **Very High** |
| SubCon Invoice | `/subConInvList`, `/DcSubConIncomingInvoice/*` | `ISubContractInvoiceService` | DocumentEditor | **High** |
| Debit Note | `/debitNoteList`, `/debitNote/*` | `IDebitNoteService`; page 3,311 LOC | DocumentEditor + picker from Invoice | **High** |

## Planning / Production

| Existing feature | Existing routes | Existing service | New Angular screen | Complexity |
|---|---|---|---|---|
| **Authorisation (approvals)** | `/approval` | `IApprovalService` | Unified approval inbox — filter by type/level, bulk approve/reject with reason | **High** ⭐ high user value, do early |
| Job Order | `/jobOrderList`, `/jobOrder/*` | `IJobOrderService` | DocumentEditor + assembly explosion | **Very High** |
| Route Card | `/routeCardList`, `/routeCard/*` | `IRouteCardService` (1,934 LOC); page 3,351 LOC | Operation-sequence editor + status timeline | **Very High** |
| Route Card Release | `/rcReleaseList`, `/rcRelease/*` | `IRcReleaseService` | Release workflow screen | **High** |
| Estimation | `/estimationList`, `/estimation/*` | `IEstimateService` | DocumentEditor + costing | **High** |
| Material Requirement Analysis | `/materialReqAnalysis` | `IMaterialReqAnalysisService` | Analysis grid → generate requisition | **High** |
| Assembly Requirement Analysis | `/assemblyReqAnalysis` | `IAssemblyRequirementService` | Analysis grid | **High** |
| Production Issue/Return/SCN — Assembly | `/productionIssueList`, `/productionReturnAssyList`, `/productionAssySCNList` + `/productionIssueAssy/*` etc. | `ProductionService` (`ProductionReturnAssyService` 2,301 LOC) | DocumentEditor ×3 + stock | **Very High** |
| Production Issue/Return/SCN — Component | `/productionCompIssueList`, `/productionCompReturnList`, `/productionCompSCNList` + upserts | `ProductionIssueCompService` (2,659 LOC), `ProductionReturnCompService` **(5,518 LOC)**; pages 3,912 / 3,645 LOC | DocumentEditor ×3 + stock | **Very High** |
| **Daily Production Log** | `/productionLogList[/{IsProduction}]`, `/productionLog/*`, `/productionLogStop/*` | `IProductionLogService` (2,273 LOC) | **Dedicated shop-floor touch UI** (large targets, start/stop, machine+operator, offline-tolerant) | **High** (bespoke UX) |

## Inventory / Stock

| Existing feature | Existing routes | Existing service | New Angular screen | Complexity |
|---|---|---|---|---|
| Stock Issue-Request | `/stockIssReqList`, `/stockIssReq/*` | `IStockIssueRequestService` | DocumentEditor + approval | **High** |
| Consumable Issue Note (MIN) | `/minIssList`, `/minIss/*` | `IMINService` | DocumentEditor + **stock issue** | **High** |
| Stock Transfer Note (SCNGen) | `/scnGenList`, `/scnGen/*` | `ISCNGenService` | DocumentEditor + **stock addition** | **High** |
| Inter-Store Transfer | `/storeIntertransList`, `/storeInterTrans/*` | `IStoreInterTransService` | DocumentEditor + dual stock movement | **High** |
| Tool Crib Issue / Return | `/tcIssueList`, `/tcReturnList`, `/tci/*`, `/tcr/*` | `IToolCribServices` | DocumentEditor ×2 | Medium |
| Stock Position (Internal / with WIP) | `/stockPosition`, `/stockPositionInternalExternal` | `IStockAddIssPosition` | Analytical grid + drilldown | Medium |
| BOM → Indent / STN | `/bomPRScn` | mixed | Explosion → generate documents | **High** |

## Inspection · Maintenance · HR · Accounts

| Existing feature | Existing routes | Existing service | New Angular screen | Complexity |
|---|---|---|---|---|
| Inspection Master / Defects Master | `/MasterInspection`, `/DefectInfo` | `IMasterInspectionService`, `IDefectInfoService` | List + form | Medium |
| GRN Inspection | `/IncomingInspection*` | `IIncomingInspectionService` | Inspection form + defect capture | Medium |
| Final Inspection | `/FinalInspection*` | `IFinalInspectionService` | Inspection form | Medium |
| Maintenance Schedule / Process / Breakdown / Calibration | `/MaintenanceSchedule`, `/MaintenanceProcess`, `/BreakdownMaintenance`, `/CalibrationMaintenance` | `MaintenanceService` | List + form + **calendar view** + due list | Medium |
| Leave Application | `/leaveApplication*`, `/leaveApplication/authorize/{id}` | `ILeaveApplicationService` | Request form + approval + balance widget | Medium |
| Attendance | `/attendanceList`, `/attendance/*`, `/biometric-Excel` | `IAttendanceService` | Monthly grid + Excel import | **High** |
| Salary / Payroll | `/salaryList`, `/salary/*` | `ISalaryService` | Payroll run + payslip preview | **Very High** |
| Staff Loan | `/staffLoanList`, `/staffLoan/*` | `IStaffLoanService` | List + form + schedule | Medium |
| Offer / Appointment Letter | `/offer_Letter*`, `/appointment_Letter*` | `IOfferLetterService`, `IAppointmentLetterService` | Form + PDF preview | Medium |
| Payments / Receipts | `/PaymentList`, `/ReceiptList`, `/Payments/*`, `/Receipts/*` | `IPaymentsService`, `IReceiptsService` | DocumentEditor + bill allocation + TDS | **Very High** |
| Advance Adjustment | `/AdvanceAdjustmentList`, `/AdvanceAdjustment/*` | `IAdvaceAdjustmentService` | Allocation screen | **High** |
| Bank Transactions | `/Fundtransactions` | `IFundTransRepository` | List + form | Medium |
| Service Bills | `/serviceBills`, `/ServiceBills/*` | `IServiceBillsService` | DocumentEditor | **High** |
| Correspondence | `/correspondenceList`, `/correspondence/*` | `ICorrespondanceRepository` | Attachment manager (any document) | Medium |

## Reports and Dashboard

All ~40 report screens share one shape: **parameter panel → server-paged grid → export**.
They are backed by stored procedures via `IReportExecutor` (BR-RPT-001 family).

**Recommendation: build one `ReportPage` component driven by a declarative report
definition** (parameters, columns, procedure name, export options). 40 screens then become
40 config objects. Rating **Medium overall, Low per report** after the framework exists.

| Group | Screens |
|---|---|
| Accounts | `/confirmationaccounts`, `/PaidBills`, `/Pending_Bills`, `/PendingStatements`, `/Profit_LossAccounts`, `/taxDetailsList`, `/tdsSummaryReport`, `/hsnCodeSummaryReport`, `/creditdebitSummaryReport`, `/summarygraphs`, `/GSTITC`, `/Daybook` |
| Track | `/SalesTrack`, `/labourTrack`, `/dc-inout-report`, `/po-tally`, `/Purchase-Sales_Track`, `/joborderAnalysis` |
| Analysis | `/StockLedger`, `/StockAnalysis`, `/Rejection`, `/RouteCardAnalysis`, `/ItemWiseReport`, `/ItemModificationReport` |
| Pending | `/SalesPoPending`, `/labourpending`, `/ProductionPending` |
| Rating | `/Ratings`, `/prpoRating` |
| Issue Summary | `/ToolCribReport` |
| Dashboard | `/dashboard` — `DashboardService` is 3,095 LOC; rebuild with Recharts |

## Cross-cutting screens

| Existing | New | Note |
|---|---|---|
| `/instantSearch` | **⌘K command palette** | Replace the page with a global palette |
| `/login`, `/qrlogin/{token}`, `/logout`, `/register` | `AuthLayout` routes | Add refresh-token handling |
| `/access-denied` | `PermissionDeniedState` inline | Prefer inline over redirect |
| `/*-home`, `/*-master` landing pages | **Delete** | Replaced by the sidebar + palette; ~20 routes removed |
| `/approval` | Approval inbox | High value, low dependency — schedule early |

## Rollup

| Complexity | Screens (approx.) |
|---|---|
| Low | ~30 |
| Medium | ~45 |
| High | ~35 |
| Very High | ~30 |
| **Total** | **~140 target screens** (from 333 existing components / 440 routes) |

The reduction comes from: collapsing create/update/details into one route, deleting ~20
landing pages, and generating ~40 report screens from one framework.
