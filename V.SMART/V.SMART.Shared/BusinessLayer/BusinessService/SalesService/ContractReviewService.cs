using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Vml.Office;
using ExcelDataReader.Log;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Data.SalesAndLabour.ContractReview;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ContractReviewVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class ContractReviewService: IContractReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public ContractReviewService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            ForeignKeyUsageChecker fkChecker)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _fkChecker = fkChecker;
        }

        #region these are ContractReviewMaster
        public async Task<List<ContractReviewMaster>> GetMasterDescription()
        {
            try
            {
                var data = await _unitOfWork.ContractReviewMasters.GetAllAsync();
                return data.ToList(); 
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,"Error in GetMasterDescription() - ContractReviewService");
                return new List<ContractReviewMaster>(); 
            }
        }

        public async Task<ContractReviewMaster?> GetMasterBySlNoAsync(int slNo)
        {
            try
            {
                return await _unitOfWork.ContractReviewMasters.GetAsync(slNo);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,"Error in GetMasterBySlNoAsync() - ContractReviewService");
                return null;
            }
        }

        public async Task<bool> DeleteMasterBySlNoAsync(int slNo)
        {
            try
            {
                var isDeleted = await _unitOfWork.ContractReviewMasters.DeleteAsync(slNo);

                if (!isDeleted)
                    return false;

                await _unitOfWork.SaveAsync(); 
                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error in DeleteMasterBySlNoAsync() - SlNo: {slNo}"
                );
                return false;
            }
        }

        public async Task<bool> IsMasterDescriptionUsedAsync(string description)
        {
            return await _unitOfWork.ContractReviews
                .AnyAsync(x => !string.IsNullOrEmpty(x.ReviewJson) &&
                               x.ReviewJson.Contains($"\"{description}\""));
        }

        public async Task<(bool CanDelete, string Message)> ValidateBeforeDeleteBySlNoAsync(int slNo)
        {
            try
            {
                var master = await _unitOfWork.ContractReviewMasters.GetAsync(slNo);

                if (master == null)
                    return (false, "Master record not found.");

                var isUsed = await IsMasterDescriptionUsedAsync(master.Description);

                if (isUsed)
                    return (false, "This description is already used in Contract Review and cannot be deleted.");

               
                return (true, "Master record can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    "Error in ValidateBeforeDeleteBySlNoAsync()"
                );
                return (false, "An unexpected error occurred.");
            }
        }

        public async Task<bool> ExistsByDescriptionAsync(string description)
        {
            description = description?.Trim();

            return await _unitOfWork.ContractReviewMasters.AnyAsync(x =>
                x.Description != null &&
                x.Description.Trim().ToLower() == description.ToLower());
        }

        public async Task<ContractReviewMaster> UpsertContractMasterAsync(ContractReviewMaster master)
        {
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (master == null)
                    throw new ArgumentNullException(nameof(master));

                master.Description = master.Description?.Trim();

                var isDuplicate = await _unitOfWork.ContractReviewMasters.AnyAsync(x =>x.Description == master.Description && x.SlNo != master.SlNo);

                if (isDuplicate)
                    throw new InvalidOperationException("Description already exists.");

                if (master.SlNo == 0)
                {
                    master.CreatedBy = await _currentUserService.GetUsernameAsync();
                    master.CreatedDate= DateTime.UtcNow;
                    await _unitOfWork.ContractReviewMasters.CreateAsync(master);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Description {master.Description}:  set to Slno '{master.SlNo}'");
                }
                else
                {
                    
                    var existing = await _unitOfWork.ContractReviewMasters.GetAsync(master.SlNo);

                    if (existing == null)
                        throw new InvalidOperationException("Record not found.");

                    existing.Description = master.Description.ToString();
                    existing.ModifiedBy= await _currentUserService.GetUsernameAsync();
                    existing.ModifiedDate = DateTime.Now;

                    var parentChanges = GetPropertyChanges(existing, master);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, master.SlNo == 0 ? "Master Description Created" : "Master Description Created");

                return master;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,"Error in UpsertContractMasterAsync()");
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

            await _logs.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<(List<ContractReviewMasterVM> reviewMasterVMs, int TotalCount)> 
                    SearchWithDynamicReviewMasterFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.ContractReviewMasters
                    .GetQueryable()
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = ContractReviewFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.SlNo)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<ContractReviewMasterVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Contract review Master)");
                throw new InvalidOperationException("Failed to load Contract review Master list.", ex);
            }
        }

        public static class ContractReviewFilterBuilder
        {
            public static IQueryable<ContractReviewMaster> ApplyFilter(
                IQueryable<ContractReviewMaster> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "Description":
                        return query.Where(x =>
                            x.Description != null &&
                            EF.Functions.Like(x.Description, $"%{val}%"));

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            EF.Functions.Like(x.CreatedBy, $"%{val}%"));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x =>
                                x.CreatedDate >= fromDate.Date);
                        return query;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x =>
                                x.CreatedDate <= toDate.Date
                                    .AddDays(1)
                                    .AddTicks(-1));
                        return query;

                    default:
                        return query;
                }
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteReviewMasterAsync(int reviewId)
        {
            try
            {
                var contractReviewMaster = await _unitOfWork.ContractReviewMasters
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.SlNo == reviewId);

                if (contractReviewMaster == null)
                    return (false, "Contract Review Master not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<ContractReviewMaster>(reviewId);

                if (usedIn != null)
                    return (false, $"Cannot delete Contract Review Master '{contractReviewMaster.Description}' because it is used in {usedIn} Screen.");

                return (true, $"Contract Review Master '{contractReviewMaster.SlNo}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Contract Review Master delete validation failed: {reviewId}");
                return (false, "Unexpected error occurred while validating Contract Review Master.");
            }
        }


        public async Task<bool> DeleteReviewMasterByReviewIdAsync(int reviewId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var reviewMaster = await _unitOfWork.ContractReviewMasters
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.SlNo == reviewId);

                if (reviewMaster == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.ContractReviewMasters.DeleteAsync(reviewMaster);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Contract Review Master List",
                    action: $"Deleted Review Description: {reviewMaster.Description}",
                    additionalInfo: $"reviewMaster Id: {reviewMaster.SlNo}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Contract Review Master: {reviewId}");
                throw;
            }
        }




        #endregion


        #region These are for ContractReviewCheckList
        //Correspond
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
          => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<string> GetContractReviewNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.ContractReviews
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.ContractNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuote != null)
                {
                    var parts = lastQuote.ContractNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating GetContractReviewNumberAsync number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate GetContractReviewNumberAsync number.");
            }
        }

        public async Task<(List<ContractReviewVM> Review, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ContractReviews.GetQueryable()
                .AsNoTracking()
                .Include(j=>j.ContractReviewSubs).ThenInclude(i=>i.Item)
                .Include(j => j.MfgPo).ThenInclude(s => s.Customer)
                .AsQueryable();

            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var vmList = _mapper.Map<List<ContractReviewVM>>(list);

            return (vmList, total);
        }

        public async Task<bool> DeleteContractReviewByIdAsync(int Id)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var review = await _unitOfWork.ContractReviews
                    .GetQueryable()
                    .Include(e => e.ContractReviewSubs)
                    .FirstOrDefaultAsync(e => e.Id == Id);

                if (review == null)
                    return false;

                var changes = new StringBuilder();

                var deleted = await _unitOfWork.ContractReviews.DeleteAsync(Id);
                if (!deleted) return false;
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "ContractReview",
                    action: $"Deleted ContractReview No: {review.ContractNo}",
                    additionalInfo: $"ContractReview Id: {review.Id}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete ContractReview: {Id}");
                throw;
            }
        }

        public async Task<ContractReviewVM> GetContractReviewByIdAsync(int Id)
        {
            try
            {
                var entity = await _unitOfWork.ContractReviews
                                .GetQueryable()
                                .AsNoTracking()
                                .Include(x => x.ContractReviewSubs)
                                    .ThenInclude(s => s.Item)
                                        .ThenInclude(i => i.Category)
                                .Include(x => x.MfgPo)
                                    .ThenInclude(p => p.Customer)
                                .FirstOrDefaultAsync(x => x.Id == Id);
                var reviewVM = _mapper.Map<ContractReviewVM?>(entity);

                return reviewVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetContractReviewByIdAsync Id:({Id})");
                return null;
            }
        }

        public async Task<ContractReviewVM> GetPoDetailsByPoIdAsync(int refPoId)
        {
            try
            {
                var mfgPoEntity = await _unitOfWork.MfgPos
                    .GetQueryable()
                    .Include(q => q.MfgPoSubs)
                        .ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .FirstOrDefaultAsync(q => q.PoId == refPoId);

                if (mfgPoEntity == null)
                    return null;

                var contractReviewVM = new ContractReviewVM
                {
                    PoId = mfgPoEntity.PoId,
                    PoNo = mfgPoEntity.PONo,
                   
                    CustName = mfgPoEntity.Customer?.CustName ?? string.Empty,
                    CustAddress = mfgPoEntity.Customer?.CustAddr ?? string.Empty,
                    CustContactNo = mfgPoEntity.Customer?.ContactNo ?? string.Empty,

                    ContractReviewSubVMS = mfgPoEntity.MfgPoSubs.Select(s => new ContractReviewSubVM
                    {
                        SlNo = s.SlNo,
                        ItemId = s.ItemId,
                        ItemCode = s.Item?.ItemCode ?? string.Empty,
                        ItemName = s.Item?.ItemName ?? string.Empty,
                        Qty = s.Qty,
                        UnitPrice=s.UnitPrice,
                        LineCGSTRate = s.LineCGSTRate,
                        LineSGSTRate = s.LineSGSTRate,
                        LineIGSTRate = s.LineIGSTRate
                    }).ToList()
                };

                return contractReviewVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Failed to get PO details for Contract Review. PoId: {refPoId}"
                );
                throw;
            }
        }

        public async Task<IEnumerable<MfgPoVM>> GetAvailableMfgPosAsync()
        {
            try
            {
                var data = await _unitOfWork.MfgPos
                                .GetQueryable()
                                .Where(es => !es.PoTally)
                                .Where(es => !_unitOfWork.ContractReviews
                                    .GetQueryable()
                                    .Any(cr => cr.PoId == es.PoId))
                                .OrderByDescending(es => es.PoId)
                                .ToListAsync();

                return _mapper.Map<List<MfgPoVM>>(data);


            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while  GetAvailableMfgPosAsync");
                throw new InvalidOperationException("Failed to generate GetAvailableMfgPosAsync number.");
            }
        }

        public async Task<ContractReviewVM> UpsertContractReview(ContractReviewVM reviewVM)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                ContractReview entity;
                if (reviewVM.Id == 0)
                {
                    entity = _mapper.Map<ContractReview>(reviewVM);

                    if (await IsDuplicateContractReviewNoAsync(entity.ContractNo, entity.Id, entity.PoId))
                        throw new InvalidOperationException("ContractReview No already exists.");

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.ContractReviewSubs = reviewVM.ContractReviewSubVMS
                    .Select(s =>
                    {
                        var sub = _mapper.Map<ContractReviewSub>(s);
                        sub.Id = entity.Id;   
                        return sub;
                    })
                    .ToList();

                    await _unitOfWork.ContractReviews.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"ContractReview Created by {currentUser}. Contractreview Number: {entity.ContractNo}");
                }
                else
                {
                    // UPDATE
                    entity = await _unitOfWork.ContractReviews
                        .GetQueryable()
                        .Include(e => e.ContractReviewSubs)
                        .FirstOrDefaultAsync(e => e.Id == reviewVM.Id)
                        ?? throw new InvalidOperationException("ContractReview not found.");

                    var parentChanges = GetPropertyChanges(entity, reviewVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(reviewVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, reviewVM.ContractReviewSubVMS, changes);
                }
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, reviewVM.Id == 0 ? "EnquiryFeasibility Sales Created" : "EnquiryFeasibility Sales Updated");

                // Return updated entity
                var savedEntity = await _unitOfWork.ContractReviews
                    .GetQueryable()
                    .Include(e => e.ContractReviewSubs).ThenInclude(s => s.Item)
                    .Include(e => e.MfgPo)
                    .FirstOrDefaultAsync(e => e.Id == entity.Id);

                return _mapper.Map<ContractReviewVM>(savedEntity!);
            }
            catch (Exception ex)
            {

                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpsertContractReview for FesId: {reviewVM.ContractNo}");
                throw;
            }
        }

        public async Task<bool> IsDuplicateContractReviewNoAsync(string ContNo, int? Id = null, int? poId = null)
        {
            try
            {
                return await _unitOfWork.ContractReviews
                    .AnyAsync(e =>
                        e.ContractNo == ContNo &&
                        (!Id.HasValue || e.Id != Id) &&
                        (!poId.HasValue || e.PoId == poId));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to check duplicate Contractreview No '{ContNo}'");
                throw;
            }
        }

        private async Task HandleChildUpdatesAsync(ContractReview existingEnquiry, List<ContractReviewSubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingEnquiry.ContractReviewSubs.Select(s => s.SubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.SubId).ToHashSet();

            // Delete removed children
            foreach (var sub in existingEnquiry.ContractReviewSubs.Where(s => !incomingSubIds.Contains(s.SubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - ContractReview SubId: {sub.SubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ContractReviewSubs.DeleteAsync(sub.SubId);
                await _unitOfWork.SaveAsync();

            }

            // Add or update children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.SubId == 0)
                {
                    var newSub = _mapper.Map<ContractReviewSub>(subVM);
                    newSub.Id = existingEnquiry.Id;
                    await _unitOfWork.ContractReviewSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();
                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}");

                }
                else
                {
                    var existingSub = existingEnquiry.ContractReviewSubs.FirstOrDefault(s => s.SubId == subVM.SubId);
                    if (existingSub != null)
                    {
                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");
                        _mapper.Map(subVM, existingSub);

                    }
                }
            }
        }

        public async Task<List<User>> GetRevisedByUsersAsync()
        {
            return await _unitOfWork.Users
                .GetQueryable()
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .Select(u => new User
                {
                    UserName = u.UserName
                })
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<UserAuthorityVM>> GetApprovedByUsersAsync()
        {
            return await _unitOfWork.UserAuthorities
                .GetQueryable()
                .AsNoTracking()
                .Select(ua => new UserAuthorityVM
                {
                    UserName = ua.User.UserName,
                })
                .Distinct()
                .OrderBy(x => x.UserName)
                .ToListAsync();
        }

        public async Task<List<ContractReviewMasterVM>> GetMasterAsync()
        {
            return await _unitOfWork.ContractReviewMasters
                .GetQueryable()
                .AsNoTracking()
                .Select(ua => new ContractReviewMasterVM
                {
                    SlNo=ua.SlNo,
                    Description = ua.Description,
                })
                .Distinct()
                .OrderBy(x => x.SlNo)
                .ToListAsync();
        }

        public async Task<string> GetNextOANumberAsync(string suffix)
        {
            try
            {
                var lastOA = await _unitOfWork.ContractReviews
                    .GetQueryable()
                    .Where(x => !string.IsNullOrEmpty(x.OANo) && x.OANo.EndsWith(suffix))
                    .OrderByDescending(x => x.Id) // or CreatedOn
                    .Select(x => x.OANo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (!string.IsNullOrWhiteSpace(lastOA))
                {
                    var numberPart = lastOA.Split('/')[0];

                    if (int.TryParse(numberPart, out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                return $"{nextNumber}/{suffix}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating OA number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Order Acceptance number.");
            }
        }


        public async Task UpdateOADetails(string contractNo, string generatedOANo,DateTime? OADate,int poid)
        {
            var contract = await _unitOfWork.ContractReviews
                                            .FirstOrDefaultAsync(x => x.ContractNo == contractNo);
            var pos = await _unitOfWork.MfgPos
                                           .FirstOrDefaultAsync(x => x.PoId == poid);

            if (contract == null && pos==null)
                return;

            contract.OANo = generatedOANo;
            contract.OADate = OADate;

            pos.OANo = generatedOANo;
            pos.OADate = OADate;
            pos.OAWONO = generatedOANo;
            pos.OAWODate = OADate;

            await _unitOfWork.SaveAsync(); 
        }

        public static class DynamicWhereBuilder
        {
            public static IQueryable<ContractReview> ApplyFilter(
                IQueryable<ContractReview> query,
                string field,
                object value)
            {
                if (value == null)
                    return query;

                string stringValue = value.ToString()?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(stringValue))
                    return query;

                switch (field)
                {
                    case "ContractNo":
                        {
                            var parts = stringValue.Split('/', 2, StringSplitOptions.TrimEntries);

                            var part1 = parts.Length > 0 ? parts[0] : string.Empty;
                            var part2 = parts.Length > 1 ? parts[1] : string.Empty;

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) ||
                                    x.ContractNo != null &&
                                    x.ContractNo.StartsWith(part1))
                                &&
                                (string.IsNullOrEmpty(part2) ||
                                    x.Suffix != null &&
                                    x.Suffix.Contains(part2))
                            );
                        }

                    case "PoNo":
                        {
                            var parts = stringValue.Split('/', 2, StringSplitOptions.TrimEntries);

                            var part1 = parts.Length > 0 ? parts[0] : string.Empty;
                            var part2 = parts.Length > 1 ? parts[1] : string.Empty;

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) ||
                                    x.MfgPo != null &&
                                    x.MfgPo.PONo.StartsWith(part1))
                                &&
                                (string.IsNullOrEmpty(part2) ||
                                    x.MfgPo.Suffix != null &&
                                    x.MfgPo.Suffix.Contains(part2))
                            );
                        }


                    case "Customer":
                        return query.Where(x =>
                            x.MfgPo != null &&
                            x.MfgPo.Customer != null &&
                            x.MfgPo.Customer.CustName.Contains(stringValue));

                    case "ItemCode":
                        return query.Where(x =>
                            x.ContractReviewSubs.Any(s =>
                                s.Item != null &&
                                s.Item.ItemCode.Contains(stringValue)));

                    case "ItemName":
                        return query.Where(x =>
                            x.ContractReviewSubs.Any(s =>
                                s.Item != null &&
                                s.Item.ItemName.Contains(stringValue)));

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            x.CreatedBy.Contains(stringValue));

                    case "FromDate":
                        if (DateTime.TryParse(stringValue, out var fromDate))
                            return query.Where(x => x.CreatedDate >= fromDate.Date);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(stringValue, out var toDate))
                            return query.Where(x => x.CreatedDate <= toDate.Date.AddDays(1).AddTicks(-1));
                        break;
                }

                return query;
            }
        }


        #endregion
    }
}
