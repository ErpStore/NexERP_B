using AutoMapper;
using AutoMapper.QueryableExtensions;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService.IEnquiryFesibility;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.SalesAndLabour.Fesibility;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.FeasibilityVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService.EnqiryFeasibility
{
    public class EnqiryFeasibilityService : IEnqiryFeasibilityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;

        public EnqiryFeasibilityService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            ICommonService commonService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;
        }

        #region Common Services
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
       => await _commonService.GetItemByItemIdAsync(itemId);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<IEnumerable<string>> ToGetActiveUsers()
        {
            try
            {
                return await _commonService.GetAllActiveUserNamesAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to get active users");
                throw;
            }
        }

        #endregion

        #region EnquiryFeasibility Operations

        public async Task<(List<EnquiryFeasibilityVM> enqFesVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.EnquiryFeasibilities
                    .GetQueryable()
                    .Include(x => x.EnquirySales)
                    .Include(x => x.Customer)
                    .Include(x => x.EnquiryFeasibilitySub)
                        .ThenInclude(s => s.Item)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = EnquiryFesFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.FesId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<EnquiryFeasibilityVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (EnquiryFeasibility)");
                throw new InvalidOperationException("Failed to load manufacturing Enquiry Feasibility list.", ex);
            }
        }

        public static class EnquiryFesFilterBuilder
        {
            public static IQueryable<EnquiryFeasibility> ApplyFilter(
                IQueryable<EnquiryFeasibility> query,
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
                        case "FesNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.FesNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                                );
                            }

                        case "EnqNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(s =>
                                        s.EnquirySales != null &&
                                        (string.IsNullOrEmpty(part1) ||
                                            s.EnquirySales.EnquiryNo.StartsWith(part1)) &&
                                        (string.IsNullOrEmpty(part2) ||
                                            (s.EnquirySales.Suffix != null && s.EnquirySales.Suffix.Contains(part2)))
                                );
                            }

                        case "Customer":
                            return query.Where(x =>
                                x.Customer != null &&
                                x.Customer.CustName.Contains(val));

                        case "ItemName":
                            return query.Where(x =>
                                x.EnquiryFeasibilitySub.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.EnquiryFeasibilitySub.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemCode.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.FesDateNow >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.FesDateNow <= toDate.Date.AddDays(1).AddTicks(-1));
                            return query;
                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }

           
        }



        public async Task<IEnumerable<EnquiryFeasibilityVM>> GetAllEnquiryFeasibilityAsync()
        {
            try
            {
                var entities = await _unitOfWork.EnquiryFeasibilities.GetAllWithIncludeAsync(q => true,
                    q => q.Customer, q => q.EnquiryFeasibilitySub);
                return _mapper.Map<IEnumerable<EnquiryFeasibilityVM>>(entities);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "GetAllEnquiryFeasibilityAsync");
                return Enumerable.Empty<EnquiryFeasibilityVM>();
            }
        }

        public async Task<EnquiryFeasibilityVM> GetEnquiryDetailsByEnqIdAsync(int enquiryId)
        {
            try
            {
                var salesEntity = await _unitOfWork.EnquirySaless
                    .GetQueryable()
                    .Include(q => q.EnquirySalesSub)
                        .ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .FirstOrDefaultAsync(q => q.EnquiryId == enquiryId);

                if (salesEntity == null)
                    return null;

                // Map EnquirySales → EnquiryFeasibility
                var feasibilityVM = new EnquiryFeasibilityVM
                {
                    RefEnqId = salesEntity.EnquiryId,
                    RefEnqNo = salesEntity.EnquiryNo,
                    RefEnqDate = salesEntity.EnquiryDate,
                    CustId = salesEntity.CustId,
                    CustomerName = salesEntity.Customer?.CustName ?? string.Empty,
                    CustomerAddress = salesEntity.Customer?.CustAddr ?? string.Empty,
                    CustomerPhoneNo = salesEntity.Customer?.ContactNo ?? string.Empty,
                    CustomerGSTNO = salesEntity.Customer?.GSTNo ?? string.Empty,

                    EnquiryFeasibilitySubVM = salesEntity.EnquirySalesSub.Select(s => new EnquiryFeasibilitySubVM
                    {

                        SlNo = s.SlNo,
                        ItemId = s.ItemId,
                        ItemCode = s.Item?.ItemCode,
                        ItemName = s.Item?.ItemName,
                        UOM = s.UOM,
                        HSNCode = s.Item.HSNCode,
                        TypeOfProject = null,
                        ToolRequired = null,
                        SpclReqOutsource = null,
                        TechCapaBilities = null,
                        DelvryTrms = null,
                        ManHrReq = 0,
                        RiskIfAny = null,
                        SpclReqCustomer = null,
                        CompbltyConfm = null,

                    }).ToList()
                };

                return feasibilityVM;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get feasibility details for EnquiryId: {enquiryId}");
                throw;
            }
        }


        public async Task<string> GetFeasibilityNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.EnquiryFeasibilities
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.FesNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuote != null)
                {
                    var parts = lastQuote.FesNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error generating GetFeasibilityNumberAsync number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate GetFeasibilityNumberAsync number.");
            }
        }
        public async Task<IEnumerable<EnquirySalesVM>> GetAvailableEnquiriesAsync()
        {
            try
            {
                var result = await _unitOfWork.EnquirySaless
                       .GetQueryable()
                       .Where(es => es.EnquiryTally == false) // <-- filter for EnquiryTally = 0
                       .Where(es => !_unitOfWork.EnquiryFeasibilities
                                       .GetQueryable()
                                       .Any(ef => ef.RefEnqNo == es.EnquiryNo))
                       .OrderByDescending(es => es.EnquiryId)
                       .ProjectTo<EnquirySalesVM>(_mapper.ConfigurationProvider)
                       .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error while  GetAvailableEnquiriesAsync");
                throw new InvalidOperationException("Failed to generate GetAvailableEnquiriesAsync number.");
            }
        }
        public async Task<EnquiryFeasibilityVM?> GetEnquiryFeasibilityDetailsByFesIdAsync(int FesId)
        {
            try
            {
                var entity = await _unitOfWork.EnquiryFeasibilities
                            .GetQueryable()
                            .Include(e => e.EnquiryFeasibilitySub)
                                .ThenInclude(s => s.Item)             // Include related Item
                            .Include(e => e.Customer)                 // Include related Customer
                            .FirstOrDefaultAsync(e => e.FesId == FesId);

                return _mapper.Map<EnquiryFeasibilityVM?>(entity);


            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get EnquiryFeasibility details for FesId: {FesId}");
                throw;
            }
        }
        public async Task<bool> DeleteEnquiryAsync(int FesId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var enquiry = await _unitOfWork.EnquiryFeasibilities
                    .GetQueryable()
                    .Include(e => e.EnquiryFeasibilitySub)
                    .FirstOrDefaultAsync(e => e.FesId == FesId);

                if (enquiry == null) return false;

                var changes = new StringBuilder();

                await _unitOfWork.EnquiryFeasibilities.DeleteAsync(enquiry);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "EnquiryFeasibility List",
                    action: $"Deleted EnquiryFeasibility: {enquiry.FesNo}",
                    additionalInfo: $"EnquiryFeasibility Id: {enquiry.FesId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete EnquiryFeasibility FesId: {FesId}");
                throw;
            }
        }

        public async Task<EnquiryFeasibilityVM> UpsertEnquiryFeasibilityAsync(EnquiryFeasibilityVM vm)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                EnquiryFeasibility entity;
                if (vm.FesId == 0)
                {
                    entity = _mapper.Map<EnquiryFeasibility>(vm);

                    if (await IsDuplicateEnquiryFeasibilityNoAsync(entity.FesNo, entity.FesId, entity.CustId))
                        throw new InvalidOperationException("EnquiryFeasibility No already exists.");

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.EnquiryFeasibilitySub = vm.EnquiryFeasibilitySubVM
                        .Select(s => _mapper.Map<EnquiryFeasibilitySub>(s))
                        .ToList();

                    await _unitOfWork.EnquiryFeasibilities.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Enquiry Created by {currentUser}. Enquiry Number: {entity.FesNo}");
                }
                else
                {
                    // UPDATE
                    entity = await _unitOfWork.EnquiryFeasibilities
                        .GetQueryable()
                        .Include(e => e.EnquiryFeasibilitySub)
                        .FirstOrDefaultAsync(e => e.FesId == vm.FesId)
                        ?? throw new InvalidOperationException("EnquiryFeasibility not found.");

                    var parentChanges = GetPropertyChanges(entity, vm);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(vm, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, vm.EnquiryFeasibilitySubVM, changes);
                }
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, vm.FesId == 0 ? "EnquiryFeasibility Sales Created" : "EnquiryFeasibility Sales Updated");

                // Return updated entity
                var savedEntity = await _unitOfWork.EnquiryFeasibilities
                    .GetQueryable()
                    .Include(e => e.EnquiryFeasibilitySub).ThenInclude(s => s.Item)
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.FesId == entity.FesId);

                return _mapper.Map<EnquiryFeasibilityVM>(savedEntity!);
            }
            catch (Exception ex)
            {

                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Error in UpsertEnquiryFeasibilityAsync for FesId: {vm.FesId}");
                throw;
            }
        }
        private async Task HandleChildUpdatesAsync(EnquiryFeasibility existingEnquiry, List<EnquiryFeasibilitySubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingEnquiry.EnquiryFeasibilitySub.Select(s => s.FesSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.FesSubId).ToHashSet();

            // Delete removed children
            foreach (var sub in existingEnquiry.EnquiryFeasibilitySub.Where(s => !incomingSubIds.Contains(s.FesSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - EnquirySubId: {sub.FesSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.EnquiryFeasibilitySubs.DeleteAsync(sub.FesSubId);
                await _unitOfWork.SaveAsync();

            }

            // Add or update children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.FesSubId == 0)
                {
                    var newSub = _mapper.Map<EnquiryFeasibilitySub>(subVM);
                    newSub.FesId = existingEnquiry.FesId;
                    await _unitOfWork.EnquiryFeasibilitySubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();
                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}");

                }
                else
                {
                    var existingSub = existingEnquiry.EnquiryFeasibilitySub.FirstOrDefault(s => s.FesSubId == subVM.FesSubId);
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
        public async Task<bool> IsDuplicateEnquiryFeasibilityNoAsync(string fesNo, int? FesId = null, int? custId = null)
        {
            try
            {
                return await _unitOfWork.EnquiryFeasibilities
                    .AnyAsync(e =>
                        e.FesNo == fesNo &&
                        (!FesId.HasValue || e.FesId != FesId) &&
                        (!custId.HasValue || e.CustId == custId));
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to check duplicate Feasibility No '{fesNo}'");
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
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }
        #endregion
    }

}
