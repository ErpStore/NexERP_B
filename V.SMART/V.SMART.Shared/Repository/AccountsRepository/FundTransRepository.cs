using DocumentFormat.OpenXml.Spreadsheet;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository.IAccountRepository.IPaymentsRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.AccountsViewModel;

namespace V.SMART.Shared.Repository.AccountsRepository
{
    public class FundTransRepository : Repository<FundTrans>, IFundTransRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;

        public FundTransRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = currentUserService;
        }
        public async Task<List<string>> LoadDistinctTransactionTypes()
        {
            return await _db.FundTrans
                .AsNoTracking()
                .Select(x => x.TransType)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<FundTransResult> GetWithFiltersAsync(FundTransFilter filter)
        {
            var query = _db.FundTrans.AsNoTracking().Include(x => x.Banks).AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.TransactionType))
                query = query.Where(x => x.TransType == filter.TransactionType);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
                query = query.Where(x => x.LedgerName!.Contains(filter.SearchTerm) || x.ChequeNo!.Contains(filter.SearchTerm));

            if (filter.StartDate.HasValue)
                query = query.Where(x => x.TransDate >= filter.StartDate);

            if (filter.EndDate.HasValue)
                query = query.Where(x => x.TransDate <= filter.EndDate);

            if (!string.IsNullOrEmpty(filter.TransactionMode))
                query = query.Where(x => x.TrasactionMode == filter.TransactionMode);

            if (!string.IsNullOrEmpty(filter.LedgerType))
                query = query.Where(x => x.LedgerType == filter.LedgerType);

            if (filter.MinAmount.HasValue)
                query = query.Where(x => x.PaidOrReceived >= filter.MinAmount);

            if (filter.MaxAmount.HasValue)
                query = query.Where(x => x.PaidOrReceived <= filter.MaxAmount);

            // Calculate totals BEFORE paging
            decimal totalCredits = await query.Where(t => t.PaidOrReceived > 0).SumAsync(t => t.PaidOrReceived);
            decimal totalDebits = Math.Abs(await query.Where(t => t.PaidOrReceived < 0 && t.BankId != null).SumAsync(t => t.PaidOrReceived));
            decimal netBalance = totalCredits - totalDebits;

            // Sorting
            query = filter.SortBy switch
            {
                "TransNo" => filter.SortDescending ? query.OrderByDescending(x => x.TransNo) : query.OrderBy(x => x.TransNo),
                "TransDate" => filter.SortDescending ? query.OrderByDescending(x => x.TransDate) : query.OrderBy(x => x.TransDate),
                "LedgerName" => filter.SortDescending ? query.OrderByDescending(x => x.LedgerName) : query.OrderBy(x => x.LedgerName),
                "PaidOrReceived" => filter.SortDescending ? query.OrderByDescending(x => x.PaidOrReceived) : query.OrderBy(x => x.PaidOrReceived),
                _ => query.OrderByDescending(x => x.FundTransId)
            };

            // Paging defaults
            filter.Page = filter.Page <= 0 ? 1 : filter.Page;
            filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            // Fetch paged items
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new FundTransResult
            {
                Items = items,
                TotalCredits = totalCredits,
                TotalDebits = totalDebits,
                NetBalance = netBalance
            };
        }


        //public async Task<IEnumerable<FundTrans>> GetWithFiltersAsync(FundTransFilter filter)
        //{
        //    var query = _db.fundTrans
        //            .Include(x=>x.Banks)
        //            .Include(x => x.Expense)         // assuming Expense navigation exists
        //            .AsQueryable();
        //    if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        //        query = query.Where(x => x.LedgerName!.Contains(filter.SearchTerm) || x.ChequeNo!.Contains(filter.SearchTerm));

        //    if (filter.StartDate.HasValue)
        //        query = query.Where(x => x.TransDate >= filter.StartDate);

        //    if (filter.EndDate.HasValue)
        //        query = query.Where(x => x.TransDate <= filter.EndDate);

        //    if (!string.IsNullOrEmpty(filter.TransactionType))
        //        query = query.Where(x => x.TransType == filter.TransactionType);

        //    if (!string.IsNullOrEmpty(filter.TransactionMode))
        //        query = query.Where(x => x.TrasactionMode == filter.TransactionMode);

        //    if (!string.IsNullOrEmpty(filter.LedgerType))
        //        query = query.Where(x => x.LedgerType == filter.LedgerType);

        //    if (filter.MinAmount.HasValue)
        //        query = query.Where(x => x.PaidOrReceived >= filter.MinAmount);

        //    if (filter.MaxAmount.HasValue)
        //        query = query.Where(x => x.PaidOrReceived <= filter.MaxAmount);

        //    // ✅ Apply dynamic sorting
        //    if (filter.SortBy == "FundTransId")
        //    {
        //        query = filter.SortDescending
        //            ? query.OrderByDescending(x => x.FundTransId)
        //            : query.OrderBy(x => x.FundTransId);
        //    }
        //    else
        //    {
        //        query = filter.SortBy switch
        //        {
        //            "TransNo" => filter.SortDescending ? query.OrderByDescending(x => x.TransNo) : query.OrderBy(x => x.TransNo),
        //            "TransDate" => filter.SortDescending ? query.OrderByDescending(x => x.TransDate) : query.OrderBy(x => x.TransDate),
        //            "LedgerName" => filter.SortDescending ? query.OrderByDescending(x => x.LedgerName) : query.OrderBy(x => x.LedgerName),
        //            "PaidOrReceived" => filter.SortDescending ? query.OrderByDescending(x => x.PaidOrReceived) : query.OrderBy(x => x.PaidOrReceived),
        //            _ => filter.SortDescending ? query.OrderByDescending(x => x.FundTransId) : query.OrderBy(x => x.FundTransId)
        //        };
        //    }

        //    filter.Page = filter.Page <= 0 ? 1 : filter.Page;
        //    filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        //    var result = await query
        //                  .Skip((filter.Page - 1) * filter.PageSize)
        //                  .Take(filter.PageSize)
        //                  .ToListAsync();

        //    // Replace IDs with Names
        //    foreach (var item in result)
        //    {
        //        item.BankName = item.Banks?.BankName;
        //        item.ExpenseName = item.Expense?.ExpenseName; // add ExpenseName property in FundTrans model
        //    }

        //    return result;

        //    // Paging
        //    // return await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
        //}

        public async Task<int> GetTotalCountAsync(FundTransFilter filter)
        {
            var query = _db.FundTrans.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                query = query.Where(x => x.LedgerName!.Contains(filter.SearchTerm));

            if (filter.StartDate.HasValue)
                query = query.Where(x => x.TransDate >= filter.StartDate);

            if (filter.EndDate.HasValue)
                query = query.Where(x => x.TransDate <= filter.EndDate);

            return await query.CountAsync();
        }

    }
}

