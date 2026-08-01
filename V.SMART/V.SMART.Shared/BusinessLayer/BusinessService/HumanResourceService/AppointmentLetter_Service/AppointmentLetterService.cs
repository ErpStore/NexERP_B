using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService;
using V.SMART.Shared.Data.HumanResource.AppointmentLetter;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AppointmentLetterVM;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService
{
    public class AppointmentLetterService : IAppointmentLetterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IExcelTemplateService _excelTemplateService;
        public AppointmentLetterService(
             IUnitOfWork unitOfWork,
           ICommonService commonService,
           CurrentUserService userService,
           ILoggingService logs,
           IMapper mapper,
           IExcelTemplateService excelTemplateService )
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _excelTemplateService = excelTemplateService;
        }
       

        public async Task<(bool CanDelete, string Message)> CanDeleteAppointmentletterAsync(int appointmentId)
        {
            try
            {
                var po = await _unitOfWork.AppointmentLetters
                                .GetQueryable()
                                .Include(e => e.AppointmetLetterSubs)
                                .Where(e => e.AppointmentId == appointmentId).FirstOrDefaultAsync();

                var candidate = await _unitOfWork.Candidate  .GetQueryable().Where(c => c.CandidateID == po.CandidateID).FirstOrDefaultAsync();

                bool hasStaff = await _unitOfWork.Staffs
                     .GetQueryable()
                     .AnyAsync(qs => qs.StaffName == candidate.CandidateName);

                if (hasStaff)
                {
                    return (false, "Unable to delete the Appointment Letter because an Staff has already been created.");
                }
                if (po == null)
                    return (true, "Sales Order can be safely deleted.");

                var PoSubIds = po.AppointmetLetterSubs
                    .Select(es => es.AppointmentSubId)
                    .ToList();



                return (true, "Sales Order can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSalesOrderAsync for AppointmentId: {appointmentId}");
                throw new Exception("Error checking Sales Order delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteAppointmentIdByAppointmentIdAsync(int AppointmentId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var po = await _unitOfWork.AppointmentLetters
                    .GetQueryable()
                    .Include(e => e.AppointmetLetterSubs)
                    .FirstOrDefaultAsync(e => e.AppointmentId == AppointmentId);

                if (po == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in po.AppointmetLetterSubs)
                {
                }

                await _unitOfWork.AppointmentLetters.DeleteAsync(po);
                await _unitOfWork.SaveAsync();

                await UpdateStutas(po.CandidateID, "Offer Sent");

                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Sales-Order List",
                    action: $"Deleted offerId: {po.AppointmentId}",
                    additionalInfo: $"Po offerId: {po.AppointmentId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete PO: {AppointmentId}");
                throw;
            }
        }

        public async Task DeleteOfferSubItemAndResequenceAsync(AppointmentLetterSubVM subitem, AppointmentLetterVM appointmentLetter)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (subitem.AppointmentSubId > 0)
                {
                    var entity = await _unitOfWork.AppointmentLetterSubs.GetAsync(subitem.AppointmentSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Subitem not found.");


                    await _unitOfWork.AppointmentLetterSubs.DeleteAsync(entity.AppointmentSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Enquiry Sales",
                        $"Deleted Subitem: {subitem.Title}",
                        $"Enquiry No: {appointmentLetter?.OfferId}"
                    );
                }
                else
                {
                    appointmentLetter.AppointmentLetterSubVM.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.AppointmentLetterSubs
                    .GetQueryable()
                    .Where(x => x.AppointmentId == appointmentLetter.AppointmentId)
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

        public async Task<AppointmentLetterVM?> GetAppointmentLetterDetailsByAppointmentIdAsync(int AppointmentId)
        {
            var entity = await _unitOfWork.AppointmentLetters
                     .GetQueryable()
                     .Include(q => q.AppointmetLetterSubs)
                     .Include(q => q.Candidate)
                     .FirstOrDefaultAsync(q => q.AppointmentId == AppointmentId);

            return _mapper.Map<AppointmentLetterVM>(entity);
        }

        public async Task<string> GetAppointmentLetterNoAsync()
        {
            try
            {
                var highest = await _unitOfWork.AppointmentLetters
                            .GetQueryable()
                            .Select(e => new { AppointmentNo = Convert.ToInt32(e.AppointmentNo) })
                            .OrderByDescending(e => e.AppointmentNo)
                            .Select(e => e.AppointmentNo)
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

        public async Task<List<string>> GetAppointmentLetterPositionsAsync()
        {
            return await _unitOfWork.AppointmentLetters
               .GetQueryable()
               .Include(o => o.Candidate)
               .Where(o => o.Candidate != null)
               .Select(o => o.Candidate.Position)
               .Distinct()
               .ToListAsync();
        }

        public async Task<List<AppointmentLetterSubVM>> GetAppointmentletterSubByAppointmentletterAsync(int AppointmentId)
        {
            try
            {
                var subs = await _unitOfWork.AppointmentLetterSubs
                    .GetQueryable()
                    .Where(s => s.AppointmentId == AppointmentId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<AppointmentLetterSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetOfferSubByofferIdAsync for OfferId: {AppointmentId}");
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

        public async Task<AppointmentLetterVM> GetLastAppointmentLetterByPositionAsync(string position)
        {
            var entity = await _unitOfWork.AppointmentLetters
                .GetQueryable()
                .Include(o => o.AppointmetLetterSubs)
                .Include(o => o.Candidate)
                .Where(o => o.Candidate.Position == position)
                .OrderByDescending(o => o.AppointmentId)   // Latest offer letter
                .FirstOrDefaultAsync();

            return _mapper.Map<AppointmentLetterVM>(entity);
        }

        public async Task<AppointmentLetterVM> GetAppointmentIdidByAppointmentIdAsync(int AppointmentId)
        {
            var entity = await _unitOfWork.AppointmentLetters
                      .GetQueryable()
                      .Include(q => q.AppointmetLetterSubs)
                      .Include(q => q.Candidate)
                      .FirstOrDefaultAsync(q => q.AppointmentId == AppointmentId);

            return _mapper.Map<AppointmentLetterVM>(entity);
        }

        public async Task<bool> IsDocumentUploaded(int appointmentid)
        {
            return await _unitOfWork.Correspondances.GetQueryable()
                     .AnyAsync(c =>
                         c.ReferenceType == " Appointment Letter" &&
                         c.DocumentType == "Correspondence" &&
                         c.ReferenceId == appointmentid);
        }

        public async Task<IEnumerable<CandidateVM>> SearchCandidatesAsync(string searchText)
        {
            return await _commonService.SearchCandidatesAsync(searchText);
        }

        public  async Task<(List<AppointmentLetterVM> appointmentLetterVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {

                var query = _unitOfWork.AppointmentLetters
                .GetQueryable()
                .Include(x => x.Candidate)
                .Include(x => x.AppointmetLetterSubs).AsQueryable();



                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = MfgPoFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query.AsNoTracking().AsSplitQuery()
                    .OrderByDescending(x => x.AppointmentId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<AppointmentLetterVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (MfgPo)");
                throw new InvalidOperationException("Failed to load manufacturing PO list.", ex);
            }
        }

        public async Task<AppointmentLetterVM> UpsertAppointmentLetterAsync(AppointmentLetterVM AppointmentLetterVM)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                AppointmentLetter entity;

                if (AppointmentLetterVM.AppointmentId == 0)
                {
                    // INSERT
                    entity = _mapper.Map<AppointmentLetter>(AppointmentLetterVM);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.AppointmetLetterSubs = AppointmentLetterVM.AppointmentLetterSubVM
                        .Select(s => _mapper.Map<AppointmetLetterSub>(s))
                        .ToList();

                    await _unitOfWork.AppointmentLetters.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    await UpdateStutas(AppointmentLetterVM.CandidateID, "Joined");

                    changes.AppendLine($"Offer Letter Created by {currentUser}. OfferId: {entity.AppointmentId}");


                }
                else
                {
                    // UPDATE
                    entity = await _unitOfWork.AppointmentLetters
                        .GetQueryable()
                        .Include(e => e.AppointmetLetterSubs)
                        .FirstOrDefaultAsync(e => e.AppointmentId == AppointmentLetterVM.AppointmentId)
                        ?? throw new InvalidOperationException("Enquiry not found.");

                    var parentChanges = GetPropertyChanges(entity, AppointmentLetterVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(AppointmentLetterVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                     await HandleChildUpdatesAsync(entity, AppointmentLetterVM.AppointmentLetterSubVM, changes);

                }
                await _unitOfWork.SaveAsync();



                await transaction.CommitAsync();

                await LogChangesAsync(changes, AppointmentLetterVM.AppointmentId == 0 ? "Appointment Letter Created" : "Appointment Letter Updated");

                // Return updated entity
                var savedEntity = await _unitOfWork.AppointmentLetters
                    .GetQueryable()
                    .Include(e => e.Candidate)
                    .FirstOrDefaultAsync(e => e.AppointmentId == entity.AppointmentId);

                return _mapper.Map<AppointmentLetterVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpsertOfferLetterAsync for OfferId: {AppointmentLetterVM.AppointmentId}");
                throw;
            }
        }
        private async Task HandleChildUpdatesAsync(AppointmentLetter existingOfferLetter, List<AppointmentLetterSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubIds = existingOfferLetter.AppointmetLetterSubs.Select(s => s.AppointmentSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.AppointmentSubId).ToHashSet();

                foreach (var sub in existingOfferLetter.AppointmetLetterSubs.Where(s => !incomingSubIds.Contains(s.AppointmentSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - EnquirySubId: {sub.AppointmentSubId}");
                    await _unitOfWork.EnquirySalesSubs.DeleteAsync(sub.AppointmentSubId);
                    await _unitOfWork.SaveAsync();

                }

                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.AppointmentSubId == 0)
                    {
                        var newSub = _mapper.Map<AppointmetLetterSub>(subVM);
                        newSub.AppointmentId = existingOfferLetter.AppointmentId;
                        await _unitOfWork.AppointmentLetterSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                    }
                    else
                    {
                        var existingSub = existingOfferLetter.AppointmetLetterSubs.FirstOrDefault(s => s.AppointmentSubId == subVM.AppointmentSubId);
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

        private async Task UpdateStutas(int? CandidateId,string Status)
        {
            if (!CandidateId.HasValue || CandidateId == 0) return;
            var Candidate = await _unitOfWork.Candidate.GetAsync(CandidateId.Value);
            if (Candidate == null) return;
            Candidate.Status = Status;
            await _unitOfWork.Candidate.UpdateAsync(Candidate);
            await _unitOfWork.SaveAsync();
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
                screen: "Appoint Letter",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public static class MfgPoFilterBuilder
        {
            public static IQueryable<AppointmentLetter> ApplyFilter(
                IQueryable<AppointmentLetter> query,
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
    }
}
