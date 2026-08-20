---
doc_id: KB-053
title: Page Map — Existing Routes and Proposed Angular Routes
module: frontend-new
source_files:
  - V.SMART/V.SMART.Shared/Pages/
  - V.SMART/V.SMART.Shared/Layout/NavMenu.razor
status: complete
confidence: confirmed
last_verified: 2026-08-12
dependencies: [KB-015, KB-052]
---

# Page Map

**Left column: as-is (Confirmed — extracted from 440 `@page` directives across 333
components).** **Right column: proposal.**

## Route conventions

| Existing | Proposed |
|---|---|
| `/{entity}List` | `/{module}/{entity}` |
| `/{Entity}/create`, `/{Entity}/create/{ParentId:int}` | `/{module}/{entity}/new?parent={id}` |
| `/{Entity}/update/{Id:int}` | `/{module}/{entity}/{id}` (edit mode) |
| `/{entity}/details/{Id:int}` | `/{module}/{entity}/{id}?mode=view` |
| `/{module}-home`, `/{module}-master` | **removed** — sidebar + ⌘K palette |

Casing is normalised to kebab-case throughout (the existing app mixes `/MfgPO/create`,
`/mfgPO/details`, `/mfgPOList`).

---

## Authentication & shell

| Existing | Proposed |
|---|---|
| `/login` | `/login` |
| `/qrlogin/{token:guid}` | `/login/qr/{token}` |
| `/logout` | client action |
| `/register` | `/register` (**verify whether this is still wanted — Q-09**) |
| `/access-denied` | inline `PermissionDeniedState` |
| `/` | `/` → redirect to `/dashboard` |
| `/dashboard` | `/dashboard` |
| `/instantSearch` | **⌘K command palette** |
| `/master-home`, `/sales-home`, `/purchase-home`, `/production-home`, `/planning-home`, `/inventory-home`, `/qc-home`, `/maintenance-home`, `/hr-home`, `/accounts-home`, `/reports-home`, `/master-upload`, `/admin-master`, `/inventory-master`, `/general-master`, `/account-master`, `/hr-master`, `/hr-masterr`, `/settings-master`, `/assemblycosting-master` | **removed (20 routes)** |

> `/hr-master` and `/hr-masterr` both exist — a typo route. **Confirmed.**

## Masters — Admin

| Existing | Proposed |
|---|---|
| `/user`, `/user/create`, `/user/update/{UserId:int}` | `/admin/users`, `/admin/users/new`, `/admin/users/{id}` |
| `/userRights` | `/admin/permissions` |
| `/userLevelAuthorization`, `/userLevelAuthorization/create`, `/userLevelAuthorization/update/{Id:int}` | `/admin/approval-authority` |
| `/screenManagement` | `/admin/screens` |

## Masters — Inventory

| Existing | Proposed |
|---|---|
| `/category`, `/category/create`, `/category/update/{CategoryCode:int}` | `/masters/categories[/new\|/{id}]` |
| `/uom` | `/masters/uom` |
| `/stores`, `/store/create`, `/store/update/{StoreId:int}` | `/masters/stores[/new\|/{id}]` |
| `/store-map` | `/masters/store-mapping` |
| `/hsnMaster`, `/hsn/create`, `/hsn/update/{Slno:int}` | `/masters/hsn[/new\|/{id}]` |
| `/rawMaterialList`, `/rawMaterial/create`, `/rawMaterial/update/{RmId:int}` | `/masters/raw-materials[/new\|/{id}]` |
| `/itemList`, `/item/create`, `/item/update/{ItemId:int}` | `/masters/items[/new\|/{id}]` |
| `/bomList`, `/bom/create`, `/bom/update/{AssmblyID:int}`, `/bom/details/{AssmblyID:int}` | `/masters/bom[/new\|/{id}]` |
| `/bomLabourList`, `/bomLabour/create`, `/bomLabour/update/{AssmblyID:int}` | `/masters/bom-labour[/new\|/{id}]` |
| `/processList`, `/process/create`, `/process/update/{ProcessId:int}` | `/masters/processes[/new\|/{id}]` |
| `/factorlist`, `/factor/create`, `/factor/update/{Id:int}` | `/masters/factors[/new\|/{id}]` |
| `/groupingList`, `/grouping/create`, `/grouping/update/{Id:int}` | `/masters/groupings[/new\|/{id}]` |
| `/ItemRate_Updation` | `/masters/items/bulk-price-update` |
| `/bom-upload`, `/bom-labour-upload` | `/masters/bom/import`, `/masters/bom-labour/import` |

