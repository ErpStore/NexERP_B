using AutoMapper;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService;
using V.SMART.Shared.Data.HumanResource.OfferLetter;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.OfferLetter;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService
{

    public class OfferLetterService : IOfferLetterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IExcelTemplateService _excelTemplateService;

        public OfferLetterService(
           IUnitOfWork unitOfWork,
           ICommonService commonService,
           CurrentUserService userService,
           ILoggingService logs,
           IMapper mapper,
           IExcelTemplateService excelTemplateService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _excelTemplateService = excelTemplateService;

        }
        public async Task<(bool CanDelete, string Message)> CanDeleteOfferLetterAsync(int offerId)
        {
            try
            {
                var Offer = await _unitOfWork.Offers
                                .GetQueryable()
                                .Include(e => e.OfferLetterSubs)
                                .Where(e => e.OfferId == offerId).FirstOrDefaultAsync();

                if (Offer == null)
                    return (false, "Offer not found. Deletion not allowed.");


                bool hasAppointment = await _unitOfWork.AppointmentLetters
                     .GetQueryable()
                     .AnyAsync(qs => qs.CandidateID == Offer.CandidateID);

                if (hasAppointment)
                {
                    return (false, "Unable to delete the Offer Letter because an Appointment letter has already been created.");
                }



                return (true, "Sales Order can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSalesOrderAsync for OfferId: {offerId}");
                throw new Exception("Error checking Sales Order delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteOfferidByOfferidAsync(int offerid)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var po = await _unitOfWork.Offers
                    .GetQueryable()
                    .Include(e => e.OfferLetterSubs)
                    .FirstOrDefaultAsync(e => e.OfferId == offerid);

                if (po == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in po.OfferLetterSubs)
                {
                }

                await _unitOfWork.Offers.DeleteAsync(po);
                
                await _unitOfWork.SaveAsync();

                await UpdateStutas(po.CandidateID, "Selected");


                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Sales-Order List",
                    action: $"Deleted offerId: {po.OfferId}",
                    additionalInfo: $"Po offerId: {po.OfferId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete PO: {offerid}");
                throw;
            }
        }

        public async Task DeleteOfferSubItemAndResequenceAsync(OfferLetterSubVM subitem, OfferLetterVM OfferLetter)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (subitem.OfferSubId > 0)
                {
                    var entity = await _unitOfWork.OffersSubs.GetAsync(subitem.OfferSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Subitem not found.");


                    await _unitOfWork.OffersSubs.DeleteAsync(entity.OfferSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Enquiry Sales",
                        $"Deleted Subitem: {subitem.Title}",
                        $"Enquiry No: {OfferLetter?.OfferId}"
                    );
                }
                else
                {
                    OfferLetter.OfferLetterSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.OffersSubs
                    .GetQueryable()
                    .Where(x => x.OfferId == OfferLetter.OfferId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                // Update Enquiry Tally Status
                
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Error in DeleteOfferSubItemAndResequenceAsync");
                throw;
            }
        }

        public async Task<CandidateVM?> GetCandidateByIdAsync(int CandidateId)
        => await _commonService.GetCandidateByIdAsync(CandidateId);

        public async Task<List<CandidateVM>> GetCandidatesAsync(int? CandidateId = null)
        {
            if (CandidateId.HasValue && CandidateId.Value > 0)
            {
                var Candidate = await _commonService.GetCandidateByIdAsync(CandidateId.Value);
                return Candidate != null ? new List<CandidateVM> { Candidate } : new List<CandidateVM>();
            }
            return await _commonService.GetAllActiveCadidatesAsync();

        }

        public async Task<OfferLetterVM> GetOfferidByOfferidAsync(int offerid)
        {
            try
            {
                var entity = await _unitOfWork.Offers.GetQueryable().AsSplitQuery().AsNoTracking()
                    .Include(q => q.OfferLetterSubs)
                    .FirstOrDefaultAsync(q => q.OfferId == offerid);

                return _mapper.Map<OfferLetterVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPoByPoIdAsync({offerid})");
                return null;
            }
        }

        public async Task<List<OfferLetterSubVM>> GetOfferSubByOfferLetterIdAsync(int OfferId)
        {
            try
            {
                var subs = await _unitOfWork.OffersSubs
                    .GetQueryable()
                    .Where(s => s.OfferId == OfferId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<OfferLetterSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetOfferSubByofferIdAsync for OfferId: {OfferId}");
                throw;
            }
        }

        public async Task<bool> IsDocumentUploaded(int offerid)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == " Offer Letter" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == offerid);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }

        public async Task<IEnumerable<CandidateVM>> SearchCandidatesAsync(string searchText)
        {
            return await _commonService.SearchCandidatesAsync(searchText);
        }

        public async Task<(List<OfferLetterVM> OfferVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {

                var query = _unitOfWork.Offers
                .GetQueryable()
                .Include(x => x.Candidate)
                .Include(x => x.OfferLetterSubs).AsQueryable();

                

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = MfgPoFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query.AsNoTracking().AsSplitQuery()
                    .OrderByDescending(x => x.OfferId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<OfferLetterVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (MfgPo)");
                throw new InvalidOperationException("Failed to load manufacturing PO list.", ex);
            }
        }

        public async Task<OfferLetterVM> UpsertOfferLetterAsync(OfferLetterVM vm)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                OfferLetter entity;

                if (vm.OfferId == 0)
                {
                    // INSERT
                    entity = _mapper.Map<OfferLetter>(vm);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.OfferLetterSubs = vm.OfferLetterSubVMs
                        .Select(s => _mapper.Map<OfferLetterSub>(s))
                        .ToList();

                    await _unitOfWork.Offers.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    await UpdateStutas(vm.CandidateID, "Offer Sent");

                    changes.AppendLine($"Offer Letter Created by {currentUser}. OfferId: {entity.OfferId}");

                  
                }
                else
                {
                    // UPDATE
                    entity = await _unitOfWork.Offers
                        .GetQueryable()
                        .Include(e => e.OfferLetterSubs)
                        .FirstOrDefaultAsync(e => e.OfferId == vm.OfferId)
                        ?? throw new InvalidOperationException("Enquiry not found.");

                    var parentChanges = GetPropertyChanges(entity, vm);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(vm, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                   await HandleChildUpdatesAsync(entity, vm.OfferLetterSubVMs, changes);

                }
                await _unitOfWork.SaveAsync();

                

                await transaction.CommitAsync();

                await LogChangesAsync(changes, vm.OfferId == 0 ? "Offer Letter Created" : "Offer Letter Updated");

                // Return updated entity
                var savedEntity = await _unitOfWork.Offers
                    .GetQueryable()
                    .Include(e => e.Candidate)
                    .FirstOrDefaultAsync(e => e.OfferId == entity.OfferId);

                return _mapper.Map<OfferLetterVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpsertOfferLetterAsync for OfferId: {vm.OfferId}");
                throw;
            }
        }
        private async Task HandleChildUpdatesAsync(OfferLetter existingOfferLetter, List<OfferLetterSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubIds = existingOfferLetter.OfferLetterSubs.Select(s => s.OfferSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.OfferSubId).ToHashSet();

                foreach (var sub in existingOfferLetter.OfferLetterSubs.Where(s => !incomingSubIds.Contains(s.OfferSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - EnquirySubId: {sub.OfferSubId}");
                    await _unitOfWork.OffersSubs.DeleteAsync(sub.OfferSubId);
                    await _unitOfWork.SaveAsync();
                    
                }

                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.OfferSubId == 0)
                    {
                        var newSub = _mapper.Map<OfferLetterSub>(subVM);
                        newSub.OfferId = existingOfferLetter.OfferId;
                        await _unitOfWork.OffersSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();
                       
                    }
                    else
                    {
                        var existingSub = existingOfferLetter.OfferLetterSubs.FirstOrDefault(s => s.OfferSubId == subVM.OfferSubId);
                        if (existingSub != null)
                        {
                            

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            
                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "[HandleChildUpdatesAsync - Enquiry] Failed to update enquiry children");
                throw new InvalidOperationException("Failed to update Enquiry details. Please contact support.");
            }
        }

        public static class MfgPoFilterBuilder
        {
            public static IQueryable<OfferLetter> ApplyFilter(
                IQueryable<OfferLetter> query,
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
                       

                       

                        case "Candidate":
                            return query.Where(x =>
                                x.Candidate != null &&
                                x.Candidate.CandidateName.Contains(val));

                       
                        

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.CreatedDate >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.CreatedDate <= toDate.Date.AddDays(1).AddTicks(-1));
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
                screen: "Offer Letter",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        private async Task UpdateStutas(int? CandidateId,string Status)
        {
            if (!CandidateId.HasValue || CandidateId == 0) return;
            var Candidate = await _unitOfWork.Candidate.GetAsync(CandidateId.Value);
            if (Candidate == null) return;
            Candidate.Status = Status;
            await _unitOfWork.Candidate.UpdateAsync(Candidate);
            await _unitOfWork.SaveAsync();
        }

        public async Task<OfferLetterVM?> GetOfferDetailsByOfferIdAsync(int OfferId)
        {
            var entity = await _unitOfWork.Offers
                     .GetQueryable()
                     .Include(q => q.OfferLetterSubs) 
                     .Include(q => q.Candidate)
                     .FirstOrDefaultAsync(q => q.OfferId == OfferId);

            return _mapper.Map<OfferLetterVM>(entity);
        }

        public async Task<List<string>> GetOfferLetterPositionsAsync()
        {
            return await _unitOfWork.Offers
                .GetQueryable()
                .Include(o => o.Candidate)
                .Where(o => o.Candidate != null)
                .Select(o => o.Candidate.Position)
                .Distinct()
                .ToListAsync();
        }

        public async Task<OfferLetterVM> GetLastOfferLetterByPositionAsync(string position)
        {
            var entity = await _unitOfWork.Offers
                .GetQueryable()
                .Include(o => o.OfferLetterSubs)
                .Include(o => o.Candidate)
                .Where(o => o.Candidate.Position == position)
                .OrderByDescending(o => o.OfferId)   // Latest offer letter
                .FirstOrDefaultAsync();

            return _mapper.Map<OfferLetterVM>(entity);
        }

        public async Task<string> GetOfferNoAsync()
        {
            try
            {
                var highest = await _unitOfWork.Offers
                            .GetQueryable()
                            .Select(e => new { OfferNo = Convert.ToInt32(e.OfferNo) })
                            .OrderByDescending(e => e.OfferNo)
                            .Select(e => e.OfferNo)
                            .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(highest.ToString()))
                    return "0001"; // first enquiry number


                if (!int.TryParse(highest.ToString(), out int numericPart))
                    numericPart = 0;


                return (numericPart + 1).ToString("D4");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GenerateEnquiryNumberAsync()");
                return string.Empty;
            }
        }
    }


}
