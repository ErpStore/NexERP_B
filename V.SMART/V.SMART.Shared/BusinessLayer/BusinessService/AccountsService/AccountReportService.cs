using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAccountReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IReportRepository.ITrackReportRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AccountReportService
{
    public class AccountReportService : IAccountReportService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;
        private readonly IMapper _mapper;

        public AccountReportService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IReportExecutor report,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _report = report;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
          => await _commonService.SearchCustomersAsync(searchText);

        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
         => await _commonService.SearchVendorsAsync(searchText);

        public async Task<List<ConfirmationOfAccountsVM>> GetAccountsAsync(string? Type, string? ExpenseType, DateTime? fromDate, DateTime? toDate, int? custId)
        {
            try
            {
                var parameters = new[]
                {

                    new SqlParameter("@Type", string.IsNullOrWhiteSpace(Type) ? DBNull.Value : Type),
                    new SqlParameter("@ExpenseType", string.IsNullOrWhiteSpace(ExpenseType) ? DBNull.Value : ExpenseType),
                    new SqlParameter("@fromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@toDate", toDate ?? (object)DBNull.Value),
                    new SqlParameter("@custId", custId.HasValue ? custId.Value : DBNull.Value),
                };

                var result = await _report.ExecuteAsync<ConfirmationOfAccountsVM>("SP_ConfirmationAccounts", parameters);
                return result ?? new List<ConfirmationOfAccountsVM>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing stored procedure GetAccountsAsync");
                return new List<ConfirmationOfAccountsVM>();
            }
        }

        public async Task<List<KeyValuePair<int, string>>> GetAllIncomeList()
        {
            try
            {
                return await _unitOfWork.Incomes
                    .GetQueryable()
                    .Select(i => new KeyValuePair<int, string>(
                        i.IncomeCode,
                        i.IncomeName))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing stored procedure GetAllIncomeList");
                return new List<KeyValuePair<int, string>>();
            }
        }

        public async Task<List<KeyValuePair<int, string>>> GetAllExpenseList()
        {
            try
            {
                return await _unitOfWork.Expenses
                    .GetQueryable()
                    .Select(i => new KeyValuePair<int, string>(
                        i.ExpenseCode,
                        i.ExpenseName))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetAllExpenseList");

                return new List<KeyValuePair<int, string>>();
            }
        }

        //Labour Opening Balance = Total Labour Invoices - Total Labour Credit Notes - Total Labour Payments
        public async Task<decimal> GetCustOpeningBalance(int custId, DateTime fromDate)
        {
            try
            {
                var totalInv = await _unitOfWork.LabInvs.GetQueryable()
                                .Where(i =>
                                    i.CustId == custId &&
                                    i.LabInvDate < fromDate)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalCredits = await _unitOfWork.CreditNotes.GetQueryable()
                                .Where(i =>
                                    i.CustId == custId && i.Labour == true &&
                                    i.CreditDate < fromDate)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalPayments = await _unitOfWork.Receiptss.GetQueryable()
                                .Where(i =>
                                    i.PayFromRefCode == custId && i.IncomeCode==2 &&
                                    i.ReceiptDate < fromDate)
                                .SumAsync(i => (decimal?)i.Amount) ?? 0;

                decimal openingBalance = totalInv - totalCredits - totalPayments;

                return openingBalance;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetOpeningBalance");

                return 0;
            }
        }

        //Sales Opening Balance = Total Sales Invoices - Total Sales Credit Notes - Total Sales Payments
        public async Task<decimal> GetCustSalesOpeningBalance(int custId, DateTime fromDate)
        {
            try
            {
                var totalInv = await _unitOfWork.MfgInvs.GetQueryable()
                                .Where(i =>
                                    i.CustId == custId &&
                                    i.InvDate < fromDate)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalCredits = await _unitOfWork.CreditNotes.GetQueryable()
                                .Where(i =>
                                    i.CustId == custId && i.Sales == true &&
                                    i.CreditDate < fromDate)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalPayments = await _unitOfWork.Receiptss.GetQueryable()
                                .Where(i =>
                                    i.PayFromRefCode == custId && i.IncomeCode== 1 &&
                                    i.ReceiptDate < fromDate)
                                .SumAsync(i => (decimal?)i.Amount) ?? 0;

                decimal openingBalance = totalInv - totalCredits - totalPayments;

                return openingBalance;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetOpeningBalance");

                return 0;
            }
        }


        //Purchase Opening Balance = Total Purchase Invoices - Total Debit Notes - Total Payments
        public async Task<decimal> GetVendorOpeningBalance(int vendorId, DateTime fromDate)
        {
            try
            {
                var totalInv = await _unitOfWork.PurchaseInvoices.GetQueryable()
                                .Where(i =>
                                    i.VendorCode == vendorId &&
                                    i.InvDate < fromDate)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalCredits = await _unitOfWork.DebitNotes.GetQueryable()
                                .Where(i =>
                                    i.VendorCode == vendorId &&
                                    i.DebitDate < fromDate && i.Purchase == true)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalPayments = await _unitOfWork.Payments.GetQueryable()
                                .Where(i =>
                                    i.PayToRefCode == vendorId &&
                                    i.PaymentDate < fromDate && i.ExpenseCode == 1)
                                .SumAsync(i => (decimal?)i.Amount) ?? 0;

                decimal openingBalance = totalInv - totalCredits - totalPayments;

                return openingBalance;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetOpeningBalance");

                return 0;
            }
        }


        //Subcontract Opening Balance = Total Purchase Invoices - Total Debit Notes - Total Payments
        public async Task<decimal> GetSubcontractOpeningBalance(int subcontractId, DateTime fromDate)
        {
            try
            {
                var totalInv = await _unitOfWork.SubConInvs.GetQueryable()
                                .Where(i =>
                                    i.VendorCode == subcontractId &&
                                    i.InvDate < fromDate)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalCredits = await _unitOfWork.DebitNotes.GetQueryable()
                                .Where(i =>
                                    i.VendorCode == subcontractId &&
                                    i.DebitDate < fromDate && i.SubContract == true)
                                .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

                var totalPayments = await _unitOfWork.Payments.GetQueryable()
                                .Where(i =>
                                    i.PayToRefCode == subcontractId &&
                                    i.PaymentDate < fromDate && i.ExpenseCode == 3)
                                .SumAsync(i => (decimal?)i.Amount) ?? 0;

                decimal openingBalance = totalInv - totalCredits - totalPayments;

                return openingBalance;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetOpeningBalance");

                return 0;
            }
        }

        public async Task<List<CustomerVM>> GetAllCustomersAsync(string type, DateTime fromdate, DateTime todate)
        {
            try
            {
                IQueryable<Customer> query = null;

                if (type == "SALES")
                {
                    query = _unitOfWork.MfgInvs.GetQueryable()
                            .Where(x => x.InvDate.Date >= fromdate.Date && x.InvDate.Date <= todate.Date)
                            .Select(x => x.Customer);
                }
                else if (type == "LABOUR WORK")
                {
                    query = _unitOfWork.LabInvs.GetQueryable()
                            .Where(x => x.LabInvDate.Date >= fromdate.Date && x.LabInvDate.Date <= todate.Date)
                            .Select(x => x.Customer);
                }

                if (query == null)
                    return new List<CustomerVM>();

                var customers = await query
                    .Distinct()
                    .ToListAsync();

                return _mapper.Map<List<CustomerVM>>(customers);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetAllCustomersAsync");
                return new List<CustomerVM>();
            }
        }

        public async Task<List<VendorVM>> GetAllVendorsAsync(string type, DateTime fromdate, DateTime todate)
        {
            try
            {
                IQueryable<Vendor> query = null;

                if (type == "PURCHASE")
                {
                    query = _unitOfWork.PurchaseInvoices.GetQueryable()
                            .Where(x => x.InvDate.Date >= fromdate.Date && x.InvDate.Date <= todate.Date)
                            .Select(x => x.Vendor);
                }
                else if (type == "SUBCONTRACT")
                {
                    query = _unitOfWork.SubConInvs.GetQueryable()
                            .Where(x => x.InvDate.Date >= fromdate.Date && x.InvDate.Date <= todate.Date)
                            .Select(x => x.Vendor);
                }

                if (query == null)
                    return new List<VendorVM>();

                var vendors = await query
                    .Distinct()
                    .ToListAsync();

                return _mapper.Map<List<VendorVM>>(vendors);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetAllVendorsAsync");
                return new List<VendorVM>();
            }
        }
    }
}
