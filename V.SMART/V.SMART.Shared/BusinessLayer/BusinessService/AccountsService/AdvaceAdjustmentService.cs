using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMARTV.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.AccountsService
{
    public class AdvaceAdjustmentService : IAdvaceAdjustmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;

        public AdvaceAdjustmentService(
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

        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
            => await _commonService.SearchVendorsAsync(searchText);
        public async Task<VendorVM?> GetVendorByIdAsync(int VendorCode)
           => await _commonService.GetVendorByVenerCodeAsync(VendorCode);

        public async Task<string> GenerateAdvanceAdjustNumberAsync()
        {
            try
            {
                var today = DateTime.Today;
                int fyStartYear = (today.Month >= 4) ? today.Year : today.Year - 1;
                var fyStart = new DateTime(fyStartYear, 4, 1);
                var fyEnd = new DateTime(fyStartYear + 1, 3, 31);

              
                var highest1 = await _unitOfWork.Payments
                                .GetQueryable()
                                .Where(e => e.PaymentDate >= fyStart && e.PaymentDate <= fyEnd)
                                .OrderByDescending(e => e.PaymentNo)
                                .Select(e => e.PaymentNo)
                                .FirstOrDefaultAsync();

              
                var highest2 = await _unitOfWork.Advaceadjustments
                                .GetQueryable()
                                .Where(e => e.AdjumentDate >= fyStart && e.AdjumentDate <= fyEnd)
                                .OrderByDescending(e => e.AdvaceadjustmentNo)
                                .Select(e => e.AdvaceadjustmentNo)
                                .FirstOrDefaultAsync();

                
                int num1 = int.TryParse(highest1, out var n1) ? n1 : 0;
                int num2 = int.TryParse(highest2, out var n2) ? n2 : 0;

                // Pick highest from both
                int highestNumeric = Math.Max(num1, num2);

                // Increment and return 4 digit format
                return (highestNumeric + 1).ToString("D4");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GenerateAdvanceAdjustNumberAsync()");
                return string.Empty;
            }
        }
        public async Task<List<BillsVM>> GetBillsByVendorAsync(int vendorCode, string billType)
        {
            try
            {
                IQueryable<BillsVM> query;

                switch (billType.ToUpper())
                {
                    case "PURCHASE":
                    case "PURCHASE ORDER ADVANCE":
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
        public async Task<AdvaceadjustmentVM> GetAdvaceAdjustsByIdAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.Advaceadjustments
                            .GetQueryable()
                            .AsNoTracking()
                            .Include(x => x.AdvaceadjustmentSubs)
                             .Include(x=>x.Income)
                             .Include(x => x.Expense)
                            .FirstOrDefaultAsync(x => x.AdjumentId == id);

                if (entity == null)
                    return null;



                var vm = _mapper.Map<AdvaceadjustmentVM>(entity);

                // Party
                if (entity.PayToRefCode > 0)
                {
                    var PartyCode = Convert.ToInt32(entity.PayToRefCode);

                    if (!entity.IsIncome)
                    {
                        var vendor = await _unitOfWork.Vendors
                       .GetQueryable()
                       .AsNoTracking()
                       .FirstOrDefaultAsync(x => x.VendorCode == PartyCode);

                        vm.PayToName = vendor?.VendorName;
                        vm.PayToRefCode = vendor?.VendorCode;
                    }
                    else
                    {
                        var Customer = await _unitOfWork.Customers
                       .GetQueryable()
                       .AsNoTracking()
                       .FirstOrDefaultAsync(x => x.CustId == PartyCode);

                        vm.PayToName = Customer?.CustName;
                        vm.PayToRefCode = Customer?.CustId;

                    }
                    
                   
                }

                return vm;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to GetPaymentsByIdAsync");
                throw;
            }

        }
        public async Task<decimal> GetAdvaceAdjustSubAmountByIdAsync(int AdjustSubId)
        {
            try
            {
                var entity = await _unitOfWork.AdvaceadjustmentSubs.GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.AdjustSubId == AdjustSubId)
                    .Select(x => x.AdjustAmount) // <-- return the decimal field you need
                    .FirstOrDefaultAsync();

                return entity; // if null, returns 0 automatically
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to GetAdvaceAdjustSubAmountByIdAsync");
                throw;
            }
        }


        public async Task<AdvaceadjustmentVM> UpsertAdvanceAdjustAsync(AdvaceadjustmentVM vm, bool BillAdjust)
        {
            using var trx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _currentUserService.GetUsernameAsync();
                var now = DateTime.Now;
                int? oldBankId = null;
                int? oldFtId = null;
                decimal OldAmount = 0;
                var oldSubs = new List<AdvaceadjustmentSub>();

                if (vm.AdjumentId > 0)
                {
                    var oldFt = await _unitOfWork.fundTranss
                                .GetQueryable()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(x => x.AdvanceRefId == vm.AdjumentId);


                    oldSubs = await _unitOfWork.AdvaceadjustmentSubs
                            .GetQueryable()
                            .AsNoTracking()
                            .Where(x => x.AdjumentId == vm.AdjumentId)
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
                    //await RecalculateBankLedgerAsync(oldBankId, oldFtId.Value);
                   //await UpdateBankBalanceAsync(oldBankId.Value);
                }
                if (entity.BankId.HasValue)
                {
                    //await RecalculateBankLedgerAsync(entity.BankId, ft.FundTransId);
                    //await UpdateBankBalanceAsync(entity.BankId.Value);
                }

                //}
                string Type = "";

                if (vm.IsIncome)
                {
                    Type = vm.IncomeName ?? "";
                }
                else
                {
                    Type = vm.ExpenseName ?? "";
                }

                if (BillAdjust && vm.AdjumentId > 0)
                {

                    var addedItems = vm.AdvaceadjustmentSubVM
                        .Where(x => oldSubs.Any(o => o.RefId == x.RefId))
                        .ToList();

                    var removedItems = oldSubs
                        .Where(x => vm.AdvaceadjustmentSubVM.Any(n => n.RefId == x.RefId && x.AdjustAmount != n.AdjustAmount))
                        .ToList();

                    if (oldSubs.Count >= 1)
                    {
                        await UpdatePendingBalanceAsync(Type, addedItems, removedItems);

                    }
                    else
                    {
                       
                        await UpdatePendingBalanceAsync(Type, vm.AdvaceadjustmentSubVM, new List<AdvaceadjustmentSub?>());
                    }


                }
                else if (BillAdjust)
                {
                   
                    await UpdatePendingBalanceAsync(Type, vm.AdvaceadjustmentSubVM, new List<AdvaceadjustmentSub>());
                }
                else
                {
                   // await UpdateOpeningBalanceAsync(entity, vm.PaymentId, OldAmount);
                }

                await trx.CommitAsync();

                return _mapper.Map<AdvaceadjustmentVM>(entity);
            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Error in UpsertPaymentsAsync");
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
        public async Task<bool> DeleteAdvanceAdjustmnetAsync(int AdjustId)
        {
            using var trx = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var payment = await _unitOfWork.Advaceadjustments
                    .GetQueryable()
                    .Include(x => x.Expense)
                    .Include(x=>x.Income)
                    .FirstOrDefaultAsync(x => x.AdjumentId == AdjustId)
                    ?? throw new Exception("Payment not found");
                // 1️⃣ Load old sub items (CRITICAL)
                var oldSubItems = await _unitOfWork.AdvaceadjustmentSubs
                    .GetQueryable()
                    .Where(x => x.AdjumentId == AdjustId)
                    .ToListAsync();

                if (payment == null) return false;

                var changes = new StringBuilder();


                // 2️⃣ Revert PO / Invoice balances
                string? Type = "";
                
                if(payment.IsIncome)
                {
                    Type = payment?.Income?.IncomeName;
                }
                else
                {
                    Type=payment?.Expense?.ExpenseName;

                }
                await UpdatePendingBalanceAsync(Type??"", new List<AdvaceadjustmentSubVM?>(), oldSubItems);

                var ft = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.AdvanceRefId == AdjustId);

                int? startFtId = ft?.FundTransId;
                int? bankId = payment?.BankId;
                int? ledegid = ft?.LedgerId;
               

                if (ft != null)
                {
                    await _unitOfWork.fundTranss.DeleteAsync(ft);
                }
                else
                {
                    return false;
                }
                await _unitOfWork.Advaceadjustments.DeleteAsync(payment);
                await _unitOfWork.SaveAsync();

                // 🔥 Recalculate remaining balances
                if (startFtId.HasValue)
                    await RecalculateVendorAdvanceAsync(startFtId.Value, ledegid??0, payment.IsIncome);

                //await UpdateBankBalanceDeleteAsync(bankId ?? 0);

                await trx.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "AdvanceAdjustment List",
                    action: $"Deleted Advance Adjustment: {payment.AdvaceadjustmentNo}",
                    additionalInfo: $"AdvaceAdjustment Id: {payment.AdjumentId}\n{changes}"
                );
                return true;

            }
            catch (Exception ex)
            {
                await trx.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete AdvaceAdjustment: {AdjustId}");
                throw;
            }

        }

        public async Task UpdatePendingBalanceAsync(string Type, List<AdvaceadjustmentSubVM?> newItems, List<AdvaceadjustmentSub?> oldItems)
        {
            try
            {
                Type = Type?.ToUpper()?.Trim() ?? "";

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

                switch (Type)
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

                                await _unitOfWork.PurchaseInvoices.UpdateAsync(inv);
                            }

                            break;
                        }
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

                                await _unitOfWork.SubConInvs.UpdateAsync(inv);
                            }

                            break;
                        }

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


        private async Task<Advaceadjustment> SaveHeaderAsync(AdvaceadjustmentVM vm, DateTime now, string user)
        {
            try
            {
                Advaceadjustment entity;

                if (vm.AdjumentId == 0)
                {
                    entity = _mapper.Map<Advaceadjustment>(vm);

                    entity.CreatedBy = user;
                    entity.CreatedDate = now;

                    entity.AdvaceadjustmentSubs = vm.AdvaceadjustmentSubVM
                        .Select(sub => _mapper.Map<AdvaceadjustmentSub>(sub))
                        .ToList();

                    await _unitOfWork.Advaceadjustments.CreateAsync(entity);
                }
                else
                {
                    entity = await _unitOfWork.Advaceadjustments
                         .GetQueryable()
                         .Include(s => s.AdvaceadjustmentSubs)
                         .FirstOrDefaultAsync(x => x.AdjumentId == vm.AdjumentId)
                         ?? throw new Exception("Advaceadjustment not found");

                    entity.ModifiedBy = user;
                    entity.ModifiedDate = now;

                    _mapper.Map(vm, entity);
                    await _unitOfWork.Advaceadjustments.UpdateAsync(entity);
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
        private async Task<FundTrans> SaveFundTransAsync(Advaceadjustment entity, AdvaceadjustmentVM vm)
        {
            try
            {
                FundTrans? ft;
                string LedgerType = "";

                if (entity.IsIncome)
                {
                    ft = await _unitOfWork.fundTranss
                        .GetQueryable()
                        .FirstOrDefaultAsync(x =>
                            x.AdvanceRefId == entity.AdjumentId &&
                            x.IncomeCode == entity.IncomeCode && x.LedgerId == entity.PayToRefCode);
                    LedgerType = "CUSTOMER";
                }
                else
                {
                    ft = await _unitOfWork.fundTranss
                        .GetQueryable()
                        .FirstOrDefaultAsync(x =>
                            x.AdvanceRefId == entity.AdjumentId &&
                            x.ExpenseCode == entity.ExpenseCode &&  x.LedgerId == entity.PayToRefCode);
                    LedgerType = "VENDOR";
                }


                bool isNew = false;

                if (ft == null)
                {
                    ft = new FundTrans
                    {
                        AdvanceRefId = entity.AdjumentId
                    };
                    isNew = true;
                }

                vm.TransactionType = TransTypes.BillAdjust;
                // 🔹 Amount sign
                decimal paidOrReceived =
                    vm.PaymentTypeName == "Opening Balance"
                        ? -vm.Amount
                        : -vm.Amount;

                decimal advance = 0;
                if (vm.ExpenseName?.ToUpper() == "PURCHASE ORDER ADVANCE")
                {
                    vm.PaymentTypeName = "Purchase Order Advance";

                }
                if (vm.ExpenseName?.ToUpper() == "PURCHASE")
                {
                    vm.PaymentTypeName = "Purchase Invoice Adjust";

                }
                if (vm.ExpenseName?.ToUpper() == "SUBCONTRACT")
                {
                    vm.PaymentTypeName = "SUBCONTRACT Invoice Adjust";

                }
                if (vm.IncomeName?.ToUpper() == "SALES")
                {
                    vm.PaymentTypeName = "SALES Invoice Adjust";
                }
                if (vm.IncomeName?.ToUpper() == "LABOUR WORK")
                {
                    vm.PaymentTypeName = "LABOUR Invoice Adjust";
                }

                bool isAdjustType =
                    vm.PaymentTypeName == "Purchase Invoice Adjust" ||
                    vm.PaymentTypeName == "SALES Invoice Adjust" ||
                    vm.PaymentTypeName == "SUBCONTRACT Invoice Adjust" ||
                    vm.PaymentTypeName == "LABOUR Invoice Adjust";

                advance = isAdjustType ? -vm.Amount : 0;

                // 🔹 Update fields
                ft.TransNo = entity.AdvaceadjustmentNo;
                ft.TransDate = entity.AdjumentDate;
                ft.TransType = vm.PaymentTypeName;
                ft.TrasactionMode = entity.PaymentMode;
                ft.LedgerId = entity.PayToRefCode;
                ft.LedgerName = entity.PayToName;
                ft.ExpenseCode = entity.ExpenseCode;
                ft.IncomeCode = entity.IncomeCode;
                ft.LedgerType = LedgerType;
                ft.BankId = null;
                ft.BillAmount = entity.Amount;
                ft.PaidOrReceived = paidOrReceived;
                ft.AdvanceBalance = advance; 
                ft.CreatedBy = entity.CreatedBy;
                ft.ModifiedBy = entity.ModifiedBy;
                ft.ModifiedDate = entity.ModifiedDate;

                if (isNew)
                    await _unitOfWork.fundTranss.CreateAsync(ft);
                else
                    await _unitOfWork.fundTranss.UpdateAsync(ft);

                await _unitOfWork.SaveAsync();

             
                    // 🔥 REBUILD RUNNING BALANCE
                    await RecalculateVendorAdvanceAsync( ft.FundTransId, ft.LedgerId??0 ,entity.IsIncome);

                    return ft;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SaveFundTransAsync");
                throw;
            }


        }
       

        private async Task RecalculateVendorAdvanceAsync(int startFtId, int ledgerId,bool IsIncome)
        {
            try
            {
                // 1 Get previous running balance
                decimal runningAdvance = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .Where(x =>
                        x.LedgerId == ledgerId &&
                        x.FundTransId < startFtId &&
                        (IsIncome ? x.IncomeCode >0 : x.ExpenseCode >0))
                    .OrderByDescending(x => x.FundTransId)
                    .Select(x => x.AdvanceBalance)
                    .FirstOrDefaultAsync();

                // 2Get transactions from start point
                var trans = await _unitOfWork.fundTranss
                    .GetQueryable()
                    .Where(x =>
                        x.LedgerId == ledgerId &&
                        x.FundTransId >= startFtId &&
                        (IsIncome ? x.IncomeCode > 0 : x.ExpenseCode > 0))
                    .OrderBy(x => x.FundTransId)
                    .ToListAsync();

                // Recalculate only advance
                foreach (var ft in trans)
                {
                    string type = ft.TransType.ToUpper();

                    if (type == "PAY ADVANCE" || type == "PURCHASE ORDER ADVANCE")
                    {
                        runningAdvance += Math.Abs(ft.BillAmount);
                    }
                    else if (type == "PURCHASE INVOICE ADJUST" || type == "SALES INVOICE ADJUST" || type== "LABOUR INVOICE ADJUST" || type== "SUBCONTRACT INVOICE ADJUST")
                    {
                        decimal invoiceAmount = Math.Abs(ft.BillAmount);
                        decimal used = Math.Min(runningAdvance, invoiceAmount);
                        runningAdvance -= used;
                    }

                    ft.AdvanceBalance = runningAdvance;
                    await _unitOfWork.fundTranss.UpdateAsync(ft);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RecalculateVendorAdvanceAsync");
                throw;
            }
        }


        public async Task<(List<AdvaceadjustmentVM> advaceadjustments, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.Advaceadjustments
                         .GetQueryable()
                         .AsSplitQuery()
                         .Include(e => e.Expense)
                         .Include(e => e.Income)
                         .Include(e => e.Banks)
                         .Include(e => e.AdvaceadjustmentSubs)
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
                        query = AdvanceadjustFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                    }
                }
            }

            //if (!string.IsNullOrEmpty(status))
            //{

            //    var completedIds = await _unitOfWork.EnquiryPurchaseVendorSubs
            //        .GetQueryable()
            //        .Where(s => s.ItemStatus.Contains("PO"))
            //        .Select(s => s.EnquiryId)
            //        .Distinct()
            //        .ToListAsync();

            //    if (status == "Completed")
            //    {
            //        query = query.Where(x => completedIds.Contains(x.EnquiryId));
            //    }
            //    else if (status == "Pending")
            //    {
            //        query = query.Where(x =>
            //            !completedIds.Contains(x.EnquiryId) &&
            //            x.Cancel == false);
            //    }
            //    else if (status == "Cancelled")
            //    {
            //        query = query.Where(x => x.Cancel == true);
            //    }
            //    else if (status == "Short Close")
            //    {
            //        query = query.Where(x => x.EnquiryShortClose == true);
            //    }
            //}

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.AdjumentId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var vmList = _mapper.Map<List<AdvaceadjustmentVM>>(list);

            //foreach (var vm in vmList)
            //{
            //    var pendingExists = await _unitOfWork.EnquiryPurchaseVendorSubs
            //    .GetQueryable()
            //    .AnyAsync(s => s.EnquiryId == vm.EnquiryId && (s.ItemStatus == "ShortClose" || s.ItemStatus == "PO"));

            //    vm.ItemStatus = pendingExists ? "Completed" : "Pending";

            //    bool pendingExist = await _unitOfWork.EnquiryPurchaseVendors
            //                      .GetQueryable()
            //                      .AnyAsync(s => s.EnquiryId == vm.EnquiryId && (s.EnquiryShortClose == true));

            //    vm.EnquiryShortClose = pendingExist;
            //}
            return (vmList, total);
        }

        public static class AdvanceadjustFilterBuilder
        {
            public static IQueryable<Advaceadjustment> ApplyFilter(
                IQueryable<Advaceadjustment> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "AdvaceadjustmentNo":
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

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.AdvaceadjustmentNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }


                    //case "VendorName":
                    //    return query.Where(x => x.ven.VendorName.Contains(val));


                    //case "ItemName":
                    //    return query.Where(x => x.EnquiryPurchaseSub
                    //        .Any(s => s.Item.ItemName.Contains(val)));

                    //case "ItemCode":
                    //    return query.Where(x => x.EnquiryPurchaseSub
                    //        .Any(s => s.Item.ItemCode.Contains(val)));

                    case "FromDate":
                        return query.Where(x => x.AdjumentDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.AdjumentDate <= DateTime.Parse(value.ToString()));


                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<Advaceadjustment> ApplyStatusFilter(
                IQueryable<Advaceadjustment> query, string status)
            {
                return status switch
                {
                    //"Completed" => query.Where(x => x.pay == true),
                    //"Pending" => query.Where(x => x.EnquiryTally == false && x.Cancel == false),
                    //"Cancelled" => query.Where(x => x.Cancel == true),
                    _ => query
                };
            }
        }


    }
}
