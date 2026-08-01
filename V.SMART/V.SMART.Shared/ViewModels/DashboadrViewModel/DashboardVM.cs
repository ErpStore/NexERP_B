using System;
using System.Collections.Generic;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;

namespace V.SMART.Shared.ViewModels.DashboadrViewModel
{

    #region Dashboard Overview
    public class KpiCard
    {
       public string Title { get; set; } = string.Empty;
        public object? Value { get; set; }
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = "#7c3aed";
    }
    public class DashboardKpiSection
    {
        public string Title { get; set; } = "";
        public string HeaderIcon { get; set; } = "";
        public string BadgeText { get; set; } = "";
        public string BadgeClass { get; set; } = "";
        public List<KpiCard> Cards { get; set; } = new();
    }
    public class DashboardFilterDto
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Period { get; set; } = "This Week";

        /// <summary>
        /// Controls how chart X-axis is grouped.
        /// "Hour"  → 12 AM, 01 AM … 11 PM       (Today)
        /// "Day"   → Mon, Tue, Wed …              (This Week)
        /// "Week"  → Week 1, Week 2 …             (This Month)
        /// "Month" → Jan, Feb, Mar …              (Quarter / Year)
        /// </summary>
        public string GroupBy { get; set; } = "Day";
    }
    public class DashboardSummaryDto
    {
        public SummaryCardDto Payment { get; set; } = new();
        public SummaryCardDto Receipt { get; set; } = new();
        public SummaryCardDto SalesEnquiryQuote { get; set; } = new();
        public SummaryCardDto Sales { get; set; } = new();
        public SummaryCardDto Labour { get; set; } = new();
        public SummaryCardDto Purchase { get; set; } = new();
        public SummaryCardDto SubContract { get; set; } = new();
        public SummaryCardDto Export { get; set; } = new();
    }
    public class SummaryCardDto
    {
        public string V1 { get; set; } = "0";
        public string V2 { get; set; } = "₹0";
        public string V3 { get; set; } = "0";
        public string V4 { get; set; } = "₹0";
    }
    public class ChartPointDto
    {
        public string Label { get; set; } = "";
        public decimal Value { get; set; }
    }
    public class ChartSeriesDto
    {
        public string Name { get; set; } = "";
        public List<ChartPointDto> Data { get; set; } = new();
    }
    public class OverviewDto
    {
        /// <summary>One series per trade type: Sales, Purchase, Export, Labour, Sub-Con</summary>
        public List<ChartSeriesDto> SalesTrend { get; set; } = new();
        public List<BusinessDataDto> BusinessSplit { get; set; } = new();
        public List<TransactionDto> Transactions { get; set; } = new();
    }
    public class BusinessDataDto
    {
        public string Category { get; set; } = "";
        public decimal Value { get; set; }
    }

    #endregion

    #region SalesDto
    public class SalesDto
    {
        // =========================
        // ENQUIRY
        // =========================
        public int TotalEnquiries { get; set; }
        public int CompletedEnquiries { get; set; }
        public int PendingEnquiries { get; set; }
        public int CancelledEnquiries { get; set; }
        public int ShortClosedEnquiries { get; set; }

        // =========================
        // QUOTATION
        // =========================
        public int TotalQuotations { get; set; }
        public int CompletedQuotations { get; set; }
        public int PendingQuotations { get; set; }
        public int CancelledQuotations { get; set; }
        public int ShortClosedQuotations { get; set; }

        // =========================
        // SALES ORDER / PO
        // =========================
        public int TotalPurchaseOrders { get; set; }
        public int CompletedPurchaseOrders { get; set; }
        public int PendingPurchaseOrders { get; set; }
        public int CancelledPurchaseOrders { get; set; }
        public int ShortClosedPurchaseOrders { get; set; }

        public decimal TotalPurchaseOrderValue { get; set; }

        // =========================
        // INVOICE
        // =========================
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int CancelledInvoices { get; set; }

        public decimal TotalInvoiceValue { get; set; }
        public decimal PendingInvoiceValue { get; set; }

        // =========================
        // REVENUE
        // =========================
        public decimal TotalRevenue { get; set; }
        public decimal CollectedRevenue { get; set; }
        public decimal PendingRevenue { get; set; }

        // =========================
        // CUSTOMER
        // =========================
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int RepeatCustomers { get; set; }

        // =========================
        // SALES ANALYTICS
        // =========================
        public decimal AverageOrderValue { get; set; }
        public decimal AverageInvoiceValue { get; set; }

        public int TotalItemsSold { get; set; }

        // =========================
        // CHARTS
        // =========================
        public List<ChartPointDto> ActualSales { get; set; } = new();
        public List<ChartPointDto> TargetSales { get; set; } = new();

        public List<BusinessDataDto> PaymentSplit { get; set; } = new();

        // =========================
        // CUSTOMER ANALYTICS
        // =========================
        public List<TopCustomerDto> TopCustomers { get; set; } = new();

        // =========================
        // SALES ORDERS
        // =========================
        public List<MfgInvVM> Orders { get; set; } = new();
    }
    #endregion

    #region Labour DTO
    public class LabourDto
    {
        // =========================
        // ENQUIRY
        // =========================
        public int TotalEnquiries { get; set; }
        public int CompletedEnquiries { get; set; }
        public int PendingEnquiries { get; set; }
        public int CancelledEnquiries { get; set; }
        public int ShortClosedEnquiries { get; set; }

        // =========================
        // QUOTATION
        // =========================
        public int TotalQuotations { get; set; }
        public int CompletedQuotations { get; set; }
        public int PendingQuotations { get; set; }
        public int CancelledQuotations { get; set; }
        public int ShortClosedQuotations { get; set; }

        // =========================
        // LABOUR ORDER / PO
        // =========================
        public int TotalPurchaseOrders { get; set; }
        public int CompletedPurchaseOrders { get; set; }
        public int PendingPurchaseOrders { get; set; }
        public int CancelledPurchaseOrders { get; set; }
        public int ShortClosedPurchaseOrders { get; set; }

        public decimal TotalPurchaseOrderValue { get; set; }

        // =========================
        // INVOICE
        // =========================
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int CancelledInvoices { get; set; }

        public decimal TotalInvoiceValue { get; set; }
        public decimal PendingInvoiceValue { get; set; }

        // =========================
        // REVENUE
        // =========================
        public decimal TotalRevenue { get; set; }
        public decimal CollectedRevenue { get; set; }
        public decimal PendingRevenue { get; set; }

        // =========================
        // CUSTOMER
        // =========================
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public int RepeatCustomers { get; set; }

        // =========================
        // LABOUR ANALYTICS
        // =========================
        public decimal AverageOrderValue { get; set; }
        public decimal AverageInvoiceValue { get; set; }

        public int TotalJobsCompleted { get; set; }
        public int TotalLabourHours { get; set; }

        // =========================
        // CHARTS
        // =========================
        public List<ChartPointDto> ActualLabour { get; set; } = new();
        public List<ChartPointDto> TargetLabour { get; set; } = new();

        public List<BusinessDataDto> PaymentSplit { get; set; } = new();

        // =========================
        // CUSTOMER ANALYTICS
        // =========================
        public List<TopCustomerDto> TopCustomers { get; set; } = new();

        // =========================
        // LABOUR ORDERS
        // =========================
        public List<LabInvVM> Orders { get; set; } = new();
    }
    #endregion

    #region Customer And Vendor DTO
    public class TopCustomerDto
    {
        public string Name { get; set; } = "";
        public int Orders { get; set; }
        public string Revenue { get; set; } = "";
    }

    public class TopVendorDto
    {
        public string Name { get; set; } = "";
        public int Orders { get; set; }
        public string Revenue { get; set; } = "";
    }
    #endregion

    #region PurchaseDto
    public class PurchaseDto
    {
        // =========================
        // PURCHASE REQUISITION (PR)
        // =========================
        public int TotalPurchaseRequisitions { get; set; }
        public int CompletedPurchaseRequisitions { get; set; }
        public int ApprovedPurchaseRequisitions { get; set; }
        public int PendingPurchaseRequisitions { get; set; }
        public int CancelledPurchaseRequisitions { get; set; }

        // =========================
        // PURCHASE ENQUIRY
        // =========================
        public int TotalEnquiries { get; set; }
        public int CompletedEnquiries { get; set; }
        public int PendingEnquiries { get; set; }
        public int CancelledEnquiries { get; set; }
        public int ShortClosedEnquiries { get; set; }

        // =========================
        // PURCHASE Quote
        // =========================
        public int TotalQuotations { get; set; }
        public int CompletedQuotations { get; set; }
        public int PendingQuotations { get; set; }
        public int CancelledQuotations { get; set; }
        public int ShortClosedQuotations { get; set; }


        // =========================
        // PURCHASE ORDER (PO)
        // =========================
        public int TotalPurchaseOrders { get; set; }
        public int CompletedPurchaseOrders { get; set; }
        public int PendingPurchaseOrders { get; set; }
        public int CancelledPurchaseOrders { get; set; }
        public int ShortClosedPurchaseOrders { get; set; }


        public decimal TotalPurchaseOrderValue { get; set; }

        // =========================
        // PURCHASE INVOICE
        // =========================
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int CancelledInvoices { get; set; }

        public decimal TotalInvoiceValue { get; set; }
        public decimal PendingInvoiceValue { get; set; }

        // =========================
        // EXPENSE ANALYTICS
        // =========================
        public decimal TotalExpense { get; set; }
        public decimal PaidExpense { get; set; }
        public decimal OutstandingExpense { get; set; }

        // =========================
        // VENDOR ANALYTICS
        // =========================
        public int TotalVendors { get; set; }
        public int ActiveVendors { get; set; }
        public int NewVendors { get; set; }

        // =========================
        // PURCHASE ANALYTICS
        // =========================
        public decimal AveragePOValue { get; set; }
        public decimal AverageInvoiceValue { get; set; }

        public int TotalItemsPurchased { get; set; }

        // =========================
        // CHARTS
        // =========================
        public List<ChartPointDto> PurchaseTrend { get; set; } = new();

        public List<ChartPointDto> ExpenseTrend { get; set; } = new();

        public List<BusinessDataDto> VendorSplit { get; set; } = new();

        public List<BusinessDataDto> ExpenseCategorySplit { get; set; } = new();

        // =========================
        // TOP VENDORS
        // =========================
        public List<TopVendorDto> TopVendors { get; set; } = new();

        // =========================
        // PURCHASE INVOICES
        // =========================
        public List<PurchaseInvoiceVM> Invoices { get; set; } = new();
    }
    #endregion

    #region SubcontractDto
    public class SubcontractDto
    {
        // SUBCONTRACT ENQUIRY
        public int TotalEnquiries { get; set; }
        public int CompletedEnquiries { get; set; }
        public int PendingEnquiries { get; set; }
        public int CancelledEnquiries { get; set; }
        public int ShortClosedEnquiries { get; set; }

        // SUBCONTRACT QUOTATION
        public int TotalQuotations { get; set; }
        public int CompletedQuotations { get; set; }
        public int PendingQuotations { get; set; }
        public int CancelledQuotations { get; set; }
        public int ShortClosedQuotations { get; set; }

        // SUBCONTRACT ORDER
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int PendingOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int ShortClosedOrders { get; set; }

        public decimal TotalOrderValue { get; set; }

        // SUBCONTRACT INVOICE
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int PendingInvoices { get; set; }
        public int CancelledInvoices { get; set; }

        public decimal TotalInvoiceValue { get; set; }
        public decimal PendingInvoiceValue { get; set; }

        // COST ANALYTICS
        public decimal TotalContractValue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }

        // CONTRACTOR ANALYTICS
        public int TotalContractors { get; set; }
        public int ActiveContractors { get; set; }
        public int NewContractors { get; set; }

        // SUBCONTRACT ANALYTICS
        public decimal AverageOrderValue { get; set; }
        public decimal AverageInvoiceValue { get; set; }

        public int TotalJobsAssigned { get; set; }

        // CHARTS
        public List<ChartPointDto> OrderTrend { get; set; } = new();

        public List<ChartPointDto> InvoiceTrend { get; set; } = new();

        public List<BusinessDataDto> ContractorSplit { get; set; } = new();

        public List<BusinessDataDto> WorkCategorySplit { get; set; } = new();

        // TOP CONTRACTORS
        public List<TopVendorDto> TopContractors { get; set; } = new();

        // RECENT INVOICES
        public List<SubConInvVM> Invoices { get; set; } = new();
    }

    #endregion

    #region JobCardDto

    public class JobCardDto
    {
        // =========================================================
        // JOB ORDER SUMMARY
        // =========================================================
        public int TotalJobOrders { get; set; }
        public int CompletedJobOrders { get; set; }
        public int InProgressJobOrders { get; set; }
        public int PendingJobOrders { get; set; }

        public decimal PlannedAssemblyQty { get; set; }
        public decimal BalanceAssemblyQty { get; set; }
        public decimal ProducedAssemblyQty { get; set; }

        // =========================================================
        // ISSUE SUMMARY
        // =========================================================
        public int TotalIssues { get; set; }
        public int CompletedIssues { get; set; }
        public decimal IssuedQty { get; set; }

        // =========================================================
        // RETURN SUMMARY
        // =========================================================
        public int TotalReturns { get; set; }
        public int CompletedReturns { get; set; }
        public decimal ReturnedQty { get; set; }

        // =========================================================
        // SCN SUMMARY
        // =========================================================
        public int TotalSCNs { get; set; }
        public decimal SCNQty { get; set; }

        // =========================================================
        // EFFICIENCY
        // =========================================================
        public decimal MaterialConsumption { get; set; }
        public decimal ReturnPercentage { get; set; }
        public decimal ProductionEfficiency { get; set; }

        // =========================================================
        // CHARTS
        // =========================================================
        public List<ChartPointDto> JobOrderTrend { get; set; } = new();
        public List<ChartPointDto> IssueTrend { get; set; } = new();
        public List<ChartPointDto> ReturnTrend { get; set; } = new();
        public List<ChartPointDto> SCNTrend { get; set; } = new();

        public List<BusinessDataDto> JobStatusSplit { get; set; } = new();

        // =========================================================
        // TOP ASSEMBLIES
        // =========================================================
        public List<TopItemDto> TopAssemblies { get; set; } = new();

        // =========================================================
        // RECENT DATA
        // =========================================================
        public List<JobOrderVM> JobOrders { get; set; } = new();

        public List<ProductionSCNAssyVM> RecentSCNs { get; set; } = new();
    }

    #endregion


    public class TopItemDto
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Jobs { get; set; }
        public decimal PlannedQty { get; set; }
        public decimal ProducedQty { get; set; }
        public decimal CompletedQty { get; set; }
        public decimal Value { get; set; }
        public decimal CompletionPercentage { get; set; }
    }





    public class TransactionDto
    {
        public string DocNo { get; set; } = "";
        public string Party { get; set; } = "";
        public string Type { get; set; } = "";
        public string Date { get; set; } = "";
        public string Amount { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusClass { get; set; } = ""; // done | pending | failed
    }


    public class WorkOrderDto
    {
        public string Id { get; set; } = "";
        public string Product { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusClass { get; set; } = "";
    }

    // ─── Inventory ────────────────────────────────────────────────────────────
    public class InventoryDto
    {
        public List<ChartPointDto> LevelChart { get; set; } = new();
        public List<StockAlertDto> LowStock { get; set; } = new();
        public List<InventoryItemDto> Items { get; set; } = new();
    }

    public class StockAlertDto
    {
        public string Name { get; set; } = "";
        public int Qty { get; set; }
    }

    public class InventoryItemDto
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int In { get; set; }
        public int Out { get; set; }
        public int Balance { get; set; }
        public string Location { get; set; } = "";
    }

    // ─── Finance ──────────────────────────────────────────────────────────────
    public class FinanceDto
    {
        public string NetProfit { get; set; } = "₹0";
        public string TotalExpense { get; set; } = "₹0";
        public string ArPending { get; set; } = "₹0";
        public string ApPending { get; set; } = "₹0";

        public List<ChartPointDto> CashflowIn { get; set; } = new();
        public List<ChartPointDto> CashflowOut { get; set; } = new();
        public List<BusinessDataDto> ExpenseBreak { get; set; } = new();
    }

    // ─── HR ───────────────────────────────────────────────────────────────────
    public class HrDto
    {
        public List<BusinessDataDto> DeptSplit { get; set; } = new();
        public List<ChartPointDto> AttendanceWeek { get; set; } = new();
        public List<EmployeeDto> Employees { get; set; } = new();
    }

    public class EmployeeDto
    {
        public string Name { get; set; } = "";
        public string Dept { get; set; } = "";
        public string Role { get; set; } = "";
        public string Status { get; set; } = "";
        public string StatusClass { get; set; } = "";
        public int Attendance { get; set; }
    }





    public class JobCardRowDto
    {
        public string? JobCardNo { get; set; }
        public string? SalesOrderNo { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? AssignedTo { get; set; }
        public string? Priority { get; set; }   // Critical | High | Medium | Low
        public string? StartDate { get; set; }
        public string? DueDate { get; set; }
        public int Qty { get; set; }
        public string? Status { get; set; }
        public string? StatusClass { get; set; }   // done | pending | failed
    }

    
    // ── Route Card ───────────────────────────────────────────────────────────
    public class RouteCardDto
    {
        public int TotalRouteCards { get; set; }
        public int Completed { get; set; }
        public int InProcess { get; set; }
        public int Pending { get; set; }
        public int TotalOperations { get; set; }
        public int AvgCompletionPct { get; set; }

        // Charts
        public List<ChartPointDto> CreatedTrend { get; set; } = new();
        public List<ChartPointDto> CompletedTrend { get; set; } = new();
        public List<BusinessDataDto> OperationSplit { get; set; } = new();

        // Workstation performance cards
        public List<WorkstationStatDto> WorkstationStats { get; set; } = new();

        // Table
        public List<RouteCardRowDto> RouteCards { get; set; } = new();
    }

    public class WorkstationStatDto
    {
        public string? WorkstationName { get; set; }
        public string? OperationType { get; set; }
        public int AssignedCount { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
    }

    public class RouteCardRowDto
    {
        public string? RouteCardNo { get; set; }
        public string? JobCardNo { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? OperationName { get; set; }
        public string? WorkstationName { get; set; }
        public string? OperatorName { get; set; }
        public int PlannedQty { get; set; }
        public int CompletedQty { get; set; }
        public string? Status { get; set; }
        public string? StatusClass { get; set; }
    }


    // Gauge KPIs
    public class GaugeKpiDto
    {
        public string Label { get; set; } = "";
        public int Value { get; set; }        // 0–100 percent
        public string Color { get; set; } = "#3b82f6";
    }

    // Sparkline series
    public class SparklineSeriesDto
    {
        public string Name { get; set; } = "";
        public List<decimal> Values { get; set; } = new();
        public string DisplayValue { get; set; } = "";
        public decimal ChangePercent { get; set; }
        public string Color { get; set; } = "#3b82f6";
    }

    // Funnel stage
    public class FunnelStageDto
    {
        public string Label { get; set; } = "";
        public int Count { get; set; }
        public string Color { get; set; } = "#3b82f6";
    }

    // Scatter / Bubble customer
    public class CustomerBubbleDto
    {
        public string Name { get; set; } = "";
        public int Orders { get; set; }
        public decimal Revenue { get; set; }   // in Lakhs
        public decimal AvgOrderValue { get; set; }
        public int BubbleRadius { get; set; }
        public string Segment { get; set; } = "Mid"; // Top, Key, Mid, SME
    }

    // Waterfall
    public class WaterfallPointDto
    {
        public string Label { get; set; } = "";
        public decimal Inflow { get; set; }
        public decimal Outflow { get; set; }
        public decimal Net => Inflow - Outflow;
    }

    // Heatmap cell
    public class HeatmapCellDto
    {
        public int Hour { get; set; }  // 0–23
        public int DayOfWeek { get; set; }  // 0 Mon … 6 Sun
        public int Count { get; set; }
    }

    // Radar dept metric
    public class RadarDatasetDto
    {
        public string Label { get; set; } = "";
        public List<int> Values { get; set; } = new();  // one per axis, 0-100
        public string Color { get; set; } = "#3b82f6";
    }

    // Budget progress
    public class BudgetModuleDto
    {
        public string Name { get; set; } = "";
        public decimal Spent { get; set; }
        public decimal Allocated { get; set; }
        public string Color { get; set; } = "#3b82f6";
    }

    // Aggregated analytics DTO
    public class AnalyticsDto
    {
        public List<GaugeKpiDto> Gauges { get; set; } = new();
        public List<SparklineSeriesDto> Sparklines { get; set; } = new();
        public List<FunnelStageDto> Funnel { get; set; } = new();
        public List<CustomerBubbleDto> CustomerBubbles { get; set; } = new();
        public List<WaterfallPointDto> Waterfall { get; set; } = new();
        public List<HeatmapCellDto> HeatmapCells { get; set; } = new();
        public List<string> RadarAxes { get; set; } = new();
        public List<RadarDatasetDto> RadarDatasets { get; set; } = new();
        public List<BudgetModuleDto> BudgetModules { get; set; } = new();
    }


    #region Kpi Summary

    public sealed class StatusSummary
    {
        public int Total { get; set; }
        public int Completed { get; set; }
        public int Pending { get; set; }
        public int Cancelled { get; set; }
        public int ShortClosed { get; set; }
    }

    public sealed class RevenueSummary
    {
        public decimal TotalRevenue { get; set; }
        public decimal CollectedRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
    }

    #endregion Kpi Summary




}
