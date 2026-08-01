using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.HRMasterService
{
    public class CandidateService:ICandidateService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        public CandidateService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteCandidateAsync(int candidateID)
        {
            try
            {
                var candidate = await _unitOfWork.Candidate
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.CandidateID == candidateID);

                if (candidate == null)
                    return (false, "Candidate not found. Deletion not allowed.");


                bool hasOffer = await _unitOfWork.Offers
                     .GetQueryable()
                     .AnyAsync(qs => qs.CandidateID == candidateID);

                if (hasOffer)
                {
                    return (false, "Unable to delete the candidate because an offer letter has already been created.");
                }

                return (true, "Candidate can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanDeleteAsync for CandidateID: {candidateID}");
                throw new Exception("Error while checking Employee delete eligibility.", ex);
            }
        }

        public  async Task<bool> DeleteCandidateAsync(int cadidateid)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var candidate = await _unitOfWork.Candidate.GetQueryable()
                    .FirstOrDefaultAsync(c => c.CandidateID == cadidateid);

                if (candidate == null)
                {
                    return false;
                }

                string Candidatename = candidate.CandidateName ?? "Unknown";

                var changes = new StringBuilder();

                // Delete the customer
                await _unitOfWork.Candidate.DeleteAsync(candidate);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "candidate List",
                    action: $"Deleted Candidate: {candidate.CandidateName}",
                    additionalInfo: $"candidate Id: {candidate.CandidateID}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete Candidate: {cadidateid}");
                throw;
            }
        }

        public async Task<(List<CandidateVM> CandidateVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.Candidate.GetQueryable();

            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.CandidateID)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 🔥 Map Entity List -> VM List
            var list = _mapper.Map<List<CandidateVM>>(entities);

            return (list, total);
        }

        public static class DynamicWhereBuilder
        {
            public static IQueryable<Candidate> ApplyFilter(
                IQueryable<Candidate> query, string field, object value)
            {
                if (value == null) return query;

                switch (field)
                {
                    case "CandidateName":
                        return query.Where(x => x.CandidateName.Contains(value.ToString()));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(value.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(value.ToString()));
                }

                return query;
            }
        }
    }
}
