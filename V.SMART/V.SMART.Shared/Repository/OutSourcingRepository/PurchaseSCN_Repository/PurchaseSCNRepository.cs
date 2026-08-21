using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseSCN_Repository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchaseSCN_Repository
{
    public class PurchaseSCNRepository : Repository<PurchaseSCN>, IPurchaseSCNRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public PurchaseSCNRepository(ApplicationDbContext db, ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<string> GetLastSCNNoAsync(string suffix)
        {
            var lastNumberStr = await _db.PurchaseSCN
                .FromSqlRaw(@"
                SELECT TOP 1 * 
                FROM PurchaseSCN WITH (UPDLOCK, ROWLOCK) 
                WHERE suffix = {0} 
                ORDER BY TRY_CAST(SCNNo AS INT) DESC", suffix)
                .Select(d => d.SCNNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            // Ensure null safety before parsing
            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
            return null;
        }

        public async Task<IEnumerable<PurchaseSCN>> GetAllWithItemsAsync()
        {
            try
            {
                return await _db.PurchaseSCN
                        .Include(d => d.Vendor)
                        .Include(d => d.PurchaseSCNSubs)
                        .ToListAsync();
                return null;
            }
            catch (Exception Ex)
            {

                throw;
            }

        }

        public async Task<List<PurchaseSCNVM>> SearchPurchaseSCNAsync(PurchaseSCNVM search)
        {
            try
            {
                var query = _db.PurchaseSCN
                            .AsNoTracking()
                            .Include(s => s.StoreIssue)
                            .Include(s => s.StoreAdd)
                            .Include(s => s.Vendor)
                            .Include(s => s.PurchaseSCNSubs).ThenInclude(s=>s.Item)
                            
                            .AsQueryable();

                if (!string.IsNullOrWhiteSpace(search.SCNNo))
                {
                    var scnNo = search.SCNNo.Trim();
                    query = query.Where(i => EF.Functions.Like(i.SCNNo.Trim(), $"%{scnNo}%"));
                }

                if (!string.IsNullOrWhiteSpace(search.VendorName))
                {
                    var vendor = search.VendorName.Trim();
                    query = query.Where(i => EF.Functions.Like(i.Vendor.VendorName.Trim(), $"%{vendor}%"));
                }

                if (!string.IsNullOrWhiteSpace(search.AddStoreName))
                {
                    var addStore = search.AddStoreName.Trim();
                    query = query.Where(i => EF.Functions.Like(i.StoreAdd.StoreName.Trim(), $"%{addStore}%"));
                }

                if (!string.IsNullOrWhiteSpace(search.IssueStoreName))
                {
                    var issueStore = search.IssueStoreName.Trim();
                    query = query.Where(i => EF.Functions.Like(i.StoreIssue.StoreName.Trim(), $"%{issueStore}%"));
                }

                var scns = await query
                    .OrderBy(s => s.SCNNo)
                    .Take(200)
                    .Select(scn => new PurchaseSCNVM
                    {
                        SCNId = scn.SCNId,
                        SCNNo = scn.SCNNo,
                        SCNDate = scn.SCNDate,
                        IssueStoreName = scn.StoreIssue.StoreName,
                        AddStoreName = scn.StoreAdd.StoreName,
                        VendorName = scn.Vendor.VendorName,
                        VendorGstNo = scn.Vendor.GSTNo,
                        VendorAddress = scn.Vendor.VendorAddress,
                        VendorContactNo = scn.Vendor.ContactNo
                    })
                    .ToListAsync();
                var scnIds = scns.Select(s => s.SCNId).ToList();

                var subs = await _db.PurchaseSCNSub
                    .AsNoTracking()
                    .Where(sub => scnIds.Contains(sub.SCNId))
                    .Select(sub => new PurchaseSCNSubVM
                    {
                        SCNId = sub.SCNId,
                        ItemCode = sub.Item.ItemCode,
                        ItemName = sub.Item.ItemName,
                        Specification = sub.Item.Specification,
                        MeasureUnit = sub.Item.MeasureUnit,
                        HSNCode = sub.Item.HSNCode,
                        CategoryName = sub.Item.Category.CategoryName,
                       
                    })
                    .ToListAsync();
                foreach (var scn in scns)
                {
                    scn.PurchaseSCNSubVMs = subs
                        .Where(sub => sub.SCNId == scn.SCNId)
                        .ToList();
                }


                return scns;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "SearchPurchaseSCNAsync");
                return new List<PurchaseSCNVM>();
            }
        }




    }
}
