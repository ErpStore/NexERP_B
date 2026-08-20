---
doc_id: KB-012
title: Database and Data Model (As-Is)
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs
  - V.SMART/V.SMART.Shared/Data/MasterDbContext.cs
  - V.SMART/V.SMART.Shared/Data/TenantInfo.cs
  - V.SMART/V.SMART.Shared/Migrations/
  - db/stored-procedures/
  - db/deploy-stored-procedures.ps1
entities: [TenantInfo, User, Item, Customer, Vendor, MfgPo, MfgPoSub, StockAdd, StockIssue, StockIssueTrack, Screens, UserRight]
api_endpoints: []
database_tables: [Tenants, Users, UserRights, Screens, Item, Customer, Vendor, MfgPo, MfgPoSub, StockAdd, StockIssue, StockIssueTrack]
business_rules: [BR-STK-001]
status: complete
confidence: confirmed
last_verified: 2026-08-13
dependencies: [KB-010, KB-013]
---

# Database and Data Model (As-Is)

## Two contexts

| Context | DbSets | Lifetime | Connection |
|---|---|---|---|
| `MasterDbContext` | **1** — `Tenants` | `AddDbContext` (scoped, pooled options) | `ConnectionStrings:MasterDb` from `appsettings.json` |
| `ApplicationDbContext` | **196** | manually constructed per request by `TenantDbContextFactory` | `TenantInfo.ConnectionString` for the resolved tenant |

`Data/MasterDbContext.cs` is 9 lines. `TenantInfo` is
`{ int Id, string Name, string Hostname, string ConnectionString }`.

**Confirmed.** This means the "master" database is *only* a tenant directory — all business
data, including users, permissions, and company details, lives in the per-tenant database.

## Isolation model

**Database per tenant.** Each tenant gets a full copy of the 196-table schema. There is no
`TenantId` discriminator column and no EF global query filter — isolation is by connection
string. **Confirmed** (`TenantDbContextFactory.CreateDbContext()` builds a fresh
`DbContextOptionsBuilder<ApplicationDbContext>` with the tenant's connection string and a
60-second command timeout).

Consequences:
- Strong isolation; simple queries; easy per-tenant restore.
- Schema migrations must be applied N times. **Unknown:** how migrations are currently
  rolled out to tenant databases — no deployment script exists in the repo (Q-02).
- Cross-tenant reporting is impossible without federation.

## Entity families (196 DbSets)

Grouped by the `Data/` folder that owns them.