## Masters — General

| Existing | Proposed |
|---|---|
| `/customer`, `/customer/create`, `/customer/update/{CustId:int}` | `/masters/customers[/new\|/{id}]` |
| `/vendor`, `/vendor/create`, `/vendor/update/{VendorCode:int}` | `/masters/vendors[/new\|/{id}]` |
| `/machineList`, `/machine/create`, `/machine/update/{MachineId:int}` | `/masters/machines[/new\|/{id}]` |
| `/terms-and-conditions`, `/terms-and-conditions/create`, `/terms-and-conditions/update/{Id:int}` | `/masters/terms[/new\|/{id}]` |
| `/state` | `/masters/states` |
| `/contractReviewMasterList`, `/reviewMaster/create`, `/reviewMaster/update/{SlNo:int}` | `/masters/contract-review[/new\|/{id}]` |
| `/RejectionMaster` | `/masters/rejection-reasons` |
| `/myCompany`, `/myCompany/update/{CompanyId:int}` | `/settings/company` |

## Masters — Accounts

| Existing | Proposed |
|---|---|
| `/expense`, `/expense/create`, `/expense/update/{ExpenseCode:int}` | `/masters/expenses[/new\|/{id}]` |
| `/income`, `/income/create`, `/income/update/{IncomeCode:int}` | `/masters/incomes[/new\|/{id}]` |
| `/bank`, `/bank/update/{BankId:int?}` | `/masters/banks[/{id}]` |
| `/currency`, `/currency/create`, `/currency/update/{CurrId:int}` | `/masters/currencies[/new\|/{id}]` |
| `/currency_today` | `/masters/currency-rates` |
| `/project-type-master` | `/masters/project-types` |
| `/costcenter`, `/costCenter/create`, `/costCenter/update/{Id:int}` | `/masters/cost-centres[/new\|/{id}]` |

## Masters — HR

| Existing | Proposed |
|---|---|
| `/candidate`, `/candidate/create`, `/candidate/update/{CandidateID:int}` | `/hr/candidates[/new\|/{id}]` |
| `/Staff`, `/employee-details/create`, `/employee-details/create/candidate/{CandidateID:int}`, `/employee-details/update/{StaffID:int}` | `/hr/employees[/new\|/{id}]` |
| `/hrMaster/holidayList` | `/hr/holidays` |
| `/leaveType`, `/leaveType/create`, `/leaveType/update/{LeaveTypeId:int}` | `/hr/leave-types[/new\|/{id}]` |
| `/employeeLeaveBalance`, `/employeeLeaveBalance/create`, `/employeeLeaveBalance/update/{LeaveBalanceID:int}` | `/hr/leave-balances[/new\|/{id}]` |
| `/shiftAllocation`, `/shiftAllocation/create`, `/shiftAllocation/update/{ShiftId:int}` | `/hr/shifts[/new\|/{id}]` |

## Settings

`/generalSettings` → `/settings/general` · `/print-management` → `/settings/printing` ·
`/InspectionSettings` → `/settings/inspection` · `/production-log-settings` →
`/settings/production-log` · `/print-salaryHeadSettings` → `/settings/payroll-print` ·
`/biometric-Excel` → `/settings/biometric-import` · `/store-map` → see Masters

## Sales

| Existing | Proposed |
|---|---|
| `/Leads`, `/Leads-details/create`, `/Leads-details/update/{LeadId:int}` | `/sales/leads[/new\|/{id}]` |
| `/enquirySales`, `/enquirySales/create[/{CustId:int}]`, `/enquirySales/update/{EnquiryId:int}`, `/enquiryDetails/{EnquiryId:int}` | `/sales/enquiries[/new\|/{id}]` |
| `/enquiryfeasibility`, `/enquiryFeasibility/create[/{refEnqId:int}]`, `/enquiryFeasibility/update/{FesId:int}`, `/enquiryFeasibiltyDetails/{FesId:int}` | `/sales/feasibility[/new\|/{id}]` |
| `/mfgQuoteList`, `/MfgQuote/create[/{CustId:int}]`, `/MfgQuote/update/{QuoteId:int}`, `/quote/details/{QuoteId:int}` | `/sales/quotations[/new\|/{id}]` |
| `/mfgPOList`, `/MfgPO/create[/{CustId:int}]`, `/MfgPO/update/{PoId:int}`, `/mfgPO/details/{PoId:int}` | `/sales/orders[/new\|/{id}]` |
| `/contractReviewCheckList`, `/contractReviewCheckList/create`, `/contractReviewCheckList/update/{Id:int}` | `/sales/contract-reviews[/new\|/{id}]` |
| `/PerformaInvList`, `/PerformaInv/create[/{CustId:int}]`, `/PerformaInv/update/{InvId:int}`, `/performaInv/details/{InvId:int}` | `/sales/proforma-invoices[/new\|/{id}]` |

