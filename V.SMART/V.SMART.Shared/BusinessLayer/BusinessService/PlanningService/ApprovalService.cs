using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Data.Planning;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.PlanningViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static V.SMART.Shared.Pages.Planning_Module_Pages.Authorization_Pages.Authorization;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class ApprovalService : IApprovalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;

        public ApprovalService(IUnitOfWork unitOfWork, ILoggingService loggingService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
        }

        #region Get Pending

        public async Task<List<ApprovalVM>> GetPendingApprovalsAsync(string type, string level, string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                    return new List<ApprovalVM>();

                int currentLevel = 0;
                if (!string.IsNullOrEmpty(level) && level.StartsWith("Level") &&
                    int.TryParse(level.Replace("Level", ""), out int parsedLevel))
                {
                    currentLevel = parsedLevel;
                }

                return type.ToLower() switch
                {
                    "leave" => await GetPendingLeaveApprovalsAsync(currentLevel),
                    "purchase requisition" => await GetPendingPRApprovalsAsync(currentLevel),
                    "po" => await GetPendingPOApprovalsAsync(currentLevel),
                    "purchasescn" => await GetPendingPurchaseSCNApprovalsAsync(currentLevel),
                    "sales po" => await GetPendingSalesPoApprovalsAsync(currentLevel),
                    "sales quotation" => await GetPendingSalesQuotationApprovalsAsync(currentLevel),
                    "stock issue request" => await GetPendingStockissueReqApprovalsAsync(currentLevel),

                    _ => new List<ApprovalVM>()
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }
        private async Task<List<ApprovalVM>> GetPendingStockissueReqApprovalsAsync(int level)
        {
            try
            {
                var query = _unitOfWork.StockRequestIssues.GetQueryable()

                    .Where(l => l.IsAuthorized == false && l.IsRejected == false && !l.ReqTally);
                query = level switch
                {
                    1 => query.Where(l => !l.IsLevel1),
                    2 => query.Where(l => l.IsLevel1 && !l.IsLevel2),
                    3 => query.Where(l => l.IsLevel1 && l.IsLevel2 && !l.IsLevel3),
                    _ => query
                };

                var list = await query.AsNoTracking().ToListAsync();
                return list.Select(l => new ApprovalVM
                {
                    ID = l.RequestId,
                    DocNo = l.RequestNo,
                    DocDate = l.RequestDate,
                    NoOfItems = l.NoOfItems ?? 0,
                    RequestedBy = l.CreatedBy,
                    status = l.IsAuthorized ? "Approved" : "Pending"
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingStockissueReqApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }
        private async Task<List<ApprovalVM>> GetPendingPurchaseSCNApprovalsAsync(int level)
        {
            try
            {
                var query = _unitOfWork.PurchaseSCNs.GetQueryable()
                     .Include(l => l.Vendor)
                    .Where(l => l.Authorized == false && l.IsRejected == false && !l.SCNCancel && !l.SCNTally);
                query = level switch
                {
                    1 => query.Where(l => !l.IsLevel1),
                    2 => query.Where(l => l.IsLevel1 && !l.IsLevel2),
                    3 => query.Where(l => l.IsLevel1 && l.IsLevel2 && !l.IsLevel3),
                    _ => query
                };

                var list = await query.AsNoTracking().ToListAsync();
                return list.Select(l => new ApprovalVM
                {
                    ID = l.SCNId,
                    DocNo = l.SCNNo,
                    DocDate = l.SCNDate,
                    NoOfItems = l.NoOfItems,
                    Supplier = l.Vendor.VendorName,
                    Amount = 0,//l.GrandTotal,
                    RequestedBy = l.CreatedBy,
                    status = l.Authorized ? "Approved" : "Pending"
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingPRApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }

        private async Task<List<ApprovalVM>> GetPendingLeaveApprovalsAsync(int level)
        {
            try
            {
                var query = _unitOfWork.LeaveApplications
                    .GetQueryable()
                    .Where(l => l.LeaveStatus == "Pending");

                query = level switch
                {
                    1 => query.Where(l => !l.Level1),
                    2 => query.Where(l => l.Level1 && !l.Level2),
                    3 => query.Where(l => l.Level1 && l.Level2 && !l.Level3),
                    _ => query
                };

                var list = await query.AsNoTracking().ToListAsync();

                return list.Select(l => new ApprovalVM
                {
                    ID = l.LeaveAppID,
                    DocNo = l.LeaveApplicationNo,
                    Name = l.Staff?.StaffName,
                    Department = l.Department,
                    Reason = l.Reason,
                    RequestedBy = l.CreatedBy,
                    FromDate = l.FromDate,
                    ToDate = l.ToDate,
                    status = l.LeaveStatus
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingLeaveApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }
        private async Task<List<ApprovalVM>> GetPendingPRApprovalsAsync(int level)
        {
            try
            {
                var query = _unitOfWork.MaterialReqs.GetQueryable().Where(l => l.IsAuthorized == false && l.IsRejected == false && !l.MreqCancel && !l.PRTally);
                query = level switch
                {
                    1 => query.Where(l => !l.IsLevel1),
                    2 => query.Where(l => l.IsLevel1 && !l.IsLevel2),
                    3 => query.Where(l => l.IsLevel1 && l.IsLevel2 && !l.IsLevel3),
                    _ => query
                };

                var list = await query.AsNoTracking().ToListAsync();
                return list.Select(l => new ApprovalVM
                {
                    ID = l.MreqId,
                    DocNo = l.MReqNo,
                    DocDate = l.MreqDate,
                    NoOfItems = l.NoOfItems,
                    Department = l.Department,
                    RequestedBy = l.CreatedBy,
                    status = l.IsAuthorized ? "Approved" : "Pending"
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingPRApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }

        private async Task<List<ApprovalVM>> GetPendingSalesPoApprovalsAsync(int level)
        {
            try
            {
                var query = _unitOfWork.MfgPos.GetQueryable()
                    .Include(l => l.Customer)
                    .Where(l => l.IsAuthorized == false && l.IsRejected == false && !l.PoCancl && !l.PoTally);
                query = level switch
                {
                    1 => query.Where(l => !l.IsLevel1),
                    2 => query.Where(l => l.IsLevel1 && !l.IsLevel2),
                    3 => query.Where(l => l.IsLevel1 && l.IsLevel2 && !l.IsLevel3),
                    _ => query
                };

                var list = await query.AsNoTracking().ToListAsync();
                return list.Select(l => new ApprovalVM
                {
                    ID = l.PoId,
                    DocNo = l.PONo,
                    DocDate = l.PODate,
                    Supplier = l.Customer.CustName,
                    NoOfItems = l.NoOfItems,
                    RequestedBy = l.CreatedBy,
                    status = l.IsAuthorized ? "Approved" : "Pending"
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingSalesPoApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }

        private async Task<List<ApprovalVM>> GetPendingSalesQuotationApprovalsAsync(int level)
        {
            try
            {
                var query = _unitOfWork.MfgQuotes.GetQueryable()
                    .Include(l => l.Customer)
                    .Where(l => l.IsAuthorized == false && l.IsRejected == false && !l.QuotationTally && !l.IsCancel);
                query = level switch
                {
                    1 => query.Where(l => !l.IsLevel1),
                    2 => query.Where(l => l.IsLevel1 && !l.IsLevel2),
                    3 => query.Where(l => l.IsLevel1 && l.IsLevel2 && !l.IsLevel3),
                    _ => query
                };

                var list = await query.AsNoTracking().ToListAsync();
                return list.Select(l => new ApprovalVM
                {
                    ID = l.QuoteId,
                    DocNo = l.QuoteNo,
                    DocDate = l.QuoteDate,
                    Supplier = l.Customer.CustName,
                    NoOfItems = l.NoOfItems ?? 0,
                    RequestedBy = l.CreatedBy,
                    status = l.IsAuthorized ? "Approved" : "Pending"
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingSalesPoApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }

        private async Task<List<ApprovalVM>> GetPendingPOApprovalsAsync(int level)
        {
            try
            {
                var query = _unitOfWork.PurchPos.GetQueryable()
                     .Include(l => l.Vendor)
                    .Where(l => l.Authorized == false && l.IsRejected == false && !l.PoCancl && !l.PoTally);
                query = level switch
                {
                    1 => query.Where(l => !l.IsLevel1),
                    2 => query.Where(l => l.IsLevel1 && !l.IsLevel2),
                    3 => query.Where(l => l.IsLevel1 && l.IsLevel2 && !l.IsLevel3),
                    _ => query
                };

                var list = await query.AsNoTracking().ToListAsync();
                return list.Select(l => new ApprovalVM
                {
                    ID = l.PoId,
                    DocNo = l.PONo,
                    DocDate = l.PODate,
                    NoOfItems = l.NoOfItems,
                    Supplier = l.Vendor.VendorName,
                    Amount = l.GrandTotal,
                    RequestedBy = l.CreatedBy,
                    status = l.Authorized ? "Approved" : "Pending"
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while GetPendingPRApprovalsAsync()");
                return new List<ApprovalVM>();
            }
        }
        #endregion


        #region Approve
        public async Task<bool> ApproveAsync(ApprovalVM record, string type, string level, string userName, UserAuthority authority)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!int.TryParse(level.Replace("Level", ""), out int currentLevel))
                    return false;

                int maxAllowedLevel = await GetMaxApprovalLevelAsync(type);
                if (currentLevel > maxAllowedLevel) return false;

                bool success = type.ToLower() switch
                {
                    "leave" => await ApproveLeaveAsync(record.ID, currentLevel, maxAllowedLevel, userName),
                    "purchase requisition" => await ApproveIndentAsync(record.ID, currentLevel, maxAllowedLevel, userName),
                    "po" => await ApprovePOAsync(record.ID, currentLevel, maxAllowedLevel, userName),
                    "purchasescn" => await ApprovePurchaseSCNAsync(record.ID, currentLevel, maxAllowedLevel, userName),
                    "sales po" => await ApproveSalesPoAsync(record.ID, currentLevel, maxAllowedLevel, userName),
                    "sales quotation" => await ApproveSalesQuoteAsync(record.ID, currentLevel, maxAllowedLevel, userName),
                    "stock issue request" => await ApproveStockRequestAsync(record.ID, currentLevel, maxAllowedLevel, userName),

                    _ => false
                };

                if (success)
                {
                    await AddHistoryAsync(record.ID, type, currentLevel, "Approved", userName);

                    await transaction.CommitAsync();
                    return true;
                }

                await transaction.RollbackAsync();
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Error in ApproveAsync");
                return false;
            }
        }
        private async Task<bool> ApproveStockRequestAsync(int id, int currentLevel, int maxLevel, string userName)
        {
            try
            {
                var salesquote = await _unitOfWork.StockRequestIssues.GetQueryable().Where(i => i.RequestId == id).FirstOrDefaultAsync();
                if (salesquote == null) return false;

                switch (currentLevel)
                {
                    case 1: salesquote.IsLevel1 = true; salesquote.Level1Sign = userName; break;
                    case 2: salesquote.IsLevel2 = true; salesquote.Level2Sign = userName; break;
                    case 3: salesquote.IsLevel3 = true; salesquote.Level3Sign = userName; break;
                }

                if (currentLevel == maxLevel)
                {
                    salesquote.IsAuthorized = true;
                    salesquote.LastApproved = userName;
                    salesquote.ApprovedDate = DateTime.Now;

                    salesquote.IsRejected = false;
                    salesquote.RejectReason = string.Empty;
                }

                await _unitOfWork.StockRequestIssues.UpdateAsync(salesquote);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ApproveIndentAsync");
                return false;
            }
        }
        private async Task<bool> ApprovePurchaseSCNAsync(int id, int currentLevel, int maxLevel, string userName)
        {
            try
            {
                var scn = await _unitOfWork.PurchaseSCNs.GetQueryable().Where(i => i.SCNId == id).FirstOrDefaultAsync();
                if (scn == null) return false;

                switch (currentLevel)
                {
                    case 1: scn.IsLevel1 = true; scn.Level1Sign = userName; break;
                    case 2: scn.IsLevel2 = true; scn.Level2Sign = userName; break;
                    case 3: scn.IsLevel3 = true; scn.Level3Sign = userName; break;
                }

                if (currentLevel == maxLevel)
                {
                    scn.Authorized = true;
                    scn.ApprovedBy = userName;
                    scn.ApprovalDate = DateTime.Now;

                    scn.IsRejected = false;
                    scn.RejectReason = string.Empty;
                }

                await _unitOfWork.PurchaseSCNs.UpdateAsync(scn);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ApprovePurchaseSCNAsync");
                return false;
            }
        }
        private async Task<bool> ApproveLeaveAsync(int id, int currentLevel, int maxLevel, string userName)
        {
            try
            {
                var leave = await _unitOfWork.LeaveApplications.FirstOrDefaultAsync(l => l.LeaveAppID == id);
                if (leave == null) return false;

                switch (currentLevel)
                {
                    case 1: leave.Level1 = true; leave.Level1_Sign = userName; break;
                    case 2: leave.Level2 = true; leave.Level2_sign = userName; break;
                    case 3: leave.Level3 = true; leave.Level3_sign = userName; break;
                }

                if (currentLevel == maxLevel)
                {
                    leave.LeaveStatus = "Approved";
                    leave.ApprovedBy = userName;
                    leave.ApprovalDate = DateTime.Now;
                    leave.Authorized = true;
                }

                await _unitOfWork.LeaveApplications.UpdateAsync(leave);
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ApproveLeaveAsync");
                return false;
            }
        }

        private async Task<bool> ApproveSalesPoAsync(int id, int currentLevel, int maxLevel, string userName)
        {
            try
            {
                var salespo = await _unitOfWork.MfgPos.GetQueryable().Where(i => i.PoId == id).FirstOrDefaultAsync();
                if (salespo == null) return false;

                switch (currentLevel)
                {
                    case 1: salespo.IsLevel1 = true; salespo.Level1Sign = userName; break;
                    case 2: salespo.IsLevel2 = true; salespo.Level2Sign = userName; break;
                    case 3: salespo.IsLevel3 = true; salespo.Level3Sign = userName; break;
                }

                if (currentLevel == maxLevel)
                {
                    salespo.IsAuthorized = true;
                    salespo.LastApproved = userName;
                    salespo.ApprovedDate = DateTime.Now;

                    salespo.IsRejected = false;
                    salespo.RejectReason = string.Empty;
                }

                await _unitOfWork.MfgPos.UpdateAsync(salespo);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ApproveIndentAsync");
                return false;
            }
        }

        private async Task<bool> ApproveSalesQuoteAsync(int id, int currentLevel, int maxLevel, string userName)
        {
            try
            {
                var salesquote = await _unitOfWork.MfgQuotes.GetQueryable().Where(i => i.QuoteId == id).FirstOrDefaultAsync();
                if (salesquote == null) return false;

                switch (currentLevel)
                {
                    case 1: salesquote.IsLevel1 = true; salesquote.Level1Sign = userName; break;
                    case 2: salesquote.IsLevel2 = true; salesquote.Level2Sign = userName; break;
                    case 3: salesquote.IsLevel3 = true; salesquote.Level3Sign = userName; break;
                }

                if (currentLevel == maxLevel)
                {
                    salesquote.IsAuthorized = true;
                    salesquote.LastApproved = userName;
                    salesquote.ApprovedDate = DateTime.Now;

                    salesquote.IsRejected = false;
                    salesquote.RejectReason = string.Empty;
                }

                await _unitOfWork.MfgQuotes.UpdateAsync(salesquote);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ApproveIndentAsync");
                return false;
            }
        }

        private async Task<bool> ApproveIndentAsync(int id, int currentLevel, int maxLevel, string userName)
        {
            try
            {
                var indent = await _unitOfWork.MaterialReqs.GetQueryable().Where(i => i.MreqId == id).FirstOrDefaultAsync();
                if (indent == null) return false;

                switch (currentLevel)
                {
                    case 1: indent.IsLevel1 = true; indent.Level1Sign = userName; break;
                    case 2: indent.IsLevel2 = true; indent.Level2Sign = userName; break;
                    case 3: indent.IsLevel3 = true; indent.Level3Sign = userName; break;
                }

                if (currentLevel == maxLevel)
                {
                    indent.IsAuthorized = true;
                    indent.LastApproved = userName;
                    indent.ApprovedDate = DateTime.Now;

                    indent.IsRejected = false;
                    indent.RejectReason = string.Empty;
                }

                await _unitOfWork.MaterialReqs.UpdateAsync(indent);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ApproveIndentAsync");
                return false;
            }
        }
        private async Task<bool> ApprovePOAsync(int id, int currentLevel, int maxLevel, string userName)
        {
            try
            {
                var indent = await _unitOfWork.PurchPos.GetQueryable().Where(i => i.PoId == id).FirstOrDefaultAsync();
                if (indent == null) return false;

                switch (currentLevel)
                {
                    case 1: indent.IsLevel1 = true; indent.Level1Sign = userName; break;
                    case 2: indent.IsLevel2 = true; indent.Level2Sign = userName; break;
                    case 3: indent.IsLevel3 = true; indent.Level3Sign = userName; break;
                }

                if (currentLevel == maxLevel)
                {
                    indent.Authorized = true;
                    indent.ApprovedBy = userName;
                    indent.ApprovalDate = DateTime.Now;

                    indent.IsRejected = false;
                    indent.RejectReason = string.Empty;
                }

                await _unitOfWork.PurchPos.UpdateAsync(indent);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ApproveIndentAsync");
                return false;
            }
        }
        #endregion


        #region Reject
        public async Task<bool> RejectAsync(ApprovalVM record, string type, string level, string reason, string userName)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (!int.TryParse(level.Replace("Level", ""), out int currentLevel))
                    currentLevel = 1;

                bool success = type.ToLower() switch
                {
                    "leave" => await RejectLeaveAsync(record.ID, reason, userName),
                    "purchase requisition" => await RejectIndentAsync(record.ID, reason, userName),
                    "po" => await RejectPOAsync(record.ID, reason, userName),
                    "purchasescn" => await RejectPurchaseSCNAsync(record.ID, reason, userName),
                    "sales po" => await RejectSalesPoAsync(record.ID, reason, userName),
                    "sales quotation" => await RejectSalesQuoteAsync(record.ID, reason, userName),
                    "Stock Issue Request" => await RejectStockIssueReqAsync(record.ID, reason, userName),
                    _ => false
                };

                if (success)
                {
                    await AddHistoryAsync(record.ID, type, currentLevel, "Rejected", userName, reason);

                    await transaction.CommitAsync();
                    return true;
                }

                await transaction.RollbackAsync();
                return false;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Error in RejectAsync");
                return false;
            }
        }
        private async Task<bool> RejectStockIssueReqAsync(int id, string reason, string userName)
        {
            try
            {
                var salespo = await _unitOfWork.StockRequestIssues.FirstOrDefaultAsync(i => i.RequestId == id);
                if (salespo == null) return false;

                salespo.IsRejected = true;
                salespo.RejectReason = reason;
                salespo.ApprovedBy = userName;
                salespo.ApprovalDate = DateTime.Now;

                await _unitOfWork.StockRequestIssues.UpdateAsync(salespo);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RejectSalesPoAsync");
                return false;
            }
        }

        private async Task<bool> RejectPurchaseSCNAsync(int id, string reason, string userName)
        {
            try
            {
                var scn = await _unitOfWork.PurchaseSCNs.FirstOrDefaultAsync(i => i.SCNId == id);
                if (scn == null) return false;

                scn.IsRejected = true;
                scn.RejectReason = reason;
                scn.ApprovedBy = userName;
                scn.ApprovalDate = DateTime.Now;

                await _unitOfWork.PurchaseSCNs.UpdateAsync(scn);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RejectPurchaseSCNAsync");
                return false;
            }
        }

        private async Task<bool> RejectSalesPoAsync(int id, string reason, string userName)
        {
            try
            {
                var salespo = await _unitOfWork.MfgPos.FirstOrDefaultAsync(i => i.PoId == id);
                if (salespo == null) return false;

                salespo.IsRejected = true;
                salespo.RejectReason = reason;
                salespo.ApprovedBy = userName;
                salespo.ApprovalDate = DateTime.Now;

                await _unitOfWork.MfgPos.UpdateAsync(salespo);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RejectSalesPoAsync");
                return false;
            }
        }

        private async Task<bool> RejectSalesQuoteAsync(int id, string reason, string userName)
        {
            try
            {
                var quote = await _unitOfWork.MfgQuotes.FirstOrDefaultAsync(i => i.QuoteId == id);
                if (quote == null) return false;

                quote.IsRejected = true;
                quote.RejectReason = reason;
                quote.ApprovedBy = userName;
                quote.ApprovalDate = DateTime.Now;

                await _unitOfWork.MfgQuotes.UpdateAsync(quote);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RejectSalesQuoteSCNAsync");
                return false;
            }
        }

        private async Task<bool> RejectLeaveAsync(int id, string reason, string userName)
        {
            try
            {
                var leave = await _unitOfWork.LeaveApplications.FirstOrDefaultAsync(l => l.LeaveAppID == id);
                if (leave == null) return false;

                leave.LeaveStatus = "Rejected";
                leave.RejectReason = reason;
                leave.ApprovedBy = userName;
                leave.ApprovalDate = DateTime.Now;
                leave.Authorized = false;

                await _unitOfWork.LeaveApplications.UpdateAsync(leave);

                var empBalance = await _unitOfWork.EmployeeLeaveBalances
                    .FirstOrDefaultAsync(e => e.EmployeeID == leave.EmployeeID && e.LeaveType == leave.LeaveType && e.Year == leave.FromDate.Year);

                if (empBalance != null)
                {
                    decimal leaveDays = leave.TotalDays;
                    empBalance.LeavesTaken = Math.Max(0, empBalance.LeavesTaken - leaveDays);
                    empBalance.Balance += leaveDays;

                    await _unitOfWork.EmployeeLeaveBalances.UpdateAsync(empBalance);
                }

                return true;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RejectLeaveAsync");
                return false;
            }
        }

        private async Task<bool> RejectIndentAsync(int id, string reason, string userName)
        {
            try
            {
                var indent = await _unitOfWork.MaterialReqs.FirstOrDefaultAsync(i => i.MreqId == id);
                if (indent == null) return false;

                indent.IsRejected = true;
                indent.RejectReason = reason;
                indent.LastApproved = userName;
                indent.ApprovedDate = DateTime.Now;

                await _unitOfWork.MaterialReqs.UpdateAsync(indent);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RejectIndentAsync");
                return false;
            }
        }
        private async Task<bool> RejectPOAsync(int id, string reason, string userName)
        {
            try
            {
                var indent = await _unitOfWork.PurchPos.FirstOrDefaultAsync(i => i.PoId == id);
                if (indent == null) return false;

                indent.IsRejected = true;
                indent.RejectReason = reason;
                indent.ApprovedBy = userName;
                indent.ApprovalDate = DateTime.Now;

                await _unitOfWork.PurchPos.UpdateAsync(indent);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in RejectIndentAsync");
                return false;
            }
        }
        #endregion


        #region Bulk Operations
        public async Task<bool> BulkApproveAsync(List<ApprovalVM> records, string type, string level, string username)
        {
            try
            {
                foreach (var rec in records)
                {
                    var ok = await ApproveAsync(rec, type, level, username, new UserAuthority());
                    if (!ok)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in BulkApproveAsync");
                return false;
            }
        }

        public async Task<bool> BulkRejectAsync(List<ApprovalVM> records, string type, string level, string reason, string username)
        {
            try
            {
                foreach (var rec in records)
                {
                    var ok = await RejectAsync(rec, type, level, reason, username);
                    if (!ok)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in BulkRejectAsync");
                return false;
            }
        }

        #endregion


        #region Helpers
        //Sathish
        public async Task<int> GetMaxApprovalLevelAsync(string type)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(type))
                    return 0;

                var query = _unitOfWork.UserAuthorities.GetQueryable();

                return type.ToLower() switch
                {
                    "leave" => query
                        .Where(u => !string.IsNullOrEmpty(u.IsLeaveLevel) && u.IsLeaveLevel.StartsWith("Level"))
                        .AsEnumerable()
                        .Select(u =>
                        {
                            int.TryParse(u.IsLeaveLevel.Replace("Level", ""), out int lvl);
                            return lvl;
                        })
                        .DefaultIfEmpty(0)
                        .Max(),

                    "purchase requisition" => query
                        .Where(u => !string.IsNullOrEmpty(u.IsPRLevel) && u.IsPRLevel.StartsWith("Level"))
                        .AsEnumerable()
                        .Select(u =>
                        {
                            int.TryParse(u.IsPRLevel.Replace("Level", ""), out int lvl);
                            return lvl;
                        })
                        .DefaultIfEmpty(0)
                        .Max(),

                    "po" => query
                       .Where(u => !string.IsNullOrEmpty(u.IsPOLevel) && u.IsPOLevel.StartsWith("Level"))
                       .AsEnumerable()
                       .Select(u =>
                       {
                           int.TryParse(u.IsPOLevel.Replace("Level", ""), out int lvl);
                           return lvl;
                       })
                       .DefaultIfEmpty(0)
                       .Max(),

                    "purchasescn" => query
                        .Where(u => !string.IsNullOrEmpty(u.IsPurchSCNLevel) && u.IsPurchSCNLevel.StartsWith("Level"))
                        .AsEnumerable()
                        .Select(u =>
                        {
                            int.TryParse(u.IsPurchSCNLevel.Replace("Level", ""), out int lvl);
                            return lvl;
                        })
                        .DefaultIfEmpty(0)
                        .Max(),

                    "sales po" => query
                       .Where(u => !string.IsNullOrEmpty(u.IsSalesPoLevel) && u.IsSalesPoLevel.StartsWith("Level"))
                       .AsEnumerable()
                       .Select(u =>
                       {
                           int.TryParse(u.IsSalesPoLevel.Replace("Level", ""), out int lvl);
                           return lvl;
                       })
                       .DefaultIfEmpty(0)
                       .Max(),


                    "stock issue request" => query
                       .Where(u => !string.IsNullOrEmpty(u.IsStockReqLevel) && u.IsStockReqLevel.StartsWith("Level"))
                       .AsEnumerable()
                       .Select(u =>
                       {
                           int.TryParse(u.IsStockReqLevel.Replace("Level", ""), out int lvl);
                           return lvl;
                       })
                       .DefaultIfEmpty(0)
                       .Max(),

                    "sales quotation" => query
                    .Where(u => !string.IsNullOrEmpty(u.IsMfgQuoteLevel) && u.IsMfgQuoteLevel.StartsWith("Level"))
                    .AsEnumerable()
                    .Select(u =>
                    {
                        int.TryParse(u.IsMfgQuoteLevel.Replace("Level", ""), out int lvl);
                        return lvl;
                    })
                    .DefaultIfEmpty(0)
                    .Max(),
                    _ => 0
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error on GetMaxApprovalLevelAsync");
                return 0;
            }
        }

        private async Task AddHistoryAsync(
            int recordId,
            string type,
            int level,
            string status,
            string actionBy,
            string reason = null)
        {
            try
            {
                var history = new ApprovalHistory
                {
                    RecordId = recordId,
                    ApprovalType = type,
                    Level = level,
                    Status = status,
                    ActionBy = actionBy,
                    ActionDate = DateTime.Now,
                    Reason = reason
                };

                await _unitOfWork.ApprovalHistories.CreateAsync(history);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error on AddHistoryAsync");
            }
        }

        #endregion
    }

}
