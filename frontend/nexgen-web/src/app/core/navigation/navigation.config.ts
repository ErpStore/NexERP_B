import type { NavTree } from './navigation.models';

/**
 * M2-C03 — INV-033's output: the sidebar's and command palette's one source of nav data.
 *
 * **Source: the expanded `<MudNavMenu>` tree** (`V.SMART/V.SMART.Shared/Layout/NavMenu.razor`,
 * the `Authorized` block starting around line 148), not the mini-rail's `NavGroups`
 * dictionary (same file, `:487-814`). **The two disagree** — confirmed, not assumed: the
 * dictionary's `"Human Resource"` entry additionally lists Employee/Holiday/Leave Master,
 * which the expanded tree keeps solely under `Master > Human Resource Master`, and the
 * dictionary has no entry at all for `stockIssReqList` ("Stock Issue-Request"), which only
 * the expanded tree links. The expanded tree is what a non-collapsed user actually
 * navigates day to day, so it is this file's authority; the mini-rail divergence is
 * recorded here rather than silently reconciled by guessing which is "right".
 *
 * **Routes are KB-053's proposed Angular routes**, reused verbatim per this task's own
 * instruction to reuse the page map rather than re-derive it — see
 * `docs/kb/frontend-new/page-map.md`. **None of these routes exist in `app.routes.ts` yet**
 * except `/dashboard` (this task's one real destination) — this file is the nav *data*,
 * built ahead of the ~140 destination screens `M2-D01` onward will add one at a time.
 * Clicking an unbuilt item today falls through to the wildcard route, which is the honest
 * state of "the frame exists, the screens do not yet" rather than a bug to route around.
 *
 * **`screenName: null` is a distinct, confirmed state — not a placeholder for "unmapped".**
 * Exactly one item, `rate-comparison`, has it: `RateComparison.razor` declares no
 * `BaseUserRightsComponent` override and no `ScreenName` at all — Confirmed by reading the
 * file directly, not inferred. Blazor therefore shows this page to **any** authenticated
 * user, gated by nothing; the SPA reproduces that exactly rather than inventing a
 * screen-right Blazor never had. `PermissionService.forScreen()` is never called for a
 * `null`-screenName item — see `sidebar.component.ts`'s filter and `app.routes.ts`'s guard
 * construction, both of which branch on this explicitly.
 *
 * **Every `ScreenName` value below is copied verbatim from source, including its typos and
 * two likely copy-paste defects, not corrected:**
 * - `SubConGRNList.razor` → `"Sub-Contrect GRN"` (not "Sub-Contract").
 * - `AdvanceAdjustmentList.razor` → `"Advaceadjustment"` (not "AdvanceAdjustment").
 * - `Rejection.razor` (the *Rejection Analysis* report) declares `ScreenName => "Income"` —
 *   its visibility is therefore actually governed by the Income Master's `UserRight` row,
 *   not a right of its own. Confirmed by reading the file directly. Almost certainly an
 *   unintentional copy-paste in the Blazor source, but KB-002 says current source code
 *   wins over what a screen name "should" be — the SPA reproduces the actual gate, and this
 *   is recorded as a defect to fix in `V.SMART/`, not silently routed around here.
 * - `SalesPoPending.razor` and `LabourPending.razor` (two different Pending Reports) both
 *   declare `ScreenName => "Po Pendings"` — the same right gates both reports. Also
 *   reproduced verbatim, also flagged as a likely source defect, not fixed here.
 *
 * **`master-upload` ("Master Upload") has no KB-053 route.** KB-053's own "removed (20
 * routes)" list groups `/master-upload` with the `-home`/`-master` landing pages it retires
 * — but `MasterUpload.razor` has a real `ScreenName` ("Master Upload", Confirmed), unlike a
 * pure landing page, so it is not obviously the same kind of route. Included here with a
 * provisional route (`/masters/upload`, following the `/masters/bom/import` sibling
 * pattern) rather than silently dropped — whichever task builds this destination should
 * revisit whether it is a real screen or genuinely retired.
 *
 * **`instantSearch` is deliberately absent.** `InstantSearch.razor` has a real ScreenName
 * ("Instant Search", Confirmed) but this task's own spec supersedes the page with the ⌘K
 * palette and builds no `/instantSearch` route — there is nothing to link the nav item to.
 *
 * Icons are `pi-*` PrimeIcons suffixes (`primeicons`, wired in `styles.scss` by this task —
 * the first consumer of that package in this workspace).
 */