## Manufacturing Work

| Existing | Proposed |
|---|---|
| `/mfgDcList`, `/MfgDc/create[/{CustId:int}]`, `/MfgDc/update/{DcId:int}`, `/mfgDc/details/{DcId:int}` | `/sales/delivery-challans[/new\|/{id}]` |
| `/mfgInvList`, `/mfgInv/create[/{CustId:int}]`, `/mfgInv/update/{InvId:int}`, `/mfgInv/details/{InvId:int}` | `/sales/invoices[/new\|/{id}]` |
| `/expInvList`, `/expInv/create[/{CustId:int}]`, `/expInv/update/{ExpInvId:int}`, `/expInv/details/{ExpInvId:int}` | `/sales/export-invoices[/new\|/{id}]` |

## Labour Work

| Existing | Proposed |
|---|---|
| `/labourGRNList`, `/labourGRN/create[/{CustId:int}]`, `/labourGRN/update/{GRNId:int}`, `/labourGRN/details/{GRNId:int}` | `/labour/grn[/new\|/{id}]` |
| `/labourSCNList`, `/labourSCN/create[/{CustId:int}]`, `/labourSCN/update/{SCNId:int}`, `/labourSCN/details/{SCNId:int}` | `/labour/scn[/new\|/{id}]` |
| `/labourDcoutgoingList`, `/labourDcOutgoing/create[/{CustId:int}]`, `/labourDcOutgoing/update/{DcId:int}`, `/labourDcOutgoing/details/{DcId:int}` | `/labour/delivery-challans[/new\|/{id}]` |
| `/LabourInvoiceList`, `/LabourInvoice/create[/{CustId:int}]`, `/LabourInvoice/update/{LabInvId:int}`, `/LabourInvoice/details/{LabInvId:int}` | `/labour/invoices[/new\|/{id}]` |
| `/creditNoteList`, `/creditNote/create[/{CustId:int}]`, `/creditNote/update/{CrId:int}`, `/creditNote/details/{CrId:int}` | `/sales/credit-notes[/new\|/{id}]` |

## Out Sourcing / Purchase / Sub Contract

