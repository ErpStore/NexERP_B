using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseInvoice_Repository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchaseInvoice_Repository
{
    public class PurchaseInvoiceRepository: Repository<PurchaseInvoice>, IPurchaseInvoiceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public PurchaseInvoiceRepository(ApplicationDbContext db, ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

       
        public async Task<IEnumerable<PurchaseInvoice>> GetAllWithItemsAsync()
        {
            try
            {
                return await _db.PurchaseInvoice
                        .Include(d => d.Vendor)
                        .Include(d => d.PurchaseInvoiceSubs)
                        .ToListAsync();

            }
            catch (Exception Ex)
            {

                throw;
            }

        }
        //public async Task<List<PurchaseGRNVM>> SearchPurchaseInvAsync(PurchaseGRNVM search)
        //{
        //    try
        //    {
        //        var query = _db.PurchaseGRN
        //                    .AsNoTracking()
        //                    .Include(s => s.Store)
        //                    .Include(s => s.Vendor)
        //                    .Include(s => s.PurchaseGRNSubs).ThenInclude(s => s.Item)

        //                    .AsQueryable();

        //        if (!string.IsNullOrWhiteSpace(search.GRNNo))
        //        {
        //            var grnNo = search.GRNNo.Trim();
        //            query = query.Where(i => EF.Functions.Like(i.GRNNo.Trim(), $"%{grnNo}%"));
        //        }

        //        if (!string.IsNullOrWhiteSpace(search.VendorName))
        //        {
        //            var vendor = search.VendorName.Trim();
        //            query = query.Where(i => EF.Functions.Like(i.Vendor.VendorName.Trim(), $"%{vendor}%"));
        //        }

        //        if (!string.IsNullOrWhiteSpace(search.StoreName))
        //        {
        //            var addStore = search.StoreName.Trim();
        //            query = query.Where(i => EF.Functions.Like(i.Store.StoreName.Trim(), $"%{addStore}%"));
        //        }



        //        //if (!string.IsNullOrWhiteSpace(search.SearchItemCode))
        //        //{
        //        //    var code = search.SearchItemCode.Trim();

        //        //    query = query.Where(i => i.PurchaseSCNSubs.Any(s => EF.Functions.Like(s.Item.ItemCode, $"%{code}%")));
        //        //}

        //        //if (!string.IsNullOrWhiteSpace(search.SearchItemName))
        //        //{
        //        //    var itemName = search.SearchItemName.Trim();
        //        //    query = query.Where(i => i.PurchaseSCNSubs.Any(s => EF.Functions.Like(s.Item.ItemName, $"%{itemName}%")));
        //        //}

        //        if (search.FromDate.HasValue)
        //            query = query.Where(s => s.GRNDate >= search.FromDate.Value);

        //        if (search.ToDate.HasValue)
        //            query = query.Where(s => s.GRNDate <= search.ToDate.Value);

        //        var scns = await query
        //            .OrderBy(s => s.GRNNo)
        //            .Take(200)
        //            .Select(scn => new PurchaseGRNVM
        //            {
        //                GRNId = scn.GRNId,
        //                GRNNo = scn.GRNNo,
        //                GRNDate = scn.GRNDate,
        //                StoreName = scn.Store.StoreName,
        //                VendorName = scn.Vendor.VendorName,
        //                VendorGstNo = scn.Vendor.GSTNo,
        //                VendorAddress = scn.Vendor.VendorAddress,
        //                VendorContactNo = scn.Vendor.ContactNo
        //            })
        //            .ToListAsync();
        //        var scnIds = scns.Select(s => s.GRNId).ToList();

        //        var subs = await _db.PurchaseGRNSub
        //            .AsNoTracking()
        //            .Where(sub => scnIds.Contains(sub.GRNId))
        //            .Select(sub => new PurchaseGRNSubVM
        //            {
        //                GRNId = sub.GRNId,
        //                ItemCode = sub.Item.ItemCode,
        //                ItemName = sub.Item.ItemName,
        //                Specification = sub.Item.Specification,
        //                MeasureUnit = sub.Item.MeasureUnit,
        //                HSNCode = sub.Item.HSNCode,
        //                Category = sub.Item.Category.CategoryName,

        //            })
        //            .ToListAsync();
        //        foreach (var scn in scns)
        //        {
        //            scn.PurchaseGRNSubVMs = subs
        //                .Where(sub => sub.GRNId == scn.GRNId)
        //                .ToList();
        //        }


        //        return scns;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, "SearchPurchaseSCNAsync");
        //        return new List<PurchaseGRNVM>();
        //    }
        //}
    }
}