export const NAVIGATION_TREE: NavTree = {
  top: [{ label: 'Dashboard', route: '/dashboard', screenName: 'Dashboard', icon: 'gauge' }],

  groups: [
    {
      label: 'Master',
      icon: 'book',
      sections: [
        {
          heading: 'Admin Master',
          links: [
            { label: 'User Master', route: '/admin/users', screenName: 'User' },
            { label: 'User Rights Master', route: '/admin/permissions', screenName: 'User Rights' },
            {
              label: 'Authority Manage Master',
              route: '/admin/approval-authority',
              screenName: 'User Level Authorization',
            },
          ],
        },
        {
          heading: 'Inventory Master',
          links: [
            { label: 'Category Master', route: '/masters/categories', screenName: 'Category' },
            { label: 'Measurement Unit Master', route: '/masters/uom', screenName: 'UOM' },
            { label: 'Stores Master', route: '/masters/stores', screenName: 'Store' },
            { label: 'Store Mapping Master', route: '/masters/store-mapping', screenName: 'Store Map' },
            { label: 'HSN/SAC Master', route: '/masters/hsn', screenName: 'HSN Master' },
            {
              label: 'Material Master',
              route: '/masters/raw-materials',
              screenName: 'Raw Material',
            },
            { label: 'Item Master', route: '/masters/items', screenName: 'Item' },
            { label: 'BOM Master', route: '/masters/bom', screenName: 'BOM' },
            { label: 'Process Master', route: '/masters/processes', screenName: 'Process' },
            { label: 'Factors Master', route: '/masters/factors', screenName: 'Factors' },
            { label: 'Grouping Master', route: '/masters/groupings', screenName: 'Grouping' },
            {
              label: 'Item Price Update',
              route: '/masters/items/bulk-price-update',
              screenName: 'Item Rate-Updation',
            },
          ],
        },
        {
          heading: 'General Master',
          links: [
            { label: 'Customer Master', route: '/masters/customers', screenName: 'Customer' },
            { label: 'Vendor Master', route: '/masters/vendors', screenName: 'Vendor' },
            { label: 'Machine Master', route: '/masters/machines', screenName: 'Machine' },
            {
              label: 'Terms & Conditions Master',
              route: '/masters/terms',
              screenName: 'Terms and Conditions',
            },
            { label: 'States Master', route: '/masters/states', screenName: 'State' },
            // No KB-053 route — see the file-header note on `master-upload`.
            { label: 'Master Upload', route: '/masters/upload', screenName: 'Master Upload' },
            {
              label: 'Contract Review Master',
              route: '/masters/contract-review',
              screenName: 'Contract Review',
            },
            {
              label: 'Rejection Master',
              route: '/masters/rejection-reasons',
              screenName: 'RejectionMaster',
            },
          ],
        },
        {
          heading: 'Account Master',
          links: [
            { label: 'Expense Master', route: '/masters/expenses', screenName: 'Expense' },
            { label: 'Income Master', route: '/masters/incomes', screenName: 'Income' },
            { label: 'Bank Detail Master', route: '/masters/banks', screenName: 'Bank' },
            { label: 'Currency Master', route: '/masters/currencies', screenName: 'Currency' },
            {
              label: 'Currency Today Master',
              route: '/masters/currency-rates',
              screenName: 'Currency Today',
            },
            {
              label: 'Project Type Master',
              route: '/masters/project-types',
              screenName: 'Project Type Master',
            },
            {
              label: 'Cost Centre Master',
              route: '/masters/cost-centres',
              screenName: 'Cost-Center',
            },
          ],
        },
        {
          heading: 'Human Resource Master',
          links: [
            { label: 'Candidate Master', route: '/hr/candidates', screenName: 'Candidate' },
            { label: 'Offer Letter', route: '/hr/offer-letters', screenName: 'Offer Letter' },
            {
              label: 'Appointment Letter',
              route: '/hr/appointment-letters',
              screenName: 'Appointment Letter',
            },
            { label: 'Employee Master', route: '/hr/employees', screenName: 'Staff' },
            { label: 'Holiday Master', route: '/hr/holidays', screenName: 'Holiday List' },
            { label: 'Leave Master', route: '/hr/leave-types', screenName: 'LeaveType' },
            {
              label: 'Leave Allocation Master',
              route: '/hr/leave-balances',
              screenName: 'Employee Leave Balance',
            },
            {
              label: 'Shift Allocation Master',
              route: '/hr/shifts',
              screenName: 'Shift Allocation',
            },
          ],
        },
        {
          heading: 'Settings Master',
          links: [
            {
              label: 'Screen Manage Master',
              route: '/admin/screens',
              screenName: 'Screen Management',
            },
            {
              label: 'General Setting Master',
              route: '/settings/general',
              screenName: 'General Settings',
            },
          ],
        },
        {
          heading: 'Assembly Costing',
          links: [
            {
              label: 'BOM Labour',
              route: '/masters/bom-labour',
              screenName: 'BOM Labour',
            },
            {
              label: 'Labour Cost Management',
              route: '/planning/labour-costs',
              screenName: 'LabourCostManagement',
            },
            {
              label: 'BOM - Cost Estimation',
              route: '/planning/cost-calculator',
              screenName: 'BOMLabourCost',
            },
          ],
        },
        {
          links: [
            { label: 'My Company Details', route: '/settings/company', screenName: 'Company' },
          ],
        },
      ],
    },

    {
      label: 'Sales',
      icon: 'receipt',
      sections: [
        {
          links: [
            { label: 'Leads', route: '/sales/leads', screenName: 'Leads' },
            {
              label: 'Customer Enquiry',
              route: '/sales/enquiries',
              screenName: 'Enquiry Sales',
            },
            {
              label: 'Enquiry Feasibility Study',
              route: '/sales/feasibility',
              screenName: 'Enquiry Feasibility',
            },
            {
              label: 'Quotation',
              route: '/sales/quotations',
              screenName: 'Manufacturing Quotation',
            },
            { label: 'Customer Sales Order', route: '/sales/orders', screenName: 'Sales Order' },
            {
              label: 'Contract Review Check',
              route: '/sales/contract-reviews',
              screenName: 'Contract Review CheckList',
            },
            {
              label: 'Proforma Invoice',
              route: '/sales/proforma-invoices',
              screenName: 'Performa Invoice',
            },
          ],
        },
        {
          heading: 'Manufacturing Work',
          links: [
            {
              label: 'Delivery Challan',
              route: '/sales/delivery-challans',
              screenName: 'Manufacturing DC',
            },
            {
              label: 'Domestic Tax Invoice',
              route: '/sales/invoices',
              screenName: 'Manufacturing Invoice',
            },
            {
              label: 'Export Invoice',
              route: '/sales/export-invoices',
              screenName: 'Export Invoice',
            },
          ],
        },
        {
          heading: 'Labour Work',
          links: [
            {
              label: 'Goods Received Note cum DC',
              route: '/labour/grn',
              screenName: 'Labour GRN',
            },
            { label: 'Store Credit Note', route: '/labour/scn', screenName: 'Labour SCN' },
            {
              label: 'Delivery Challan',
              route: '/labour/delivery-challans',
              screenName: 'Labour Delivery Challan',
            },
            { label: 'Labour Invoice', route: '/labour/invoices', screenName: 'Labour Invoice' },
          ],
        },
        {
          links: [
            { label: 'Credit Note', route: '/sales/credit-notes', screenName: 'Credit Note' },
          ],
        },
      ],
    },

    {
      label: 'Out Sourcing',
      icon: 'truck',
      sections: [
        {
          links: [
            {
              label: 'Material Requisition / Indent',
              route: '/purchasing/requisitions',
              screenName: 'Material Requisition',
            },
            {
              label: 'Enquiry',
              route: '/purchasing/enquiries',
              screenName: 'Enquiry Purchase',
            },
            {
              label: 'Vendor Quotation',
              route: '/purchasing/quotations',
              screenName: 'Purchase-Quotation',
            },
            {
              label: 'Price Comparison',
              route: '/purchasing/price-comparison',
              // Confirmed absent: RateComparison.razor has no BaseUserRightsComponent
              // override at all — Blazor shows it to any authenticated user. See the
              // file header for how consumers must treat a null screenName.
              screenName: null,
            },
            { label: 'Purchase Order', route: '/purchasing/orders', screenName: 'Purchase Order' },
          ],
        },
        {
          heading: 'Purchase',
          links: [
            {
              label: 'Goods Received Note',
              route: '/purchasing/grn',
              screenName: 'Purchase GRN',
            },
            {
              label: 'Store Credit Note',
              route: '/purchasing/scn',
              screenName: 'Purchase SCN',
            },
            {
              label: 'Tax Invoice',
              route: '/purchasing/invoices',
              screenName: 'Purchase Invoice',
            },
          ],
        },
        {
          heading: 'Sub Contract',
          links: [
            {
              label: 'Delivery Challan',
              route: '/subcontract/delivery-challans',
              screenName: 'Sub-Contract DC-Out',
            },
            {
              label: 'Goods Received Note',
              route: '/subcontract/grn',
              // Reproduced verbatim — source spells it "Sub-Contrect".
              screenName: 'Sub-Contrect GRN',
            },
            {
              label: 'Store Credit Note',
              route: '/subcontract/scn',
              screenName: 'Sub-Contract SCN',
            },
            {
              label: 'Tax Invoice',
              route: '/subcontract/invoices',
              screenName: 'Sub-Contract Invoice',
            },
          ],
        },
        {
          links: [
            { label: 'Debit Note', route: '/purchasing/debit-notes', screenName: 'Debit Note' },
          ],
        },
      ],
    },

    {
      label: 'Production / Shop Floor',
      icon: 'cog',
      sections: [
        {
          heading: 'Assembly',
          links: [
            {
              label: 'Jobcard Material Issue Note',
              route: '/production/assembly/issues',
              screenName: 'Production Issue WO Assembly',
            },
            {
              label: 'Goods Received Note',
              route: '/production/assembly/returns',
              screenName: 'Production Return GRN Assembly',
            },
            {
              label: 'Store Credit Note',
              route: '/production/assembly/scn',
              screenName: 'Production SCN Assembly',
            },
          ],
        },
        {
          heading: 'Component',
          links: [
            {
              label: 'Product Material Issue Note',
              route: '/production/component/issues',
              screenName: 'Production Issue WO Component',
            },
            {
              label: 'Goods Received Note',
              route: '/production/component/returns',
              screenName: 'Production Return Component',
            },
            {
              label: 'Store Credit Note',
              route: '/production/component/scn',
              screenName: 'Production SCN Component',
            },
          ],
        },
        {
          links: [
            {
              label: 'Daily Production Log',
              route: '/shopfloor/production-log',
              screenName: 'Daily Production Log',
            },
          ],
        },
      ],
    },

    {
      label: 'Planning',
      icon: 'calendar',
      sections: [
        {
          links: [
            { label: 'Authorisation', route: '/approvals', screenName: 'Authorization' },
            { label: 'Job Card', route: '/planning/job-orders', screenName: 'Job Order' },
            {
              label: 'Route Card Release',
              route: '/planning/route-card-releases',
              screenName: 'Route Card Release',
            },
            { label: 'Route Card', route: '/planning/route-cards', screenName: 'Route Card' },
            {
              label: 'Material Requirement Analysis',
              route: '/planning/material-requirements',
              screenName: 'Material Requirement Analysis',
            },
            {
              label: 'Assembly Requirement Analysis',
              route: '/planning/assembly-requirements',
              screenName: 'Assembly Requirement Analysis',
            },
            { label: 'Estimation', route: '/planning/estimations', screenName: 'Estimation' },
          ],
        },
      ],
    },

    {
      label: 'Inventory / Stock',
      icon: 'warehouse',
      sections: [
        {
          links: [
            {
              label: 'Stock Issue-Request',
              route: '/inventory/issue-requests',
              screenName: 'Stock Issue-Request',
            },
            {
              label: 'Stock Position (Internal)',
              route: '/inventory/stock-position',
              screenName: 'Stock Position',
            },
            {
              label: 'Stock Position with WIP',
              route: '/inventory/stock-position-wip',
              screenName: 'Stock Position(Internal & External)',
            },
            {
              label: 'Stock Transfer Note',
              route: '/inventory/stock-transfer-notes',
              screenName: 'Stock-Add',
            },
            {
              label: 'Consumable Issue Note',
              route: '/inventory/consumable-issues',
              screenName: 'Material Issue-Note',
            },
            {
              label: 'Tool Crib Issue Note',
              route: '/inventory/tool-crib/issues',
              screenName: 'Tool-Crib Issue',
            },
            {
              label: 'Tool Crib Return Note',
              route: '/inventory/tool-crib/returns',
              screenName: 'Tool-Crib Return',
            },
            {
              label: 'Inter Store Transfer',
              route: '/inventory/inter-store-transfers',
              screenName: 'Inter Store Transfer',
            },
            {
              label: 'BOM to Indent / STN',
              route: '/inventory/bom-explosion',
              // Same ScreenName as bomList's "BOM Master" — both confirmed verbatim from
              // source; whether that is intentional (one right covers both BOM concerns)
              // or another copy-paste is unverified.
              screenName: 'BOM',
            },
          ],
        },
      ],
    },

    {
      label: 'Inspection / QC',
      icon: 'check-square',
      sections: [
        {
          links: [
            {
              label: 'Inspection Master',
              route: '/quality/inspection-masters',
              screenName: 'MasterInspection',
            },
            { label: 'Defects Master', route: '/quality/defects', screenName: 'DefectInfo' },
            {
              label: 'Final Inspection',
              route: '/quality/final-inspections',
              screenName: 'FinalInspection',
            },
            {
              label: 'GRN Inspection',
              route: '/quality/incoming-inspections',
              screenName: 'IncomingInspection',
            },
          ],
        },
      ],
    },

    {
      label: 'Maintenance',
      icon: 'wrench',
      sections: [
        {
          links: [
            {
              label: 'Machine Maintenance Schedule',
              route: '/maintenance/schedules',
              screenName: 'MaintenanceSchedule',
            },
            {
              label: 'Machine Maintenance Process',
              route: '/maintenance/processes',
              screenName: 'MaintenanceProcess',
            },
            {
              label: 'Machine Breakdown Maintenance',
              route: '/maintenance/breakdowns',
              screenName: 'BreakdownMaintenance',
            },
            {
              label: 'Gauges Calibration & History',
              route: '/maintenance/calibrations',
              screenName: 'CalibrationHistoryAndMaintenance',
            },
          ],
        },
      ],
    },

    {
      label: 'Human Resource',
      icon: 'users',
      sections: [
        {
          links: [
            {
              label: 'Leave Application',
              route: '/hr/leave-applications',
              screenName: 'Leave Application',
            },
            { label: 'Attendance', route: '/hr/attendance', screenName: 'Attendance' },
            { label: 'Salary', route: '/hr/payroll', screenName: 'Salary' },
            { label: 'StaffLoan', route: '/hr/staff-loans', screenName: 'StaffLoan' },
          ],
        },
      ],
    },

    {
      label: 'Cash Flow / Accounts',
      icon: 'wallet',
      sections: [
        {
          links: [
            { label: 'Payments', route: '/accounts/payments', screenName: 'Payments' },
            { label: 'Receipts', route: '/accounts/receipts', screenName: 'Receipts' },
            {
              label: 'Advance Adjustment',
              route: '/accounts/advance-adjustments',
              // Reproduced verbatim — source spells it "Advaceadjustment".
              screenName: 'Advaceadjustment',
            },
            {
              label: 'Service Bills',
              route: '/accounts/service-bills',
              screenName: 'Service Bills',
            },
            {
              label: 'Bank Transactions',
              route: '/accounts/bank-transactions',
              screenName: 'Fundtransactions',
            },
          ],
        },
      ],
    },

    {
      label: 'Utilities',
      icon: 'sliders-h',
      sections: [
        {
          links: [
            {
              label: 'Correspondence',
              route: '/utilities/correspondence',
              screenName: 'Correspondences',
            },
          ],
        },
      ],
    },

    {
      label: 'Reports',
      icon: 'chart-bar',
      sections: [
        {
          heading: 'Issue Summary',
          links: [
            {
              label: 'Tool Crib Issue Summary',
              route: '/reports/tool-crib-summary',
              screenName: 'ToolCribIssue Summary',
            },
          ],
        },
        {
          heading: 'Accounts',
          links: [
            {
              label: 'Confirmation Of Accounts',
              route: '/reports/confirmation-of-accounts',
              screenName: 'Confirmation Of Accounts',
            },
            {
              label: 'Bills Paid List',
              route: '/reports/bills-paid',
              screenName: 'Bill Paid List',
            },
            {
              label: 'Profit_LossAccounts',
              route: '/reports/profit-loss',
              screenName: 'Profit & Loss Accounts',
            },
            {
              label: 'Tax Details',
              route: '/reports/tax-details',
              screenName: 'TaxDetails Report',
            },
            {
              label: 'TDS Summary',
              route: '/reports/tds-summary',
              screenName: 'TDSummary Report',
            },
            {
              label: 'HSN Code Summary',
              route: '/reports/hsn-summary',
              screenName: 'HSNSummary Report',
            },
            {
              label: 'CreditDebit Summary Report',
              route: '/reports/credit-debit-summary',
              screenName: 'CreditDebit Summary Report',
            },
            {
              label: 'Summary & Graphs',
              route: '/reports/summary-graphs',
              // Reproduced verbatim — source's ScreenName reads like a copy-paste from a
              // different report ("Bill Pending Report"), not this one.
              screenName: 'Bill Pending Report',
            },
            { label: 'GSTITC_04', route: '/reports/gst-itc-04', screenName: 'GSTITC04' },
          ],
        },
        {
          heading: 'Track Reports',
          links: [
            {
              label: 'Sales Track',
              route: '/reports/sales-track',
              screenName: 'Sales Track Report',
            },
            {
              label: 'Labour Track',
              route: '/reports/labour-track',
              screenName: 'Labour Track Report',
            },
            {
              label: 'Tally DC Inwards & Outwards',
              route: '/reports/dc-in-out',
              screenName: 'ViewTallyDc-In-Out',
            },
            { label: 'View Po Track', route: '/reports/po-track', screenName: 'View Po Track' },
            {
              label: 'Purchas Sales Track',
              route: '/reports/purchase-sales-track',
              screenName: 'Purchase Sales Track',
            },
            {
              label: 'Joborder Track',
              route: '/reports/job-order-track',
              screenName: 'Joborder Track',
            },
          ],
        },
        {
          heading: 'Analysis Reports',
          links: [
            { label: 'Stock Ledger', route: '/reports/stock-ledger', screenName: 'Stock Ledger' },
            {
              label: 'Stock Analysis',
              route: '/reports/stock-analysis',
              screenName: 'Stock Analysis',
            },
            {
              label: 'Rejection Analysis',
              route: '/reports/rejection-analysis',
              // Same ScreenName as income's "Income Master" — RightsHelper.cs really does
              // gate this report on the Income Master's UserRight row. See the file header.
              screenName: 'Income',
            },
            {
              label: 'Route Card Analysis',
              route: '/reports/route-card-analysis',
              screenName: 'Route Card Analysis',
            },
          ],
        },
        {
          heading: 'Rating',
          links: [
            { label: 'Ratings', route: '/reports/vendor-ratings', screenName: 'Ratings' },
            {
              label: 'PR PO Rating',
              route: '/reports/pr-po-rating',
              screenName: 'PR PO Rating Report',
            },
          ],
        },
        {
          heading: 'Pending Report',
          links: [
            {
              label: 'PO Pending List',
              route: '/reports/sales-order-pending',
              screenName: 'Po Pendings',
            },
            {
              label: 'Bills Pending List',
              route: '/reports/bills-pending',
              screenName: 'Bill Pending List',
            },
            {
              label: 'Pending Statements',
              route: '/reports/pending-statements',
              screenName: 'Pending Statements',
            },
            {
              label: 'Labour Pending Summary',
              route: '/reports/labour-pending',
              // Same ScreenName as SalesPoPending's "Po Pendings" — see the file header.
              screenName: 'Po Pendings',
            },
            {
              label: 'Production Pending Summary',
              route: '/reports/production-pending',
              screenName: 'Production Pending Summary',
            },
          ],
        },
        {
          heading: 'History',
          links: [
            { label: 'Day Book', route: '/reports/daybook', screenName: 'Day Book' },
            {
              label: 'Item Usage Tracking',
              route: '/reports/item-usage',
              screenName: 'ItemHistory',
            },
            {
              label: 'Item Modification',
              route: '/reports/item-modifications',
              screenName: 'Item Modification',
            },
          ],
        },
      ],
    },
  ],
};
