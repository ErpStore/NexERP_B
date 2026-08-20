---
doc_id: KB-020
title: ERP Module Inventory and Dependency Graph (As-Is)
module: all
source_files:
  - V.SMART/V.SMART.Shared/Layout/NavMenu.razor
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
  - V.SMART/V.SMART.Shared/Pages/
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/
  - V.SMART/V.SMART.Web/Program.cs
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-12
dependencies: [KB-012, KB-013, KB-015]
---

# ERP Module Inventory and Dependency Graph (As-Is)

Derived from three independent sources that agree: the `NavMenu.razor` tree (user-facing
grouping), the `Data/` folder structure (entity ownership), and the
`BusinessLayer/BusinessService/` folder structure (service ownership).

## Module dependency graph

Arrows mean "consumes documents/lines from". Built from the `Ref*SubId` foreign keys in
`Data/` and the delete-guard chains in the services.

```
MASTERS  (Item · BOM/AssmblyDef · Process · Machine · Store · Customer · Vendor ·
          HSN · Currency · CostCenter · Terms · Staff · UOM · Category)
   │  every transactional module depends on Masters
   ▼
SALES
 ├── Leads
 ├── Sales Enquiry ──────────► Enquiry Feasibility
 │                                   │
 │                                   ▼
 ├── Sales Quotation (MfgQuote) ◄─────┘
 │        │  RefQuoteSubId
 │        ▼
 ├── Sales Order (MfgPo) ──────► Contract Review
 │        │  RefPoSubId (20 consumers — the busiest link in the system)
 │        ├──────────────► Proforma Invoice
 │        ├──────────────► Sales DC (MfgDc) ──► Domestic Tax Invoice (MfgInv)
 │        │                                 └─► Export Invoice (ExpInv)
 │        ├──────────────► Route Card (Planning)
 │        └──────────────► Job Order (Planning)
 └── Credit Note ◄── (against MfgInv / LabInv)

LABOUR WORK  (customer-supplied material, job-work)
 ├── Labour GRN (material received from customer)
 │        │  RefGRNSubId
 │        ▼
 ├── Labour SCN  ──► stock addition
 ├── Labour DC Outgoing (return to customer) ──► Labour Invoice
 └── Credit Note

OUT SOURCING / PURCHASE
 ├── Material Requisition (Indent) ──► Purchase Enquiry
 │        RefMReqSubId                     │  RefEnqSubId
 │                                         ▼
 │                                 Vendor Quotation (PurchaseQuote)
 │                                         │  ← Price Comparison screen
 │                                         ▼
 ├────────────────────────────────► Purchase Order (PurchPo)
 │                                         │  RefPoSubId
 │                                         ▼
 │                                 Purchase GRN ──► Purchase SCN ──► STOCK
 │                                         │
 │                                         ▼
 │                                 Purchase Invoice ──► Debit Note

SUB CONTRACT  (own material sent out for processing)
 ├── SubCon DC-Out (material issued to vendor)  [consumes STOCK]
 │        │  RefDcSubId
 │        ▼
 ├── SubCon GRN (material received back) ──► SubCon SCN ──► STOCK
 │                                              │
 │                                              ▼
 └────────────────────────────────────► SubCon Invoice ──► Debit Note

PLANNING
 ├── Estimation
 ├── Job Order (assembly)  ◄── Sales Order
 │        │  RefJobOrderId
 │        ▼
 ├── Route Card (component operations) ──► Route Card Release
 │        │  RefRcSubId — routes stock to a specific operation
 │        ▼
 ├── Material Requirement Analysis  ──► drives Material Requisition
 ├── Assembly Requirement Analysis
 └── Authorisation (approval queue for all approvable documents)

PRODUCTION / SHOP FLOOR
 ├── Assembly:  Issue (Jobcard MIN) ──► Return GRN ──► SCN Assembly ──► STOCK
 ├── Component: Issue (Product MIN) ──► Return GRN ──► SCN Component ──► STOCK
 └── Daily Production Log  (machine × operator × operation × qty)

INVENTORY / STOCK  ◄── every module above adds to or issues from here
 ├── StockAdd / StockIssue / StockIssueTrack   (the ledger — FIFO)
 ├── Stock Issue-Request ──► Material Issue Note
 ├── Stock Transfer Note (SCNGen)
 ├── Inter-Store Transfer
 ├── Tool Crib Issue ──► Tool Crib Return
 ├── Consumable Issue Note
 └── Stock Position (Internal / with WIP)

INSPECTION / QC
 ├── Inspection Master · Defects Master
 ├── GRN Inspection (Incoming) ◄── Purchase GRN / SubCon GRN
 └── Final Inspection ◄── Production

MAINTENANCE
 ├── Machine Maintenance Schedule ──► Maintenance Process
 ├── Breakdown Maintenance
 └── Gauges Calibration & History

HUMAN RESOURCES
 ├── Candidate ──► Offer Letter ──► Appointment Letter ──► Employee (Staff)
 ├── Leave Type · Leave Allocation ──► Leave Application (approvable)
 ├── Attendance (biometric Excel import) ──► Salary/Payroll
 └── Staff Loan ──► deductions in Salary

CASH FLOW / ACCOUNTS
 ├── Payments ──► Advance Adjustment
 ├── Receipts
 ├── Bank Transactions (FundTrans)
 └── Service Bills

REPORTS  (read-only, driven by 94 stored procedures — depends on everything)
UTILITIES  (Correspondence — attachments against any document)
```