| Existing | Proposed |
|---|---|
| `/materialRequisitionList`, `/materialRequisition/create`, `/materialRequisition/update/{MreqId:int}`, `/materialRequisition/details/{MReqId:int}` | `/purchasing/requisitions[/new\|/{id}]` |
| `/enquiryPurchase`, `/enquiryPurchase/create[/{VendorCode:int}]`, `/enquiryPurchase/update/{EnquiryId:int}`, `/enquiryPurchaseDetails/{EnquiryId:int}` | `/purchasing/enquiries[/new\|/{id}]` |
| `/purchaseQuoteList`, `/PurchaseQuote/create[/{VendorCode:int}]`, `/PurchaseQuote/update/{QuoteId:int}`, `/purchasequote/details/{QuoteId:int}` | `/purchasing/quotations[/new\|/{id}]` |
| `/rate-comparison` | `/purchasing/price-comparison` |
| `/PurchPOList`, `/PurchPO/create[/{VendorCode:int}]`, `/PurchPO/multi-vendor-create`, `/PurchPO/update/{PoId:int}`, `/PurchasePODetails/{PoId:int}` | `/purchasing/orders[/new\|/new-multi-vendor\|/{id}]` |
| `/purchaseGRNList`, `/purchaseGRN/create[/{VendorCode:int}]`, `/purchaseGRN/update/{GRNId:int}`, `/purchaseGRN/details/{GRNId:int}` | `/purchasing/grn[/new\|/{id}]` |
| `/purchaseSCNList`, `/purchaseSCN/create[/{VendorCode:int}]`, `/purchaseSCN/update/{SCNId:int}`, `/purchaseSCN/details/{SCNId:int}` | `/purchasing/scn[/new\|/{id}]` |
| `/purchaseInvoiceList`, `/purchaseInvoice/create[/{VendorCode:int}]`, `/purchaseInvoice/update/{InvId:int}`, `/purchaseInvoice/details/{InvId:int}` | `/purchasing/invoices[/new\|/{id}]` |
| `/subConDcOutList`, `/DcSubConOutgoing/create/{VendorCode:int}`, `/DcSubConOutgoing/update/{DcId:int}`, `/SubConDcOut/details/{DcId:int}` | `/subcontract/delivery-challans[/new\|/{id}]` |
| `/subConGrnList`, `/DcSubConIncoming/create/{VendorCode:int}`, `/DcSubConIncoming/update/{GRNId:int}`, `/DcSubConIncoming/details/{GRNId:int}` | `/subcontract/grn[/new\|/{id}]` |
| `/subConScnList`, `/DcSubConSCN/create/{VendorCode:int}`, `/DcSubConSCN/update/{SCNId:int}`, `/SubconSCN/details/{SCNId:int}` | `/subcontract/scn[/new\|/{id}]` |
| `/subConInvList`, `/DcSubConIncomingInvoice/create/{VendorCode:int}`, `/DcSubConIncomingInvoice/update/{InvId:int}`, `/DcSubConIncomingInvoice/details/{InvId:int}` | `/subcontract/invoices[/new\|/{id}]` |
| `/debitNoteList`, `/debitNote/create[/{VendorCode:int}]`, `/debitNote/update/{DbId:int}`, `/debitNote/details/{DbId:int}` | `/purchasing/debit-notes[/new\|/{id}]` |

## Planning

| Existing | Proposed |
|---|---|
| `/approval` | `/approvals` |
| `/jobOrderList`, `/jobOrder/create`, `/jobOrder/update/{jobId:int}`, `/jobOrder/details/{JobId:int}` | `/planning/job-orders[/new\|/{id}]` |
| `/routeCardList`, `/routeCard/create`, `/routeCard/update/{RCId:int}`, `/routeCard/details/{RcId:int}` | `/planning/route-cards[/new\|/{id}]` |
| `/rcReleaseList`, `/rcRelease/create`, `/rcRelease/update/{RcReleaseId:int}` | `/planning/route-card-releases[/new\|/{id}]` |
| `/estimationList`, `/estimation/create/{CustId:int?}`, `/estimation/update/{EstiamateId:int?}`, `/estimationDetails/details/{EstiamateId:int}` | `/planning/estimations[/new\|/{id}]` |
| `/materialReqAnalysis` | `/planning/material-requirements` |
| `/assemblyReqAnalysis` | `/planning/assembly-requirements` |
| `/costcalculator`, `/LabourCostManagement` | `/planning/cost-calculator`, `/planning/labour-costs` |

## Production

| Existing | Proposed |
|---|---|
| `/productionIssueList`, `/productionIssueAssy/create`, `/productionIssueAssy/update/{IssueId:int}`, `/productionIssueAssy/details/{IssueId:int}` | `/production/assembly/issues[/new\|/{id}]` |
| `/productionReturnAssyList`, `/productionReturnAssy/create`, `/productionReturnAssy/update/{ReturnId:int}`, `/productionReturnAssy/details/{ReturnId:int}` | `/production/assembly/returns[/new\|/{id}]` |
| `/productionAssySCNList`, `/productionAssyScn/create`, `/productionAssyScn/update/{SCNId:int}`, `/productionSCNAssy/details/{SCNId:int}` | `/production/assembly/scn[/new\|/{id}]` |
| `/productionCompIssueList`, `/ProductionIssueComp/create`, `/ProductionIssueComp/update/{IssueId:int}`, `/productionIssueComp/details/{IssueId:int}` | `/production/component/issues[/new\|/{id}]` |
| `/productionCompReturnList`, `/productionReturnComp/create`, `/productionReturnComp/update/{ReturnId:int}`, `/productionReturnComp/details/{ReturnId:int}` | `/production/component/returns[/new\|/{id}]` |
| `/productionCompSCNList`, `/productionCompScn/create`, `/productionCompScn/update/{SCNId:int}`, `/productionCompScn/details/{SCNId:int}` | `/production/component/scn[/new\|/{id}]` |
| `/productionLogList[/{IsProduction:bool}]`, `/productionLog/create[/{IsProduction:bool}]`, `/productionLog/update/{LogId:int}[/{IsProduction:bool}]`, `/productionlog/details/{ProductionlogId:int}[/{IsProduction:bool}]`, `/productionLogStop/create`, `/productionLogStop/update/{Id:int}` | `/shopfloor/production-log[/new\|/{id}]` — **dedicated touch UI** |

