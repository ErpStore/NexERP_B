using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Presentation;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
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


namespace V.SMART.Shared.BusinessLayer.BusinessService.AccountsService
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
       
        public PaymentsService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            ICommonService commonService,
            IMapper mapper , IHttpClientFactory httpFactory)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;
            
        }

        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
            => await _commonService.SearchVendorsAsync(searchText);
        public async Task<VendorVM?> GetVendorByIdAsync(int VendorCode)
           => await _commonService.GetVendorByVenerCodeAsync(VendorCode);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
                       => await _commonService.GetCompanyDetailsAsync();

        #region payment module

        public async Task<string> GeneratePaymentNumberAsync()
        {
            try
            {

                var today = DateTime.Today;
                int fyStartYear = (today.Month >= 4) ? today.Year : today.Year - 1;
                var fyStart = new DateTime(fyStartYear, 4, 1);
                var fyEnd = new DateTime(fyStartYear + 1, 3, 31);

                // Get highest from Payments table
                var highest1 = await _unitOfWork.Payments
                                .GetQueryable()
                                .Where(e => e.PaymentDate >= fyStart && e.PaymentDate <= fyEnd)
                                .OrderByDescending(e => e.PaymentNo)
                                .Select(e => e.PaymentNo)
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
            var grouped = await _commonService.SearchVendorsAsync(searchText);

            var partyList = grouped.Select(g => new PartyVM
            {
                PartyCode = g.VendorCode,
                PartyName = g.VendorName,
                OpeningBalance = g.OpeningBalance??0m ,
                OpeningBalancePending = g.OpeningBalancePending??0m ,
                AdvancePaid =
                    _unitOfWork.fundTranss.GetQueryable()
                        .Where(f => f.LedgerId == g.VendorCode && f.TransType == "Pay Advance")
                        .Sum(f => (decimal?)f.AdvanceBalance) ?? 0
            });

            return partyList.ToList();
        }
        public async Task<decimal> GetAdvanceAmountPaid(string type, string paymentType,int PartyCode)
        {
            try
            {
                if (type?.ToUpper().Trim() == "PURCHASE" )
                {
                    decimal result = await _unitOfWork.fundTranss.GetQueryable()
                                    .Where(f => f.LedgerId == PartyCode)
                                    .OrderByDescending(f => f.FundTransId)
                                    .Select(f => f.AdvanceBalance)
                                    .FirstOrDefaultAsync();

                    return result > 0 ? result : 0m;
                }
                return 0m;

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

                        var Vendor = await _unitOfWork.fundTranss.GetQueryable()
                                .Where(f => f.ExpenseCode > 0 && f.TransType == "Pay Advance" && f.LedgerType == "VENDOR" && f.AdvanceBalance > 0)
                                .OrderByDescending(f => f.FundTransId)
                                .Select(f => f.LedgerId)
                                .Distinct()
                                .ToListAsync();


                        var vendors = await _unitOfWork.Vendors.GetQueryable()
                            .Where(v => v.Inactive == false && Vendor.Contains(v.VendorCode))
                            .ToListAsync();

                        var list = new List<PartyVM>();

                        foreach (var v in vendors)
                        {
                            //decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                            //     .Where(f => f.ExpenseCode > 0 && f.TransType == "Pay Advance" && f.LedgerType == "VENDOR" && f.AdvanceBalance > 0)
                            //    .OrderByDescending(f => f.FundTransId)
                            //    .Select(f => f.AdvanceBalance)
                            //    .FirstOrDefaultAsync();

                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                              .Where(f => f.LedgerId == v.VendorCode && f.LedgerType == "VENDOR")
                              .OrderByDescending(f => f.FundTransId)
                              .Select(f => (decimal?)f.AdvanceBalance)
                              .FirstOrDefaultAsync() ?? 0;

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

                    case "SUBCONTRACT":

                        var Suppliers = await _unitOfWork.Vendors.GetQueryable()
                            .Where(v => v.Inactive == false)
                            .ToListAsync();

                        var lists = new List<PartyVM>();

                        foreach (var v in Suppliers)
                        {
                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                             .Where(f => f.LedgerId == v.VendorCode && f.LedgerType == "VENDOR")
                             .OrderByDescending(f => f.FundTransId)
                             .Select(f => (decimal?)f.AdvanceBalance)
                             .FirstOrDefaultAsync() ?? 0;

                            decimal pending = await _unitOfWork.PurchaseInvoices.GetQueryable()
                                .Where(pi => pi.VendorCode == v.VendorCode && pi.Balance > 0)
                                .SumAsync(pi => (decimal?)pi.Balance) ?? 0m;

                            if (lastAdvance > 0 && pending > 0)
                            {
                                lists.Add(new PartyVM
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

                        return lists.OrderBy(x => x.PartyName).ToList();


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
                                e.Vendor.OpenBalPndg,
                               
                            });

                        foreach (var g in groups)
                        {
                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                              .Where(f => f.LedgerId == g.Key.VendorCode && f.LedgerType == "CUSTOMER")
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

                    case "SUBCONTRACT ADVANCE":

                        var AdvList = await _unitOfWork.PurchPos.GetQueryable()
                            .Include(e => e.Vendor)
                            .ToListAsync();

                        var listPOAdv = new List<PartyVM>();

                        var groupss = AdvList
                            .Where(e => e.AdvanceAmountBal > 0)
                            .GroupBy(e => new
                            {
                                e.Vendor.VendorCode,
                                e.Vendor.VendorName,
                                e.Vendor.OpenBal,
                                e.Vendor.OpenBalPndg,
                               
                            });

                        foreach (var g in groupss)
                        {
                            decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
                             .Where(f => f.LedgerId == g.Key.VendorCode && f.LedgerType == "CUSTOMER")
                             .OrderByDescending(f => f.FundTransId)
                             .Select(f => (decimal?)f.AdvanceBalance)
                             .FirstOrDefaultAsync() ?? 0;


                            listPOAdv.Add(new PartyVM
                            {
                                PartyCode = g.Key.VendorCode,
                                PartyName = g.Key.VendorName,
                                OpeningBalance = g.Key.OpenBal ?? 0,
                                OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
                                AdvancePaid = lastAdvance
                            });
                        }

                        return listPOAdv.OrderBy(x => x.PartyName).ToList();

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
        public async Task<List<PartyVM>> GetVendorsByPurchaseTypeAsync(string type, string paymentType)
        {
            try
            {
                type = type?.ToUpper() ?? "";
                paymentType = paymentType?.ToUpper() ?? "";

                // 🔹 STEP 1: Get Advance in ONE QUERY (no loop)
                var advanceDict = await _unitOfWork.fundTranss.GetQueryable()
                    .Where(f => f.LedgerId != null) // ✅ FIX
                    .GroupBy(f => f.LedgerId)
                    .Select(g => new
                    {
                        LedgerId = g.Key,
                        Advance = g.OrderByDescending(x => x.FundTransId)
                                   .Select(x => x.AdvanceBalance)
                                   .FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.LedgerId, x => x.Advance);

                // 🔹 STEP 2: OPENING / ADVANCE MODE
                if (paymentType == "PAY ADVANCE" || paymentType == "OPENING BALANCE")
                {
                    var vendors = await _unitOfWork.Vendors.GetQueryable()
                        .Where(x => !x.Inactive)
                        .Select(v => new PartyVM
                        {
                            PartyCode = v.VendorCode,
                            PartyName = v.VendorName,
                            OpeningBalance = v.OpenBal ?? 0,
                            OpeningBalancePending = v.OpenBalPndg ?? 0,
                            AdvancePaid = advanceDict.ContainsKey(v.VendorCode)
                                            ? advanceDict[v.VendorCode]
                                            : 0
                        })
                        .ToListAsync();

                    return vendors.OrderBy(x => x.PartyName).ToList();
                }

                // 🔹 STEP 3: GET INVOICE DATA (COMMON SHAPE)
                IQueryable<InvoiceCommonVM> invoiceQuery = type switch
                {
                    "PURCHASE" => _unitOfWork.PurchaseInvoices.GetQueryable()
                        .Where(e => e.Balance > 0)
                        .Select(e => new InvoiceCommonVM
                        {
                            VendorCode = e.Vendor.VendorCode,
                            VendorName = e.Vendor.VendorName,
                            OpenBal = e.Vendor.OpenBal,
                            OpenBalPndg = e.Vendor.OpenBalPndg,
                            Balance = e.Balance
                        }),

                    "SUBCONTRACT" => _unitOfWork.SubConInvs.GetQueryable()
                   .Where(e => e.Balance > 0)
                   .Select(e => new InvoiceCommonVM
                   {
                       VendorCode = e.Vendor.VendorCode,
                       VendorName = e.Vendor.VendorName,
                       OpenBal = e.Vendor.OpenBal,
                       OpenBalPndg = e.Vendor.OpenBalPndg,
                       Balance = e.Balance
                   }),


                    "PURCHASE ORDER ADVANCE" =>   _unitOfWork.PurchPos.GetQueryable()
                        .Where(e => e.AdvanceAmountBal > 0)
                        .Select(e => new InvoiceCommonVM
                        {
                            VendorCode = e.Vendor.VendorCode,
                            VendorName = e.Vendor.VendorName,
                            OpenBal = e.Vendor.OpenBal,
                            OpenBalPndg = e.Vendor.OpenBalPndg,
                            Balance = e.AdvanceAmountBal
                        }),

                    _ => null
                };

                if (invoiceQuery == null)
                    return new List<PartyVM>();

                var invoiceList = await invoiceQuery.ToListAsync();

                // 🔹 STEP 4: GROUP + BUILD RESULT
                var result = invoiceList
                    .GroupBy(e => new
                    {
                        e.VendorCode,
                        e.VendorName,
                        e.OpenBal,
                        e.OpenBalPndg
                    })
                    .Select(g => new PartyVM
                    {
                        PartyCode = g.Key.VendorCode ?? 0,
                        PartyName = g.Key.VendorName,
                        OpeningBalance = g.Key.OpenBal ?? 0,
                        OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
                        AdvancePaid = advanceDict.ContainsKey(g.Key.VendorCode ?? 0)
                                        ? advanceDict[g.Key.VendorCode ?? 0]
                                        : 0
                    })
                    .OrderBy(x => x.PartyName)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed loading vendor list: {type}");
                throw;
            }
        }

        //public async Task<List<PartyVM>> GetVendorsByPurchaseTypeAsync(string type, string paymentType) ///250426
        //{
        //    try
        //    {

        //        paymentType = paymentType?.ToUpper()??"";

        //        IQueryable<PartyVM> query;

        //        switch (type?.ToUpper()??"")
        //        {
        //            case "PURCHASE" :
        //            case "SUBCONTRACT":

        //                if (paymentType == "PAY ADVANCE" || paymentType == "OPENING BALANCE")
        //                {
        //                    // Only opening balance vendors, advance = last pending
        //                    var vendors = await _unitOfWork.Vendors.GetQueryable()
        //                        .Where(x => x.Inactive == false && x.Inactive == false)
        //                        .ToListAsync();

        //                    var result = new List<PartyVM>();

        //                    foreach (var v in vendors)
        //                    {
        //                        decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
        //                            .Where(f => f.LedgerId == v.VendorCode)
        //                            .OrderByDescending(f => f.FundTransId)
        //                            .Select(f => f.AdvanceBalance)
        //                            .FirstOrDefaultAsync();

        //                        result.Add(new PartyVM
        //                        {
        //                            PartyCode = v.VendorCode,
        //                            PartyName = v.VendorName,
        //                            OpeningBalance = v.OpenBal ?? 0,
        //                            OpeningBalancePending = v.OpenBalPndg ?? 0,
        //                            AdvancePaid = lastAdvance
        //                        });
        //                    }

        //                    return result.OrderBy(x => x.PartyName).ToList();
        //                }
        //                else
        //                {

        //                    // Purchase invoices pending, advance = last pending
        //                    var invoices = await _unitOfWork.PurchaseInvoices.GetQueryable()
        //                        .Include(e => e.Vendor)
        //                        .Where(e => e.Balance > 0)
        //                        .ToListAsync();

        //                    var result = new List<PartyVM>();

        //                    var groups = invoices.GroupBy(e => new
        //                    {
        //                        e.Vendor?.VendorCode,
        //                        e.Vendor?.VendorName,
        //                        e.Vendor?.OpenBal,
        //                        e.Vendor?.OpenBalPndg
        //                    });

        //                    foreach (var g in groups)
        //                    {
        //                        decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
        //                            .Where(f => f.LedgerId == g.Key.VendorCode)
        //                            .OrderByDescending(f => f.FundTransId)
        //                            .Select(f => f.AdvanceBalance)
        //                            .FirstOrDefaultAsync();

        //                        result.Add(new PartyVM
        //                        {
        //                            PartyCode = g.Key.VendorCode??0,
        //                            PartyName = g.Key.VendorName,
        //                            OpeningBalance = g.Key.OpenBal ?? 0,
        //                            OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
        //                            AdvancePaid = lastAdvance
        //                        });
        //                    }

        //                    return result.OrderBy(x => x.PartyName).ToList();
        //                }

        //            case "PURCHASE ORDER ADVANCE":

        //                var poList = await _unitOfWork.PurchPos.GetQueryable()
        //                    .Include(e => e.Vendor)
        //                    .Where(e => e.AdvanceAmountBal > 0)
        //                    .ToListAsync();

        //                var resultPO = new List<PartyVM>();

        //                var groupsPO = poList.GroupBy(e => new
        //                {
        //                    e.Vendor?.VendorCode,
        //                    e.Vendor?.VendorName,
        //                    e.Vendor?.OpenBal,
        //                    e.Vendor?.OpenBalPndg
        //                });

        //                foreach (var g in groupsPO)
        //                {
        //                    decimal lastAdvance = await _unitOfWork.fundTranss.GetQueryable()
        //                        .Where(f => f.LedgerId == g.Key.VendorCode)
        //                        .OrderByDescending(f => f.FundTransId)
        //                        .Select(f => f.AdvanceBalance)
        //                        .FirstOrDefaultAsync();

        //                    resultPO.Add(new PartyVM
        //                    {
        //                        PartyCode = g.Key.VendorCode??0,
        //                        PartyName = g.Key.VendorName,
        //                        OpeningBalance = g.Key.OpenBal ?? 0,
        //                        OpeningBalancePending = g.Key.OpenBalPndg ?? 0,
        //                        AdvancePaid = lastAdvance
        //                    });
        //                }

        //                return resultPO.OrderBy(x => x.PartyName).ToList();




        //            default:
        //                return new List<PartyVM>();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, $"Failed loading vendor list: {type}");
        //        throw;
        //    }
        //}



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

        public async Task<List<BillsVM>> GetBillsByVendorAsync(int vendorCode, string billType)
        {
            try
            {
                IQueryable<BillsVM> query;

                switch (billType.ToUpper())
                {
                    case "PURCHASE":
                        query = _unitOfWork.PurchaseInvoices.GetQueryable()
                            .Include(e => e.Vendor)
                            .Where(e => e.Balance > 0 && e.VendorCode == vendorCode)
                            .OrderByDescending(e => e.InvId)
                            .Select(e => new BillsVM
                            {
                                Id = e.InvId,
                                PartyCode = e.Vendor.VendorCode,
                                BillNo = e.InvNo,
                                BillDate = e.InvDate,
                                Balance = e.Balance
                            });
                        break;

                    case "PURCHASE ORDER ADVANCE":
                        query = _unitOfWork.PurchPos.GetQueryable()
                            .Include(e => e.Vendor)
                            .Where(e => e.AdvanceAmountBal > 0 && e.VendorCode == vendorCode)
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

                    case "SUBCONTRACT":
                        query = _unitOfWork.SubConInvs.GetQueryable()
                            .Include(e => e.Vendor)
                            .Where(e => e.Balance > 0 && e.VendorCode == vendorCode)
                            .OrderByDescending(e => e.InvId)
                            .Select(e => new BillsVM
                            {
                                Id = e.InvId,
                                PartyCode = e.Vendor.VendorCode,
                                BillNo = e.InvNo,
                                BillDate = e.InvDate,
                                Balance = e.Balance
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
        public async Task<PaymentsVM> UpsertPaymentsAsync(PaymentsVM vm, bool BillAdjust)
        {
            using var trx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _currentUserService.GetUsernameAsync();
                var now = DateTime.Now;
                int? oldBankId = null;
                int? oldFtId = null;
                int? oldFtExpenseCode = null;
                decimal OldAmount= 0;
                var oldSubs = new List<PaymentsSub>();

                if (vm.PaymentId > 0)
                {
                    var oldFt = await _unitOfWork.fundTranss
                                .GetQueryable()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.PaymentRefId == vm.PaymentId);


                    oldSubs = await _unitOfWork.PaymentsSubs
                            .GetQueryable()
                            .AsNoTracking()
                            .Where(x => x.PaymentId == vm.PaymentId)
                            .ToListAsync();

                    oldBankId = oldFt?.BankId;
                    oldFtId = oldFt?.FundTransId;
                    OldAmount= oldFt.BillAmount;
                    oldFtExpenseCode = oldFt.ExpenseCode;
                }
                var entity = await SaveHeaderAsync(vm, now, user);

                var ft = await SaveFundTransAsync(entity, vm);

                //if(vm.ExpenseName != "Opening Balance")
                //{
                    if (oldBankId.HasValue && oldBankId != entity.BankId && oldFtId.HasValue)
                    {
                        await RecalculateBankLedgerAsync(oldBankId, oldFtId.Value,entity.PayToRefCode??0);
                        await UpdateBankBalanceAsync(oldBankId.Value);
                    }
                    if (entity.BankId.HasValue)
                    {
                        await RecalculateBankLedgerAsync(entity.BankId, ft.FundTransId,entity.PayToRefCode ?? 0);
                        await UpdateBankBalanceAsync(entity.BankId.Value);
                    }

                //}

                if (BillAdjust && vm.PaymentId > 0)
                {

                    var addedItems = vm.PaymentSubVM
                        .Where(x => oldSubs.Any(o => o.RefId == x.RefId))
                        .ToList();

                    var removedItems = oldSubs
                        .Where(x => vm.PaymentSubVM.Any(n => n.RefId == x.RefId && x.AdjustAmount !=n.AdjustAmount))
                        .ToList();
                    if(oldSubs.Count>=1)
                    {
                        await UpdatePendingBalanceAsync(vm.ExpenseName, addedItems, removedItems);

                    }
                    else
                    {
                        await UpdatePendingBalanceAsync(vm.ExpenseName, vm.PaymentSubVM, new List<PaymentsSub>());
                    }

                  
                }
                else if (BillAdjust)
                {
                    
                    await UpdatePendingBalanceAsync(vm.ExpenseName, vm.PaymentSubVM, new List<PaymentsSub>());
                }
                else
                {
                    await UpdateOpeningBalanceAsync(entity,vm.PaymentId, OldAmount);
                }

                    await trx.CommitAsync();

                return _mapper.Map<PaymentsVM>(entity);
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Error in UpsertPaymentsAsync");
                throw;
            }
        }

        public async Task UpdateOpeningBalanceAsync(Payments Vm, int paymentid, decimal OldAmount)
        {
            try
            {
                var expenseType = Vm.Expense?.ExpenseName?.ToUpperInvariant().Trim() ?? "";

                if (Vm.PayToRefCode == null || Vm.PayToRefCode == 0)
                    return;

                decimal newAmount = Vm.Amount;

                switch (expenseType)
                {
                    case "PURCHASE":
                        {
                            if (Vm.PaymentTypeName.ToUpper() == "OPENING BALANCE")
                            {
                                var vendor = await _unitOfWork.Vendors
                                    .GetQueryable()
                                    .FirstOrDefaultAsync(x => x.VendorCode == Vm.PayToRefCode);

                                if (vendor == null) return;

                                decimal pending = vendor.OpenBalPndg ?? 0m;

                                if (paymentid > 0) // EDIT MODE
                                {
                                    // 🔥 Apply same logic style as PO
                                    pending = pending + OldAmount - newAmount;
                                }
                                else // NEW PAYMENT MODE
                                {
                                    pending = pending - newAmount;
                                }

                                vendor.OpenBalPndg = Math.Max(0m, pending);

                                await _unitOfWork.Vendors.UpdateAsync(vendor);
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

        public async Task UpdatePendingBalanceAsync(string expenseType, List<PaymentSubVM> newItems, List<PaymentsSub> oldItems)
        {
            try
            {
                expenseType = expenseType?.ToUpper()?.Trim() ?? "";

                var allRefIds = newItems
                    .Select(x => x.RefId ?? 0)
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

                switch (expenseType)
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
                    case "PURCHASE":
                        {

                            var invoices = await _unitOfWork.PurchaseInvoices
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

                                await _unitOfWork.PurchaseInvoices.UpdateAsync(inv);
                            }
                            break;
                        }
                    // -------------------------------
                    // SUBCONTRACT INVOICE
                    // -------------------------------
                    case "SUBCONTRACT":
                        {

                            var invoices = await _unitOfWork.SubConInvs
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
                                await _unitOfWork.SubConInvs.UpdateAsync(inv);
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


        private async Task<Payments> SaveHeaderAsync(PaymentsVM vm, DateTime now, string user)
        {
            try
            {
                Payments entity;

                if (vm.PaymentId == 0)
                {
                    entity = _mapper.Map<Payments>(vm);

                    entity.CreatedBy = user;
                    entity.CreatedDate = now;

                    entity.PaymentsSubs = vm.PaymentSubVM
                        .Select(sub => _mapper.Map<PaymentsSub>(sub))
                        .ToList();

                    await _unitOfWork.Payments.CreateAsync(entity);
                }
                else
                {
                    entity = await _unitOfWork.Payments
                         .GetQueryable()
                         .Include(s => s.PaymentsSubs)
                         .FirstOrDefaultAsync(x => x.PaymentId == vm.PaymentId)
                         ?? throw new Exception("Payment not found");

                    entity.ModifiedBy = user;
                    entity.ModifiedDate = now;

                    _mapper.Map(vm, entity);
                    await _unitOfWork.Payments.UpdateAsync(entity);
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
        private async Task<FundTrans> SaveFundTransAsync(Payments entity, PaymentsVM vm)
        {
            try
            {
                // 🔹 Find existing fund transaction
                var ft = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.PaymentRefId == entity.PaymentId && x.ExpenseCode == entity.ExpenseCode);

                bool isNew = false;

                if (ft == null)
                {
                    ft = new FundTrans
                    {
                        PaymentRefId = entity.PaymentId
                    };
                    isNew = true;
                }
                // 🔹 Amount sign
                decimal paidOrReceived =
                    vm.PaymentTypeName == "Opening Balance"
                        ? -vm.Amount
                        : -vm.Amount;
                if (vm.ExpenseName.ToUpper() == "PURCHASE ORDER ADVANCE")
                {
                    vm.PaymentTypeName = "Pay Advance";
                    
                }
                decimal advance =
                      (vm.PaymentTypeName?.Equals("Pay Advance", StringComparison.OrdinalIgnoreCase) == true ||
                       vm.PaymentTypeName?.Equals("Purchase Order Advance", StringComparison.OrdinalIgnoreCase) == true)
                          ? vm.Amount 
                          : 0;

                // 🔹 Update fields
                ft.TransNo = entity.PaymentNo;
                ft.TransDate = entity.PaymentDate;
                ft.TransType = vm.PaymentTypeName;
                ft.TrasactionMode = entity.PaymentMode;
                ft.LedgerId = entity.PayToRefCode;
                ft.LedgerName = entity.PayToName;
                ft.ExpenseCode = entity.ExpenseCode;
                ft.LedgerType = "VENDOR";
                ft.BankId = entity.BankId;
                ft.ChequeNo = entity.ChequeNo;
                ft.ChequeDate = entity.ChequeDate;
                ft.BillAmount = entity.Amount;
                ft.PaidOrReceived = paidOrReceived;
                ft.AdvanceBalance = advance;
                ft.TransactionType= TransTypes.Payment;
                ft.CreatedBy = entity.CreatedBy;
                ft.ModifiedBy = entity.ModifiedBy;
                ft.ModifiedDate=entity.ModifiedDate;

                if (isNew)
                    await _unitOfWork.fundTranss.CreateAsync(ft);
                else
                    await _unitOfWork.fundTranss.UpdateAsync(ft);

                await _unitOfWork.SaveAsync();

                // 🔥 REBUILD RUNNING BALANCE
                await RecalculateBankLedgerAsync(entity.BankId, ft.FundTransId,entity.PayToRefCode??0);

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
                        if (!string.Equals(ft.TransType, "PURCHASE ORDER ADVANCE", StringComparison.OrdinalIgnoreCase))
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
                                                 && x.LedgerType == "VENDOR"
                                                 && x.FundTransId >= startFtId)
                                        .OrderBy(x => x.FundTransId)
                                        .ToListAsync();

                decimal runningAdvance = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .Where(x => x.LedgerId == ledgerId
                             && x.LedgerType == "VENDOR"
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
                        else if (ft.TransType.ToUpper().Contains("PURCHASE INVOICE ADJUST") || ft.TransType.ToUpper().Contains("SUBCONTRACT INVOICE ADJUST"))
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


        //private async Task RecalculateBankLedgerAsync(int? bankId, int startFtId)
        //{
        //    try
        //    {
        //        if (bankId == null)
        //            return;

        //        var trans = await _unitOfWork.fundTranss
        //            .GetQueryable()
        //            .Where(x => x.BankId == bankId && x.FundTransId >= startFtId && x.ExpenseCode==ExpenseCode)
        //            .OrderBy(x => x.FundTransId)
        //            .ToListAsync();

        //        decimal running = await _unitOfWork.fundTranss
        //            .GetQueryable()
        //            .Where(x => x.BankId == bankId && x.FundTransId < startFtId && x.ExpenseCode==ExpenseCode)
        //            .OrderByDescending(x => x.FundTransId)
        //            .Select(x => x.RunningBalance)
        //            .FirstOrDefaultAsync();

        //        decimal runningAdvance = await _unitOfWork.fundTranss
        //            .GetQueryable()
        //            .Where(x => x.BankId == bankId && x.FundTransId < startFtId && x.ExpenseCode == ExpenseCode )
        //            .OrderByDescending(x => x.FundTransId)
        //            .Select(x => x.AdvanceBalance)
        //            .FirstOrDefaultAsync();


        //        foreach (var ft in trans)
        //        {
        //            // 1️⃣ Update bank balance only if not invoice adjust
        //            if (ft.TransType.ToUpper() != "PURCHASE INVOICE ADJUST")
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

        //        await _unitOfWork.SaveAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, "Error in RecalculateBankLedgerAsync");
        //        throw;
        //    }
        //}



        public async Task<bool> DeletePaymentAsync(int paymentId)
        {
            using var trx = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var payment = await _unitOfWork.Payments
                    .GetQueryable()
                    .Include(x=>x.Expense)
                    .FirstOrDefaultAsync(x => x.PaymentId == paymentId)
                    ?? throw new Exception("Payment not found");
                // 1️⃣ Load old sub items (CRITICAL)
                var oldSubItems = await _unitOfWork.PaymentsSubs
                    .GetQueryable()
                    .Where(x => x.PaymentId == paymentId)
                    .ToListAsync();

                if (payment == null) return false;

                var changes = new StringBuilder();

                // 2️⃣ Revert PO / Invoice balances
                await UpdatePendingBalanceAsync( payment.Expense.ExpenseName, new List<PaymentSubVM>(), // delete → empty
                    oldSubItems );
                var ft = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.PaymentRefId == paymentId && x.ExpenseCode== payment.ExpenseCode);

                int? startFtId = ft?.FundTransId;
                int? bankId = payment.BankId;

                if (ft != null)
                {
                    await _unitOfWork.fundTranss.DeleteAsync(ft);
                }
                else
                {
                    return false;
                }
                await _unitOfWork.Payments.DeleteAsync(payment);
                await _unitOfWork.SaveAsync();

                // 🔥 Recalculate remaining balances
                if (startFtId.HasValue)
                    await RecalculateBankLedgerAsync(bankId, startFtId.Value,ft.ExpenseCode??0);

                await UpdateBankBalanceDeleteAsync(bankId ?? 0);

                await trx.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Payment List",
                    action: $"Deleted Payments: {payment.PaymentNo}",
                    additionalInfo: $"Payments Id: {payment.PaymentId}\n{changes}"
                );
                return true;

            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete Payments: {paymentId}");
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

        public async Task<bool> DeletePaymentSubidAsync(int paymentId)
        {
            try
            {
                var oldSubItems = await _unitOfWork.PaymentsSubs
                 .GetQueryable()
                 .Where(x => x.PaymentSubId == paymentId)
                 .ToListAsync();

                if (oldSubItems != null)
                {
                    await _unitOfWork.PaymentsSubs.DeleteAsync(paymentId);
                    await _unitOfWork.SaveAsync();
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex )
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
                        (!PaymentId.HasValue || e.PaymentId != PaymentId) );
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to check Payments Number '{PaymentNo}'");
                throw;
            }
        }

        public async Task<IEnumerable<PaymentsVM>> GetAllPaymentsAsync()
        {
            try
            {
                var entityList = await _unitOfWork.Payments
                       .GetQueryable()
                       .AsSplitQuery()
                       .Include(e => e.Expense)
                       .Include(e => e.Banks)
                       .Include(e => e.PaymentsSubs)
                       .OrderByDescending(e => e.PaymentId)
                       .ToListAsync();


                var vmList = _mapper.Map<List<PaymentsVM>>(entityList);


                return vmList;  // ✔ IEnumerable

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to cGetAllPaymentsAsync");
                throw;


            }
          
        }

        //////Search
        public async Task<(List<PaymentsVM> payments, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.Payments
                         .GetQueryable()
                         .AsSplitQuery()
                         .Include(e => e.Expense)
                         .Include(e => e.Banks)
                         .Include(e => e.PaymentsSubs)
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
                .OrderByDescending(x => x.PaymentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

           
            var vmList = _mapper.Map<List<PaymentsVM>>(list);

            
            return (vmList, total);
        }

        public static class PaymnentsFilterBuilder
        {
            public static IQueryable<Payments> ApplyFilter(
                IQueryable<Payments> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "PaymentNo":
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

                            //Sathish
                            return query.Where(x => (string.IsNullOrEmpty(part1) || x.PaymentNo.ToString().Contains(part1)) &&
                                                 (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2))));
                        }


                    //!------------------!

                    case "PayToName":
                        return query.Where(x => x.PayToName.Contains(val));


                    case "PaymentMode":
                        return query.Where(x => x.PaymentMode.Contains(val));

                    case "PaymentTypeName":
                        return query.Where(x => x.PaymentTypeName.Contains(val));

                    case "Amount":
                        return query.Where(x => x.Amount.ToString().Contains(val));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.ToString().Contains(val));


                    //-------------------------------

                    case "FromDate":
                        return query.Where(x => x.PaymentDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.PaymentDate <= DateTime.Parse(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<Payments> ApplyStatusFilter(
                IQueryable<Payments> query, string status)
            {
                return status switch
                {
                    _ => query
                };
            }
        }

        public async Task<decimal> GetPaymentSubAmountByIdAsync(int paymentSubId)
        {
            try
            {
                var entity = await _unitOfWork.PaymentsSubs.GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.PaymentSubId == paymentSubId)
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


        public async Task<PaymentsVM> GetPaymentsByIdAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.Payments
                            .GetQueryable()
                            .AsNoTracking()
                            .Include(x => x.PaymentsSubs)
                            .Include(x => x.Banks)
                            .Include(x => x.Expense)
                            .FirstOrDefaultAsync(x => x.PaymentId == id);

                if (entity == null)
                    return null;



                var vm = _mapper.Map<PaymentsVM>(entity);

                // Party
                if (entity.PayToRefCode > 0)
                {
                    var vendorCode = Convert.ToInt32(entity.PayToRefCode);
                    var vendor = await _unitOfWork.Vendors
                        .GetQueryable()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.VendorCode == vendorCode);

                    vm.PayToName = vendor?.VendorName;
                    vm.PayToRefCode = vendor?.VendorCode;
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