## Module table

Legend for **Frontend complexity**: how hard the *Angular rebuild* is, driven mainly by how
much logic sits in `@code` and how many upstream document pickers are involved.

| # | Module | Purpose | Key entities | Screens | Service folder | Complexity |
|---|---|---|---|---|---|---|
| 1 | **Masters — Admin** | users, screen rights, approval authority | `User`, `UserRight`, `UserAuthority`, `Screens` | `/user`, `/userRights`, `/userLevelAuthorization` | `MasterService/AdminService` | Medium |
| 2 | **Masters — Inventory** | item/BOM/process catalogue | `Item`, `ItemSub`, `AssmblyDef`, `AssemblyDefLabour`, `RawMaterial`, `Category`, `UOM`, `Store`, `StoreMap`, `HSNMaster`, `Process`, `ProcessFlowChart`, `Factor`, `Grouping` | `/itemList`, `/bomList`, `/rawMaterialList`, `/processList`, `/stores`, `/store-map`, `/hsnMaster`, `/category`, `/uom`, `/factorlist`, `/groupingList`, `/ItemRate_Updation` | `MasterService/InventoryService` | **High** (ItemUpsert = 4,731 LOC; BOM tree) |
| 3 | **Masters — General** | parties & terms | `Customer`, `CustomerIndirect`, `ContactPerson`, `Vendor`, `VendorContact`, `VendorInDirect`, `State`, `TermsAndConditions`, `ContractReviewMaster`, `RejectionMaster` | `/customer`, `/vendor`, `/machineList`, `/terms-and-conditions`, `/state`, `/master-upload`, `/contractReviewMasterList`, `/RejectionMaster` | `MasterService/GeneralService` | **Low–Medium** ← best first migration |
| 4 | **Masters — Accounts** | financial reference data | `Expense`, `Income`, `Banks`, `Currency`, `CurrencyToday`, `CostCenter`, `ProjectTypeMaster` | `/expense`, `/income`, `/bank`, `/currency`, `/currency_today`, `/costcenter`, `/project-type-master` | `MasterService/AccountsService` | **Low** ← already has an API |
| 5 | **Masters — HR** | people reference data | `Staff`, `Candidate`, `LeaveType`, `EmployeeLeaveBalance`, `HolidayList`, `ShiftAllocation`, `StaffFamilyDetails`, `StaffEducation`, `StaffEmergency` | `/Staff`, `/candidate`, `/leaveType`, `/employeeLeaveBalance`, `/hrMaster/holidayList`, `/shiftAllocation` | `MasterService/HRMasterService` | Medium |
| 6 | **Masters — Settings** | screen catalogue, general settings, print setup | `ScreenManagement`, `PrintSetting`, `InspectionSettings`, `ProductionLogSetting`, `HRMasterSetting`, `BiometricExcelSetting`, `SalaryHeadPrintSetting` | `/screenManagement`, `/generalSettings`, `/print-management`, `/InspectionSettings`, `/production-log-settings`, `/print-salaryHeadSettings`, `/biometric-Excel` | `SettingsService` | Medium |
| 7 | **Assembly Costing** | labour cost & BOM estimation | `AssemblyDefLabour`, `AssemblyCharge`, `Companydetails` | `/bomLabourList`, `/LabourCostManagement`, `/costcalculator`, `/myCompany` | `MasterService/InventoryService`, `CostingService` | **High** (calculation-dense) |
| 8 | **Sales** | pre-order pipeline → order | `Leads`, `EnquirySales(+Sub)`, `EnquiryFeasibility(+Sub)`, `MfgQuote(+Sub)`, `MfgPo(+Sub)`, `PoType`, `ContractReview(+Sub)`, `PerformaInv(+Sub)` | `/Leads`, `/enquirySales`, `/enquiryFeasibility`, `/mfgQuoteList`, `/mfgPOList`, `/contractReviewCheckList`, `/PerformaInvList` | `SalesService`, `LeadService` | **Very High** |
| 9 | **Manufacturing Work** | despatch & billing of own goods | `MfgDc(+Sub)`, `MfgInv(+Sub)`, `ExpInv(+Sub)` | `/mfgDcList`, `/mfgInvList`, `/expInvList` | `SalesService` | **Very High** (e-Invoice + e-Way Bill) |
| 10 | **Labour Work** | job-work on customer material | `LabourGRN(+Sub)`, `LabourSCN(+Sub)`, `LabourDcOutgoing(+Sub)`, `LabInv(+Sub)`, `CreditNote(+Sub)`, `LabourDcReturnCompTrack` | `/labourGRNList`, `/labourSCNList`, `/labourDcoutgoingList`, `/LabourInvoiceList`, `/creditNoteList` | `LabourServices` | **Very High** (6,528-LOC page) |
| 11 | **Out Sourcing** | procure-to-order | `MaterialReq(+Sub)`, `EnquiryPurchase(+Sub)`, `EnquiryPurchaseVendorAssign`, `PurchaseQuote(+Sub)`, `PurchPo(+Sub)` | `/materialRequisitionList`, `/EnquiryPurchase`, `/purchaseQuoteList`, `/rate-comparison`, `/PurchPOList` | `OutSourcingService` | **High** |
| 12 | **Purchase** | receipt-to-pay | `PurchaseGRN(+Sub)`, `PurchaseSCN(+Sub)`, `PurchaseInvoice(+Sub)` | `/purchaseGRNList`, `/purchaseSCNList`, `/purchaseInvoiceList` | `OutSourcingService` | **High** |
| 13 | **Sub Contract** | outsourced processing | `SubConDcOut(+Sub)`, `SubConGRN(+Sub, Track)`, `SubConSCN(+Sub)`, `SubConInv(+Sub)`, `DebitNote(+Sub)` | `/subConDcOutList`, `/subConGrnList`, `/subConScnList`, `/subConInvList`, `/debitNoteList` | `OutSourcingService` | **Very High** (5,631-LOC service) |
| 14 | **Planning** | order → shop-floor plan | `JobOrder(+Sub)`, `RouteCard(+Sub)`, `RouteCardRelease(+Sub)`, `Estimate(+Sub)`, `ApprovalHistory` | `/jobOrderList`, `/routeCardList`, `/rcReleaseList`, `/estimationList`, `/materialReqAnalysis`, `/assemblyReqAnalysis`, `/approval` | `PlanningService` | **Very High** |
| 15 | **Production — Assembly** | assembly execution | `ProductionIssueAssy(+Sub)`, `ProductionReturnAssy(+Sub, Track)`, `ProductionSCNAssy(+Sub)` | `/productionIssueList`, `/productionReturnAssyList`, `/productionAssySCNList` | `ProductionService` | **High** |
| 16 | **Production — Component** | component execution + shop-floor log | `ProductionIssueComp(+Sub)`, `ProductionReturnComp(+Sub, Track)`, `ProductionSCNComp(+Sub)`, `ProductionLog(+Sub)` | `/productionCompIssueList`, `/productionCompReturnList`, `/productionCompSCNList`, `/productionLogList` | `ProductionService` | **High** (+ dedicated operator UI) |
| 17 | **Inventory / Stock** | the stock ledger | `StockAdd`, `StockIssue`, `StockIssueTrack`, `SCNGen(+Sub)`, `MaterialIssNote(+Sub)`, `StoreInterTrans(+Sub)`, `ToolCribIssue/Return(+Sub)`, `StockIssueRequest(+Sub)` | `/stockIssReqList`, `/stockPosition`, `/stockPositionInternalExternal`, `/scnGenList`, `/minIssList`, `/tcIssueList`, `/tcReturnList`, `/storeIntertransList`, `/bomPRScn` | `InventoryService` | **High** (FIFO correctness) |
| 18 | **Inspection / QC** | incoming & final inspection | `MasterInspection`, `InspectionRef`, `DefectInfo`, `IncomingInspection(+Ref)`, `FinalInspection(+Ref)` | `/MasterInspection`, `/DefectInfo`, `/IncomingInspection`, `/FinalInspection` | `InspectionService` | Medium |
| 19 | **Maintenance** | machine & gauge upkeep | `MaintenanceSchedule`, `MaintenanceProcess`, `BreakdownMaintenance`, `InstrumentDetails`, `CalibrationHistory` | `/MaintenanceSchedule`, `/MaintenanceProcess`, `/BreakdownMaintenance`, `/CalibrationMaintenance` | `MaintenanceService` | Medium |
| 20 | **Human Resources** | leave, attendance, payroll | `LeaveApplication`, `DailyLeaveRecord`, `Attendance`, `Salary`, `StaffLoan`, `OfferLetter(+Sub)`, `AppointmentLetter(+Sub)` | `/leaveApplication`, `/attendanceList`, `/salaryList`, `/staffLoanList`, `/offer_Letter`, `/appointment_Letter` | `HumanResourceService` | **High** (payroll) |
| 21 | **Cash Flow / Accounts** | money in/out | `Payments(+Sub)`, `Receipts(+Sub)`, `Advaceadjustment(+Sub)`, `FundTrans`, `ServiceBills(+Sub)` | `/PaymentList`, `/ReceiptList`, `/AdvanceAdjustmentList`, `/serviceBills`, `/Fundtransactions` | `AccountsService`, `CashFlowService` | **High** (TDS, adjustments) |
| 22 | **Reports** | 40+ read-only reports | keyless projections | `/SalesTrack`, `/labourTrack`, `/StockLedger`, `/StockAnalysis`, `/taxDetailsList`, `/GSTITC`, `/Ratings`, `/Daybook`, `/Profit_LossAccounts`, … | `ReportService/*` | **Medium** (uniform pattern) |
| 23 | **Dashboard** | KPI widgets | — | `/dashboard`, `/summarygraphs` | `DashboardService` (3,095 LOC) | Medium |
| 24 | **Utilities** | attachments | `Correspondence` | `/correspondenceList`, `/correspondence/upload`, `/correspondence/by-reference` | `UtilitiesRepository` | Low |
| 25 | **E-Invoice / E-Way** | GST statutory filing | e-Invoice + e-Way payloads | embedded in invoice/DC screens | `EInvoiceAPIService` | **High** (external, stateful) |