## Inventory / Stock

| Existing | Proposed |
|---|---|
| `/stockIssReqList`, `/stockIssReq/create`, `/stockIssReq/update/{RequestId:int}`, `/stockIssReq/details/{ReqID:int}` | `/inventory/issue-requests[/new\|/{id}]` |
| `/minIssList`, `/minIss/create`, `/minIss/update/{MINId:int}`, `/minIss/details/{MINId:int}` | `/inventory/consumable-issues[/new\|/{id}]` |
| `/scnGenList`, `/scnGen/create`, `/scnGen/update/{SCNGenId:int}`, `/scnGen/details/{SCNGenId:int}` | `/inventory/stock-transfer-notes[/new\|/{id}]` |
| `/storeIntertransList`, `/storeInterTrans/create`, `/storeInterTrans/update/{ISTId:int}`, `/storeInterTrans/details/{ISTId:int}` | `/inventory/inter-store-transfers[/new\|/{id}]` |
| `/tcIssueList`, `/tci/create`, `/tci/update/{TCIssueId:int}`, `/toolcribissueDetails/{TCId:int}` | `/inventory/tool-crib/issues[/new\|/{id}]` |
| `/tcReturnList`, `/tcr/create`, `/tcr/update/{TCReturnId:int}`, `/tcr/details/{DcId:int}` | `/inventory/tool-crib/returns[/new\|/{id}]` |
| `/stockPosition`, `/stockPositionInternalExternal` | `/inventory/stock-position`, `/inventory/stock-position-wip` |
| `/bomPRScn` | `/inventory/bom-explosion` |

## Inspection / QC · Maintenance

| Existing | Proposed |
|---|---|
| `/MasterInspection`, `/MasterInspection/create`, `/MasterInspection/update/{InspectId:int}`, `/MasterInspection/details/{InspectionId:int}` | `/quality/inspection-masters[/new\|/{id}]` |
| `/DefectInfo`, `/DefectInfo/create`, `/DefectInfo/update/{DefectId:int}` | `/quality/defects[/new\|/{id}]` |
| `/IncomingInspection`, `/IncomingInspection/create`, `/IncomingInspection/update/{InspectId:int}`, `/IncomingInspection/details/{InspectionId:int}` | `/quality/incoming-inspections[/new\|/{id}]` |
| `/FinalInspection`, `/FinalInspection/create`, `/FinalInspection/update/{InspectId:int}`, `/FinalInspection/details/{InspectionId:int}` | `/quality/final-inspections[/new\|/{id}]` |
| `/MaintenanceSchedule` | `/maintenance/schedules` |
| `/MaintenanceProcess`, `/maintenanceprocess/details/`, `/maintenanceprocess/maintenancereport/` | `/maintenance/processes[/{id}]`, `/maintenance/report` |
| `/BreakdownMaintenance`, `/BreakdownMaintenance/create`, `/BreakdownMaintenance/update/{BreakdownId:int}` | `/maintenance/breakdowns[/new\|/{id}]` |
| `/CalibrationMaintenance`, `/CalibrationMaintenance/create`, `/CalibrationMaintenance/update/{CalibrateId:int}`, `/CalibrationMaintenance/CalibrationMaintenanceDueList` | `/maintenance/calibrations[/new\|/{id}\|/due]` |

## Human Resources

