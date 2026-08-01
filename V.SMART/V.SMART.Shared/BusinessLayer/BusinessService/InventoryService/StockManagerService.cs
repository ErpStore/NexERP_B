using DocumentFormat.OpenXml.InkML;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class StockManagerService : IStockManagerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrentUserService _userService;
        private readonly ILoggingService _logs;

        public StockManagerService(IUnitOfWork unitOfWork, CurrentUserService currentUserService, ILoggingService logs)
        {
            _unitOfWork = unitOfWork;
            _userService = currentUserService;
            _logs = logs;
        }

        #region Add Stock
        public async Task AddOrUpdateStockAsync(int itemId,int storeId,decimal qty,decimal unitPrice,string? batchNo,int screenCode,int subItemRefId,
                            string refNo,DateTime refDate,string? remarks,int? RcSubId = null,bool allowMultipleAdd = false)
        {
            try
            {
                StockAdd? existingStock = null;

                if (!allowMultipleAdd)
                {
                    existingStock = await _unitOfWork.StockAdds.GetQueryable()
                                   .Where(x => x.ItemId == itemId &&
                                       x.RefNo == refNo &&
                                       x.SubItemRefID == subItemRefId &&
                                       x.ScreenCode == screenCode)
                                   .FirstOrDefaultAsync();
                }

                var username = await _userService.GetUsernameAsync();
                var now = DateTime.Now;

                if (existingStock != null)
                {
                    decimal usedQty = existingStock.AddQty - existingStock.BalQty;

                    existingStock.StoreId = storeId;
                    existingStock.AddQty = qty;
                    existingStock.BalQty = Math.Max(qty - usedQty, 0);
                    existingStock.UnitPrice = unitPrice;
                    existingStock.BatchNo = batchNo;
                    existingStock.RefDate = refDate;
                    existingStock.RcSubID = RcSubId;    
                    existingStock.Remarks = remarks;

                    await _unitOfWork.StockAdds.UpdateAsync(existingStock);
                }
                else
                {
                    var stockAdd = new StockAdd
                    {
                        AddDate = now,
                        ItemId = itemId,
                        StoreId = storeId,
                        AddQty = qty,
                        BalQty = qty,
                        UnitPrice = unitPrice,
                        BatchNo = batchNo,
                        ScreenCode = screenCode,
                        SubItemRefID = subItemRefId,
                        RefNo = refNo,
                        RefDate = refDate,
                        Remarks = remarks,
                        RcSubID = RcSubId,

                        // Audit fields
                        AddedBy = username,
                        CreatedDate = now
                    };

                    await _unitOfWork.StockAdds.CreateAsync(stockAdd);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in AddOrUpdateStockAsync");
                throw;
            }
        }

        #endregion

        #region Issue Stock

        public async Task IssueOrUpdateStockAsync(int itemId,int storeId,decimal issueQty,decimal unitPrice,string? batchNo,int screenCode,
                                            int? subItemRefId,string? refNo,DateTime? refDate, int? rcSubId = null ,bool allowMultipleIssue = false)
        {
            try
            {
                StockIssue? existingIssue = null;

                if (!allowMultipleIssue)
                {
                    existingIssue = await _unitOfWork.StockIssues.GetQueryable()
                                    .FirstOrDefaultAsync(x =>
                                        x.ItemId == itemId &&
                                        x.SubItemRefID == subItemRefId &&
                                        x.RefNo == refNo &&
                                        x.ScreenCode == screenCode);
                }

                if (existingIssue != null)
                {
                    existingIssue.StoreId = storeId;
                    existingIssue.IssueQty = issueQty;
                    existingIssue.UnitPrice = unitPrice;
                    existingIssue.BatchNo = batchNo;
                    existingIssue.RefDate = refDate;
                    existingIssue.IssuedBy = await _userService.GetUsernameAsync();
                    existingIssue.CreatedDate = DateTime.Now;

                    await _unitOfWork.StockIssues.UpdateAsync(existingIssue);
                    await TrackStockUsageAsync(existingIssue, issueQty, itemId, storeId, rcSubId);
                }
                else
                {
                    var stockIssue = new StockIssue
                    {
                        IssueDate = DateTime.Now,
                        ItemId = itemId,
                        StoreId = storeId,
                        IssueQty = issueQty,
                        UnitPrice = unitPrice,
                        BatchNo = batchNo,
                        ScreenCode = screenCode,
                        SubItemRefID = subItemRefId,
                        RefNo = refNo,
                        RefDate = refDate,
                        IssuedBy = await _userService.GetUsernameAsync(),
                        CreatedDate = DateTime.Now

                    };

                    await _unitOfWork.StockIssues.CreateAsync(stockIssue);
                    await _unitOfWork.SaveAsync();

                    await TrackStockUsageAsync(stockIssue, issueQty, itemId, storeId, rcSubId);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IssueOrUpdateStockAsync");

                throw new InvalidOperationException("Failed to issue stock. Please contact support.",ex);
            }
        }

        #endregion

        #region FIFO Stock Tracking
        private async Task TrackStockUsageAsync(StockIssue stockIssue, decimal newIssueQty, int itemId, int storeId , int? rcSubId)
        {
            try
            {
                var previousTracks = await _unitOfWork.StockIssueTracks.GetQueryable()
                .Where(t => t.IssueId == stockIssue.IssueId)
                .OrderBy(t => t.AddId)
                .ToListAsync();

                decimal previousQty = previousTracks.Sum(t => t.UsedQty);

                foreach (var track in previousTracks)
                {
                    var add = await _unitOfWork.StockAdds.GetAsync(track.AddId.Value);
                    if (add != null)
                    {
                        add.BalQty += track.UsedQty;
                        await _unitOfWork.StockAdds.UpdateAsync(add);
                    }
                    await _unitOfWork.StockIssueTracks.DeleteAsync(track);
                }

                await _unitOfWork.SaveAsync();

                var remainingQty = newIssueQty;

                var stockAdds = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa => sa.ItemId == itemId && sa.StoreId == storeId && sa.RcSubID == rcSubId // Work on Routecard Based on Process by Process Important RcSubId
                            && sa.BalQty > 0)
                    .OrderBy(sa => sa.AddDate)
                    .ToListAsync();

                if (!stockAdds.Any())
                    throw new InvalidOperationException("No available stock to issue.");

                foreach (var add in stockAdds)
                {
                    if (remainingQty <= 0)
                        break;

                    var used = Math.Min(add.BalQty, remainingQty);
                    add.BalQty -= used;
                    remainingQty -= used;

                    await _unitOfWork.StockAdds.UpdateAsync(add);

                    var newTrack = new StockIssueTrack
                    {
                        IssueId = stockIssue.IssueId,
                        AddId = add.AddId,
                        UsedQty = used
                    };

                    await _unitOfWork.StockIssueTracks.CreateAsync(newTrack);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"TrackStockUsageAsync failed | IssueId: {stockIssue?.IssueId}, " +
                    $"ItemId: {itemId}, StoreId: {storeId}, NewIssueQty: {newIssueQty}");

                throw new InvalidOperationException(
                    "Failed to process stock issue. Please contact support.",
                    ex);
            }
        }

        #endregion

        #region Delete Operations

        public async Task DeleteStockAddAsync(int addId)
        {
            try
            {
                var add = await _unitOfWork.StockAdds.GetAsync(addId);

                if (add == null)
                    return;

                var used = await _unitOfWork.StockIssueTracks.GetQueryable()
                    .AnyAsync(t => t.AddId == addId);

                if (used)
                    throw new Exception("Cannot delete. Stock already issued from this batch.");

                await _unitOfWork.StockAdds.DeleteAsync(addId);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in DeleteStockAddAsync");
                throw;
            }
        }

        public async Task DeleteStockIssueAsync(int issueId)
        {
            try
            {
                var issue = await _unitOfWork.StockIssues.GetAsync(issueId)
                             ?? throw new Exception("Stock Issue not found.");

                var tracks = await _unitOfWork.StockIssueTracks.GetQueryable()
                    .Where(t => t.IssueId == issueId)
                    .ToListAsync();

                // Reverse stock usage
                foreach (var track in tracks)
                {
                    var add = await _unitOfWork.StockAdds.GetAsync(track.AddId.Value);
                    if (add != null)
                    {
                        add.BalQty += track.UsedQty;
                        await _unitOfWork.StockAdds.UpdateAsync(add);
                        await _unitOfWork.SaveAsync();
                    }

                    await _unitOfWork.StockIssueTracks.DeleteAsync(track);

                }

                await _unitOfWork.StockIssues.DeleteAsync(issue);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in DeleteStockIssueAsync");
                throw;
            }
        }

        #endregion

        public async Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                            int screenCode,
                            int storeId,
                            int itemId,
                            int subItemRefId)
        {
            try
            {
                // Fetch only AddQty and BalQty
                var stock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(x =>
                        x.ItemId == itemId &&
                        x.StoreId == storeId &&
                        x.SubItemRefID == subItemRefId &&
                        x.ScreenCode == screenCode)
                    .Select(x => new
                    {
                        x.AddQty,
                        x.BalQty
                    })
                    .FirstOrDefaultAsync();

                return stock != null ? (stock.AddQty, stock.BalQty) : (0, 0);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching stock for Screen Code: {screenCode}, SubItemRefId: {subItemRefId}");
                throw new InvalidOperationException("Failed to retrieve stock details.");
            }
        }


        #region Get Current Stock
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId)
        {
            try
            {
                var ids = itemIds.Distinct().ToList();

                var query = _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa => ids.Contains(sa.ItemId) && sa.BalQty > 0);

                // If storeId is NULL → exclude store 6 and 7
                if (!storeId.HasValue)
                {
                    query = query.Where(sa => sa.StoreId != 6 && sa.StoreId != 7);
                }
                else
                {
                    // If storeId is provided → filter by that store
                    query = query.Where(sa => sa.StoreId == storeId.Value);
                }

                var stocks = await query
                    .GroupBy(sa => sa.ItemId)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        TotalStock = g.Sum(sa => sa.BalQty)
                    })
                    .ToDictionaryAsync(x => x.ItemId, x => x.TotalStock);

                return stocks;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error fetching multi-item stock. ItemIds: {string.Join(",", itemIds)}, StoreId: {storeId}");
                throw new InvalidOperationException("Failed to retrieve stock details for multiple items.");
            }
        }

        public async Task<decimal> GetStockForItemAsync(int itemId, int? storeId)
        {
            try
            {
                var query = _unitOfWork.StockAdds.GetQueryable()
                                 .Where(s => s.ItemId == itemId);

                if (!storeId.HasValue)
                {
                    // If storeId is NULL → EXCLUDE store 6 and 7
                    query = query.Where(s => s.StoreId != 6 && s.StoreId != 7);
                }
                else if (storeId.Value > 0)
                {
                    // If storeId is provided → FILTER by that store
                    query = query.Where(s => s.StoreId == storeId.Value);
                }

                var stock = await query.SumAsync(s => s.BalQty);

                return stock;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error fetching stock for ItemId: {itemId}, StoreId: {storeId}");
                throw new InvalidOperationException("Failed to retrieve stock details.");
            }
        }

        #endregion

        #region Get Stock Weighted Average Rate

        public async Task<Dictionary<int, decimal>> GetStockRateForItemsAsync(
            IEnumerable<int> itemIds, int? storeId)
        {
            try
            {
                var ids = itemIds.Distinct().ToList();

                var query = _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa => ids.Contains(sa.ItemId)
                              && sa.BalQty > 0
                              && sa.UnitPrice > 0);

                // Same store logic as stock qty
                if (!storeId.HasValue)
                {
                    query = query.Where(sa => sa.StoreId != 6 && sa.StoreId != 7);
                }
                else
                {
                    query = query.Where(sa => sa.StoreId == storeId.Value);
                }

                var rateDict = await query
                    .GroupBy(sa => sa.ItemId)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        AvgRate = g.Sum(x => x.BalQty * x.UnitPrice) / (g.Sum(x => x.BalQty) == 0 ? 1 : g.Sum(x => x.BalQty))
                    })
                    .ToDictionaryAsync(x => x.ItemId, x => x.AvgRate);

                return rateDict;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching weighted avg rate. ItemIds: {string.Join(",", itemIds)}, StoreId: {storeId}");
                throw new InvalidOperationException("Failed to retrieve stock rate details.");
            }
        }

        #endregion

        public async Task<decimal> GetStockForItemRcSubIdAsync(int itemId, int? storeId, int? rcSubId)
        {
            try
            {
                var query = _unitOfWork.StockAdds.GetQueryable()
                                .Where(s => s.ItemId == itemId);


                if (rcSubId.HasValue && rcSubId > 0)
                {

                    query = query.Where(s => s.RcSubID == rcSubId);
                }
                else
                {

                    query = query.Where(s => s.RcSubID == null || s.RcSubID == 0);
                }

                if (!storeId.HasValue)
                {
                    query = query.Where(s => s.StoreId != 6 && s.StoreId != 7);
                }
                else if (storeId.Value > 0)
                {
                    query = query.Where(s => s.StoreId == storeId.Value);
                }

                var stock = await query.SumAsync(s => (decimal?)s.BalQty) ?? 0;

                return stock;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error fetching stock for ItemId: {itemId}, StoreId: {storeId}, RcSubId: {rcSubId}");

                throw new InvalidOperationException("Failed to retrieve stock details.");
            }
        }



    }
}