| Family | Folder | Representative entities |
|---|---|---|
| **Admin / security** | `Data/Master/Admin_Module`, `Data/Master/MasterScreeenManagement_Module` | `User`, `UserRight`, `UserAuthority`, `UserColumnPreference`, `UserPreference`, `UserThemePreference`, `Screens`, `ScreenManagement` |
| **Inventory masters** | `Data/Master/Inventory_module` (21 files) | `Item`, `ItemSub`, `ItemHistory`, `ItemProductAssign`, `RawMaterial`, `Category`, `UOM`, `Store`, `StoreMap`, `HSNMaster`, `Process`, `ProcessFlowChart`, `Machine`, `Factor`, `Grouping`, `GroupingSub`, `AssmblyDef`, `AssemblyDefLabour`, `AssemblyModify`, `CompMaster` |
| **General masters** | `Data/Master/General_Module` | `Customer`, `CustomerIndirect`, `ContactPerson`, `Vendor`, `VendorContact`, `VendorInDirect`, `State`, `TermsAndConditions` |
| **Accounts masters** | `Data/Master/Accounts_Module` | `Expense`, `Income`, `Banks`, `Currency`, `CurrencyToday`, `CostCenter`, `ProjectTypeMaster` |
| **HR masters** | `Data/Master/HumanResourceMaster_Module` (11 files) | `Staff`, `StaffFamilyDetails`, `StaffEducation`, `StaffEmergency`, `LeaveType`, `EmployeeLeaveBalance`, `LeaveApplication`, `DailyLeaveRecord`, `HolidayList`, `ShiftAllocation`, `Candidate` |
| **Sales & labour** | `Data/SalesAndLabour` | `Leads`, `EnquirySales(+Sub)`, `EnquiryFeasibility(+Sub)`, `MfgQuote(+Sub)`, `MfgPo(+Sub)`, `PoType`, `ContractReview(+Sub, Master)`, `PerformaInv(+Sub)`, `MfgDc(+Sub)`, `MfgInv(+Sub)`, `ExpInv(+Sub)`, `CreditNote(+Sub)`, `LabourDcOutgoing(+Sub)`, `LabourGRN(+Sub)`, `LabourSCN(+Sub)`, `LabInv(+Sub)`, `AssemblyPoComponentTracker`, `LabourDcReturnCompTrack` |
| **Outsourcing / purchase / subcontract** | `Data/OutSourcing` | `MaterialReq(+Sub)`, `EnquiryPurchase(+Sub)`, `EnquiryPurchaseVendorAssign`, `PurchaseQuote(+Sub)`, `PurchPo(+Sub)`, `PurchaseGRN(+Sub)`, `PurchaseSCN(+Sub)`, `PurchaseInvoice(+Sub)`, `SubConDcOut(+Sub)`, `SubConGRN(+Sub, Track)`, `SubConSCN(+Sub)`, `SubConInv(+Sub)`, `DebitNote(+Sub)` |
| **Planning** | `Data/Planning` | `JobOrder(+Sub)`, `RouteCard(+Sub)`, `RouteCardRelease(+Sub)`, `Estimate(+Sub)`, `ApprovalHistory` |
| **Production** | `Data/Production` | `ProductionIssueAssy(+Sub)`, `ProductionReturnAssy(+Sub, Track)`, `ProductionSCNAssy(+Sub)`, `ProductionIssueComp(+Sub)`, `ProductionReturnComp(+Sub, Track)`, `ProductionSCNComp(+Sub)`, `ProductionLog(+Sub)` |
| **Inventory / stock** | `Data/Inventory(Stock)` | `StockAdd`, `StockIssue`, `StockIssueTrack`, `SCNGen(+Sub)`, `MaterialIssNote(+Sub)`, `StoreInterTrans(+Sub)`, `ToolCribIssue(+Sub)`, `ToolCribReturn(+Sub)`, `StockIssueRequest(+Sub)` |
| **Inspection / QC** | `Data/Inspection` | `MasterInspection`, `InspectionRef`, `DefectInfo`, `FinalInspection(+Ref)`, `IncomingInspection(+Ref)`, `InspectionSettings` |
| **Maintenance** | `Data/Maintanence` | `MaintenanceSchedule`, `MaintenanceProcess`, `BreakdownMaintenance`, `InstrumentDetails`, `CalibrationHistory` |
| **Accounts / cash flow** | `Data/AccountsModule`, `Data/ServiceBills` | `Payments(+Sub)`, `Receipts(+Sub)`, `Advaceadjustment(+Sub)`, `FundTrans`, `ServiceBills(+Sub)` |
| **HR transactions** | `Data/HumanResource` | `Salary`, `Attendance`, `StaffLoan`, `OfferLetter(+Sub)`, `AppointmentLetter(+Sub)` |
| **Settings** | `Data/GeneralSettingsMaster`, `Data/Configuration` | `PrintSetting`, `ProductionLogSetting`, `HRMasterSetting`, `BiometricExcelSetting`, `SalaryHeadPrintSetting`, `Correspondence`, `Companydetails`, `RejectionMaster` |
| **Numbering** | `Data/DcAutoRunning`, `Data/InvoiceAutoRunning` | `DcRunningNumber`, `InvoiceAutoRunningNumber` |
| **Keyless report projections** | `ViewModels/…` registered as DbSets | `LabourTrackSummaryVM`, `PoPendingDetailsVM` |

## The dominant structural pattern: header + `Sub` lines

Almost every transactional document is a pair: `X` (header) and `XSub` (lines). ~65 such
pairs exist. Lines carry the `Ref…SubId` pointer to the upstream document line they consume.

Measured `Ref*` foreign-key property frequency across `Data/` (**Confirmed**):

| Property | Count | Meaning |
|---|---|---|
| `RefPoSubId` | 20 | line traces to a Purchase or Sales Order line |
| `RefGRNSubId` | 7 | line traces to a goods-receipt line |
| `RefSCNSubId` | 5 | line traces to a store-credit-note line |
| `RefRcSubId` | 5 | line traces to a route-card operation |
| `RefDcSubId` | 5 | line traces to a delivery-challan line |
| `RefReturnSubId`, `RefIssueSubId` | 4 each | production return / issue linkage |
| `RefEnqSubId`, `RefQuoteSubId`, `RefMReqSubId`, `RefJobOrderId`, `RefBomId`, … | 1–3 each | upstream document linkage |

**This is the backbone of the whole ERP.** Balance quantities are derived by walking these
links (e.g. `MfgPoService.GetQuoteItemBalQtyFromQuoteSubId`,
`CanSalesOrderItemCancelCheckAsync`). Any new UI must expose and respect these chains —
see [`modules/module-inventory.md`](../modules/module-inventory.md) for the full graph.

## Relationship configuration

`OnModelCreating` (in `ApplicationDbContext.cs`, ~1,700 of its 1,844 lines) does three
things:

1. Applies three `IEntityTypeConfiguration` classes: `UserConfiguration`,
   `ItemConfiguration`, `CustomerConfiguration` (`Data/Configuration/MasterConfiguration/`).
2. Declares dozens of explicit `HasOne/WithMany/HasForeignKey` relationships, almost all
   with `OnDelete(DeleteBehavior.Restrict)` or `NoAction` — cascading delete is
   deliberately avoided. **Confirmed.**
3. Seeds reference data via `HasData`.

### Seeded data (Confirmed)