| Existing | Proposed |
|---|---|
| `/leaveApplication`, `/leaveApplication/create`, `/leaveApplication/update/{LeaveAppID:int}`, `/leaveApplication/authorize/{LeaveAppID:int}`, `/leaveDetails/{leaveAppId:int}` | `/hr/leave-applications[/new\|/{id}]`; authorise via `/approvals` |
| `/attendanceList`, `/attendance/create`, `/attendance/update/{Id:int}`, `/attendance/details/{Id:int}` | `/hr/attendance[/new\|/{id}]` |
| `/salaryList`, `/salary/create`, `/salary/update/{RowId:int}`, `/salary/details/{RowId:int}` | `/hr/payroll[/new\|/{id}]` |
| `/staffLoanList`, `/staffLoan/create`, `/staffLoan/update/{LoanId:int}` | `/hr/staff-loans[/new\|/{id}]` |
| `/offer_Letter`, `/offer_Letter/create`, `/offer_Letter/update/{OfferId:int}`, `/offer_Letter/details/{OfferId:int}` | `/hr/offer-letters[/new\|/{id}]` |
| `/appointment_Letter`, `/appointment_Letter/create`, `/appointment_Letter/update/{AppointmentId:int}`, `/appointment_Letter/details/{AppointmentId:int}` | `/hr/appointment-letters[/new\|/{id}]` |

## Accounts / Cash Flow

| Existing | Proposed |
|---|---|
| `/PaymentList`, `/Payments/Create`, `/Payments/update/{PaymentId:int}`, `/PaymentDetails/{PaymentId:int}` | `/accounts/payments[/new\|/{id}]` |
| `/ReceiptList`, `/Receipts/Create`, `/Receipts/update/{ReceiptId:int}`, `/ReceiptDetails/{ReceiptId:int}` | `/accounts/receipts[/new\|/{id}]` |
| `/AdvanceAdjustmentList`, `/AdvanceAdjustment/Create`, `/AdvanceAdjustment/update/{AdjumentId:int}` | `/accounts/advance-adjustments[/new\|/{id}]` |
| `/Fundtransactions` | `/accounts/bank-transactions` |
| `/serviceBills`, `/ServiceBills/create`, `/ServiceBills/update/{ServiceId:int}`, `/ServiceBills/details/{ServiceId:int}` | `/accounts/service-bills[/new\|/{id}]` |

## Utilities

`/correspondenceList` → `/utilities/correspondence` ·
`/correspondence/upload` → `/utilities/correspondence/upload` ·
`/correspondence/update/{CorresId:int?}` → `/utilities/correspondence/{id}` ·
`/correspondence/by-reference` → drawer within each document

## Reports

All become `/reports/{slug}` driven by one `ReportPage` framework.

| Existing | Proposed slug |
|---|---|
| `/confirmationaccounts` | `confirmation-of-accounts` |
| `/PaidBills` · `/Pending_Bills` · `/PendingStatements` | `bills-paid` · `bills-pending` · `pending-statements` |
| `/Profit_LossAccounts` | `profit-loss` |
| `/taxDetailsList` · `/tdsSummaryReport` · `/hsnCodeSummaryReport` · `/creditdebitSummaryReport` | `tax-details` · `tds-summary` · `hsn-summary` · `credit-debit-summary` |
| `/summarygraphs` · `/GSTITC` · `/Daybook` | `summary-graphs` · `gst-itc-04` · `daybook` |
| `/SalesTrack` · `/labourTrack` · `/dc-inout-report` · `/po-tally` · `/Purchase-Sales_Track` · `/joborderAnalysis` | `sales-track` · `labour-track` · `dc-in-out` · `po-track` · `purchase-sales-track` · `job-order-track` |
| `/StockLedger` · `/StockAnalysis` · `/Rejection` · `/RouteCardAnalysis` · `/ItemWiseReport` · `/ItemModificationReport` | `stock-ledger` · `stock-analysis` · `rejection-analysis` · `route-card-analysis` · `item-usage` · `item-modifications` |
| `/SalesPoPending` · `/labourpending` · `/ProductionPending` | `sales-order-pending` · `labour-pending` · `production-pending` |
| `/Ratings` · `/prpoRating` | `vendor-ratings` · `pr-po-rating` |
| `/ToolCribReport` | `tool-crib-summary` |

---

## Counts

| | Existing | Proposed |
|---|---|---|
| Routes | **440** | ~220 |
| Components/screens | **333** | ~140 |

Reduction sources: create/update/details collapse into one route with a mode; ~20 landing
pages deleted; ~40 report screens generated from one framework; `/instantSearch` becomes
a palette.
