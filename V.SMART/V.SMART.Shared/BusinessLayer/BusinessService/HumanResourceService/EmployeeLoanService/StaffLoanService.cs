using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IEmployeeLoanService;
using V.SMART.Shared.Data.HumanResource.StaffLoan;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.StaffLoanViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService.EmployeeLoanService
{
    public class StaffLoanService : IStaffLoanService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public StaffLoanService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        // 🔹 Quotation operations

        public async Task<bool> IsLoanTransactionsMatchedAsync(int loanId, StaffLoanVM loanVMs)
        {
            try
            {
                var loanIds = await _unitOfWork.StaffLoans
                    .GetQueryable()
                    .Where(x => x.LoanId == loanId)
                    //.Select(x => x.QuoteSubId)
                    .ToListAsync();

                bool hasLoan = loanIds.Any();

                bool hasTransactions = false;

                if (hasLoan)
                {
                    // Check Manufacturing Quotation references
                    hasTransactions = await _unitOfWork.Salaries
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.LoanId.HasValue);
                }

                //Quantity mismatch check
                bool amountMismatch = false;

                var list = loanVMs;
                //if (list != null && list.Any())
                //{
                //    decimal totalAmount = list.Sum(x => x.Amount ?? 0);
                //    decimal totalBalQty = list.Sum(x => x.BalanceAmount ?? 0);

                //    amountMismatch = totalAmount != totalBalQty;
                //}

                // If either transactions exist OR quantity mismatch → return true
                return hasTransactions || amountMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for LoanId: {loanId}");
                throw new InvalidOperationException("Failed to verify enquiry transactions.", ex);
            }
        }

        public async Task<string> GetLoanNumberAsync()
        {
            try
            {
                var lastLoan = await _unitOfWork.StaffLoans
                    .GetQueryable()
                    .OrderByDescending(q => q.LoanId) // safest way
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (lastLoan != null &&
                    !string.IsNullOrWhiteSpace(lastLoan.LoanNo) &&
                    int.TryParse(lastLoan.LoanNo, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }

                return nextNumber.ToString();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error generating loan number.");
                throw new InvalidOperationException("Failed to generate loan number.");
            }
        }

        public async Task<(List<StaffLoan> staffLoanVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.StaffLoans
                    .GetQueryable()
                    .Include(x => x.StaffId)
                   
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = StaffLoanFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.LoanId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<StaffLoan>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (MfgQuote)");
                throw new InvalidOperationException("Failed to load manufacturing quotation list.", ex);
            }
        }

        public static class StaffLoanFilterBuilder
        {
            public static IQueryable<StaffLoan> ApplyFilter(
                IQueryable<StaffLoan> query,
                string field,
                object value)
            {
                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

                    switch (field)
                    {
                        case "LoanNo":
                            {
                                string part1 = val;
                                //string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    //part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.LoanNo.StartsWith(part1))
                                    
                                );
                            }

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.DateOfIssue >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.DateOfIssue <= toDate.Date.AddDays(1).AddTicks(-1));
                            return query;

                        case "Status":
                            return ApplyStatusFilter(query, val);
                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }

            private static IQueryable<StaffLoan> ApplyStatusFilter(
                IQueryable<StaffLoan> query,
                string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.IsFinished),

                        "Pending" =>
                            query.Where(x =>
                                !x.IsFinished),
                        _ => query
                    };
                }
                catch
                {
                    return query;
                }
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteStaffLoanAsync(int loanId)
        {
            try
            {
                var staffloan = await _unitOfWork.StaffLoans
                    .GetQueryable()
                    .FirstOrDefaultAsync(a => a.LoanId == loanId);

                if (staffloan == null)
                    return (false, "StaffLoan record not found.");

                // ✅ Check salary transaction against this LoanId
                bool salaryExists = await _unitOfWork.Salaries
                    .GetQueryable()
                    .AnyAsync(s => s.LoanId == loanId);


                if (salaryExists)
                {
                    return (false, "Salary already issued for this staff and month. StaffLoan cannot be deleted.");
                }

                return (true, "StaffLoan can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteIdAsync for Id: {loanId}");
                return (false, "Error checking delete eligibility.");
            }
        }

        public async Task<Staff?> GetStaffByStaffIDAsync(int staffId)
        {
            return await _unitOfWork.Staffs
                .GetQueryable()
                .FirstOrDefaultAsync(s => s.StaffID == staffId);
        }

        //public async Task<StaffLoanVM?> GetStaffLoanByLoanIdAsync(int loanId)
        //{
        //    try
        //    {
        //        var entity = await _unitOfWork.StaffLoans.GetQueryable()
        //            .FirstOrDefaultAsync(q => q.LoanId == loanId);

        //        return _mapper.Map<StaffLoanVM?>(entity);
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"GetStaffLoanAsync({loanId})");
        //        return null;
        //    }
        //}
        //public async Task<StaffLoanVM?> GetStaffLoanByLoanIdAsync(int loanId)
        //{
        //    try
        //    {
        //        var entity = await _unitOfWork.StaffLoans.GetQueryable()
        //            .FirstOrDefaultAsync(q => q.LoanId == loanId);

        //        if (entity == null) return null;

        //        var vm = _mapper.Map<StaffLoanVM>(entity);

        //        // Manually populate LoanRows from the entity properties
        //        // Example: assuming you have Amount, BalanceAmount, NumberOfMonthsRecoveries stored in StaffLoan itself
        //        vm.LoanRows = new List<StaffLoanVM>
        //        {
        //            new StaffLoanVM
        //            {
        //                Amount = entity.Amount, // if exists
        //                BalanceAmount = entity.BalanceAmount, // if exists
        //                NumberOfMonthsRecoveries = entity.NumberOfMonthsRecoveries // if exists
        //            }
        //        };

        //        return vm;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"GetStaffLoanAsync({loanId})");
        //        return null;
        //    }
        //}
        public async Task<StaffLoanVM> GetStaffLoanByLoanIdAsync(int loanId)
        {
            var entity = await _unitOfWork.StaffLoans.GetAsync(loanId);
            if (entity == null) return null;

            var vm = _mapper.Map<StaffLoanVM>(entity);

            // Load loan items
            vm.LoanRows = await _unitOfWork.StaffLoans.GetQueryable()
                .Where(x => x.LoanId == loanId)
                .Select(x => new StaffLoanVM
                {
                    LoanId = x.LoanId,
                    LoanNo = x.LoanNo,
                    Amount = x.Amount,
                    NumberOfMonthsRecoveries = x.NumberOfMonthsRecoveries,
                    BalanceAmount = x.BalanceAmount,
                    //SlNo = x.SlNo,
                    IsEditable = false,
                    RowType = "LoanItem"
                }).ToListAsync();

            // Load loan recoveries
            vm.LoanRecoveries = await _unitOfWork.StaffLoans.GetQueryable()
                .Where(r => r.LoanId == loanId)
                .Select(r => new StaffLoanVM
                {
                    LoanId = r.LoanId,
                    LoanNo = r.LoanNo,
                    LoanDedId = r.LoanId,
                    SalDate = r.DateOfIssue,
                    LoanAmtDed = r.Amount,
                    RowType = "Recovery"
                }).ToListAsync();

            return vm;
        }

        //public async Task<List<StaffLoanVM>> GetRecoveriesByLoanIdAsync(int loanId)
        //{
        //    return await _unitOfWork.StaffLoans.GetQueryable()
        //        .Where(x => x.LoanId == loanId)
        //        .Select(x => new StaffLoanVM
        //        {
        //            LoanDedId = x.LoanId,
        //            LoanId = x.LoanId,
        //            SalDate = x.DateOfIssue,
        //            LoanAmtDed = x.Amount
        //        }).ToListAsync();
        //}

        public async Task<List<StaffLoanVM>> GetRecoveriesByLoanIdAsync(int loanId, int staffId)
        {
            var recoveries = await (from s in _unitOfWork.Salaries.GetQueryable()
                                    join l in _unitOfWork.StaffLoans.GetQueryable()
                                        on s.LoanId equals l.LoanId
                                    where s.LoanId == loanId
                                       && s.StaffId == staffId
                                       && s.LoanAmtDed > 0
                                    orderby s.SalDate
                                    select new StaffLoanVM
                                    {
                                        LoanId = s.LoanId,
                                        LoanNo = l.LoanNo,   // Correct Loan Number
                                        StaffId = s.StaffId.Value,
                                        SalDate = s.SalDate,
                                        LoanAmtDed = s.LoanAmtDed
                                    }).ToListAsync();

            return recoveries;
        }


        public async Task<IEnumerable<StaffLoanVM>> GetAllLoanAsync()
        {
            try
            {
                var loans = await _unitOfWork.StaffLoans.GetQueryable()
                    .AsNoTracking()
                    .ToListAsync();

                var loanIds = loans.Select(q => q.LoanId).ToList();

                var subItems = await _unitOfWork.StaffLoans.GetQueryable()
                    .Where(s => loanIds.Contains(s.LoanId))
                    .AsNoTracking()
                    .ToListAsync();

                var loanVMs = _mapper.Map<IEnumerable<StaffLoanVM>>(loans);
                return loanVMs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllLoanAsync");
                return Enumerable.Empty<StaffLoanVM>();
            }
        }

        //public async Task<StaffLoanVM?> GetStaffLoanByStaffIdAsync(int staffId)
        //{
        //    var entity = await _unitOfWork.StaffLoans.GetQueryable()
        //        .Where(x => x.StaffId == staffId)
        //        .FirstOrDefaultAsync();

        //    if (entity == null) return null;

        //    var vm = _mapper.Map<StaffLoanVM>(entity);

        //    // Populate LoanRows
        //    vm.LoanRows = new List<StaffLoanVM>
        //    {
        //        new StaffLoanVM
        //        {
        //            Amount = entity.Amount,
        //            BalanceAmount = entity.BalanceAmount,
        //            NumberOfMonthsRecoveries = entity.NumberOfMonthsRecoveries
        //        }
        //    };

        //    // Add SlNo
        //    for (int i = 0; i < vm.LoanRows.Count; i++)
        //        vm.LoanRows[i].SlNo = i + 1;

        //    return vm;
        //}

        public async Task<StaffLoanVM?> GetStaffLoanByStaffIdAsync(int staffId)
        {
            var entity = await _unitOfWork.StaffLoans.GetQueryable()
                .Where(x => x.StaffId == staffId && x.BalanceAmount > 0)
                .FirstOrDefaultAsync();

            if (entity == null)
                return null;

            var vm = _mapper.Map<StaffLoanVM>(entity);

            // Calculate EMI
            decimal emi = 0;

            if (entity.NumberOfMonthsRecoveries > 0)
            {
                emi = entity.Amount / entity.NumberOfMonthsRecoveries;
            }

            // Populate LoanRows
            vm.LoanRows = new List<StaffLoanVM>
            {
                new StaffLoanVM
                {
                    SlNo = 1,
                    LoanId = entity.LoanId,
                    Amount = entity.Amount,
                    BalanceAmount = entity.BalanceAmount,
                    NumberOfMonthsRecoveries = entity.NumberOfMonthsRecoveries,
                    EMIAmount = emi
                }
            };

            return vm;
        }

        public async Task<StaffLoan?> GetLastLoanAsync(int staffId)
        {
            try
            {
                return await _unitOfWork.StaffLoans.GetLatestAsync(
                    q => q.StaffId == staffId,
                    q => q.LoanId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastLoanAsync for StaffId: {staffId}");
                throw new InvalidOperationException("Failed to retrieve last staffloan. Please try again.");
            }
        }

        public async Task<StaffLoanVM> UpsertLoanAsync(StaffLoanVM loanVM)
        {
            if (loanVM == null)
                throw new ArgumentNullException(nameof(loanVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                StaffLoan entity;

                // ✅ STEP 1: Calculate totals FIRST
                if (loanVM.LoanRows == null || !loanVM.LoanRows.Any())
                    throw new Exception("Loan details are required.");

                loanVM.Amount = loanVM.LoanRows.Sum(x => x.Amount);
                loanVM.NumberOfMonthsRecoveries = loanVM.LoanRows.Sum(x => x.NumberOfMonthsRecoveries);

                if (loanVM.LoanId == 0)
                {
                    entity = _mapper.Map<StaffLoan>(loanVM);

                    var nextNumber = await _unitOfWork.StaffLoans.GetLastLoanNoAsync();
                    entity.LoanNo = nextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Status ??= "Active";

                    entity.BalanceAmount = entity.Amount; // Now it won't be 0

                    await _unitOfWork.StaffLoans.CreateAsync(entity);
                }
                else
                {
                    entity = await _unitOfWork.StaffLoans
                        .GetQueryable()
                        .FirstOrDefaultAsync(x => x.LoanId == loanVM.LoanId);

                    if (entity == null)
                    {
                        entity = _mapper.Map<StaffLoan>(loanVM);
                        var nextNumber = await _unitOfWork.StaffLoans.GetLastLoanNoAsync();
                        entity.LoanNo = nextNumber;
                        entity.CreatedBy = currentUser;
                        entity.CreatedDate = now;
                        entity.Status ??= "Active";
                        entity.BalanceAmount = entity.Amount;

                        await _unitOfWork.StaffLoans.CreateAsync(entity);
                    }
                    else
                    {
                        // Only map once
                        _mapper.Map(loanVM, entity);
                        entity.ModifiedBy = currentUser;
                        entity.ModifiedDate = now;
                    }
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return _mapper.Map<StaffLoanVM>(entity);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Get property changes for logging
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

        // Logging
        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _logs.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<bool> HasAnySalaryTransactionMadeAsync(int loanId)
        {
            try
            {
                var loanIds = await _unitOfWork.StaffLoans
                    .GetQueryable()
                    .Where(s => s.LoanId == loanId)
                    .ToListAsync();

                if (!loanIds.Any())
                    return false;

                return await _unitOfWork.Salaries
                    .GetQueryable()
                    .AnyAsync(qs => qs.LoanId.HasValue);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnySalaryTransactionMadeAsync for LoanId: {loanId}");
                throw;
            }
        }

        //public async Task DeleteAndResequenceAsync(MfgQuoteSubVM subitem, MfgQuoteVM quote)
        //{
        //    using var transaction = await _unitOfWork.BeginTransactionAsync();
        //    var changes = new StringBuilder();

        //    try
        //    {
        //        if (subitem.QuoteSubId > 0)
        //        {
        //            var entity = await _unitOfWork.MfgQuoteSubs.GetAsync(subitem.QuoteSubId);
        //            if (entity == null)
        //                throw new InvalidOperationException("Sub item not found.");

        //            //if (entity.RefEnqSubId > 0)
        //            //{
        //            //    await AdjustEnquiryBalanceAsync(subitem.RefEnqSubId, entity.Qty, 0, "Quote Deletion");
        //            //}
        //            //else
        //            //{
        //            //    await UpdateCostCenterAsync(subitem.CostId, false, changes);
        //            //}

        //            await _unitOfWork.MfgQuoteSubs.DeleteAsync(entity);
        //            await _unitOfWork.SaveAsync();

        //            await _logs.LogUserAction(
        //                await _currentUserService.GetUsernameAsync(),
        //                _currentUserService.MachineName,
        //                _currentUserService.IpAddress,
        //                "Mfg Quote",
        //                $"Deleted Item: {subitem.ItemCode}",
        //                $"Quotation No: {quote?.QuoteNo}"
        //            );
        //        }
        //        else
        //        {
        //            quote.MfgQuoteSubVM.Remove(subitem);
        //            return;
        //        }

        //        var remaining = await _unitOfWork.MfgQuoteSubs
        //            .GetQueryable()
        //            .Where(x => x.QuoteId == quote.QuoteId)
        //            .OrderBy(x => x.SlNo)
        //            .ToListAsync();

        //        int slno = 1;
        //        foreach (var item in remaining)
        //        {
        //            item.SlNo = slno++;
        //        }

        //        await _unitOfWork.SaveAsync();

        //        // Update Quotation Tally Status
        //        await UpdateQuotationTallyStatusAsync(loan.LoanId);

        //        await transaction.CommitAsync();
        //    }
        //    catch
        //    {
        //        await transaction.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task<bool> DeleteStaffLoanByLoanIdAsync(int loanId)
        {
            //using var transaction = await _unitOfWork.BeginTransactionAsync();
            //try
            //{
            //    // Get the quotation with its sub-items
            //    var loan = await _unitOfWork.StaffLoans
            //        .GetQueryable()
            //        .FirstOrDefaultAsync(e => e.LoanId == LoanId);

            //    if (loan == null)
            //    {
            //        return false;
            //    }

            //    var changes = new StringBuilder();

            //    //foreach (var sub in quotation.MfgQuoteSub)
            //    //{
            //    //    if (sub.RefEnqSubId > 0)
            //    //    {
            //    //        await AdjustEnquiryBalanceAsync(sub.RefEnqSubId, sub.Qty, 0, "Quote Deletion");
            //    //    }
            //    //    else
            //    //    {
            //    //        await UpdateCostCenterAsync(sub.CostId, false, changes);
            //    //    }
            //    //}

            //    // Delete the quotation
            //    await _unitOfWork.StaffLoans.DeleteAsync(loan);

            //    await _unitOfWork.SaveAsync();
            //    await transaction.CommitAsync();

            //    // Log the user action
            //    await _logs.LogUserAction(
            //        UserName: await _currentUserService.GetUsernameAsync(),
            //        Machine: _currentUserService.MachineName,
            //        IP_Address: _currentUserService.IpAddress,
            //        screen: "StaffLoan List",
            //        action: $"Deleted StaffLoan: {loan.LoanNo}",
            //        additionalInfo: $"StaffLoan Id: {loan.LoanId}\n{changes}"
            //    );

            //    return true;
            //}
            await using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var staffloan = await _unitOfWork.StaffLoans
                    .GetQueryable()
                    .FirstOrDefaultAsync(a => a.LoanId == loanId);

                if (staffloan == null)
                    return false;

                // 🔴 Check Salary issued
                var salaryExists = await _unitOfWork.Salaries
                    .GetQueryable()
                    .AnyAsync(s =>
                        s.StaffId == staffloan.StaffId &&
                        s.LoanId == staffloan.LoanId &&
                        s.LoanAmtDed == staffloan.Amount);

                if (salaryExists)
                    throw new Exception("Salary already issued. Cannot delete staffloan.");

                await _unitOfWork.StaffLoans.DeleteAsync(staffloan);
                await _unitOfWork.SaveAsync();
                await tx.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "StaffLoan",
                    action: "Delete",
                    additionalInfo: $"Deleted Loan Id: {staffloan.LoanId}"
                );

                return true;
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete StaffLoan: {loanId}");
                throw;
            }
        }

        public async Task<List<StaffLoan>> GetLoanByLoanIdAsync(int loanId)
        {
            try
            {
                var subs = await _unitOfWork.StaffLoans
                    .GetQueryable()
                    .Where(s => s.LoanId == loanId)
                    .OrderBy(s => s.LoanId)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching StaffLoan items for LoanId: {loanId}");
                throw new InvalidOperationException("Failed to retrieve loan sub-items. Please try again.");
            }
        }
        
        //public async Task<string> GetLoanNumberAsync()
        //{
        //    try
        //    {
        //        var lastLoan = await _unitOfWork.StaffLoans
        //            .GetQueryable()
        //            .OrderByDescending(q => q.LoanNo)
        //            .FirstOrDefaultAsync();

        //        int nextNumber = 1;
        //        //if (lastLoan != null)
        //        //{
        //        //    var parts = lastLoan.LoanNo;
        //        //    if (int.TryParse(parts[0], out int lastNumber))
        //        //        nextNumber = lastNumber + 1;
        //        //}

        //        return $"{nextNumber}";
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"Error generating staffloan number");
        //        throw new InvalidOperationException("Failed to generate staffloan number.");
        //    }
        //}
        
        //public async Task<StaffLoanVM?> GetLoanDetailByLoanIdAsync(int loanId)
        //{
        //    try
        //    {
        //        return await _unitOfWork.StaffLoans
        //            .GetQueryable()
        //            .Where(q => q.LoanId == loanId)
        //            .Select(q => new StaffLoanVM
        //            {
        //                Amount = q.Amount,
        //                BalanceAmount = q.BalanceAmount
        //            })
        //            .FirstOrDefaultAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"Error fetching loan sub item detail for LoanId: {loanId}");
        //        throw new InvalidOperationException("Failed to retrieve loan sub-item details.");
        //    }
        //}
    }
}