| Entity | Rows | Note |
|---|---|---|
| `Screens` | **152** | The permission catalogue — `ScreenCode` 1…152, `ScreenName` e.g. `"Sales Order"`, `"Purchase Order"`, `"Stock-Add"`. **This is the authoritative permission vocabulary.** |
| `User` | 1 | `UserName = "Administrator"`, `Role = Administrator`, with a **committed PBKDF2 hash** — a known default credential (risk R-09) |
| `ScreenManagement`, `InspectionSettings`, `Category` | several | reference data |

`ScreenCode` doubles as the **stock movement source discriminator**: `StockAdd.ScreenCode`
and `StockIssue.ScreenCode` record which screen produced the movement, and
`StockManagerService` takes `int screenCode` as a parameter. Callers pass integer literals.
There is **no enum or constants class** for screen codes — they are magic numbers
correlated only with the seeded `Screens` table. **Confirmed.** This is a real hazard
(risk R-10): the new API must introduce a typed constant set derived from the seed.

## Migrations

219 files under `Migrations/`, ~2.5M lines total. Individual `*.Designer.cs` and
`ApplicationDbContextModelSnapshot.cs` files are ~29,000 lines each, and each migration
carries a full snapshot copy.

- First migration: `20260217110637_InitialCreate`
- Latest observed: `20260723064009_jobtrack`
- 1 additional migration under `Migrations/MasterDb/` applies `MasterDbContext`'s single
  `Tenants` table (`20260308101245_AddMasterDbContect`) — 219 migrations total, matching
  the figure cited elsewhere in the KB.
- Q-02 (how migrations are rolled out to a tenant database in production) remains
  **Unknown**. `db/RUNBOOK-rebuild-tenant-database.md` §5 (M0-01-03) records one candidate
  `dotnet ef database update --connection <explicit>` command for a single directly-reached
  database — UNVERIFIED until the rebuild drill runs, and explicitly not a description of
  the production rollout procedure.

The `Migrations/` folder alone accounts for roughly 90% of repository line count. This
bloats clone size, IDE indexing, and every full-text search. **Consolidating the migration
history is recommended but is not a blocker.** (Risk R-11, Low.)

## Raw SQL in the data layer

43 `FromSqlRaw` / `ExecuteSqlRaw` / `SqlQuery` call sites (**Confirmed**), in two shapes:

1. **`SELECT TOP 1 * … ORDER BY … DESC` next-number lookups** in ~20 repositories
   (`PurchPoRepository`, `SubConGRNRepository`, `RouteCardRepository`,
   `ProductionLogRepository`, `StoreInterTransRepository`, …). These fetch the last
   document to derive the next number.
   > **Concurrency risk (Inferred, high confidence):** last-number-plus-one under no lock
   > will produce duplicate document numbers under concurrent create. There is no
   > `SERIALIZABLE` isolation, `UPDLOCK` hint, or DB sequence in these queries. Risk R-12.
2. **`EXEC dbo.<proc>`** via `ReportExecutor` for the 94 report procedures.

`ItemRepository.cs:52` also uses `ExecuteSqlRawAsync` for a bulk item-rate update.

**Deployment step (M0-01-03, 2026-08-13, UNVERIFIED).** DDL for every stored procedure the
application calls is now in source control at `db/stored-procedures/` (the flat directory
holds the 78 procedures M0-01-02 captured from a live tenant; `relocated-legacy/` holds the
13 that were already scripted, moved there from `Existing Store Procedures/StoredProcedures/`
— that folder is retired). `db/deploy-stored-procedures.ps1` applies every `.sql` file
under both locations to a target database, idempotently (every file is
`CREATE OR ALTER PROCEDURE`), with a completeness check against
`db/stored-procedures/manifest.csv` before it runs. It has **not** been executed against a
real database as of this writing — see `db/RUNBOOK-rebuild-tenant-database.md` and
`db/REBUILD-DRILL-LOG.md` for the rebuild drill this claim depends on. 4 of the 94
referenced names still have no DDL anywhere in the repository
(`Sp_BomAnalysis`, `Sp_Print_Estimation`, `Sp_Print_Receipts`,
`Sp_Print_SingleProcessInspection` — `db/stored-procedures/CAPTURE-STATUS.md`), and one
scripted-but-unreferenced procedure (`Sp_Print_PurchaseOrder`) is deployed pending a human
keep/delete decision.

## Indexing

One migration is named `AddInDexingToCustomer`. Beyond that, explicit index configuration
is sparse. **Unknown:** the actual index inventory in the live tenant databases — the EF
model is not necessarily the deployed truth (Q-03). Given that grid screens do
`SearchWithDynamicFilterAsync` over large document tables, index coverage should be audited
before load increases from an SPA. Risk R-13.

## What must be preserved from the data layer

Everything. **No schema change is proposed.** The Angular migration is additive: new HTTP
controllers over the same services over the same schema. Any schema change must be its own
ADR with a stated reason.
