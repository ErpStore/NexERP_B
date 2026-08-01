using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Diagrams;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using static MudBlazor.Icons;

namespace V.SMART.Shared.BusinessLayer.BusinessService.AccountsService
{
    public class ReceiptsService : IReceiptsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public ReceiptsService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            ICommonService commonService,
            IMapper mapper, IHttpClientFactory httpFactory)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;

        }

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
            => await _commonService.SearchCustomersAsync(searchText);
        public async Task<CustomerVM?> GetCustomerByIdAsync(int Custid)
           => await _commonService.GetCustomerByIdAsync(Custid);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        #region payment module

        public async Task<string> GenerateReceiptsNumberAsync()
        {
            try
            {

                var today = DateTime.Today;
                int fyStartYear = (today.Month >= 4) ? today.Year : today.Year - 1;
                var fyStart = new DateTime(fyStartYear, 4, 1);
                var fyEnd = new DateTime(fyStartYear + 1, 3, 31);

                // Get highest from Payments table
                var highest1 = await _unitOfWork.Receiptss
                                .GetQueryable()
                                .Where(e => e.ReceiptDate >= fyStart && e.ReceiptDate <= fyEnd)
                                .OrderByDescending(e => e.ReceiptNo)
                                .Select(e => e.ReceiptNo)
                                .FirstOrDefaultAsync();

                // Get highest from AdvanceAdjustment table
                var highest2 = await _unitOfWork.Advaceadjustments
                                .GetQueryable()
                                .Where(e => e.AdjumentDate >= fyStart && e.AdjumentDate <= fyEnd)
                                .OrderByDescending(e => e.AdvaceadjustmentNo)
                                .Select(e => e.AdvaceadjustmentNo)
                                .FirstOrDefaultAsync();

                // Convert to numbers safely
                int num1 = int.TryParse(highest1, out var n1) ? n1 : 0;
                int num2 = int.TryParse(highest2, out var n2) ? n2 : 0;

                // Pick highest from both
                int highestNumeric = Math.Max(num1, num2);

                // Increment and return 4 digit format
                return (highestNumeric + 1).ToString("D4");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GenerateEnquiryNumberAsync()");
                return string.Empty;
            }
        }
        #endregion

        public async Task<List<PartyVM>> SearchPartysAsync(string searchText)
        {
            var grouped = await _commonService.SearchCustomersAsync(searchText);

            var partyList = grouped.Select(g => new PartyVM
            {
                PartyCode = g.CustId,
                PartyName = g.CustName,
                OpeningBalance = g.OpeningBalance??0m,
                OpeningBalancePending = g.OpeningBalancePending??0m,
                AdvancePaid =
                    _unitOfWork.fundTranss.GetQueryable()
                        .Where(f => f.LedgerId == g.CustId && f.TransType == "Pay Advance")
                        .Sum(f => (decimal?)f.AdvanceBalance) ?? 0
            });

            return partyList.ToList();
        }
        public async Task<decimal> GetAdvanceAmountPaid(string type, string paymentType, int PartyCode)
        {
            try
            {
                    //decimal result = await _unitOfWork.fundTranss.GetQueryable()
                    //                .Where(f => f.LedgerId == PartyCode)
                    //                .OrderByDescending(f => f.FundTransId)
                    //                .Select(f => f.AdvanceBalance)
                    //                .FirstOrDefaultAsync();

                    decimal result = await _unitOfWork.fundTranss.GetQueryable()
                               .Where(f => f.LedgerId == PartyCode && f.IncomeCode > 0)
                               .OrderByDescending(f => f.FundTransId)
                               .Select(f => f.AdvanceBalance)
                               .FirstOrDefaultAsync();

                    return result > 0 ? result : 0m;
               

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed loading vendor list: {type}");
                throw;
            }
        }

        public async Task<List<PartyVM>> GetVendorsByPurchaseTypeAdvacnceAsync(string type, string paymentType)
        {
            try
            {
                paymentType = paymentType.ToUpper();

                IQueryable<PartyVM> query;

                switch (type.ToUpper())
                {
                    case "PURCHASE":


                        var vendors = await _unitOfWork.Vendors.GetQueryable()
                            .Where(v => v.Inactive == false)
                            .ToListAsync();

                        var list = new List<PartyVM>();

                        foreach (var v in vendors)
                        {
                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                .Where(f => f.LedgerId == v.VendorCode)
                                .OrderByDescending(f => f.FundTransId)
                                .Select(f => f.AdvanceBalance)
                                .FirstOrDefaultAsync();

                            decimal pending = await _unitOfWork.PurchaseInvoices.GetQueryable()
                                .Where(pi => pi.VendorCode == v.VendorCode && pi.Balance > 0)
                                .SumAsync(pi => (decimal?)pi.Balance) ?? 0m;

                            if (lastAdvance > 0 && pending > 0)
                            {
                                list.Add(new PartyVM
                                {
                                    PartyCode = v.VendorCode,
                                    PartyName = v.VendorName,
                                    OpeningBalance = v.OpenBal ?? 0,
                                    OpeningBalancePending = v.OpenBalPndg ?? 0,
                                    AdvancePaid = lastAdvance,
                                    PartyPending = pending
                                });
                            }
                        }

                        return list.OrderBy(x => x.PartyName).ToList();

                    case "PURCHASE ORDER ADVANCE":

                        var poList = await _unitOfWork.PurchPos.GetQueryable()
                            .Include(e => e.Vendor)
                            .ToListAsync();

                        var listPO = new List<PartyVM>();

                        var groups = poList
                            .Where(e => e.AdvanceAmountBal > 0)
                            .GroupBy(e => new
                            {
                                e.Vendor.VendorCode,
                                e.Vendor.VendorName,
                                e.Vendor.OpenBal,
                                e.Vendor.OpenBalPndg
                            });

                        foreach (var g in groups)
                        {
                            //decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                            //    .Where(f => f.LedgerId == g.Key.VendorCode)
                            //    .OrderByDescending(f => f.FundTransId)
                            //    .Select(f => f.AdvanceBalance)
                            //    .FirstOrDefaultAsync();

                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                            .Where(f => f.LedgerId == g.Key.VendorCode && f.LedgerType == "VENDOR")
                            .OrderByDescending(f => f.FundTransId)
                            .Select(f => (decimal?)f.AdvanceBalance)
                            .FirstOrDefaultAsync() ?? 0;

                            listPO.Add(new PartyVM
                            {
                                PartyCode = g.Key.VendorCode,
                                PartyName = g.Key.VendorName,
                                OpeningBalance = g.Key.OpenBal ?? 0,
                                OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
                                AdvancePaid = lastAdvance
                            });
                        }

                        return listPO.OrderBy(x => x.PartyName).ToList();

                    default:
                        return new List<PartyVM>();
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed loading vendor list: {type}");
                throw;
            }
        }

        public async Task<List<PartyVM>> GetCustomerByIncomeTypeAsync(string type, string IncomeType)
        {
            try
            {

                IncomeType = IncomeType?.ToUpper() ?? "";

                IQueryable<PartyVM> query;

                switch (type?.ToUpper() ?? "")
                {
                    case "SALES":

                        if (IncomeType == "PAY ADVANCE" || IncomeType == "OPENING BALANCE")
                        {
                            // Only opening balance vendors, advance = last pending
                            var vendors = await _unitOfWork.Customers.GetQueryable()
                                .Where(x => x.Inactive == false && x.Inactive == false)
                                .ToListAsync();

                            var result = new List<PartyVM>();

                            foreach (var v in vendors)
                            {
                                decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                    .Where(f => f.LedgerId == v.CustId)
                                    .OrderByDescending(f => f.FundTransId)
                                    .Select(f => f.AdvanceBalance)
                                    .FirstOrDefaultAsync();

                                result.Add(new PartyVM
                                {
                                    PartyCode = v.CustId,
                                    PartyName = v.CustName,
                                    OpeningBalance = v.OpenBal ?? 0,
                                    OpeningBalancePending = v.OpenBalPndg ?? 0,
                                    AdvancePaid = lastAdvance
                                });
                            }

                            return result.OrderBy(x => x.PartyName).ToList();
                        }
                        else
                        {
                           
                            var invoices = await _unitOfWork.MfgInvs.GetQueryable()
                                .Include(e => e.Customer)
                                .Where(e => e.Balance > 0)
                                .ToListAsync();

                            var result = new List<PartyVM>();

                            var groups = invoices.GroupBy(e => new
                            {
                                e.Customer.CustId,
                                e.Customer.CustName,
                                e.Customer.OpenBal,
                                e.Customer.OpenBalPndg
                            });

                            foreach (var g in groups)
                            {
                                decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                    .Where(f => f.LedgerId == g.Key.CustId)
                                    .OrderByDescending(f => f.FundTransId)
                                    .Select(f => f.AdvanceBalance)
                                    .FirstOrDefaultAsync();

                                result.Add(new PartyVM
                                {
                                    PartyCode = g.Key.CustId,
                                    PartyName = g.Key.CustName,
                                    OpeningBalance = g.Key.OpenBal ?? 0,
                                    OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
                                    AdvancePaid = lastAdvance
                                });
                            }

                            return result.OrderBy(x => x.PartyName).ToList();
                        }

                    case "LABOUR WORK":

                        if (IncomeType == "PAY ADVANCE" || IncomeType == "OPENING BALANCE")
                        {
                            // Only opening balance vendors, advance = last pending
                            var vendors = await _unitOfWork.Customers.GetQueryable()
                                .Where(x => x.Inactive == false && x.Inactive == false)
                                .ToListAsync();

                            var result = new List<PartyVM>();

                            foreach (var v in vendors)
                            {
                                decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                    .Where(f => f.LedgerId == v.CustId)
                                    .OrderByDescending(f => f.FundTransId)
                                    .Select(f => f.AdvanceBalance)
                                    .FirstOrDefaultAsync();

                                result.Add(new PartyVM
                                {
                                    PartyCode = v.CustId,
                                    PartyName = v.CustName,
                                    OpeningBalance = v.OpenBal ?? 0,
                                    OpeningBalancePending = v.OpenBalPndg ?? 0,
                                    AdvancePaid = lastAdvance
                                });
                            }

                            return result.OrderBy(x => x.PartyName).ToList();
                        }
                        else
                        {

                            var invoices = await _unitOfWork.LabInvs.GetQueryable()
                                .Include(e => e.Customer)
                                .Where(e => e.Balance > 0)
                                .ToListAsync();

                            var result = new List<PartyVM>();

                            var groups = invoices.GroupBy(e => new
                            {
                                e.Customer?.CustId,
                                e.Customer?.CustName,
                                e.Customer?.OpenBal,
                                e.Customer?.OpenBalPndg
                            });

                            foreach (var g in groups)
                            {
                                decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                    .Where(f => f.LedgerId == g.Key.CustId)
                                    .OrderByDescending(f => f.FundTransId)
                                    .Select(f => f.AdvanceBalance)
                                    .FirstOrDefaultAsync();

                                result.Add(new PartyVM
                                {
                                    PartyCode = g.Key.CustId??0,
                                    PartyName = g.Key.CustName??"",
                                    OpeningBalance = g.Key.OpenBal ?? 0,
                                    OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
                                    AdvancePaid = lastAdvance
                                });
                            }

                            return result.OrderBy(x => x.PartyName).ToList();
                        }



                    case "PURCHASE ORDER ADVANCE":

                        var poList = await _unitOfWork.PurchPos.GetQueryable()
                            .Include(e => e.Vendor)
                            .Where(e => e.AdvanceAmountBal > 0)
                            .ToListAsync();

                        var resultPO = new List<PartyVM>();

                        var groupsPO = poList.GroupBy(e => new
                        {
                            e.Vendor.VendorCode,
                            e.Vendor.VendorName,
                            e.Vendor.OpenBal,
                            e.Vendor.OpenBalPndg
                        });

                        foreach (var g in groupsPO)
                        {
                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                .Where(f => f.LedgerId == g.Key.VendorCode)
                                .OrderByDescending(f => f.FundTransId)
                                .Select(f => f.AdvanceBalance)
                                .FirstOrDefaultAsync();

                            resultPO.Add(new PartyVM
                            {
                                PartyCode = g.Key.VendorCode,
                                PartyName = g.Key.VendorName,
                                OpeningBalance = g.Key.OpenBal ?? 0,
                                OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
                                AdvancePaid = lastAdvance
                            });
                        }

                        return resultPO.OrderBy(x => x.PartyName).ToList();

                    default:
                        return new List<PartyVM>();
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed loading vendor list: {type}");
                throw;
            }
        }



        //public async Task<List<PartyVM>> GetVendorsByPurchaseTypeAsync(string type,string paymentType)
        //{
        //    try
        //    {
        //        IQueryable<PartyVM> query;
        //        paymentType.ToUpper();

        //        switch (type.ToUpper())
        //        {
        //            case "PURCHASE" :

        //                // Exclude advance & opening balance payment types
        //                if (paymentType.ToUpper() == "PAY ADVANCE" || paymentType.ToUpper() == "OPENING BALANCE")
        //                {
        //                    query = _unitOfWork.Vendors.GetQueryable()
        //                            .Where(x => x.Inactive == false)
        //                            .GroupBy(e => new
        //                            {
        //                                e.VendorCode,
        //                                e.VendorName,
        //                                e.OpenBal,
        //                                e.OpenBalPndg
        //                            })
        //                            .Select(g => new PartyVM
        //                            {
        //                                PartyCode = g.Key.VendorCode,
        //                                PartyName = g.Key.VendorName,
        //                                OpeningBalance = g.Key.OpenBal ?? 0,
        //                                OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
        //                                AdvancePaid =
        //                                    _unitOfWork.fundTranss.GetQueryable()
        //                                        .Where(f => f.LedgerId == g.Key.VendorCode && f.TransType == "Pay Advance")
        //                                        .Sum(f => (decimal?)f.BillAmount) ?? 0
        //                            });

        //                }
        //                else
        //                {
        //                    query = _unitOfWork.PurchaseInvoices.GetQueryable()
        //                      .Include(e => e.Vendor)
        //                      .Where(e => e.Balance > 0)
        //                      .GroupBy(e => new
        //                      {
        //                          e.Vendor.VendorCode,
        //                          e.Vendor.VendorName,
        //                          e.Vendor.OpenBal,
        //                          e.Vendor.OpenBalPndg
        //                      })
        //                      .Select(g => new PartyVM
        //                      {
        //                          PartyCode = g.Key.VendorCode,
        //                          PartyName = g.Key.VendorName,
        //                          OpeningBalance = g.Key.OpenBal ?? 0,
        //                          OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
        //                          AdvancePaid =
        //                              _unitOfWork.fundTranss.GetQueryable()
        //                                  .Where(f => f.LedgerId == g.Key.VendorCode &&
        //                                              f.TransType == "Pay Advance")
        //                                  .Sum(f => (decimal?)f.BillAmount) ?? 0
        //                      });

        //                }
        //                break;

        //            case "PURCHASE ORDER ADVANCE":
        //                query = _unitOfWork.PurchPos.GetQueryable()
        //                    .Include(e => e.Vendor)
        //                    .Where(e => e.AdvanceAmountBal > 0)
        //                    .GroupBy(e => new
        //                    {
        //                        e.Vendor.VendorCode,
        //                        e.Vendor.VendorName,
        //                        e.Vendor.OpenBal,
        //                        e.Vendor.OpenBalPndg
        //                    })
        //                    .Select(g => new PartyVM
        //                    {
        //                        PartyCode = g.Key.VendorCode,
        //                        PartyName = g.Key.VendorName,
        //                        OpeningBalance = g.Key.OpenBal ?? 0,
        //                        OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
        //                        AdvancePaid =
        //                            _unitOfWork.fundTranss.GetQueryable()
        //                                .Where(f => f.LedgerId == g.Key.VendorCode &&
        //                                            f.TransType == "Pay Advance")
        //                                .Sum(f => (decimal?)f.BillAmount) ?? 0
        //                    });
        //                break;

        //            default:
        //                return new List<PartyVM>();
        //        }

        //        return await query
        //            .OrderBy(v => v.PartyName)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, $"Failed loading vendor list: {type}");
        //        throw;
        //    }
        //}

        public async Task<List<BillsVM>> GetBillsByCustomerAsync(int CustId, string billType)
        {
            try
            {
                IQueryable<BillsVM> query;

                switch (billType.ToUpper())
                {
                    case "SALES":
                        query = _unitOfWork.MfgInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId)
                            .OrderByDescending(e => e.InvId)
                            .Select(e => new BillsVM
                            {
                                Id = e.InvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.InvNo,
                                BillDate = e.InvDate,
                                Balance = e.Balance
                            });
                        break;

                    case "LABOUR WORK":
                        query = _unitOfWork.LabInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId)
                            .OrderByDescending(e => e.LabInvId)
                            .Select(e => new BillsVM
                            {
                                Id = e.LabInvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.LabInvNo,
                                BillDate = e.LabInvDate,
                                Balance = e.Balance
                            });
                        break;

                    case "SALES + LABOUR":

                        var salesQuery = _unitOfWork.MfgInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId)
                            .Select(e => new BillsVM
                            {
                                Id = e.InvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.InvNo,
                                BillDate = e.InvDate,
                                Balance = e.Balance,
                                BillType = "SALES"
                            });

                        var labourQuery = _unitOfWork.LabInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId)
                            .Select(e => new BillsVM
                            {
                                Id = e.LabInvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.LabInvNo,
                                BillDate = e.LabInvDate,
                                Balance = e.Balance,
                                BillType = "LABOUR"
                            });

                        query = salesQuery
                            .Concat(labourQuery)
                            .OrderByDescending(e => e.BillDate);

                        break;




                    case "PURCHASE ORDER ADVANCE":
                        query = _unitOfWork.PurchPos.GetQueryable()
                            .Include(e => e.Vendor)
                            .Where(e => e.AdvanceAmountBal > 0)
                            .OrderByDescending(e => e.PoId)
                            .Select(e => new BillsVM
                            {
                                Id = e.PoId,
                                PartyCode = e.Vendor.VendorCode,
                                BillNo = e.PONo,
                                BillDate = e.PODate,
                                Balance = e.AdvanceAmountBal
                            });
                        break;

                    default:
                        return new List<BillsVM>();
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to load pending bills: {billType}");
                throw;
            }
        }
        public async Task<ReceiptsVM> UpsertReceiptsAsync(ReceiptsVM vm, bool BillAdjust)
        {
            using var trx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _currentUserService.GetUsernameAsync();
                var now = DateTime.Now;
                int? oldBankId = null;
                int? oldFtId = null;
                decimal OldAmount = 0;
                var oldSubs = new List<ReceiptsSub>();

                if (vm.ReceiptId > 0)
                {
                    var oldFt = await _unitOfWork.fundTranss
                                .GetQueryable()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.ReceiptRefId == vm.ReceiptId);


                    oldSubs = await _unitOfWork.ReceiptsSubs
                            .GetQueryable()
                            .AsNoTracking()
                            .Where(x => x.ReceiptId == vm.ReceiptId)
                            .ToListAsync();

                    oldBankId = oldFt?.BankId;
                    oldFtId = oldFt?.FundTransId;
                    OldAmount = oldFt.BillAmount;
                }
                var entity = await SaveHeaderAsync(vm, now, user);

                var ft = await SaveFundTransAsync(entity, vm);

                //if(vm.ExpenseName != "Opening Balance")
                //{
                if (oldBankId.HasValue && oldBankId != entity.BankId && oldFtId.HasValue)
                {
                    await RecalculateBankLedgerAsync(oldBankId, oldFtId.Value, entity.PayFromRefCode??0);
                    await UpdateBankBalanceAsync(oldBankId.Value);
                }
                if (entity.BankId.HasValue)
                {
                    await RecalculateBankLedgerAsync(entity.BankId, ft.FundTransId,entity.PayFromRefCode??0);
                    await UpdateBankBalanceAsync(entity.BankId.Value);
                }

                //}

                if (BillAdjust && vm.ReceiptId > 0)
                {

                    var addedItems = vm.ReceiptsSubVMs
                        .Where(x => oldSubs.Any(o => o.RefId == x.RefId))
                        .ToList();

                    var removedItems = oldSubs
                        .Where(x => vm.ReceiptsSubVMs.Any(n => n.RefId == x.RefId && x.AdjustAmount != n.AdjustAmount))
                        .ToList();
                    if (oldSubs.Count >= 1)
                    {
                        await UpdatePendingBalanceAsync(vm.InComeName, addedItems, removedItems);

                    }
                    else
                    {
                        await UpdatePendingBalanceAsync(vm.InComeName, vm.ReceiptsSubVMs, new List<ReceiptsSub>());
                    }


                }
                else if (BillAdjust)
                {

                    await UpdatePendingBalanceAsync(vm.InComeName, vm.ReceiptsSubVMs, new List<ReceiptsSub>());
                }
                else
                {
                    await UpdateOpeningBalanceAsync(entity, vm.ReceiptId, OldAmount);
                }

                await trx.CommitAsync();

                return _mapper.Map<ReceiptsVM>(entity);
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Error in UpsertReceiptsAsync");
                throw;
            }
        }

        public async Task UpdateOpeningBalanceAsync(Receipts Vm, int ReceiptId, decimal OldAmount)
        {
            try
            {
                var expenseType = Vm.Income?.IncomeName?.ToUpperInvariant().Trim() ?? "";

                if (Vm.PayFromName == null || Vm.PayFromRefCode == 0)
                    return;

                decimal newAmount = Vm.Amount;

                switch (expenseType)
                {
                    case "PURCHASE":
                        {
                            if (Vm.PaymentTypeName.ToUpper() == "OPENING BALANCE")
                            {
                                var vendor = await _unitOfWork.Customers
                                    .GetQueryable()
                                    .FirstOrDefaultAsync(x => x.CustId == Vm.PayFromRefCode);

                                if (vendor == null) return;

                                decimal pending = vendor.OpenBalPndg ?? 0m;

                                if (ReceiptId > 0) // EDIT MODE
                                {
                                    // Apply same logic style as PO
                                    pending = pending + OldAmount - newAmount;
                                }
                                else // NEW PAYMENT MODE
                                {
                                    pending = pending - newAmount;
                                }

                                vendor.OpenBalPndg = Math.Max(0m, pending);

                                await _unitOfWork.Customers.UpdateAsync(vendor);
                            }

                            break;
                        }

                    default:
                        return;
                }

                await _unitOfWork.SaveAsync();

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in UpdateOpeningBalanceAsync");
                throw;
            }


        }
        public async Task<List<PartyVM>> GetCustomerByReceiptTypeAdvacnceAsync(string type, string paymentType)
        {
            try
            {
                paymentType = paymentType.ToUpper();

                IQueryable<PartyVM> query;

                switch (type.ToUpper())
                {
                    case "SALES":


                        var vendors = await _unitOfWork.Customers.GetQueryable()
                            .Where(v => v.Inactive == false)
                            .ToListAsync();

                        var list = new List<PartyVM>();

                        foreach (var v in vendors)
                        {
                            //decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                            //    .Where(f => f.LedgerId == v.CustId && f.IncomeCode > 0 && f.TransType == "Pay Advance" && f.LedgerType == "CUSTOMER" && f.AdvanceBalance > 0)
                            //    .OrderByDescending(f => f.FundTransId)
                            //    .Select(f => f.AdvanceBalance)
                            //    .FirstOrDefaultAsync();

                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                .Where(f => f.LedgerId == v.CustId && f.LedgerType == "CUSTOMER")
                                .OrderByDescending(f => f.FundTransId)
                                .Select(f => (decimal?)f.AdvanceBalance)
                                .FirstOrDefaultAsync() ?? 0;

                            decimal pending = await _unitOfWork.MfgInvs.GetQueryable()
                                .Where(pi => pi.CustId == v.CustId && pi.Balance > 0)
                                .SumAsync(pi => (decimal?)pi.Balance) ?? 0m;

                            if (lastAdvance > 0 && pending > 0)
                            {
                                list.Add(new PartyVM
                                {
                                    PartyCode = v.CustId,
                                    PartyName = v.CustName,
                                    OpeningBalance = v.OpenBal ?? 0,
                                    OpeningBalancePending = v.OpenBalPndg ?? 0,
                                    AdvancePaid = lastAdvance,
                                    PartyPending = pending
                                });
                            }
                        }

                        return list.OrderBy(x => x.PartyName).ToList();

                    case "LABOUR WORK":

                        var Cust = await _unitOfWork.Customers.GetQueryable()
                            .Where(v => v.Inactive == false)
                            .ToListAsync();

                        var Custlist = new List<PartyVM>();

                        foreach (var v in Cust)
                        {
                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                                .Where(f => f.LedgerId == v.CustId && f.LedgerType == "CUSTOMER")
                                .OrderByDescending(f => f.FundTransId)
                                .Select(f => (decimal?)f.AdvanceBalance)
                                .FirstOrDefaultAsync() ?? 0;

                            decimal pending = await _unitOfWork.LabInvs.GetQueryable()
                                .Where(pi => pi.CustId == v.CustId && pi.Balance > 0)
                                .SumAsync(pi => (decimal?)pi.Balance) ?? 0m;

                            if (lastAdvance > 0 && pending > 0)
                            {
                                Custlist.Add(new PartyVM
                                {
                                    PartyCode = v.CustId,
                                    PartyName = v.CustName,
                                    OpeningBalance = v.OpenBal ?? 0,
                                    OpeningBalancePending = v.OpenBalPndg ?? 0,
                                    AdvancePaid = lastAdvance,
                                    PartyPending = pending
                                });
                            }
                        }

                        return Custlist.OrderBy(x => x.PartyName).ToList();

                    default:
                        return new List<PartyVM>();
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed loading vendor list: {type}");
                throw;
            }
        }

        public async Task UpdatePendingBalanceAsync(string ReceiptsType, List<ReceiptsSubVM> newItems, List<ReceiptsSub> oldItems)
        {
            try
            {
                ReceiptsType = ReceiptsType?.ToUpper()?.Trim() ?? "";

                var allRefIds = newItems
                    .Select(x => x.RefId)
                    .Concat(oldItems.Select(x => x.RefId))
                    .Distinct()
                    .ToList();

                if (!allRefIds.Any())
                    return;

                // Map old adjustments by RefId
                var oldMap = oldItems
                    .GroupBy(x => x.RefId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.AdjustAmount));


                var newMap = newItems
                    .GroupBy(x => x.RefId)
                    .ToDictionary(g => g.Key, g => g.Sum(x => x.AdjustAmount));

                switch (ReceiptsType)
                {

                    // -------------------------------
                    case "PURCHASE ORDER ADVANCE":
                        {
                            var pos = await _unitOfWork.PurchPos
                                .GetQueryable()
                                .Where(x => allRefIds.Contains(x.PoId))
                                .ToListAsync();

                            foreach (var po in pos)
                            {
                                oldMap.TryGetValue(po.PoId, out var oldAdj);
                                newMap.TryGetValue(po.PoId, out var newAdj);

                                decimal delta = newAdj - oldAdj; // 🔥 KEY LINE

                                po.AdvanceAmountBal -= delta;

                                if (po.AdvanceAmountBal < 0)

                                    po.AdvanceAmountBal = 0;

                                await _unitOfWork.PurchPos.UpdateAsync(po);
                            }
                            break;
                        }

                    // -------------------------------
                    // PURCHASE INVOICE
                    // -------------------------------
                    case "SALES":
                        {

                            var invoices = await _unitOfWork.MfgInvs
                                .GetQueryable()
                                .Where(x => allRefIds.Contains(x.InvId))
                                .ToListAsync();

                            foreach (var inv in invoices)
                            {
                                oldMap.TryGetValue(inv.InvId, out var oldAdj);
                                newMap.TryGetValue(inv.InvId, out var newAdj);

                                decimal delta = newAdj - oldAdj;

                                inv.Balance -= delta;

                                if (inv.Balance < 0)
                                    inv.Balance = 0;
                                inv.InvTally = inv.Balance == 0;
                                await _unitOfWork.MfgInvs.UpdateAsync(inv);
                            }
                            break;
                        }
                    case "LABOUR WORK":
                        {

                            var invoices = await _unitOfWork.LabInvs
                                .GetQueryable()
                                .Where(x => allRefIds.Contains(x.LabInvId))
                                .ToListAsync();

                            foreach (var inv in invoices)
                            {
                                oldMap.TryGetValue(inv.LabInvId, out var oldAdj);
                                newMap.TryGetValue(inv.LabInvId, out var newAdj);

                                decimal delta = newAdj - oldAdj;

                                inv.Balance -= delta;

                                if (inv.Balance < 0)
                                    inv.Balance = 0;

                                inv.LabInvTally = inv.Balance == 0;
                                await _unitOfWork.LabInvs.UpdateAsync(inv);
                            }
                            break;
                        }



                    default:
                        return; // unknown expense type → do nothing safely
                }
                await _unitOfWork.SaveAsync();

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in UpdatePendingBalanceAsync");
                throw;
            }


        }


        private async Task<Receipts> SaveHeaderAsync(ReceiptsVM vm, DateTime now, string user)
        {
            try
            {
                Receipts entity;

                if (vm.ReceiptId == 0)
                {
                    entity = _mapper.Map<Receipts>(vm);

                    entity.CreatedBy = user;
                    entity.CreatedDate = now;

                    entity.ReceiptsSubs = vm.ReceiptsSubVMs
                        .Select(sub => _mapper.Map<ReceiptsSub>(sub))
                        .ToList();

                    await _unitOfWork.Receiptss.CreateAsync(entity);
                }
                else
                {
                    entity = await _unitOfWork.Receiptss
                         .GetQueryable()
                         .Include(s => s.ReceiptsSubs)
                         .FirstOrDefaultAsync(x => x.ReceiptId == vm.ReceiptId)
                         ?? throw new Exception("Payment not found");

                    entity.ModifiedBy = user;
                    entity.ModifiedDate = now;

                    _mapper.Map(vm, entity);
                    await _unitOfWork.Receiptss.UpdateAsync(entity);
                }

                await _unitOfWork.SaveAsync();
                return entity;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SaveHeaderAsync");
                throw;

            }


        }
        private async Task<FundTrans> SaveFundTransAsync(Receipts entity, ReceiptsVM vm)
        {
            try
            {
                // 🔹 Find existing fund transaction
                var ft = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.ReceiptRefId == entity.ReceiptId && x.IncomeCode == entity.IncomeCode);

                bool isNew = false;

                if (ft == null)
                {
                    ft = new FundTrans
                    {
                        ReceiptRefId = entity.ReceiptId
                    };
                    isNew = true;
                }
                // 🔹 Amount sign
                decimal paidOrReceived =
                    vm.PaymentTypeName == "Opening Balance"
                        ? -vm.Amount
                        : vm.Amount;
                if (vm.InComeName.ToUpper() == "PURCHASE ORDER ADVANCE")
                {
                    vm.PaymentTypeName = "Pay Advance";

                }
                decimal advance =
                      (vm.PaymentTypeName?.Equals("Pay Advance", StringComparison.OrdinalIgnoreCase) == true ||
                       vm.PaymentTypeName?.Equals("Purchase Order Advance", StringComparison.OrdinalIgnoreCase) == true)
                          ? vm.Amount
                          : 0;

                // 🔹 Update fields
                ft.TransNo = entity.ReceiptNo;
                ft.TransDate = entity.ReceiptDate;
                ft.TransType = vm.PaymentTypeName;
                ft.TrasactionMode = entity.PaymentMode;
                ft.LedgerId = entity.PayFromRefCode;
                ft.LedgerName = entity.PayFromName;
                ft.IncomeCode = entity.IncomeCode;
                ft.LedgerType = "CUSTOMER";
                ft.BankId = entity.BankId;
                ft.ChequeNo = entity.ChequeNo;
                ft.ChequeDate = entity.ChequeDate;
                ft.BillAmount = entity.Amount;
                ft.PaidOrReceived = paidOrReceived;
                ft.AdvanceBalance = advance;
                ft.TransactionType = TransTypes.Receipt;
                ft.CreatedBy = entity.CreatedBy;
                ft.ModifiedBy = entity.ModifiedBy;
                ft.ModifiedDate = entity.ModifiedDate;

                if (isNew)
                    await _unitOfWork.fundTranss.CreateAsync(ft);
                else
                    await _unitOfWork.fundTranss.UpdateAsync(ft);

                await _unitOfWork.SaveAsync();

                //  REBUILD RUNNING BALANCE
                await RecalculateBankLedgerAsync(entity.BankId, ft.FundTransId,ft.LedgerId??0);

                return ft;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SaveFundTransAsync");
                throw;
            }

        }
        private async Task RecalculateBankLedgerAsync(int? bankId, int startFtId, int ledgerId)
        {
            try
            {
                // ================================
                // 1️⃣ BANK RUNNING RECALCULATION
                // ================================
                if (bankId != null)
                {
                    var bankTrans = await _unitOfWork.fundTranss
                        .GetQueryable()
                        .Where(x => x.BankId == bankId && x.FundTransId >= startFtId)
                        .OrderBy(x => x.FundTransId)
                        .ToListAsync();

                    decimal running = await _unitOfWork.fundTranss
                        .GetQueryable()
                        .Where(x => x.BankId == bankId && x.FundTransId < startFtId)
                        .OrderByDescending(x => x.FundTransId)
                        .Select(x => x.RunningBalance)
                        .FirstOrDefaultAsync();

                    foreach (var ft in bankTrans)
                    {
                        if (!string.Equals(ft.TransType, "PERFORMA ADVANCE", StringComparison.OrdinalIgnoreCase))
                        {
                            running += ft.PaidOrReceived;
                        }

                        ft.RunningBalance = running;
                        ft.BalanceAfter = running;
                    }
                }

                // ================================
                // 2️⃣ LEDGER ADVANCE RECALCULATION
                // ================================

                var ledgerTrans = await _unitOfWork.fundTranss
                                        .GetQueryable()
                                        .Where(x => x.LedgerId == ledgerId
                                                 && x.LedgerType == "CUSTOMER"
                                                 && x.FundTransId >= startFtId)
                                        .OrderBy(x => x.FundTransId)
                                        .ToListAsync();

                decimal runningAdvance = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .Where(x => x.LedgerId == ledgerId
                             && x.LedgerType == "CUSTOMER"
                             && x.FundTransId < startFtId)
                    .OrderByDescending(x => x.FundTransId)
                    .Select(x => x.AdvanceBalance)
                    .FirstOrDefaultAsync();

                foreach (var ft in ledgerTrans)
                {
                    if (!string.IsNullOrEmpty(ft.TransType))
                    {
                        if (ft.TransType.Equals("PAY ADVANCE", StringComparison.OrdinalIgnoreCase))
                        {
                            runningAdvance += Math.Abs(ft.BillAmount);
                        }
                        else if (ft.TransType.ToUpper().Contains("SALES INVOICE ADJUST"))
                        {
                            runningAdvance -= Math.Abs(ft.BillAmount);
                        }
                    }

                    if (runningAdvance < 0)
                        runningAdvance = 0;

                    ft.AdvanceBalance = runningAdvance;
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RecalculateBankLedgerAsync");
                throw;
            }
        }

        //private async Task RecalculateBankLedgerAsync(int? bankId, int startFtId,int ledgerId)
        //{
        //    try
        //    {
        //        if (bankId == null)
        //            return;

        //        var trans = await _unitOfWork.fundTranss
        //            .GetQueryable()
        //            .Where(x => x.BankId == bankId && x.FundTransId >= startFtId)
        //            .OrderBy(x => x.FundTransId)
        //            .ToListAsync();

        //        decimal running = await _unitOfWork.fundTranss
        //            .GetQueryable()
        //            .Where(x => x.BankId == bankId && x.FundTransId < startFtId)
        //            .OrderByDescending(x => x.FundTransId)
        //            .Select(x => x.RunningBalance)
        //            .FirstOrDefaultAsync();

        //        decimal runningAdvance = await _unitOfWork.fundTranss
        //            .GetQueryable()
        //            .Where(x => x.BankId == bankId && x.FundTransId < startFtId)
        //            .OrderByDescending(x => x.FundTransId)
        //            .Select(x => x.AdvanceBalance)
        //            .FirstOrDefaultAsync();


        //        foreach (var ft in trans)
        //        {
        //            // 1️⃣ Update bank balance only if not invoice adjust
        //            if (ft.TransType.ToUpper() != "PERFORMA ADVANCE")
        //            {
        //                running += ft.PaidOrReceived;
        //                if (running < 0)
        //                    running = 0;
        //            }

        //            // 2️⃣ Advance handling using transaction delta (BillAmount)
        //            if (ft.TransType.ToUpper() == "PAY ADVANCE")
        //            {
        //                runningAdvance += Math.Abs(ft.BillAmount);
        //                ft.AdvanceBalance = runningAdvance;
        //            }
        //            else if (ft.TransType.ToUpper() == "PURCHASE INVOICE ADJUST")
        //            {
        //                decimal used = Math.Min(runningAdvance, Math.Abs(ft.BillAmount));
        //                runningAdvance -= used;
        //                ft.AdvanceBalance = runningAdvance;
        //            }

        //            // 3️⃣ Assign recalculated values
        //            ft.RunningBalance = running;
        //            ft.BalanceAfter = running;
        //            await _unitOfWork.fundTranss.UpdateAsync(ft);
        //        }
        //        if (ledgerId != null)
        //        {
        //            var ledgerTrans = await _unitOfWork.fundTranss
        //                .GetQueryable()
        //                .Where(x => x.LedgerId == ledgerId
        //                         && x.LedgerType == "CUSTOMER" && x.FundTransId >= startFtId)
        //                .OrderBy(x => x.TransDate)
        //                .ThenBy(x => x.FundTransId)
        //                .ToListAsync();

        //            decimal runningAdvances = 0;

        //            foreach (var ft in ledgerTrans)
        //            {
        //                if (ft.TransType != null)
        //                {
        //                    if (ft.TransType.Equals("PAY ADVANCE", StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        runningAdvances += Math.Abs(ft.BillAmount);
        //                    }
        //                    else if (ft.TransType.ToUpper().Contains("SALES INVOICE ADJUST")) 
        //                    {
        //                        runningAdvances -= Math.Abs(ft.BillAmount);
        //                    }
        //                }

        //                if (runningAdvances < 0)
        //                    runningAdvances = 0;

        //                ft.AdvanceBalance = runningAdvances;
        //                await _unitOfWork.fundTranss.UpdateAsync(ft);
        //            }
        //        }


        //        await _unitOfWork.SaveAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, "Error in RecalculateBankLedgerAsync");
        //        throw;
        //    }
        //}



        public async Task<bool> DeletePaymentAsync(int ReceiptId)
        {
            using var trx = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var Receipts = await _unitOfWork.Receiptss
                    .GetQueryable()
                    .Include(x => x.Income)
                    .FirstOrDefaultAsync(x => x.ReceiptId == ReceiptId)
                    ?? throw new Exception("Payment not found");
                // 1️⃣ Load old sub items (CRITICAL)
                var oldSubItems = await _unitOfWork.ReceiptsSubs
                    .GetQueryable()
                    .Where(x => x.ReceiptId == ReceiptId)
                    .ToListAsync();

                if (Receipts == null) return false;

                var changes = new StringBuilder();

                // 2️⃣ Revert PO / Invoice balances
                await UpdatePendingBalanceAsync(Receipts.Income.IncomeName, new List<ReceiptsSubVM>(), // delete → empty
                    oldSubItems);
                var ft = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.ReceiptRefId == ReceiptId && x.IncomeCode == Receipts.IncomeCode);

                int? startFtId = ft?.FundTransId;
                int? bankId = Receipts.BankId;

                if (ft != null)
                {
                    await _unitOfWork.fundTranss.DeleteAsync(ft);
                }
                else
                {
                    return false;
                }
                await _unitOfWork.Receiptss.DeleteAsync(Receipts);
                await _unitOfWork.SaveAsync();

                // 🔥 Recalculate remaining balances
                if (startFtId.HasValue)
                    await RecalculateBankLedgerAsync(bankId, startFtId.Value, Receipts.PayFromRefCode??0);

                await UpdateBankBalanceDeleteAsync(bankId ?? 0);

                await trx.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Receipts List",
                    action: $"Deleted Receipts: {Receipts.ReceiptNo}",
                    additionalInfo: $"Receipts Id: {Receipts.ReceiptId}\n{changes}"
                );
                return true;

            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete Receipts: {ReceiptId}");
                throw;
            }

        }      

        private async Task UpdateBankBalanceDeleteAsync(int bankId)
        {
            try
            {
                var lastBalance = await _unitOfWork.fundTranss
               .GetQueryable()
               .Where(x => x.BankId == bankId)
               .OrderByDescending(x => x.FundTransId)
               .Select(x => x.RunningBalance)
               .FirstOrDefaultAsync();

                var bank = await _unitOfWork.Banks
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.BankId == bankId);

                if (bank != null)
                {
                    bank.CurrentBalance = lastBalance;
                    bank.BankOPBal = lastBalance;
                    await _unitOfWork.Banks.UpdateAsync(bank);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SaveFundTransAsync");
                throw;
            }

        }

        private async Task UpdateBankBalanceAsync(int bankId)
        {
            try
            {
                var lastBalance = await _unitOfWork.fundTranss
               .GetQueryable()
               .Where(x => x.BankId == bankId)
               .OrderByDescending(x => x.FundTransId)
               .Select(x => x.RunningBalance)
               .FirstOrDefaultAsync();

                var bank = await _unitOfWork.Banks
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.BankId == bankId);

                if (bank != null)
                {
                    bank.CurrentBalance = lastBalance;
                    await _unitOfWork.Banks.UpdateAsync(bank);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SaveFundTransAsync");
                throw;
            }
        }

        public async Task<bool> DeleteReceiptSubidAsync(int receiptsid)
        {
            try
            {
                var oldSubItems = await _unitOfWork.ReceiptsSubs
                 .GetQueryable()
                 .Where(x => x.ReceiptSubId == receiptsid)
                 .ToListAsync();
                if (oldSubItems != null)
                {
                    await _unitOfWork.ReceiptsSubs.DeleteAsync(receiptsid);
                    await _unitOfWork.SaveAsync();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SaveFundTransAsync");
                throw;

            }


        }


        private string GetPropertyChanges<TSource, TTarget>(TSource entity, TTarget vm)
        {
            var sb = new StringBuilder();
            foreach (var prop in typeof(TSource).GetProperties())
            {
                var vmProp = typeof(TTarget).GetProperty(prop.Name);
                if (vmProp == null) continue;

                var oldVal = prop.GetValue(entity)?.ToString() ?? "null";
                var newVal = vmProp.GetValue(vm)?.ToString() ?? "null";

                if (oldVal != newVal)
                    sb.AppendLine($"{prop.Name}: '{oldVal}' → '{newVal}'");
            }
            return sb.ToString();
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Enquiry Purchase",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<bool> IsDuplicatePaymentsAsync(string PaymentNo, int? PaymentId = null)
        {
            try
            {
                return await _unitOfWork.Payments
                    .AnyAsync(e =>
                        e.PaymentNo == PaymentNo &&
                        (!PaymentId.HasValue || e.PaymentId != PaymentId));
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to check Payments Number '{PaymentNo}'");
                throw;
            }
        }

        public async Task<IEnumerable<ReceiptsVM>> GetAllReceiptsAsync()
        {
            try
            {
                var entityList = await _unitOfWork.Receiptss
                       .GetQueryable()
                       .AsSplitQuery()
                       .Include(e => e.Income)
                       .Include(e => e.Banks)
                       .Include(e => e.ReceiptsSubs)
                       .OrderByDescending(e => e.ReceiptId)
                       .ToListAsync();


                var vmList = _mapper.Map<List<ReceiptsVM>>(entityList);


                return vmList;  // ✔ IEnumerable

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to cGetAllPaymentsAsync");
                throw;


            }

        }

        //////Search
        public async Task<(List<ReceiptsVM> ReceiptsVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.Receiptss
                         .GetQueryable()
                         .AsSplitQuery()
                         .Include(e => e.Income)
                         .Include(e => e.Banks)
                         .Include(e => e.ReceiptsSubs)
                         .AsQueryable();

            string? status = null;
            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    if (f.Key == "Status")
                    {
                        status = f.Value?.ToString();
                    }
                    else
                    {
                        query = PaymnentsFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                    }
                }
            }


            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.ReceiptId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var vmList = _mapper.Map<List<ReceiptsVM>>(list);

            return (vmList, total);
        }

        public static class PaymnentsFilterBuilder
        {
            public static IQueryable<Receipts> ApplyFilter(
                IQueryable<Receipts> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                //Sathish

                switch (field)
                {
                    case "ReceiptNo":
                        {
                            var input = val.ToString()?.Trim();
                            if (string.IsNullOrEmpty(input))
                                return query;

                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex).Trim();
                            }

                            return query.Where(x => (string.IsNullOrEmpty(part1) || x.ReceiptNo.ToString().Contains(part1)) &&
                                                  (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2))));
                        }
                    //Sathish
                    case "PayFromName":
                        return query.Where(x => x.PayFromName.Contains(val));


                    case "PaymentMode":
                        return query.Where(x => x.PaymentMode.Contains(val));

                    case "PaymentTypeName":
                        return query.Where(x => x.PaymentTypeName.Contains(val));

                    case "Amount":
                        return query.Where(x => x.Amount.ToString().Contains(val));

                    case "FromDate":
                        return query.Where(x => x.ReceiptDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.ReceiptDate <= DateTime.Parse(value.ToString()));


                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<Receipts> ApplyStatusFilter(
                IQueryable<Receipts> query, string status)
            {
                return status switch
                {
                    
                    _ => query
                };
            }
        }

        public async Task<decimal> GetReceiptsSubAmountByIdAsync(int receipSubid)
        {
            try
            {
                var entity = await _unitOfWork.ReceiptsSubs.GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.ReceiptSubId == receipSubid)
                    .Select(x => x.AdjustAmount) // <-- return the decimal field you need
                    .FirstOrDefaultAsync();

                return entity; // if null, returns 0 automatically
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to GetPaymentSubAmountByIdAsync");
                throw;
            }
        }


        public async Task<ReceiptsVM> GetReceiptsByIdAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.Receiptss
                            .GetQueryable()
                            .AsNoTracking()
                            .Include(x => x.ReceiptsSubs)
                            .Include(x => x.Banks)
                            .Include(x => x.Income)
                            .FirstOrDefaultAsync(x => x.ReceiptId == id);

                if (entity == null)
                    return null;



                var vm = _mapper.Map<ReceiptsVM>(entity);

                // Party
                if (entity.PayFromRefCode > 0)
                {
                    var CustId = Convert.ToInt32(entity.PayFromRefCode);
                    var Customer = await _unitOfWork.Customers
                        .GetQueryable()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.CustId == CustId);

                    vm.PayFromName = Customer?.CustName;
                    vm.PayFromRefCode = Customer?.CustId;
                }

                return vm;
            }
            catch (Exception ex)    
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to GetPaymentsByIdAsync");
                throw;
            }

        }

    }
}
