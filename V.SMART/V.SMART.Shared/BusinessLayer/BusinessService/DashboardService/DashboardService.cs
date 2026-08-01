using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IDashboardService;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.DashboadrViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.DashboardService
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public DashboardService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        #region Summary Cards

        public async Task<DashboardSummaryDto> GetSummaryAsync(DashboardFilterDto filter)
        {
            try
            {
                var from = filter.FromDate.Date;
                var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // ───────────────── PAYMENT ─────────────────

                var payments = await _unitOfWork.Payments.GetQueryable()
                    .Where(p =>
                        p.CreatedDate.HasValue &&
                        p.CreatedDate.Value >= from &&
                        p.CreatedDate.Value <= to)
                    .ToListAsync();

                // ───────────────── RECEIPT ─────────────────

                var receipts = await _unitOfWork.Receiptss.GetQueryable()
                    .Where(r =>
                        r.CreatedDate >= from &&
                        r.CreatedDate <= to)
                    .ToListAsync();

                // ───────────────── ENQUIRY ─────────────────

                var enquiries = await _unitOfWork.EnquirySaless.GetQueryable()
                    .Where(e =>
                        e.EnquiryDate >= from &&
                        e.EnquiryDate <= to)
                    .ToListAsync();

                // ───────────────── QUOTATION ───────────────

                var quotations = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Where(q =>
                        q.QuoteDate >= from &&
                        q.QuoteDate <= to)
                    .ToListAsync();

                // ───────────────── SALES ───────────────────

                var salesPOs = await _unitOfWork.MfgPos.GetQueryable()
                    .Where(s =>
                        !s.MfgORLab &&
                        s.PODateNow >= from &&
                        s.PODateNow <= to)
                    .ToListAsync();

                var salesInvs = await _unitOfWork.MfgInvs.GetQueryable()
                    .Where(i =>
                        i.InvDateNow >= from &&
                        i.InvDateNow <= to)
                    .ToListAsync();

                // ───────────────── LABOUR ──────────────────

                var labPOs = await _unitOfWork.MfgPos.GetQueryable()
                    .Where(s =>
                        s.MfgORLab &&
                        s.PODateNow >= from &&
                        s.PODateNow <= to)
                    .ToListAsync();

                var labInvs = await _unitOfWork.LabInvs.GetQueryable()
                    .Where(i =>
                        i.LabInvDateNow >= from &&
                        i.LabInvDateNow <= to)
                    .ToListAsync();

                // ───────────────── PURCHASE ────────────────

                var purPOs = await _unitOfWork.PurchPos.GetQueryable()
                    .Where(p =>
                        !p.PurchORSubCon &&
                        p.PODateNow >= from &&
                        p.PODateNow <= to)
                    .ToListAsync();

                var purInvs = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .Where(i =>
                        i.InvDateNow >= from &&
                        i.InvDateNow <= to)
                    .ToListAsync();

                // ───────────────── SUB-CONTRACT ────────────

                var subconPOs = await _unitOfWork.PurchPos.GetQueryable()
                    .Where(p =>
                        p.PurchORSubCon &&
                        p.PODateNow >= from &&
                        p.PODateNow <= to)
                    .ToListAsync();

                var subconInvs = await _unitOfWork.SubConInvs.GetQueryable()
                    .Where(i =>
                        i.InvDateNow >= from &&
                        i.InvDateNow <= to)
                    .ToListAsync();

                // ───────────────── EXPORT ──────────────────

                var expPOs = await _unitOfWork.MfgPos.GetQueryable()
                    .Where(e =>
                        !e.MfgORLab &&
                        e.PODateNow >= from &&
                        e.PODateNow <= to)
                    .ToListAsync();

                var expInvs = await _unitOfWork.ExpInvs.GetQueryable()
                    .Where(i =>
                        i.ExpInvDateNow >= from &&
                        i.ExpInvDateNow <= to)
                    .ToListAsync();

                // ───────────────── PENDING COUNTS ──────────

                var noOfPayments =
                (
                    purInvs.Count(x =>
                        x.Balance > 0 &&
                        x.InvCancel == false)

                    +

                    subconInvs.Count(x =>
                        x.Balance > 0 &&
                        x.InvCancel == false)

                ).ToString();

                var noOfReceipts =
                (
                    salesInvs.Count(x =>
                        x.Balance > 0 &&
                        x.IsCancel == false)

                    +

                    labInvs.Count(x =>
                        x.Balance > 0 &&
                        x.InvCancel == false)

                ).ToString();

                // ───────────────── PENDING AMOUNTS ─────────

                var pendingPayAmount =
                (
                    purInvs
                        .Where(x =>
                            x.Balance > 0 &&
                            x.InvCancel == false)
                        .Sum(x => x.Balance)

                    +

                    subconInvs
                        .Where(x =>
                            x.Balance > 0 &&
                            x.InvCancel == false)
                        .Sum(x => x.Balance)

                );

                var pendingReceiveAmount =
                (
                    salesInvs
                        .Where(x =>
                            x.Balance > 0 &&
                            x.IsCancel == false)
                        .Sum(x => x.Balance)

                    +

                    labInvs
                        .Where(x =>
                            x.Balance > 0 &&
                            x.InvCancel == false)
                        .Sum(x => x.Balance)

                );

                // ───────────────── PAYMENT CARD ────────────

                var paymentCard = new SummaryCardDto
                {
                    V1 = payments.Count.ToString(),

                    V2 = FormatCurrency(
                        payments.Sum(x => x.Amount)),

                    V3 = noOfPayments,

                    V4 = FormatCurrency(pendingPayAmount),
                };

                // ───────────────── RECEIPT CARD ────────────

                var receiptCard = new SummaryCardDto
                {
                    V1 = receipts.Count.ToString(),

                    V2 = FormatCurrency(
                        receipts.Sum(x => x.Amount)),

                    V3 = noOfReceipts,

                    V4 = FormatCurrency(pendingReceiveAmount),
                };

                // ───────────────── ENQUIRY / QUOTE ─────────

                var seqCard = new SummaryCardDto
                {
                    V1 = enquiries.Count.ToString(),

                    V2 = enquiries.Count(x =>
                            !x.EnquiryTally &&
                            !x.Cancel &&
                            !x.ShortClose)
                        .ToString(),

                    V3 = quotations.Count.ToString(),

                    V4 = FormatCurrency(
                        quotations.Sum(x => x.GrandTotal)),
                };

                // ───────────────── SALES CARD ──────────────

                var salesCard = new SummaryCardDto
                {
                    V1 = salesPOs.Count.ToString(),

                    V2 = FormatCurrency(
                        salesPOs.Sum(x => x.GrandTotal)),

                    V3 = salesInvs.Count.ToString(),

                    V4 = FormatCurrency(
                        salesInvs.Sum(x => x.GrandTotal)),
                };

                // ───────────────── LABOUR CARD ─────────────

                var labourCard = new SummaryCardDto
                {
                    V1 = labPOs.Count.ToString(),

                    V2 = FormatCurrency(
                        labPOs.Sum(x => x.GrandTotal)),

                    V3 = labInvs.Count.ToString(),

                    V4 = FormatCurrency(
                        labInvs.Sum(x => x.GrandTotal)),
                };

                // ───────────────── PURCHASE CARD ───────────

                var purchaseCard = new SummaryCardDto
                {
                    V1 = purPOs.Count.ToString(),

                    V2 = FormatCurrency(
                        purPOs.Sum(x => x.GrandTotal)),

                    V3 = purInvs.Count.ToString(),

                    V4 = FormatCurrency(
                        purInvs.Sum(x => x.GrandTotal)),
                };

                // ───────────────── SUB-CONTRACT CARD ──────

                var subContractCard = new SummaryCardDto
                {
                    V1 = subconPOs.Count.ToString(),

                    V2 = FormatCurrency(
                        subconPOs.Sum(x => x.GrandTotal)),

                    V3 = subconInvs.Count.ToString(),

                    V4 = FormatCurrency(
                        subconInvs.Sum(x => x.GrandTotal)),
                };

                // ───────────────── EXPORT CARD ─────────────

                var expCard = new SummaryCardDto
                {
                    V1 = expPOs.Count.ToString(),

                    V2 = FormatCurrency(
                        expPOs.Sum(x => x.GrandTotal)),

                    V3 = expInvs.Count.ToString(),

                    V4 = FormatCurrency(
                        expInvs.Sum(x => x.GrandTotal)),
                };

                // ───────────────── RETURN ──────────────────

                return new DashboardSummaryDto
                {
                    Payment = paymentCard,
                    Receipt = receiptCard,
                    SalesEnquiryQuote = seqCard,
                    Sales = salesCard,
                    Labour = labourCard,
                    Purchase = purchaseCard,
                    SubContract = subContractCard,
                    Export = expCard,
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error occurred while loading dashboard summary.");
                throw;
            }
        }

        #endregion Summary Cards

        #region OVERVIEW TAB
        public async Task<OverviewDto> GetOverviewAsync(DashboardFilterDto filter)
        {
            try
            {
                var from = filter.FromDate.Date;
                var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // ── 1. FETCH ALL 5 INVOICE TYPES ──────────────────────────────

                var mfgInvs = await _unitOfWork.MfgInvs.GetQueryable()
                    .Where(i => i.InvDateNow >= from && i.InvDateNow <= to)
                    .Select(i => new
                    {
                        Date = i.InvDateNow,
                        i.GrandTotal,
                        Customer = i.Customer != null ? i.Customer.CustName : "-",
                        DocNo = i.InvNo
                    })
                    .ToListAsync();

                var purInvs = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .Where(i => i.InvDateNow >= from && i.InvDateNow <= to)
                    .Select(i => new
                    {
                        Date = i.InvDateNow,
                        i.GrandTotal,
                        DocNo = i.InvNo
                    })
                    .ToListAsync();

                var expInvs = await _unitOfWork.ExpInvs.GetQueryable()
                    .Where(i => i.ExpInvDateNow >= from && i.ExpInvDateNow <= to)
                    .Select(i => new
                    {
                        Date = i.ExpInvDateNow,
                        i.GrandTotal,
                        DocNo = i.ExpInvNo
                    })
                    .ToListAsync();

                var labInvs = await _unitOfWork.LabInvs.GetQueryable()
                    .Where(i => i.LabInvDateNow >= from && i.LabInvDateNow <= to)
                    .Select(i => new
                    {
                        Date = i.LabInvDateNow,
                        i.GrandTotal,
                        DocNo = i.LabInvNo
                    })
                    .ToListAsync();

                var subconInvs = await _unitOfWork.SubConInvs.GetQueryable()
                    .Where(i => i.InvDateNow >= from && i.InvDateNow <= to)
                    .Select(i => new
                    {
                        Date = i.InvDateNow,
                        i.GrandTotal,
                        DocNo = i.InvNo
                    })
                    .ToListAsync();

                var slots = BuildSlots(from, to, filter.GroupBy);

                // ── 3. HELPER: slot key for a given DateTime ──────────────────
                string GetSlotKey(DateTime dt) => filter.GroupBy switch
                {
                    "Hour" => $"{from:yyyy-MM-dd} {dt.Hour:00}",
                    "Day" => dt.ToString("ddd"),
                    "Week" => $"W{GetWeekNumber(dt, from)}",
                    "Month" => dt.ToString("MMM"),
                    _ => dt.Date.ToString("yyyy-MM-dd")
                };

                // ── 4. LOCAL HELPER: build a chart series from any invoice list
                List<ChartPointDto> BuildSeries<T>(
                    IEnumerable<T> source,
                    Func<T, DateTime> dateSelector,
                    Func<T, decimal> valueSelector)
                {
                    var grouped = source
                        .GroupBy(x => GetSlotKey(dateSelector(x)))
                        .ToDictionary(g => g.Key, g => g.Sum(valueSelector));

                    return slots.Select(slot => new ChartPointDto
                    {
                        Label = slot.Label,
                        Value = grouped.TryGetValue(slot.Key, out var v) ? v : 0m
                    }).ToList();
                }

                // ── 5. MULTI-SERIES LINE CHART ────────────────────────────────
                var salesTrend = new List<ChartSeriesDto>
                {
                    new() { Name = "Sales",    Data = BuildSeries(mfgInvs,    x => x.Date, x => x.GrandTotal) },
                    new() { Name = "Purchase", Data = BuildSeries(purInvs,    x => x.Date, x => x.GrandTotal) },
                    new() { Name = "Export",   Data = BuildSeries(expInvs,    x => x.Date, x => x.GrandTotal) },
                    new() { Name = "Labour",   Data = BuildSeries(labInvs,    x => x.Date, x => x.GrandTotal) },
                    new() { Name = "Sub-Con",  Data = BuildSeries(subconInvs, x => x.Date, x => x.GrandTotal) },
                };

                // ── 6. DONUT CATEGORY SPLIT ───────────────────────────────────
                var businessSplit = new List<BusinessDataDto>
                {
                    new() { Category = "Sales",    Value = mfgInvs.Sum(x => x.GrandTotal)    },
                    new() { Category = "Purchase", Value = purInvs.Sum(x => x.GrandTotal)    },
                    new() { Category = "Export",   Value = expInvs.Sum(x => x.GrandTotal)    },
                    new() { Category = "Labour",   Value = labInvs.Sum(x => x.GrandTotal)    },
                    new() { Category = "Sub-Con",  Value = subconInvs.Sum(x => x.GrandTotal) },
                }
                .Where(b => b.Value > 0)
                .ToList();

                // ── 7. COMBINED RECENT TRANSACTIONS ───────────────────────────
                var txList = new List<TransactionDto>();

                txList.AddRange(mfgInvs.Select(i => new TransactionDto
                {
                    DocNo = i.DocNo,
                    Party = i.Customer,
                    Type = "Sales",
                    Date = i.Date.ToString("dd-MMM-yyyy"),
                    Amount = FormatCurrency(i.GrandTotal),
                    Status = "Completed",
                    StatusClass = "done"
                }));

                txList.AddRange(purInvs.Select(i => new TransactionDto
                {
                    DocNo = i.DocNo,
                    Party = "Vendor",
                    Type = "Purchase",
                    Date = i.Date.ToString("dd-MMM-yyyy"),
                    Amount = FormatCurrency(i.GrandTotal),
                    Status = "Completed",
                    StatusClass = "done"
                }));

                txList.AddRange(expInvs.Select(i => new TransactionDto
                {
                    DocNo = i.DocNo,
                    Party = "-",
                    Type = "Export",
                    Date = i.Date.ToString("dd-MMM-yyyy"),
                    Amount = FormatCurrency(i.GrandTotal),
                    Status = "Completed",
                    StatusClass = "done"
                }));

                txList.AddRange(labInvs.Select(i => new TransactionDto
                {
                    DocNo = i.DocNo,
                    Party = "-",
                    Type = "Labour",
                    Date = i.Date.ToString("dd-MMM-yyyy"),
                    Amount = FormatCurrency(i.GrandTotal),
                    Status = "Completed",
                    StatusClass = "done"
                }));

                txList.AddRange(subconInvs.Select(i => new TransactionDto
                {
                    DocNo = i.DocNo,
                    Party = "-",
                    Type = "Sub-Con",
                    Date = i.Date.ToString("dd-MMM-yyyy"),
                    Amount = FormatCurrency(i.GrandTotal),
                    Status = "Completed",
                    StatusClass = "done"
                }));

                var recentTx = txList
                    .OrderByDescending(t =>
                        DateTime.ParseExact(t.Date, "dd-MMM-yyyy", CultureInfo.InvariantCulture))
                    .Take(10)
                    .ToList();

                return new OverviewDto
                {
                    SalesTrend = salesTrend,
                    BusinessSplit = businessSplit,
                    Transactions = recentTx,
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error occurred while loading dashboard overview.");
                return new OverviewDto
                {
                    SalesTrend = new List<ChartSeriesDto>(),
                    BusinessSplit = new List<BusinessDataDto>(),
                    Transactions = new List<TransactionDto>()
                };
            }
        }

        #endregion OVERVIEW TAB

        #region SALES TAB
        public async Task<SalesDto> GetSalesAsync(DashboardFilterDto filter)
        {
            try
            {
                var fromDate = filter.FromDate.Date;
                var toDate = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // ==========================================================
                // ENQUIRIES
                // ==========================================================
                var enquiries = await _unitOfWork.EnquirySaless.GetQueryable()
                    .Where(x =>
                        x.EnquiryDateNow >= fromDate &&
                        x.EnquiryDateNow <= toDate && x.MfgORLab)
                    .Select(x => new
                    {
                        x.EnquiryTally,
                        x.Cancel,
                        x.ShortClose
                    })
                    .ToListAsync();

                // ==========================================================
                // QUOTATIONS
                // ==========================================================
                var quotations = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Where(x =>
                        x.QuoteDateNow >= fromDate &&
                        x.QuoteDateNow <= toDate && x.MfgOrLab)
                    .Select(x => new
                    {
                        x.QuotationTally,
                        x.IsCancel,
                        x.ShortClose
                    })
                    .ToListAsync();

                // ==========================================================
                // PURCHASE ORDERS
                // ==========================================================
                var purchaseOrders = await _unitOfWork.MfgPos.GetQueryable()
                    .Where(x =>
                        x.PODateNow >= fromDate &&
                        x.PODateNow <= toDate &&
                        !x.MfgORLab)
                    .Select(x => new
                    {
                        x.PODateNow,
                        x.GrandTotal,
                        x.PoTally,
                        x.PoCancl,
                        x.ShortClose
                    })
                    .ToListAsync();

                // ==========================================================
                // SALES INVOICES
                // ==========================================================
                var salesInvoices = await _unitOfWork.MfgInvs.GetQueryable()
                    .Where(x =>
                        x.InvDateNow >= fromDate &&
                        x.InvDateNow <= toDate)
                    .Select(x => new
                    {
                        x.InvDateNow,
                        x.InvDate,
                        x.Prefix,
                        x.InvNo,
                        x.Suffix,
                        x.GrandTotal,
                        x.Balance,
                        x.IsCancel,
                        x.NoOfItems,
                        x.InvTally,
                        x.PoShortClose,
                        x.CustId,
                        CustomerName = x.Customer.CustName
                    })
                    .ToListAsync();

                // ==========================================================
                // RECENT TRANSACTIONS
                // ==========================================================
                var recentTransactions = salesInvoices
                    .OrderByDescending(x => x.InvDateNow)
                    .Take(5)
                    .Select(x => new MfgInvVM
                    {
                        InvNo = $"{x.Prefix}{x.InvNo}{x.Suffix}",
                        InvDate = x.InvDate,
                        CustName = x.CustomerName,
                        NoOfItems = x.NoOfItems,
                        GrandTotal = x.GrandTotal,
                        InvTally = x.InvTally,
                        PoShortClose = x.PoShortClose,
                        IsCancel = x.IsCancel,
                        Balance = x.Balance
                    })
                    .ToList();

                // ==========================================================
                // TOP CUSTOMERS
                // ==========================================================
                var topCustomers = salesInvoices
                    .GroupBy(x => x.CustomerName)
                    .Select(g => new TopCustomerDto
                    {
                        Name = g.Key,
                        Orders = g.Count(),
                        Revenue = FormatCurrency(g.Sum(x => x.GrandTotal))
                    })
                    .OrderByDescending(x => x.Orders)
                    .Take(5)
                    .ToList();

                // ==========================================================
                // ENQUIRY SUMMARY
                // ==========================================================
                var totalEnquiries = enquiries.Count;
                var completedEnquiries = enquiries.Count(x => x.EnquiryTally);
                var cancelledEnquiries = enquiries.Count(x => x.Cancel);
                var shortClosedEnquiries = enquiries.Count(x => x.ShortClose);
                var pendingEnquiries = enquiries.Count(x => !x.EnquiryTally);

                // ==========================================================
                // QUOTATION SUMMARY
                // ==========================================================
                var totalQuotations = quotations.Count;
                var completedQuotations = quotations.Count(x => x.QuotationTally);
                var cancelledQuotations = quotations.Count(x => x.IsCancel);
                var shortClosedQuotations = quotations.Count(x => x.ShortClose);
                var pendingQuotations = quotations.Count(x => !x.QuotationTally);

                // ==========================================================
                // PURCHASE ORDER SUMMARY
                // ==========================================================
                var totalPurchaseOrders = purchaseOrders.Count;
                var completedPurchaseOrders = purchaseOrders.Count(x => x.PoTally);

                var cancelledPurchaseOrders = purchaseOrders.Count(x =>
                    x.PoCancl &&
                    !x.PoTally &&
                    !x.ShortClose);

                var shortClosedPurchaseOrders = purchaseOrders.Count(x =>
                    x.ShortClose &&
                    !x.PoTally &&
                    !x.PoCancl);

                var pendingPurchaseOrders = purchaseOrders.Count(x =>
                    !x.PoTally &&
                    !x.PoCancl &&
                    !x.ShortClose);

                var totalPurchaseOrderValue = purchaseOrders.Sum(x => x.GrandTotal);

                // ==========================================================
                // INVOICE SUMMARY
                // ==========================================================
                var totalInvoices = salesInvoices.Count;

                var paidInvoices = salesInvoices.Count(x =>
                    x.Balance == 0 &&
                    !x.IsCancel);

                var pendingInvoices = salesInvoices.Count(x =>
                    x.Balance > 0 &&
                    !x.IsCancel);

                var cancelledInvoices = salesInvoices.Count(x => x.IsCancel);

                var totalInvoiceValue = salesInvoices.Sum(x => x.GrandTotal);

                var pendingInvoiceValue = salesInvoices
                    .Where(x => !x.IsCancel)
                    .Sum(x => x.Balance);

                // ==========================================================
                // REVENUE
                // ==========================================================
                var totalRevenue = salesInvoices
                    .Where(x => !x.IsCancel)
                    .Sum(x => x.GrandTotal);

                var collectedRevenue = salesInvoices
                    .Where(x => !x.IsCancel)
                    .Sum(x => x.GrandTotal - x.Balance);

                var pendingRevenue = salesInvoices
                    .Where(x => !x.IsCancel)
                    .Sum(x => x.Balance);

                // ==========================================================
                // CUSTOMER ANALYTICS
                // ==========================================================
                var totalCustomers = salesInvoices
                    .Select(x => x.CustId)
                    .Distinct()
                    .Count();

                var repeatCustomers = salesInvoices
                    .GroupBy(x => x.CustId)
                    .Count(g => g.Count() > 1);

                var newCustomers = totalCustomers - repeatCustomers;

                // ==========================================================
                // SALES ANALYTICS
                // ==========================================================
                var averageOrderValue = purchaseOrders.Any()
                    ? purchaseOrders.Average(x => x.GrandTotal)
                    : 0;

                var averageInvoiceValue = salesInvoices.Any()
                    ? salesInvoices.Average(x => x.GrandTotal)
                    : 0;

                var totalItemsSold = salesInvoices.Sum(x => x.NoOfItems);

                // ==========================================================
                // SALES TREND
                // ==========================================================

                var slots = BuildSlots(fromDate, toDate, filter.GroupBy);

                string GetSlotKey(DateTime dt) => filter.GroupBy switch
                {
                    "Hour" => $"{fromDate:yyyy-MM-dd} {dt.Hour:00}",
                    "Day" => dt.ToString("ddd"),
                    "Week" => $"W{GetWeekNumber(dt, fromDate)}",
                    "Month" => dt.ToString("MMM"),
                    _ => dt.Date.ToString("yyyy-MM-dd")
                };

                var actualSales = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = salesInvoices
                        .Where(i => GetSlotKey(i.InvDateNow) == s.Key)
                        .Sum(i => i.GrandTotal)
                }).ToList();

                var targetSales = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = purchaseOrders
                        .Where(p => GetSlotKey(p.PODateNow) == s.Key)
                        .Sum(p => p.GrandTotal)
                }).ToList();

                // ==========================================================
                // PAYMENT SPLIT
                // ==========================================================

                var paidAmount = salesInvoices
                    .Where(x => x.Balance == 0 && !x.IsCancel)
                    .Sum(x => x.GrandTotal);

                var pendingAmount = salesInvoices
                    .Where(x => x.Balance > 0 && !x.IsCancel)
                    .Sum(x => x.Balance);

                var paymentSplit = new List<BusinessDataDto>
                {
                    new()
                    {
                        Category = "Paid",
                        Value = paidAmount
                    },
                    new()
                    {
                        Category = "Pending",
                        Value = pendingAmount
                    }
                }
                                .Where(x => x.Value > 0)
                                .ToList();


                return new SalesDto
                {
                    TotalEnquiries = totalEnquiries,
                    CompletedEnquiries = completedEnquiries,
                    PendingEnquiries = pendingEnquiries,
                    CancelledEnquiries = cancelledEnquiries,
                    ShortClosedEnquiries = shortClosedEnquiries,

                    TotalQuotations = totalQuotations,
                    CompletedQuotations = completedQuotations,
                    PendingQuotations = pendingQuotations,
                    CancelledQuotations = cancelledQuotations,
                    ShortClosedQuotations = shortClosedQuotations,

                    TotalPurchaseOrders = totalPurchaseOrders,
                    CompletedPurchaseOrders = completedPurchaseOrders,
                    PendingPurchaseOrders = pendingPurchaseOrders,
                    CancelledPurchaseOrders = cancelledPurchaseOrders,
                    ShortClosedPurchaseOrders = shortClosedPurchaseOrders,
                    TotalPurchaseOrderValue = totalPurchaseOrderValue,

                    TotalInvoices = totalInvoices,
                    PaidInvoices = paidInvoices,
                    PendingInvoices = pendingInvoices,
                    CancelledInvoices = cancelledInvoices,
                    TotalInvoiceValue = totalInvoiceValue,
                    PendingInvoiceValue = pendingInvoiceValue,

                    TotalRevenue = totalRevenue,
                    CollectedRevenue = collectedRevenue,
                    PendingRevenue = pendingRevenue,

                    TotalCustomers = totalCustomers,
                    NewCustomers = newCustomers,
                    RepeatCustomers = repeatCustomers,

                    AverageOrderValue = averageOrderValue,
                    AverageInvoiceValue = averageInvoiceValue,
                    TotalItemsSold = totalItemsSold ?? 0,

                    ActualSales = actualSales,
                    TargetSales = targetSales,

                    PaymentSplit = paymentSplit,

                    TopCustomers = topCustomers,
                    Orders = recentTransactions
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    "Error occurred while loading sales dashboard.");

                return new SalesDto();
            }
        }
        #endregion


        #region LABOUR TAB
        public async Task<LabourDto> GetLabourAsync(DashboardFilterDto filter)
        {
            try
            {
                var fromDate = filter.FromDate.Date;
                var toDate = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // LABOUR ENQUIRIES
                var enquiries = await _unitOfWork.EnquirySaless.GetQueryable()
                    .Where(x =>
                        x.EnquiryDateNow >= fromDate &&
                        x.EnquiryDateNow <= toDate &&
                        !x.MfgORLab)
                    .Select(x => new
                    {
                        x.EnquiryTally,
                        x.Cancel,
                        x.ShortClose
                    })
                    .ToListAsync();

                // LABOUR QUOTATIONS
                var quotations = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Where(x =>
                        x.QuoteDateNow >= fromDate &&
                        x.QuoteDateNow <= toDate &&
                        !x.MfgOrLab)
                    .Select(x => new
                    {
                        x.QuotationTally,
                        x.IsCancel,
                        x.ShortClose
                    })
                    .ToListAsync();

                // LABOUR PURCHASE ORDERS
                var purchaseOrders = await _unitOfWork.MfgPos.GetQueryable()
                    .Where(x =>
                        x.PODateNow >= fromDate &&
                        x.PODateNow <= toDate &&
                        x.MfgORLab)
                    .Select(x => new
                    {
                        x.PODateNow,
                        x.GrandTotal,
                        x.PoTally,
                        x.PoCancl,
                        x.ShortClose
                    })
                    .ToListAsync();

                // LABOUR INVOICES
                var invoices = await _unitOfWork.LabInvs.GetQueryable()
                    .Where(x =>
                        x.LabInvDateNow >= fromDate &&
                        x.LabInvDateNow <= toDate)
                    .Select(x => new
                    {
                        x.Prefix,
                        x.LabInvNo,
                        x.Suffix,
                        x.LabInvDate,
                        x.LabInvDateNow,
                        CustomerName = x.Customer.CustName,
                        x.GrandTotal,
                        x.Balance,
                        x.InvCancel,
                        x.LabInvTally,
                        x.CreatedBy,
                        x.CreatedDate
                    })
                    .ToListAsync();

                // SUMMARY - ENQUIRY
                var totalEnquiries = enquiries.Count;
                var completedEnquiries = enquiries.Count(x => x.EnquiryTally);
                var cancelledEnquiries = enquiries.Count(x => x.Cancel);
                var shortClosedEnquiries = enquiries.Count(x => x.ShortClose);
                var pendingEnquiries = enquiries.Count(x => !x.EnquiryTally && !x.Cancel && !x.ShortClose);


                // SUMMARY - QUOTATION
                var totalQuotations = quotations.Count;
                var completedQuotations = quotations.Count(x => x.QuotationTally);
                var pendingQuotations = quotations.Count(x => !x.QuotationTally);
                var cancelledQuotations = quotations.Count(x => x.IsCancel);
                var shortClosedQuotations = quotations.Count(x => x.ShortClose);

                // SUMMARY - PO
                var totalPurchaseOrders = purchaseOrders.Count;
                var completedPurchaseOrders = purchaseOrders.Count(x => x.PoTally);

                var cancelledPurchaseOrders = purchaseOrders.Count(x =>
                    x.PoCancl && !x.PoTally && !x.ShortClose);

                var shortClosedPurchaseOrders = purchaseOrders.Count(x =>
                    x.ShortClose && !x.PoTally && !x.PoCancl);

                var pendingPurchaseOrders = purchaseOrders.Count(x =>
                    !x.PoTally && !x.PoCancl && !x.ShortClose);

                var totalPurchaseOrderValue = purchaseOrders.Sum(x => x.GrandTotal);

                // SUMMARY - INVOICES
                var totalInvoices = invoices.Count;

                var paidInvoices = invoices.Count(x => x.Balance == 0 && !x.InvCancel);
                var pendingInvoices = invoices.Count(x => x.Balance > 0 && !x.InvCancel);
                var cancelledInvoices = invoices.Count(x => x.InvCancel);

                var totalInvoiceValue = invoices.Sum(x => x.GrandTotal);

                var pendingInvoiceValue = invoices
                    .Where(x => !x.InvCancel)
                    .Sum(x => x.Balance);

                // REVENUE
                var totalRevenue = invoices.Where(x => !x.InvCancel)
                    .Sum(x => x.GrandTotal);

                var collectedRevenue = invoices.Where(x => !x.InvCancel)
                    .Sum(x => x.GrandTotal - x.Balance);

                var pendingRevenue = invoices.Where(x => !x.InvCancel)
                    .Sum(x => x.Balance);

                // CUSTOMER ANALYTICS
                var totalCustomers = invoices.Select(x => x.CustomerName).Distinct().Count();

                var repeatCustomers = invoices
                    .GroupBy(x => x.CustomerName)
                    .Count(g => g.Count() > 1);

                var newCustomers = totalCustomers - repeatCustomers;

                // AVERAGE
                var averageOrderValue = purchaseOrders.Any()
                    ? purchaseOrders.Average(x => x.GrandTotal)
                    : 0;

                var averageInvoiceValue = invoices.Any()
                    ? invoices.Average(x => x.GrandTotal)
                    : 0;

                // LABOUR TOTALS
                var totalJobsCompleted = purchaseOrders.Count(x => x.PoTally);

                var totalLabourHours = 0; // (if you track hours add here)

                // TREND
                var slots = BuildSlots(fromDate, toDate, filter.GroupBy);

                string GetSlotKey(DateTime dt) => filter.GroupBy switch
                {
                    "Hour" => $"{fromDate:yyyy-MM-dd} {dt.Hour:00}",
                    "Day" => dt.ToString("ddd"),
                    "Week" => $"W{GetWeekNumber(dt, fromDate)}",
                    "Month" => dt.ToString("MMM"),
                    _ => dt.Date.ToString("yyyy-MM-dd")
                };

                var actualLabour = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = invoices
                        .Where(i => GetSlotKey(i.LabInvDateNow) == s.Key)
                        .Sum(i => i.GrandTotal)
                }).ToList();

                var targetLabour = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = purchaseOrders
                        .Where(p => GetSlotKey(p.PODateNow) == s.Key)
                        .Sum(p => p.GrandTotal)
                }).ToList();

                // PAYMENT SPLIT
                var paidAmount = invoices
                    .Where(x => x.Balance == 0 && !x.InvCancel)
                    .Sum(x => x.GrandTotal);

                var pendingAmount = invoices
                    .Where(x => x.Balance > 0 && !x.InvCancel)
                    .Sum(x => x.Balance);

                var paymentSplit = new List<BusinessDataDto>
        {
            new() { Category = "Paid", Value = paidAmount },
            new() { Category = "Pending", Value = pendingAmount }
        }
                .Where(x => x.Value > 0)
                .ToList();

                // TOP CUSTOMERS
                var topCustomers = invoices
                    .GroupBy(x => x.CustomerName)
                    .Select(g => new TopCustomerDto
                    {
                        Name = g.Key,
                        Orders = g.Count(),
                        Revenue = FormatCurrency(g.Sum(x => x.GrandTotal))
                    })
                    .OrderByDescending(x => x.Orders)
                    .Take(5)
                    .ToList();

                // RECENT ORDERS
                var recentOrders = invoices
                    .OrderByDescending(x => x.LabInvDateNow)
                    .Take(5)
                    .Select(x => new LabInvVM
                    {
                        LabInvNo = $"{x.Prefix}{x.LabInvNo}{x.Suffix}",
                        LabInvDate = x.LabInvDate,
                        CustName = x.CustomerName,
                        GrandTotal = x.GrandTotal,
                        Balance = x.Balance,
                        InvCancel = x.InvCancel,
                        LabInvTally = x.LabInvTally,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate

                    })
                    .ToList();

                return new LabourDto
                {
                    TotalEnquiries = totalEnquiries,
                    CompletedEnquiries = completedEnquiries,
                    PendingEnquiries = pendingEnquiries,
                    CancelledEnquiries = cancelledEnquiries,
                    ShortClosedEnquiries = shortClosedEnquiries,

                    TotalQuotations = totalQuotations,
                    CompletedQuotations = completedQuotations,
                    PendingQuotations = pendingQuotations,
                    CancelledQuotations = cancelledQuotations,
                    ShortClosedQuotations = shortClosedQuotations,

                    TotalPurchaseOrders = totalPurchaseOrders,
                    CompletedPurchaseOrders = completedPurchaseOrders,
                    PendingPurchaseOrders = pendingPurchaseOrders,
                    CancelledPurchaseOrders = cancelledPurchaseOrders,
                    ShortClosedPurchaseOrders = shortClosedPurchaseOrders,
                    TotalPurchaseOrderValue = totalPurchaseOrderValue,

                    TotalInvoices = totalInvoices,
                    PaidInvoices = paidInvoices,
                    PendingInvoices = pendingInvoices,
                    CancelledInvoices = cancelledInvoices,
                    TotalInvoiceValue = totalInvoiceValue,
                    PendingInvoiceValue = pendingInvoiceValue,

                    TotalRevenue = totalRevenue,
                    CollectedRevenue = collectedRevenue,
                    PendingRevenue = pendingRevenue,

                    TotalCustomers = totalCustomers,
                    NewCustomers = newCustomers,
                    RepeatCustomers = repeatCustomers,

                    AverageOrderValue = averageOrderValue,
                    AverageInvoiceValue = averageInvoiceValue,

                    TotalJobsCompleted = totalJobsCompleted,
                    TotalLabourHours = totalLabourHours,

                    ActualLabour = actualLabour,
                    TargetLabour = targetLabour,

                    PaymentSplit = paymentSplit,
                    TopCustomers = topCustomers,
                    Orders = recentOrders
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error occurred while loading labour dashboard.");

                return new LabourDto
                {
                    ActualLabour = new(),
                    TargetLabour = new(),
                    PaymentSplit = new(),
                    Orders = new()
                };
            }
        }

        #endregion LABOUR TAB


        #region PURCHASE TAB
        public async Task<PurchaseDto> GetPurchaseAsync(DashboardFilterDto filter)
        {
            try
            {
                var from = filter.FromDate.Date;
                var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // PURCHASE REQUISITIONS
                var requisitions = await _unitOfWork.MaterialReqs.GetQueryable()
                    .Where(x =>
                        x.MreqDateNow >= from &&
                        x.MreqDateNow <= to &&
                        x.IsPurchOrSubCon)
                    .Select(x => new
                    {
                        x.IsAuthorized,
                        x.PRTally,
                        x.MreqCancel,
                        x.ShortClose,
                        CategoryIds = x.MaterialReqSubs
                            .Select(s => s.Item.CategoryCode)
                            .ToList()
                    })
                    .ToListAsync();


                // PURCHASE ENQUIRIES
                var enquiries = await _unitOfWork.EnquiryPurchases.GetQueryable()
                    .Where(x =>
                        x.EnquiryDateNow >= from &&
                        x.EnquiryDateNow <= to
                        && x.PurchORSubCon)
                    .Select(x => new
                    {
                        x.EnquiryTally,
                        x.Cancel,
                        x.EnquiryShortClose
                    })
                    .ToListAsync();

                // PURCHASE Quotation
                var quotation = await _unitOfWork.PurchaseQuotes.GetQueryable()
                    .Where(x =>
                        x.QuoteDateNow >= from &&
                        x.QuoteDateNow <= to
                        && x.PurchOrSub)
                    .Select(x => new
                    {
                        x.QuotationTally,
                        x.IsCancel,
                        x.QuoteShortClose
                    })
                    .ToListAsync();

                // PURCHASE ORDERS
                var purchaseOrders = await _unitOfWork.PurchPos.GetQueryable()
                    .Include(x => x.PurchPoSubs)
                        .ThenInclude(x => x.Item)
                    .Where(x =>
                        x.PODateNow >= from &&
                        x.PODateNow <= to
                        && x.PurchORSubCon)
                    .Select(x => new
                    {
                        x.PODateNow,
                        x.GrandTotal,
                        x.PoTally,
                        x.PoCancl,
                        x.PoShortClose,
                        VendorName = x.Vendor.VendorName

                    })
                    .ToListAsync();


                // PURCHASE INVOICES
                var invoices = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .Where(x =>
                        x.InvDateNow >= from &&
                        x.InvDateNow <= to)
                    .Select(x => new
                    {
                        x.InvNo,
                        x.Suffix,
                        x.InvDate,
                        x.InvDateNow,
                        x.NoOfItems,
                        x.GrandTotal,
                        x.Balance,
                        x.InvCancel,
                        VendorName = x.Vendor.VendorName,
                        x.CreatedBy,
                        x.CreatedDate
                    })
                    .ToListAsync();

                // SUMMARY - ENQUIRY
                var totalEnquiries = enquiries.Count;

                var completedEnquiries = enquiries.Count(x => x.EnquiryTally && !x.Cancel &&
                    !x.EnquiryShortClose);

                var cancelledEnquiries = enquiries.Count(x => x.Cancel && !x.EnquiryTally &&
                    !x.EnquiryShortClose);

                var shortClosedEnquiries = enquiries.Count(x => x.EnquiryShortClose && !x.EnquiryTally &&
                    !x.Cancel);

                var pendingEnquiries = enquiries.Count(x =>
                    !x.EnquiryTally &&
                    !x.Cancel &&
                    !x.EnquiryShortClose);

                // SUMMARY - PURCHASE REQUISITION
                var totalPurchaseRequisitions = requisitions.Count;

                var completedPurchaseRequisitions =
                    requisitions.Count(x => x.PRTally && !x.ShortClose && !x.MreqCancel);

                var approvedPurchaseRequisitions =
                    requisitions.Count(x => x.IsAuthorized);

                var cancelledPurchaseRequisitions =
                    requisitions.Count(x => x.MreqCancel && !x.ShortClose && !x.PRTally);

                var pendingPurchaseRequisitions =
                    requisitions.Count(x =>
                            !x.PRTally &&
                            !x.MreqCancel &&
                            !x.ShortClose);

                // SUMMARY - Quotation
                var totalquotation = quotation.Count;

                var completedquotation = quotation.Count(x => x.QuotationTally && !x.IsCancel &&
                    !x.QuoteShortClose);

                var cancelledquotation = quotation.Count(x => x.IsCancel && !x.QuotationTally &&
                    !x.QuoteShortClose);

                var shortClosedquotation = quotation.Count(x => x.QuoteShortClose && !x.QuotationTally &&
                    !x.IsCancel);

                var pendingquotation = quotation.Count(x =>
                    !x.QuotationTally &&
                    !x.IsCancel &&
                    !x.QuoteShortClose);


                // SUMMARY - PURCHASE ORDER
                var totalPurchaseOrders = purchaseOrders.Count;

                var completedPurchaseOrders =
                    purchaseOrders.Count(x => x.PoTally);

                var cancelledPurchaseOrders =
                    purchaseOrders.Count(x =>
                        x.PoCancl &&
                        !x.PoTally &&
                        !x.PoShortClose);

                var shortClosedPurchaseOrders =
                    purchaseOrders.Count(x =>
                        x.PoShortClose &&
                        !x.PoTally &&
                        !x.PoCancl);

                var pendingPurchaseOrders =
                    purchaseOrders.Count(x =>
                        !x.PoTally &&
                        !x.PoCancl &&
                        !x.PoShortClose);

                var totalPurchaseOrderValue =
                    purchaseOrders.Sum(x => x.GrandTotal);

                // SUMMARY - PURCHASE INVOICE
                var totalInvoices = invoices.Count;

                var paidInvoices =
                    invoices.Count(x =>
                        x.Balance == 0 &&
                        !x.InvCancel);

                var pendingInvoices =
                    invoices.Count(x =>
                        x.Balance > 0 &&
                        !x.InvCancel);

                var cancelledInvoices =
                    invoices.Count(x =>
                        x.InvCancel);

                var totalInvoiceValue =
                    invoices.Sum(x => x.GrandTotal);

                var pendingInvoiceValue =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Sum(x => x.Balance);

                // EXPENSE ANALYTICS
                var totalExpense =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Sum(x => x.GrandTotal);

                var paidExpense =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Sum(x => x.GrandTotal - x.Balance);

                var outstandingExpense =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Sum(x => x.Balance);

                // VENDOR ANALYTICS
                var totalVendors =
                    invoices
                        .Select(x => x.VendorName)
                        .Distinct()
                        .Count();

                var activeVendors =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Select(x => x.VendorName)
                        .Distinct()
                        .Count();

                var newVendors = totalVendors;

                // PURCHASE ANALYTICS
                var averagePOValue =
                    purchaseOrders.Any()
                        ? purchaseOrders.Average(x => x.GrandTotal)
                        : 0;

                var averageInvoiceValue =
                    invoices.Any()
                        ? invoices.Average(x => x.GrandTotal)
                        : 0;

                var totalItemsPurchased = 0; // Add actual item qty calculation

                // TREND
                var slots = BuildSlots(from, to, filter.GroupBy);

                string GetSlotKey(DateTime dt) => filter.GroupBy switch
                {
                    "Hour" => $"{from:yyyy-MM-dd} {dt.Hour:00}",
                    "Day" => dt.ToString("ddd"),
                    "Week" => $"W{GetWeekNumber(dt, from)}",
                    "Month" => dt.ToString("MMM"),
                    _ => dt.Date.ToString("yyyy-MM-dd")
                };

                var purchaseTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = purchaseOrders
                        .Where(x => GetSlotKey(x.PODateNow) == s.Key)
                        .Sum(x => x.GrandTotal)
                }).ToList();

                var expenseTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = invoices
                        .Where(x => GetSlotKey(x.InvDateNow) == s.Key)
                        .Sum(x => x.GrandTotal)
                }).ToList();

                // VENDOR SPLIT
                var vendorSplit = invoices
                    .GroupBy(x => x.VendorName)
                    .Select(g => new BusinessDataDto
                    {
                        Category = g.Key,
                        Value = g.Sum(x => x.GrandTotal)
                    })
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .ToList();

                // EXPENSE CATEGORY SPLIT
                var expenseCategorySplit = await _unitOfWork.PurchPos.GetQueryable()
                    .Where(x =>
                        x.PODateNow >= from &&
                        x.PODateNow <= to &&
                        x.PurchORSubCon)
                    .SelectMany(x => x.PurchPoSubs)
                    .GroupBy(x => x.Item.Category.CategoryName)
                    .Select(g => new BusinessDataDto
                    {
                        Category = g.Key,
                        Value = g.Sum(x => x.Qty) // or Amount / Total if available
                    })
                    .OrderByDescending(x => x.Value)
                    .ToListAsync();

                // TOP VENDORS
                var topVendors = invoices
                    .GroupBy(x => x.VendorName)
                    .Select(g => new TopVendorDto
                    {
                        Name = g.Key,
                        Orders = g.Count(),
                        Revenue = FormatCurrency(g.Sum(x => x.GrandTotal))
                    })
                    .OrderByDescending(x => x.Orders)
                    .Take(5)
                    .ToList();

                // RECENT INVOICES
                var recentInvoices = invoices
                    .OrderByDescending(x => x.InvDateNow)
                    .Take(5)
                    .Select(x => new PurchaseInvoiceVM
                    {
                        InvNo = $"{x.InvNo}{x.Suffix}",
                        InvDate = x.InvDate,
                        VendorName = x.VendorName,
                        GrandTotal = x.GrandTotal,
                        Balance = x.Balance,
                        InvCancel = x.InvCancel,
                        CreatedBy = x.CreatedBy,
                        CreatedDate = x.CreatedDate
                    })
                    .ToList();

                return new PurchaseDto
                {
                    TotalPurchaseRequisitions = totalPurchaseRequisitions,
                    CompletedPurchaseRequisitions = completedPurchaseRequisitions,
                    ApprovedPurchaseRequisitions = approvedPurchaseRequisitions,
                    PendingPurchaseRequisitions = pendingPurchaseRequisitions,
                    CancelledPurchaseRequisitions = cancelledPurchaseRequisitions,

                    TotalEnquiries = totalEnquiries,
                    CompletedEnquiries = completedEnquiries,
                    PendingEnquiries = pendingEnquiries,
                    CancelledEnquiries = cancelledEnquiries,
                    ShortClosedEnquiries = shortClosedEnquiries,

                    TotalQuotations = totalquotation,
                    CompletedQuotations = completedquotation,
                    PendingQuotations = pendingquotation,
                    CancelledQuotations = cancelledquotation,
                    ShortClosedQuotations = shortClosedquotation,

                    TotalPurchaseOrders = totalPurchaseOrders,
                    CompletedPurchaseOrders = completedPurchaseOrders,
                    PendingPurchaseOrders = pendingPurchaseOrders,
                    CancelledPurchaseOrders = cancelledPurchaseOrders,
                    ShortClosedPurchaseOrders = shortClosedPurchaseOrders,
                    TotalPurchaseOrderValue = totalPurchaseOrderValue,

                    TotalInvoices = totalInvoices,
                    PaidInvoices = paidInvoices,
                    PendingInvoices = pendingInvoices,
                    CancelledInvoices = cancelledInvoices,
                    TotalInvoiceValue = totalInvoiceValue,
                    PendingInvoiceValue = pendingInvoiceValue,

                    TotalExpense = totalExpense,
                    PaidExpense = paidExpense,
                    OutstandingExpense = outstandingExpense,

                    TotalVendors = totalVendors,
                    ActiveVendors = activeVendors,
                    NewVendors = newVendors,

                    AveragePOValue = averagePOValue,
                    AverageInvoiceValue = averageInvoiceValue,
                    TotalItemsPurchased = totalItemsPurchased,

                    PurchaseTrend = purchaseTrend,
                    ExpenseTrend = expenseTrend,

                    VendorSplit = vendorSplit,
                    ExpenseCategorySplit = expenseCategorySplit,

                    TopVendors = topVendors,
                    Invoices = recentInvoices
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "GetPurchaseAsync failed.");
                return new PurchaseDto();

            }
        }

        #endregion PURCHASE TAB


        #region Subcontract TAB
        public async Task<SubcontractDto> GetSubcontractAsync(DashboardFilterDto filter)
        {
            try
            {
                var from = filter.FromDate.Date;
                var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                var enquiries = await _unitOfWork.EnquiryPurchases.GetQueryable()
                                .Where(x =>
                                    x.EnquiryDateNow >= from &&
                                    x.EnquiryDateNow <= to &&
                                    !x.PurchORSubCon)
                                .Select(x => new
                                {
                                    x.EnquiryTally,
                                    x.Cancel,
                                    x.EnquiryShortClose
                                })
                                .ToListAsync();

                var quotations = await _unitOfWork.PurchaseQuotes.GetQueryable()
                                .Where(x =>
                                    x.QuoteDateNow >= from &&
                                    x.QuoteDateNow <= to &&
                                    !x.PurchOrSub)
                                .Select(x => new
                                {
                                    x.QuotationTally,
                                    x.IsCancel,
                                    x.QuoteShortClose
                                })
                                .ToListAsync();


                var orders = await _unitOfWork.PurchPos.GetQueryable()
                            .Where(x =>
                                x.PODateNow >= from &&
                                x.PODateNow <= to &&
                                !x.PurchORSubCon)
                            .Select(x => new
                            {
                                x.PODateNow,
                                x.GrandTotal,
                                x.PoTally,
                                x.PoCancl,
                                x.PoShortClose,
                                ContractorName = x.Vendor.VendorName
                            })
                            .ToListAsync();

                var invoices = await _unitOfWork.SubConInvs.GetQueryable()
                    .Where(x =>
                        x.InvDateNow >= from &&
                        x.InvDateNow <= to)
                    .Select(x => new
                    {
                        x.InvNo,
                        x.Suffix,
                        x.InvDate,
                        x.InvDateNow,
                        x.GrandTotal,
                        x.Balance,
                        x.InvCancel,
                        ContractorName = x.Vendor.VendorName,
                        x.CreatedBy,
                        x.CreatedDate
                    })
                    .ToListAsync();


                // ==========================================================
                // SUMMARY - ENQUIRY
                // ==========================================================
                var totalEnquiries = enquiries.Count;

                var completedEnquiries =
                    enquiries.Count(x =>
                        x.EnquiryTally &&
                        !x.Cancel &&
                        !x.EnquiryShortClose);

                var cancelledEnquiries =
                    enquiries.Count(x =>
                        x.Cancel &&
                        !x.EnquiryTally &&
                        !x.EnquiryShortClose);

                var shortClosedEnquiries =
                    enquiries.Count(x =>
                        x.EnquiryShortClose &&
                        !x.EnquiryTally &&
                        !x.Cancel);

                var pendingEnquiries =
                    enquiries.Count(x =>
                        !x.EnquiryTally &&
                        !x.Cancel &&
                        !x.EnquiryShortClose);


                // ==========================================================
                // SUMMARY - QUOTATION
                // ==========================================================
                var totalQuotations = quotations.Count;

                var completedQuotations =
                    quotations.Count(x =>
                        x.QuotationTally &&
                        !x.IsCancel &&
                        !x.QuoteShortClose);

                var cancelledQuotations =
                    quotations.Count(x =>
                        x.IsCancel &&
                        !x.QuotationTally &&
                        !x.QuoteShortClose);

                var shortClosedQuotations =
                    quotations.Count(x =>
                        x.QuoteShortClose &&
                        !x.QuotationTally &&
                        !x.IsCancel);

                var pendingQuotations =
                    quotations.Count(x =>
                        !x.QuotationTally &&
                        !x.IsCancel &&
                        !x.QuoteShortClose);


                var totalOrders = orders.Count;

                var completedOrders =
                    orders.Count(x => x.PoTally);

                var cancelledOrders =
                    orders.Count(x =>
                        x.PoCancl &&
                        !x.PoTally &&
                        !x.PoShortClose);

                var shortClosedOrders =
                    orders.Count(x =>
                        x.PoShortClose &&
                        !x.PoTally &&
                        !x.PoCancl);

                var pendingOrders =
                    orders.Count(x =>
                        !x.PoTally &&
                        !x.PoCancl &&
                        !x.PoShortClose);

                var totalOrderValue =
                    orders.Sum(x => x.GrandTotal);


                var totalInvoices = invoices.Count;

                var paidInvoices =
                    invoices.Count(x =>
                        x.Balance == 0 &&
                        !x.InvCancel);

                var pendingInvoices =
                    invoices.Count(x =>
                        x.Balance > 0 &&
                        !x.InvCancel);

                var cancelledInvoices =
                    invoices.Count(x => x.InvCancel);

                var totalInvoiceValue =
                    invoices.Sum(x => x.GrandTotal);

                var pendingInvoiceValue =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Sum(x => x.Balance);

                var totalContractValue =
                        invoices
                            .Where(x => !x.InvCancel)
                            .Sum(x => x.GrandTotal);


                var paidAmount =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Sum(x => x.GrandTotal - x.Balance);

                var outstandingAmount =
                    invoices
                        .Where(x => !x.InvCancel)
                        .Sum(x => x.Balance);


                var totalContractors =invoices
                                .Select(x => x.ContractorName)
                                .Distinct()
                                .Count();

                var activeContractors =invoices
                        .Where(x => !x.InvCancel)
                        .Select(x => x.ContractorName)
                        .Distinct()
                        .Count();

                var newContractors = totalContractors;

                var averageOrderValue = orders.Any()
                                ? orders.Average(x => x.GrandTotal)
                                : 0;

                var averageInvoiceValue = invoices.Any()
                        ? invoices.Average(x => x.GrandTotal)
                        : 0;

                var totalJobsAssigned = totalOrders;

                // ── Time-series ───────────────────────────────────────────────
                var slots = BuildSlots(from, to, filter.GroupBy);
                string GetSlotKey(DateTime dt) => filter.GroupBy switch
                {
                    "Hour" => $"{from:yyyy-MM-dd} {dt.Hour:00}",
                    "Day" => dt.ToString("ddd"),
                    "Week" => $"W{GetWeekNumber(dt, from)}",
                    "Month" => dt.ToString("MMM"),
                    _ => dt.Date.ToString("yyyy-MM-dd")
                };

                var orderTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = orders
                        .Where(x => GetSlotKey(x.PODateNow) == s.Key)
                        .Sum(x => x.GrandTotal)
                }).ToList();

                var invoiceTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = invoices
                        .Where(x => GetSlotKey(x.InvDateNow) == s.Key)
                        .Sum(x => x.GrandTotal)
                }).ToList();

                var workCategorySplit = await _unitOfWork.PurchPos.GetQueryable()
                                        .Where(x =>
                                            x.PODateNow >= from &&
                                            x.PODateNow <= to &&
                                            !x.PurchORSubCon)
                                        .SelectMany(x => x.PurchPoSubs)
                                        .GroupBy(x => x.Item.Category.CategoryName)
                                        .Select(g => new BusinessDataDto
                                        {
                                            Category = g.Key,
                                            Value = g.Sum(x => x.Qty)
                                        })
                                        .OrderByDescending(x => x.Value)
                                        .ToListAsync();

                // ── Contractor split ──────────────────────────────────────────
                var contractorSplit = invoices
                    .GroupBy(x => x.ContractorName)
                    .Select(g => new BusinessDataDto
                    {
                        Category = g.Key,
                        Value = g.Sum(x => x.GrandTotal)
                    })
                    .OrderByDescending(x => x.Value)
                    .Take(5)
                    .ToList();

                var topContractors = invoices
                                .GroupBy(x => x.ContractorName)
                                .Select(g => new TopVendorDto
                                {
                                    Name = g.Key,
                                    Orders = g.Count(),
                                    Revenue = FormatCurrency(g.Sum(x => x.GrandTotal))
                                })
                                .OrderByDescending(x => x.Orders)
                                .Take(5)
                                .ToList();

                var recentInvoices = invoices
                                    .OrderByDescending(x => x.InvDateNow)
                                    .Take(5)
                                    .Select(x => new SubConInvVM
                                    {
                                        InvNo = $"{x.InvNo}{x.Suffix}",
                                        InvDate = x.InvDate,
                                        VendorName = x.ContractorName,
                                        GrandTotal = x.GrandTotal,
                                        Balance = x.Balance,
                                        InvCancel = x.InvCancel,
                                        CreatedBy = x.CreatedBy,
                                        CreatedDate = x.CreatedDate
                                    })
                                    .ToList();

                return new SubcontractDto
                {
                    TotalEnquiries = totalEnquiries,
                    CompletedEnquiries = completedEnquiries,
                    PendingEnquiries = pendingEnquiries,
                    CancelledEnquiries = cancelledEnquiries,
                    ShortClosedEnquiries = shortClosedEnquiries,

                    TotalQuotations = totalQuotations,
                    CompletedQuotations = completedQuotations,
                    PendingQuotations = pendingQuotations,
                    CancelledQuotations = cancelledQuotations,
                    ShortClosedQuotations = shortClosedQuotations,

                    TotalOrders = totalOrders,
                    CompletedOrders = completedOrders,
                    PendingOrders = pendingOrders,
                    CancelledOrders = cancelledOrders,
                    ShortClosedOrders = shortClosedOrders,
                    TotalOrderValue = totalOrderValue,

                    TotalInvoices = totalInvoices,
                    PaidInvoices = paidInvoices,
                    PendingInvoices = pendingInvoices,
                    CancelledInvoices = cancelledInvoices,
                    TotalInvoiceValue = totalInvoiceValue,
                    PendingInvoiceValue = pendingInvoiceValue,

                    TotalContractValue = totalContractValue,
                    PaidAmount = paidAmount,
                    OutstandingAmount = outstandingAmount,

                    TotalContractors = totalContractors,
                    ActiveContractors = activeContractors,
                    NewContractors = newContractors,

                    AverageOrderValue = averageOrderValue,
                    AverageInvoiceValue = averageInvoiceValue,
                    TotalJobsAssigned = totalJobsAssigned,

                    OrderTrend = orderTrend,
                    InvoiceTrend = invoiceTrend,

                    ContractorSplit = contractorSplit,
                    WorkCategorySplit = workCategorySplit,

                    TopContractors = topContractors,
                    Invoices = recentInvoices
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "GetSubcontractAsync failed.");
                return new SubcontractDto();
                
            }
        }
        #endregion Subcontract TAB



        public async Task<JobCardDto> GetJobCardAsync(DashboardFilterDto filter)
        {
            try
            {
                var from = filter.FromDate.Date;
                var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // ==========================================================
                // JOB ORDERS
                // ==========================================================
                var jobCards = await _unitOfWork.JobOrders.GetQueryable()
                    .Where(x => x.CreatedDate >= from && x.CreatedDate <= to)
                    .Select(x => new
                    {
                        x.JobNo,
                        x.CreatedDate,
                        x.JobOrderQty,
                        x.JobBalQty,
                        x.JobTally,

                        PONo = x.MfgPo.PONo,
                        ItemCode = x.AssyItem.ItemCode,
                        ItemName = x.AssyItem.ItemName
                    })
                    .ToListAsync();

                // ==========================================================
                // ISSUES
                // ==========================================================
                var issues = await _unitOfWork.ProductionIssueAssys.GetQueryable()
                    .Where(x => x.IssueDateNow >= from && x.IssueDateNow <= to)
                    .Select(x => new
                    {
                        x.IssueNo,
                        x.IssueDateNow,
                        x.IssueQty,
                        x.IssueTally
                    })
                    .ToListAsync();

                // ==========================================================
                // RETURNS
                // ==========================================================
                var returns = await _unitOfWork.ProductionReturnAssys.GetQueryable()
                    .Where(x => x.ReturnDateNow >= from && x.ReturnDateNow <= to)
                    .Select(x => new
                    {
                        x.Return,
                        x.ReturnDateNow,
                        x.ReturnTally,
                        QtyReturned =
                            x.ProductionReturnAssySubs.Sum(s => (decimal?)s.QtyReturned) ?? 0
                    })
                    .ToListAsync();

                // ==========================================================
                // SCNs
                // ==========================================================
                var scns = await _unitOfWork.ProductionSCNAssys.GetQueryable()
                    .Where(x => x.SCNDateNow >= from && x.SCNDateNow <= to)
                    .Include(x => x.ProductionSCNAssySubs)
                    .ToListAsync();

                // ==========================================================
                // JOB SUMMARY
                // ==========================================================
                var totalJobOrders = jobCards.Count;

                var completedJobOrders =
                    jobCards.Count(x => x.JobTally);

                var pendingJobOrders =
                    jobCards.Count(x =>
                        !x.JobTally &&
                        x.JobOrderQty == x.JobBalQty);

                var inProgressJobOrders =
                    jobCards.Count(x =>
                        !x.JobTally &&
                        x.JobBalQty > 0 &&
                        x.JobBalQty < x.JobOrderQty);

                var plannedAssemblyQty =
                    jobCards.Sum(x => x.JobOrderQty);

                var balanceAssemblyQty =
                    jobCards.Sum(x => x.JobBalQty);

                var producedAssemblyQty =
                    plannedAssemblyQty - balanceAssemblyQty;

                // ==========================================================
                // ISSUE SUMMARY
                // ==========================================================
                var totalIssues = issues.Count;
                var completedIssues = issues.Count(x => x.IssueTally);
                var issuedQty = issues.Sum(x => x.IssueQty);

                // ==========================================================
                // RETURN SUMMARY
                // ==========================================================
                var totalReturns = returns.Count;
                var completedReturns = returns.Count(x => x.ReturnTally);
                var returnedQty = returns.Sum(x => x.QtyReturned);

                // ==========================================================
                // SCN SUMMARY
                // ==========================================================
                var totalSCNs = scns.Count;

                var scnQty = scns.Sum(x =>
                    x.ProductionSCNAssySubs.Sum(s => s.AccQty));

                // ==========================================================
                // KPI CALCULATIONS
                // ==========================================================
                var materialConsumption = issuedQty - returnedQty;

                var returnPercentage =
                    issuedQty == 0
                        ? 0
                        : Math.Round((returnedQty / issuedQty) * 100, 2);

                var productionEfficiency =
                    plannedAssemblyQty == 0
                        ? 0
                        : Math.Round((producedAssemblyQty / plannedAssemblyQty) * 100, 2);

                // ==========================================================
                // CHARTS
                // ==========================================================
                var slots = BuildSlots(from, to, filter.GroupBy);

                string GetSlotKey(DateTime dt) =>
                    filter.GroupBy switch
                    {
                        "Hour" => $"{from:yyyy-MM-dd} {dt.Hour:00}",
                        "Day" => dt.ToString("ddd"),
                        "Week" => $"W{GetWeekNumber(dt, from)}",
                        "Month" => dt.ToString("MMM"),
                        _ => dt.ToString("yyyy-MM-dd")
                    };

                var jobTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = jobCards.Count(x => GetSlotKey(x.CreatedDate) == s.Key)
                }).ToList();

                var issueTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = issues.Count(x => GetSlotKey(x.IssueDateNow) == s.Key)
                }).ToList();

                var returnTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = returns.Count(x => GetSlotKey(x.ReturnDateNow) == s.Key)
                }).ToList();

                var scnTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = scns.Count(x => GetSlotKey(x.SCNDateNow) == s.Key)
                }).ToList();

                // ==========================================================
                // STATUS DONUT
                // ==========================================================
                var statusSplit = new List<BusinessDataDto>
        {
            new()
            {
                Category = "Completed",
                Value = completedJobOrders
            },
            new()
            {
                Category = "In Progress",
                Value = inProgressJobOrders
            },
            new()
            {
                Category = "Pending",
                Value = pendingJobOrders
            }
        };

                // ==========================================================
                // TOP ASSEMBLIES
                // ==========================================================
                var topAssemblies = jobCards
                    .GroupBy(x => new
                    {
                        x.ItemCode,
                        x.ItemName
                    })
                    .Select(g => new TopItemDto
                    {
                        ItemCode = g.Key.ItemCode,
                        ItemName = g.Key.ItemName,
                        Jobs = g.Count(),
                        PlannedQty = g.Sum(x => x.JobOrderQty),
                        ProducedQty =
                            g.Sum(x => x.JobOrderQty - x.JobBalQty),

                        CompletedQty =
                            g.Where(x => x.JobTally)
                             .Sum(x => x.JobOrderQty),

                        CompletionPercentage =
                            g.Sum(x => x.JobOrderQty) == 0
                                ? 0
                                : (g.Sum(x => x.JobOrderQty - x.JobBalQty) /
                                   g.Sum(x => x.JobOrderQty)) * 100
                    })
                    .OrderByDescending(x => x.PlannedQty)
                    .Take(10)
                    .ToList();

                // ==========================================================
                // RECENT JOB ORDERS
                // ==========================================================
                var recentJobs = await _unitOfWork.JobOrders.GetQueryable()
                    .Where(x => x.CreatedDate >= from && x.CreatedDate <= to)
                    .OrderByDescending(x => x.CreatedDate)
                    .Take(50)
                    .Select(x => new JobOrderVM
                    {
                        JobNo = x.JobNo,
                        JobOrderQty = x.JobOrderQty,
                        JobBalQty = x.JobBalQty,
                        JobTally = x.JobTally,

                        RefPoNo = x.MfgPo.PONo,
                        AssemblyItemCode = x.AssyItem.ItemCode,

                        CreatedDate = x.CreatedDate

                        // Add remaining VM properties here
                    })
                    .ToListAsync();

                // ==========================================================
                // RECENT SCNs
                // ==========================================================
                var recentScns = await _unitOfWork.ProductionSCNAssys.GetQueryable()
                    .Where(x => x.SCNDateNow >= from && x.SCNDateNow <= to)
                    .OrderByDescending(x => x.SCNDateNow)
                    .Take(5)
                    .Select(x => new ProductionSCNAssyVM
                    {
                        SCNNo = x.SCNNo,
                        SCNDateNow = x.SCNDateNow

                        // Add remaining VM properties here
                    })
                    .ToListAsync();

                // ==========================================================
                // DTO
                // ==========================================================
                return new JobCardDto
                {
                    TotalJobOrders = totalJobOrders,
                    CompletedJobOrders = completedJobOrders,
                    InProgressJobOrders = inProgressJobOrders,
                    PendingJobOrders = pendingJobOrders,


                    PlannedAssemblyQty = plannedAssemblyQty,
                    BalanceAssemblyQty = balanceAssemblyQty,
                    ProducedAssemblyQty = producedAssemblyQty,

                    TotalIssues = totalIssues,
                    CompletedIssues = completedIssues,
                    IssuedQty = issuedQty,

                    TotalReturns = totalReturns,
                    CompletedReturns = completedReturns,
                    ReturnedQty = returnedQty,

                    TotalSCNs = totalSCNs,
                    SCNQty = scnQty,

                    MaterialConsumption = materialConsumption,
                    ReturnPercentage = returnPercentage,
                    ProductionEfficiency = productionEfficiency,

                    JobOrderTrend = jobTrend,
                    IssueTrend = issueTrend,
                    ReturnTrend = returnTrend,
                    SCNTrend = scnTrend,

                    JobStatusSplit = statusSplit,

                    TopAssemblies = topAssemblies,

                    JobOrders = recentJobs,
                    RecentSCNs = recentScns
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    "GetJobCardAsync failed.");

                return new JobCardDto();
            }
        }




        // ═════════════════════════════════════════════════════════════════════════════
        //  ROUTE CARD TAB
        //  Reads from: _unitOfWork.RouteCards  (adjust to your actual repository)
        //  Assumes entity properties below — rename to match your actual entity.
        // ═════════════════════════════════════════════════════════════════════════════
        public async Task<RouteCardDto> GetRouteCardAsync(DashboardFilterDto filter)
        {
            try
            {
                var from = filter.FromDate.Date;
                var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // ── 1. Fetch route cards in range ─────────────────────────────────
                // TODO: replace _unitOfWork.RouteCards with your actual repository name.
                // Assumed entity columns:
                //   RouteCardNo, JobCardNo, ItemCode, ItemName,
                //   OperationName, WorkstationName, OperatorName,
                //   PlannedQty (int), CompletedQty (int),
                //   CreatedDate (DateTime), CompletedDate (DateTime?),
                //   Status (string: "Completed"/"In Process"/"Pending")

                var routeCards = await _unitOfWork.RouteCards.GetQueryable()
                    .Where(r => r.CreatedDate >= from && r.CreatedDate <= to)
                    .Select(r => new
                    {
                        r.RCNo,
                        r.ComponentItem.ItemCode,
                        r.ComponentItem.ItemName,
                        //r.OperationName,
                        //r.WorkstationName,
                        //r.OperatorName,
                        r.RcQty,
                        //r.CompletedQty,
                        r.CreatedDate,
                        //CompletedDate = (DateTime?)r.CompletedDate,
                        r.RcStatus,
                    })
                    .ToListAsync();

                // ── 2. KPI counts ─────────────────────────────────────────────────
                var total = routeCards.Select(r => r.RCNo).Distinct().Count();
                var completed = routeCards.Count(r => r.RcStatus == 2); // 0=Pending 1=InProgress 2=Completed 3=Cancelled
                var inProcess = routeCards.Count(r => r.RcStatus == 1);
                var pending = routeCards.Count(r => r.RcStatus == 0);
                var totalOps = routeCards.Count;

                //int avgPct = totalOps > 0
                //    ? (int)Math.Round(routeCards
                //        .Where(r => r.RcQty > 0)
                //        .Average(r => (double)r.CompletedQty / r.PlannedQty * 100))
                //    : 0;

                // ── 3. Time-series ────────────────────────────────────────────────
                var slots = BuildSlots(from, to, filter.GroupBy);

                string GetSlotKey(DateTime dt) => filter.GroupBy switch
                {
                    "Hour" => $"{from:yyyy-MM-dd} {dt.Hour:00}",
                    "Day" => dt.ToString("ddd"),
                    "Week" => $"W{GetWeekNumber(dt, from)}",
                    "Month" => dt.ToString("MMM"),
                    _ => dt.Date.ToString("yyyy-MM-dd")
                };

                var createdTrend = slots.Select(s => new ChartPointDto
                {
                    Label = s.Label,
                    Value = routeCards.Count(r => GetSlotKey(r.CreatedDate) == s.Key)
                }).ToList();

                //var completedTrend = slots.Select(s => new ChartPointDto
                //{
                //    Label = s.Label,
                //    Value = routeCards.Count(r =>
                //        r..HasValue && GetSlotKey(r.CompletedDate.Value) == s.Key)
                //}).ToList();

                // ── 4. Operation split ────────────────────────────────────────────
                //var operationSplit = routeCards
                //    .GroupBy(r => r.OperationName ?? "Unknown")
                //    .Select(g => new BusinessDataDto
                //    {
                //        Category = g.Key,
                //        Value = g.Count()
                //    })
                //    .Where(b => b.Value > 0)
                //    .OrderByDescending(b => b.Value)
                //    .Take(7)
                //    .ToList();

                // ── 5. Workstation performance ────────────────────────────────────
                //var workstationStats = routeCards
                //    .GroupBy(r => r.WorkstationName ?? "Unknown")
                //    .Select(g => new WorkstationStatDto
                //    {
                //        WorkstationName = g.Key,
                //        OperationType = g.FirstOrDefault()?.OperationName ?? "—",
                //        AssignedCount = g.Count(),
                //        CompletedCount = g.Count(r => r.Status == "Completed"),
                //        PendingCount = g.Count(r => r.Status != "Completed"),
                //    })
                //    .OrderByDescending(w => w.AssignedCount)
                //    .Take(8)
                //    .ToList();

                // ── 6. Table rows ─────────────────────────────────────────────────
                var rows = routeCards
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(50)
                    .Select(r => new RouteCardRowDto
                    {
                        RouteCardNo = r.RCNo,
                        //JobCardNo = r.JobCardNo,
                        ItemCode = r.ItemCode,
                        ItemName = r.ItemName,
                        //OperationName = r.OperationName,
                        //WorkstationName = r.WorkstationName,
                        //OperatorName = r.OperatorName,
                        //PlannedQty = r.RcQty,
                        //CompletedQty = r.CompletedQty,
                        //Status = r.Status,
                        //StatusClass = MapStatusClass(r.Status ?? ""),
                    })
                    .ToList();

                return new RouteCardDto
                {
                    TotalRouteCards = total,
                    Completed = completed,
                    InProcess = inProcess,
                    Pending = pending,
                    TotalOperations = totalOps,
                    //AvgCompletionPct = Math.Clamp(avgPct, 0, 100),
                    CreatedTrend = createdTrend,
                    //CompletedTrend = completedTrend,
                    //OperationSplit = operationSplit,
                    //WorkstationStats = workstationStats,
                    RouteCards = rows,
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "GetRouteCardAsync failed.");
                return new RouteCardDto
                {
                    CreatedTrend = new(),
                    CompletedTrend = new(),
                    OperationSplit = new(),
                    WorkstationStats = new(),
                    RouteCards = new(),
                };
            }
        }


        // ─────────────────────────────────────────────────────────────────────
        //  INVENTORY TAB
        // ─────────────────────────────────────────────────────────────────────
        public async Task<InventoryDto> GetInventoryAsync(DashboardFilterDto filter)
        {
            //var from = filter.FromDate.Date;
            //var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

            //// TODO: replace _db.InventoryItems and column names
            ////       (.ItemCode, .ItemName, .Category, .QtyIn, .QtyOut, .ClosingBalance, .Location, .CurrentStock)
            //var items = await _db.InventoryItems
            //    .Where(i => i.TransactionDate >= from && i.TransactionDate <= to)
            //    .Select(i => new InventoryItemDto
            //    {
            //        Code = i.ItemCode,
            //        Name = i.ItemName,
            //        Category = i.Category,
            //        In = i.QtyIn,
            //        Out = i.QtyOut,
            //        Balance = i.ClosingBalance,
            //        Location = i.Location,
            //    })
            //    .ToListAsync();

            //// Low-stock alerts: items whose current stock is below threshold
            //// TODO: replace _db.Items and threshold column
            //var lowStock = await _db.Items
            //    .Where(i => i.CurrentStock <= i.ReorderLevel)
            //    .OrderBy(i => i.CurrentStock)
            //    .Take(10)
            //    .Select(i => new StockAlertDto
            //    {
            //        Name = i.ItemName,
            //        Qty = i.CurrentStock,
            //    })
            //    .ToListAsync();

            //// Chart: stock per category
            //var levelChart = await _db.Items
            //    .GroupBy(i => i.Category)
            //    .Select(g => new ChartPointDto
            //    {
            //        Label = g.Key,
            //        Value = g.Sum(i => i.CurrentStock),
            //    })
            //    .ToListAsync();

            //return new InventoryDto
            //{
            //    LevelChart = levelChart,
            //    LowStock = lowStock,
            //    Items = items,
            //};

            return new InventoryDto();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  FINANCE TAB
        // ─────────────────────────────────────────────────────────────────────
        public async Task<FinanceDto> GetFinanceAsync(DashboardFilterDto filter)
        {
            //var from = filter.FromDate.Date;
            //var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

            //// TODO: replace _db.Receipts, _db.Payments, _db.Expenses
            ////       and column names (.ReceiptDate, .Amount, .PaymentDate, .ExpenseDate, .Category)
            //var totalIncome = await _db.Receipts.Where(r => r.ReceiptDate >= from && r.ReceiptDate <= to).SumAsync(r => r.Amount);
            //var totalExpense = await _db.Payments.Where(p => p.PaymentDate >= from && p.PaymentDate <= to).SumAsync(p => p.Amount);
            //var arPending = await _db.Receipts.Where(r => !r.IsSettled).SumAsync(r => r.Amount);
            //var apPending = await _db.Payments.Where(p => !p.IsPaid).SumAsync(p => p.Amount);

            //// Monthly cashflow
            //var cashflowIn = await _db.Receipts
            //    .Where(r => r.ReceiptDate >= from && r.ReceiptDate <= to)
            //    .GroupBy(r => r.ReceiptDate.Month)
            //    .Select(g => new ChartPointDto
            //    {
            //        Label = new DateTime(DateTime.Today.Year, g.Key, 1).ToString("MMM"),
            //        Value = g.Sum(r => r.Amount) / 100000m, // convert to Lakhs
            //    })
            //    .OrderBy(p => p.Label)
            //    .ToListAsync();

            //var cashflowOut = await _db.Payments
            //    .Where(p => p.PaymentDate >= from && p.PaymentDate <= to)
            //    .GroupBy(p => p.PaymentDate.Month)
            //    .Select(g => new ChartPointDto
            //    {
            //        Label = new DateTime(DateTime.Today.Year, g.Key, 1).ToString("MMM"),
            //        Value = g.Sum(p => p.Amount) / 100000m,
            //    })
            //    .OrderBy(p => p.Label)
            //    .ToListAsync();

            //// Expense breakdown by category
            //var expenseBreak = await _db.Expenses
            //    .Where(e => e.ExpenseDate >= from && e.ExpenseDate <= to)
            //    .GroupBy(e => e.Category)
            //    .Select(g => new BusinessDataDto
            //    {
            //        Category = g.Key,
            //        Value = g.Sum(e => e.Amount),
            //    })
            //    .ToListAsync();

            //return new FinanceDto
            //{
            //    NetProfit = FormatCurrency(totalIncome - totalExpense),
            //    TotalExpense = FormatCurrency(totalExpense),
            //    ArPending = FormatCurrency(arPending),
            //    ApPending = FormatCurrency(apPending),
            //    CashflowIn = cashflowIn,
            //    CashflowOut = cashflowOut,
            //    ExpenseBreak = expenseBreak,
            //};
            return new FinanceDto();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  HR TAB
        // ─────────────────────────────────────────────────────────────────────
        public async Task<HrDto> GetHrAsync(DashboardFilterDto filter)
        {
            //var from = filter.FromDate.Date;
            //var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

            //// TODO: replace _db.Employees, _db.Attendance and column names
            //var deptSplit = await _db.Employees
            //    .Where(e => e.IsActive)
            //    .GroupBy(e => e.Department)
            //    .Select(g => new BusinessDataDto
            //    {
            //        Category = g.Key,
            //        Value = g.Count(),
            //    })
            //    .ToListAsync();

            //// Attendance % per day this week
            //var attendanceWeek = await _db.Attendance
            //    .Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to)
            //    .GroupBy(a => a.AttendanceDate)
            //    .Select(g => new ChartPointDto
            //    {
            //        Label = g.Key.ToString("ddd"),
            //        Value = (decimal)g.Count(a => a.IsPresent) / g.Count() * 100,
            //    })
            //    .OrderBy(p => p.Label)
            //    .ToListAsync();

            //var employees = await _db.Employees
            //    .Where(e => e.IsActive)
            //    .OrderBy(e => e.Name)
            //    .Select(e => new EmployeeDto
            //    {
            //        Name = e.Name,
            //        Dept = e.Department,
            //        Role = e.Designation,
            //        Status = e.EmploymentStatus,
            //        StatusClass = MapStatusClass(e.EmploymentStatus),
            //        Attendance = e.AttendancePercent,
            //    })
            //    .ToListAsync();

            //return new HrDto
            //{
            //    DeptSplit = deptSplit,
            //    AttendanceWeek = attendanceWeek,
            //    Employees = employees,
            //};
            return new HrDto();
        }



        // ─────────────────────────────────────────────────────────────────────
        //  HELPERS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a decimal to a compact Indian-currency string.
        /// e.g. 3,44,43,029 → "₹3.44Cr"  |  84,000 → "₹84K"
        /// </summary>
        private static string FormatCurrency(decimal value)
        {
            return value switch
            {
                >= 10_000_000 => $"₹{value / 10_000_000m:0.##}Cr",
                >= 100_000 => $"₹{value / 100_000m:0.##}L",
                >= 1_000 => $"₹{value / 1_000m:0.##}K",
                _ => $"₹{value:0.##}",
            };
        }

        /// <summary>
        /// Maps status strings to CSS class names used in the Razor UI.
        /// Extend the switch as needed to cover your status values.
        /// </summary>
        private static string MapStatusClass(string status) => status?.ToLower() switch
        {
            "completed" or "done" or "shipped" or "active" => "done",
            "in progress" or "processing" or "confirmed"
                         or "pending" or "on leave" => "pending",
            "on hold" or "failed" or "cancelled" => "failed",
            _ => "pending",
        };

        private static List<(string Key, string Label)> BuildSlots(
            DateTime from, DateTime to, string groupBy)
        {
            var slots = new List<(string Key, string Label)>();

            switch (groupBy)
            {
                case "Hour":
                    // 24 hours of the selected day (or starting day for multi-day)
                    for (int h = 0; h < 24; h++)
                    {
                        var dt = from.AddHours(h);
                        var key = $"{from:yyyy-MM-dd} {h:00}";
                        var lbl = dt.ToString("hh tt", CultureInfo.InvariantCulture); // 12 AM … 11 PM
                        slots.Add((key, lbl));
                    }
                    break;

                case "Day":
                    // One slot per distinct day-of-week in the range (Mon, Tue, …)
                    var daysSeen = new HashSet<string>();
                    for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
                    {
                        var key = d.ToString("ddd");
                        if (daysSeen.Add(key))
                            slots.Add((key, key)); // Label = "Mon", "Tue" …
                    }
                    break;

                case "Week":
                    // Week 1 = days 0-6 from 'from', Week 2 = days 7-13, …
                    var weekStart = from.Date;
                    int wNum = 1;
                    while (weekStart <= to.Date)
                    {
                        slots.Add(($"W{wNum}", $"Week {wNum}"));
                        weekStart = weekStart.AddDays(7);
                        wNum++;
                    }
                    break;

                case "Month":
                    // One slot per calendar month in the range
                    var month = new DateTime(from.Year, from.Month, 1);
                    while (month <= to)
                    {
                        var key = month.ToString("MMM");
                        slots.Add((key, key)); // Label = "Jan", "Feb" …
                        month = month.AddMonths(1);
                    }
                    break;

                default:
                    // Daily fallback — "01 May", "02 May" …
                    for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
                        slots.Add((d.ToString("yyyy-MM-dd"), d.ToString("dd MMM")));
                    break;
            }

            return slots;
        }

        /// <summary>
        /// Returns the 1-based week number of <paramref name="dt"/> relative to
        /// the filter start date (week 1 = days 0-6, week 2 = days 7-13, …).
        /// </summary>
        private static int GetWeekNumber(DateTime dt, DateTime from)
            => (int)Math.Floor((dt.Date - from.Date).TotalDays / 7) + 1;




        public async Task<AnalyticsDto> GetAnalyticsAsync(DashboardFilterDto filter)
        {
            try
            {
                var from = filter.FromDate.Date;
                var to = filter.ToDate.Date.AddDays(1).AddTicks(-1);

                // ── Fetch all needed data sequentially ────────────────────────────
                var mfgPos = await _unitOfWork.MfgPos.GetQueryable()
                    .Where(p => !p.MfgORLab && p.PODateNow >= from && p.PODateNow <= to)
                    .ToListAsync();

                var mfgInvs = await _unitOfWork.MfgInvs.GetQueryable()
                    .Where(i => i.InvDateNow >= from && i.InvDateNow <= to)
                    .ToListAsync();

                var labPos = await _unitOfWork.MfgPos.GetQueryable()
                    .Where(p => p.MfgORLab && p.PODateNow >= from && p.PODateNow <= to)
                    .ToListAsync();

                var labInvs = await _unitOfWork.LabInvs.GetQueryable()
                    .Where(i => i.LabInvDateNow >= from && i.LabInvDateNow <= to)
                    .ToListAsync();

                var purPos = await _unitOfWork.PurchPos.GetQueryable()
                    .Where(p => !p.PurchORSubCon && p.PODateNow >= from && p.PODateNow <= to)
                    .ToListAsync();

                var purInvs = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .Where(i => i.InvDateNow >= from && i.InvDateNow <= to)
                    .ToListAsync();

                var subPos = await _unitOfWork.PurchPos.GetQueryable()
                    .Where(p => p.PurchORSubCon && p.PODateNow >= from && p.PODateNow <= to)
                    .ToListAsync();

                var subInvs = await _unitOfWork.SubConInvs.GetQueryable()
                    .Where(i => i.InvDateNow >= from && i.InvDateNow <= to)
                    .ToListAsync();

                var expInvs = await _unitOfWork.ExpInvs.GetQueryable()
                    .Where(i => i.ExpInvDateNow >= from && i.ExpInvDateNow <= to)
                    .ToListAsync();

                var payments = await _unitOfWork.Payments.GetQueryable()
                    .Where(p => p.CreatedDate.HasValue && p.CreatedDate.Value >= from && p.CreatedDate.Value <= to)
                    .ToListAsync();

                var receipts = await _unitOfWork.Receiptss.GetQueryable()
                    .Where(r => r.CreatedDate >= from && r.CreatedDate <= to)
                    .ToListAsync();

                var enquiries = await _unitOfWork.EnquirySaless.GetQueryable()
                    .Where(e => e.EnquiryDate >= from && e.EnquiryDate <= to)
                    .ToListAsync();

                var quotes = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Where(q => q.QuoteDate >= from && q.QuoteDate <= to)
                    .ToListAsync();

                var jobCards = await _unitOfWork.JobOrders.GetQueryable()
                    .Where(j => j.CreatedDate >= from && j.CreatedDate <= to)
                    .ToListAsync();

                var routeCards = await _unitOfWork.RouteCards.GetQueryable()
                    .Where(r => r.CreatedDate >= from && r.CreatedDate <= to)
                    .ToListAsync();


                // ══════════════════════════════════════════════════════════════════
                //  1. RADIAL GAUGES  — % achieved vs period target
                // ══════════════════════════════════════════════════════════════════

                // Sales: invoiced value vs PO value
                var salesPoTotal = mfgPos.Sum(p => p.GrandTotal);
                var salesInvTotal = mfgInvs.Sum(i => i.GrandTotal);
                var salesGaugePct = salesPoTotal > 0
                    ? (int)Math.Min(Math.Round(salesInvTotal / salesPoTotal * 100), 100)
                    : 0;

                // Collection: receipts received vs total invoiced (sales + labour)
                var totalReceivable = mfgInvs.Sum(i => i.GrandTotal) + labInvs.Sum(i => i.GrandTotal);
                var totalReceived = receipts.Sum(r => r.Amount);
                var collectionPct = totalReceivable > 0
                    ? (int)Math.Min(Math.Round(totalReceived / totalReceivable * 100), 100)
                    : 0;

                // PO Fulfilment: completed POs vs total POs (sales + purchase)
                var totalPos = mfgPos.Count + purPos.Count;
                var completedPos = mfgPos.Count(p => p.PoTally) + purPos.Count(p => p.PoTally);
                var fulfilmentPct = totalPos > 0
                    ? (int)Math.Round((double)completedPos / totalPos * 100)
                    : 0;

                // Job completion: tallied job cards vs total
                var totalJobs = jobCards.Count;
                var completedJobs = jobCards.Count(j => j.JobTally);
                var jobCompPct = totalJobs > 0
                    ? (int)Math.Round((double)completedJobs / totalJobs * 100)
                    : 0;

                var gauges = new List<GaugeKpiDto>
        {
            new() { Label = "Sales Target",    Value = salesGaugePct,  Color = "#3b82f6" },
            new() { Label = "Collection",      Value = collectionPct,  Color = "#10b981" },
            new() { Label = "PO Fulfilment",   Value = fulfilmentPct,  Color = "#f59e0b" },
            new() { Label = "Job Completion",  Value = jobCompPct,     Color = "#7c3aed" },
        };

                // ══════════════════════════════════════════════════════════════════
                //  2. SPARKLINES  — last N slots per module
                // ══════════════════════════════════════════════════════════════════

                var slots = BuildSlots(from, to, filter.GroupBy);

                string GetSlotKey(DateTime dt) => filter.GroupBy switch
                {
                    "Hour" => $"{from:yyyy-MM-dd} {dt.Hour:00}",
                    "Day" => dt.ToString("ddd"),
                    "Week" => $"W{GetWeekNumber(dt, from)}",
                    "Month" => dt.ToString("MMM"),
                    _ => dt.Date.ToString("yyyy-MM-dd")
                };

                List<decimal> SlotValues<T>(
                    IEnumerable<T> source,
                    Func<T, DateTime> dateSelector,
                    Func<T, decimal> valueSelector)
                {
                    var grouped = source
                        .GroupBy(x => GetSlotKey(dateSelector(x)))
                        .ToDictionary(g => g.Key, g => g.Sum(valueSelector));
                    return slots.Select(s => grouped.TryGetValue(s.Key, out var v) ? v : 0m).ToList();
                }

                List<decimal> SlotCounts<T>(IEnumerable<T> source, Func<T, DateTime> dateSelector)
                {
                    var grouped = source
                        .GroupBy(x => GetSlotKey(dateSelector(x)))
                        .ToDictionary(g => g.Key, g => (decimal)g.Count());
                    return slots.Select(s => grouped.TryGetValue(s.Key, out var v) ? v : 0m).ToList();
                }

                decimal CalcChange(List<decimal> vals)
                {
                    if (vals.Count < 2) return 0m;
                    var prev = vals[^2];
                    var curr = vals[^1];
                    return prev == 0 ? 0m : Math.Round((curr - prev) / prev * 100, 1);
                }

                var salesVals = SlotValues(mfgInvs, i => i.InvDateNow, i => i.GrandTotal / 100_000m);
                var purVals = SlotValues(purInvs, i => i.InvDateNow, i => i.GrandTotal / 100_000m);
                var labVals = SlotValues(labInvs, i => i.LabInvDateNow, i => i.GrandTotal / 100_000m);
                var subVals = SlotValues(subInvs, i => i.InvDateNow, i => i.GrandTotal / 100_000m);
                var jcVals = SlotCounts(jobCards, j => j.CreatedDate);
                var rcVals = SlotCounts(routeCards, r => r.CreatedDate);

                var sparklines = new List<SparklineSeriesDto>
        {
            new() { Name = "Sales",       Values = salesVals, ChangePercent = CalcChange(salesVals), DisplayValue = FormatCurrency(mfgInvs.Sum(i => i.GrandTotal)),  Color = "#3b82f6" },
            new() { Name = "Purchase",    Values = purVals,   ChangePercent = CalcChange(purVals),   DisplayValue = FormatCurrency(purInvs.Sum(i => i.GrandTotal)),   Color = "#10b981" },
            new() { Name = "Labour",      Values = labVals,   ChangePercent = CalcChange(labVals),   DisplayValue = FormatCurrency(labInvs.Sum(i => i.GrandTotal)),   Color = "#f59e0b" },
            new() { Name = "Sub-Con",     Values = subVals,   ChangePercent = CalcChange(subVals),   DisplayValue = FormatCurrency(subInvs.Sum(i => i.GrandTotal)),   Color = "#7c3aed" },
            new() { Name = "Job Cards",   Values = jcVals,    ChangePercent = CalcChange(jcVals),    DisplayValue = jobCards.Count.ToString(),                        Color = "#0891b2" },
            new() { Name = "Route Cards", Values = rcVals,    ChangePercent = CalcChange(rcVals),    DisplayValue = routeCards.Select(r => r.RCNo).Distinct().Count().ToString(), Color = "#22c55e" },
        };

                // ══════════════════════════════════════════════════════════════════
                //  3. SALES FUNNEL  — enquiry → quote → PO → invoice → collected
                // ══════════════════════════════════════════════════════════════════

                var funnel = new List<FunnelStageDto>
        {
            new() { Label = "Enquiries",    Count = enquiries.Count,                                                   Color = "#3b82f6" },
            new() { Label = "Quotations",   Count = quotes.Count,                                                      Color = "#7c3aed" },
            new() { Label = "POs Received", Count = mfgPos.Count,                                                     Color = "#0891b2" },
            new() { Label = "Invoiced",     Count = mfgInvs.Count,                                                    Color = "#10b981" },
            new() { Label = "Collected",    Count = mfgInvs.Count(i => i.Balance == 0 && !i.IsCancel),                Color = "#16a34a" },
        };

                // ══════════════════════════════════════════════════════════════════
                //  4. CUSTOMER BUBBLE CHART  — orders vs revenue vs avg order value
                // ══════════════════════════════════════════════════════════════════

                var customerBubbles = mfgInvs
                    .GroupBy(i => i.Customer?.CustName ?? "Unknown")
                    .Select(g =>
                    {
                        var orders = g.Count();
                        var revenue = g.Sum(x => x.GrandTotal) / 100_000m;  // in Lakhs
                        var avgOv = orders > 0 ? revenue / orders : 0m;

                        // Bubble radius: 3–20, scaled by avg order value
                        var maxAvg = 10m; // cap at ₹10L for radius scaling
                        var radius = (int)Math.Clamp(Math.Round(avgOv / maxAvg * 17) + 3, 3, 20);

                        var segment = revenue > 200 ? "Top"
                                     : revenue > 80 ? "Key"
                                     : revenue > 20 ? "Mid"
                                     : "SME";

                        return new CustomerBubbleDto
                        {
                            Name = g.Key,
                            Orders = orders,
                            Revenue = Math.Round(revenue, 1),
                            AvgOrderValue = Math.Round(avgOv, 1),
                            BubbleRadius = radius,
                            Segment = segment,
                        };
                    })
                    .OrderByDescending(c => c.Revenue)
                    .Take(20)
                    .ToList();

                // ══════════════════════════════════════════════════════════════════
                //  5. WATERFALL  — monthly cash inflow / outflow / net
                // ══════════════════════════════════════════════════════════════════

                // Build month slots spanning the filter range
                var monthSlots = new List<DateTime>();
                var mSlot = new DateTime(from.Year, from.Month, 1);
                while (mSlot <= to)
                {
                    monthSlots.Add(mSlot);
                    mSlot = mSlot.AddMonths(1);
                }

                // Inflow = sales invoices + labour invoices (what we should receive)
                // Outflow = purchase invoices + subcon invoices (what we owe)
                var waterfall = monthSlots.Select(ms =>
                {
                    var mEnd = ms.AddMonths(1).AddTicks(-1);
                    var inflow = mfgInvs.Where(i => i.InvDateNow >= ms && i.InvDateNow <= mEnd).Sum(i => i.GrandTotal / 100_000m)
                               + labInvs.Where(i => i.LabInvDateNow >= ms && i.LabInvDateNow <= mEnd).Sum(i => i.GrandTotal / 100_000m);
                    var outflow = purInvs.Where(i => i.InvDateNow >= ms && i.InvDateNow <= mEnd).Sum(i => i.GrandTotal / 100_000m)
                                + subInvs.Where(i => i.InvDateNow >= ms && i.InvDateNow <= mEnd).Sum(i => i.GrandTotal / 100_000m);
                    return new WaterfallPointDto
                    {
                        Label = ms.ToString("MMM"),
                        Inflow = Math.Round(inflow, 1),
                        Outflow = Math.Round(outflow, 1),
                    };
                }).ToList();

                // ══════════════════════════════════════════════════════════════════
                //  6. ACTIVITY HEATMAP  — transaction count by hour × day-of-week
                // ══════════════════════════════════════════════════════════════════

                // Combine all transaction timestamps into one list
                var allTimestamps = new List<DateTime>();
                allTimestamps.AddRange(mfgInvs.Select(i => i.InvDateNow));
                allTimestamps.AddRange(purInvs.Select(i => i.InvDateNow));
                allTimestamps.AddRange(labInvs.Select(i => i.LabInvDateNow));
                allTimestamps.AddRange(subInvs.Select(i => i.InvDateNow));
                allTimestamps.AddRange(expInvs.Select(i => i.ExpInvDateNow));
                allTimestamps.AddRange(payments.Where(p => p.CreatedDate.HasValue).Select(p => p.CreatedDate!.Value));
                allTimestamps.AddRange(receipts.Select(r => r.CreatedDate.Value));

                // Map to working-hour buckets (8 AM – 6 PM = indices 0-5 matching hourLabels)
                var hourBuckets = new[] { 8, 10, 12, 14, 16, 18 };

                var heatmapCells = allTimestamps
                    .Select(dt => new
                    {
                        HourBucket = Array.FindLastIndex(hourBuckets, h => dt.Hour >= h),
                        DayOfWeek = ((int)dt.DayOfWeek + 6) % 7
                    })
                    .Where(x => x.HourBucket >= 0)
                    .GroupBy(x => (x.HourBucket, x.DayOfWeek))
                    .Select(g => new HeatmapCellDto
                    {
                        Hour = g.Key.HourBucket,
                        DayOfWeek = g.Key.DayOfWeek,
                        Count = g.Count(),
                    })
                    .ToList();

                // ══════════════════════════════════════════════════════════════════
                //  7. RADAR  — normalised dept performance scores
                // ══════════════════════════════════════════════════════════════════

                // Helper: normalise a raw value to 0-100 given a target ceiling
                static int Normalise(decimal value, decimal ceiling)
                    => ceiling <= 0 ? 0 : (int)Math.Min(Math.Round(value / ceiling * 100), 100);

                var salesRevenue = mfgInvs.Sum(i => i.GrandTotal);
                var purchaseSpend = purInvs.Sum(i => i.GrandTotal);
                var labourRevenue = labInvs.Sum(i => i.GrandTotal);

                // Production dept KPIs
                var prodEfficiency = Normalise(completedJobs, Math.Max(totalJobs, 1));
                var prodQuality = Normalise(                     // jobs tallied without overdue
                    jobCards.Count(j => j.JobTally), Math.Max(totalJobs, 1));
                var prodDelivery = Normalise(
                    routeCards.Count(r => r.RcStatus == 2),
                    Math.Max(routeCards.Count, 1));
                var prodCost = Normalise(salesRevenue, Math.Max(salesRevenue + purchaseSpend, 1));
                var prodUtilisation = Normalise(
                    routeCards.Select(r => r.RCNo).Distinct().Count(),
                    Math.Max(routeCards.Count, 1));
                var prodCompliance = Normalise(mfgPos.Count(p => p.PoTally), Math.Max(mfgPos.Count, 1));

                // Procurement dept KPIs
                var procEfficiency = Normalise(purPos.Count(p => p.PoTally), Math.Max(purPos.Count, 1));
                var procQuality = Normalise(purInvs.Count(i => !i.InvCancel), Math.Max(purInvs.Count, 1));
                var procDelivery = Normalise(subPos.Count(p => p.PoTally), Math.Max(subPos.Count + 1, 1));
                var procCost = Normalise(purchaseSpend > 0 ? salesRevenue - purchaseSpend : 0, Math.Max(salesRevenue, 1));
                var procUtilisation = Normalise(purInvs.Count, Math.Max(purPos.Count, 1));
                var procCompliance = Normalise(subInvs.Count(i => !i.InvCancel), Math.Max(subInvs.Count, 1));

                // Sales dept KPIs
                var salesEfficiency = Normalise(mfgInvs.Count, Math.Max(mfgPos.Count, 1));
                var salesQuality = Normalise(mfgInvs.Count(i => !i.IsCancel), Math.Max(mfgInvs.Count, 1));
                var salesDelivery = Normalise(mfgPos.Count(p => p.PoTally), Math.Max(mfgPos.Count, 1));
                var salesCost = Normalise(salesRevenue, Math.Max(salesRevenue + labourRevenue, 1));
                var salesUtilisation = Normalise(enquiries.Count(e => !e.Cancel), Math.Max(enquiries.Count, 1));
                var salesCompliance = Normalise(quotes.Count, Math.Max(enquiries.Count, 1));

                var radarAxes = new List<string>
            { "Efficiency", "Quality", "Delivery", "Cost Control", "Utilisation", "Compliance" };

                var radarDatasets = new List<RadarDatasetDto>
        {
            new() { Label = "Production",  Values = new() { prodEfficiency,  prodQuality,  prodDelivery,  prodCost,  prodUtilisation,  prodCompliance  }, Color = "#3b82f6" },
            new() { Label = "Procurement", Values = new() { procEfficiency,  procQuality,  procDelivery,  procCost,  procUtilisation,  procCompliance  }, Color = "#10b981" },
            new() { Label = "Sales",       Values = new() { salesEfficiency, salesQuality, salesDelivery, salesCost, salesUtilisation, salesCompliance }, Color = "#f59e0b" },
        };

                // ══════════════════════════════════════════════════════════════════
                //  8. BUDGET UTILISATION  — spent vs total invoiced per module
                // ══════════════════════════════════════════════════════════════════

                // "Allocated" = PO value (what was committed); "Spent" = invoice value (what was billed)
                // Both converted to Lakhs for display
                var budgetModules = new List<BudgetModuleDto>
        {
            new() { Name = "Sales",    Spent = Math.Round(mfgInvs.Sum(i => i.GrandTotal)  / 100_000m, 1), Allocated = Math.Round(mfgPos.Sum(p => p.GrandTotal)  / 100_000m, 1), Color = "#3b82f6" },
            new() { Name = "Purchase", Spent = Math.Round(purInvs.Sum(i => i.GrandTotal)  / 100_000m, 1), Allocated = Math.Round(purPos.Sum(p => p.GrandTotal)  / 100_000m, 1), Color = "#10b981" },
            new() { Name = "Labour",   Spent = Math.Round(labInvs.Sum(i => i.GrandTotal)  / 100_000m, 1), Allocated = Math.Round(labPos.Sum(p => p.GrandTotal)  / 100_000m, 1), Color = "#f59e0b" },
            new() { Name = "Sub-Con",  Spent = Math.Round(subInvs.Sum(i => i.GrandTotal)  / 100_000m, 1), Allocated = Math.Round(subPos.Sum(p => p.GrandTotal)  / 100_000m, 1), Color = "#7c3aed" },
            new() { Name = "Export",   Spent = Math.Round(expInvs.Sum(i => i.GrandTotal)  / 100_000m, 1), Allocated = Math.Round(expInvs.Sum(i => i.GrandTotal) / 100_000m, 1), Color = "#0891b2" },
        }
                // Only include modules that have some activity
                .Where(m => m.Allocated > 0 || m.Spent > 0)
                .ToList();

                // ── Assemble and return ───────────────────────────────────────────
                return new AnalyticsDto
                {
                    Gauges = gauges,
                    Sparklines = sparklines,
                    Funnel = funnel,
                    CustomerBubbles = customerBubbles,
                    Waterfall = waterfall,
                    HeatmapCells = heatmapCells,
                    RadarAxes = radarAxes,
                    RadarDatasets = radarDatasets,
                    BudgetModules = budgetModules,
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "GetAnalyticsAsync failed.");
                return new AnalyticsDto
                {
                    Gauges = new(),
                    Sparklines = new(),
                    Funnel = new(),
                    CustomerBubbles = new(),
                    Waterfall = new(),
                    HeatmapCells = new(),
                    RadarAxes = new(),
                    RadarDatasets = new(),
                    BudgetModules = new(),
                };
            }
        }


    }


}