## Cross-module shared services

| Service | Used by | Why it matters |
|---|---|---|
| `ICalculationService` | every document module | one tax/total engine — **preserve verbatim** |
| `IStockManagerService` | Purchase SCN, SubCon SCN, Labour SCN, Production SCN, SCNGen, MIN, Tool Crib, Inter-Store Transfer, Stock Issue Request | the FIFO ledger — **highest-risk shared code** |
| `ICommonService` (2,579 LOC) | most modules | print settings, lookups, shared helpers |
| `IApprovalService` | Quotation, Sales Order, PR, PO, SCN ×4, Leave, Route Card, Stock Request | multi-level approval |
| `ReportService` / `IReportExecutor` | all reporting + every document print | FastReport + SPs |
| `ForeignKeyUsageChecker` | delete paths | referential-integrity guard |
| `IColumnPreferenceService` | all list screens | per-user grids |
| `ICorrespondanceRepository` | all document screens | attachments against any `RefId`/`RefType` |
| `ICostingService` | Sales Order, BOM costing | costing |

## Recommended migration order (rationale in [`migration/migration-strategy.md`](../migration/migration-strategy.md))

1. **Masters — Accounts** (Currency already proven end-to-end)
2. **Masters — General** (Customer, Vendor) — low logic, high visibility
3. **Masters — Inventory** (Item, BOM) — unlocks every document module
4. **Sales** (Enquiry → Quotation → Sales Order)
5. **Out Sourcing + Purchase**
6. **Inventory / Stock** (must follow, because SCN screens write the ledger)
7. **Planning + Production**
8. **Sub Contract, Labour Work**
9. **Accounts, HR, QC, Maintenance**
10. **Reports + Dashboard** (can run in parallel from step 3 — they are read-only)
