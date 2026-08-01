using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class ProcessLogService : IProcessLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public ProcessLogService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            IMapper mapper,
            ILoggingService logs)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _mapper = mapper;
            _logs = logs;
        }

        public async Task<List<ProductionLogVM>> GetDailyProductionLogsAsync(int rcSubId)
        {
            try
            {
                var logs = await _unitOfWork.ProductionLogs
                    .GetQueryable()
                    .Where(x => x.RCProcessId == rcSubId)
                    .Include(x => x.Machine)
                    .Include(x => x.Process)
                    .ToListAsync();

                return _mapper.Map<List<ProductionLogVM>>(logs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Failed to load Daily Production Logs for RCSubId : {rcSubId}");
                throw;
            }
        }

        public async Task<List<ProductionIssueCompVM>> GetProductionIssueLogsAsync(int rcSubId)
        {
            try
            {
                var issueLogs = await _unitOfWork.ProductionIssueComps
                    .GetQueryable()
                    .Include(x => x.ProductionIssueCompSubs
                        .Where(s => s.RcSubId == rcSubId && s.TransType == "Out"))
                        .ThenInclude(s => s.Machine)
                    .Include(x => x.ProductionIssueCompSubs
                        .Where(s => s.RcSubId == rcSubId && s.TransType == "Out"))
                        .ThenInclude(s => s.Process)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<List<ProductionIssueCompVM>>(issueLogs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Failed to load Production Issue Component Logs for RCSubId : {rcSubId}");
                throw;
            }
        }

        public async Task<List<ProductionReturnCompVM>> GetProductionReturnLogsAsync(int rcSubId)
        {
            try
            {
                var returnLogs = await _unitOfWork.ProductionReturnComps
                    .GetQueryable()
                    .Include(x => x.ProductionReturnCompSubs
                        .Where(s => s.RefRcSubId == rcSubId))
                        .ThenInclude(s => s.Machine)
                    .Include(x => x.ProductionReturnCompSubs
                        .Where(s => s.RefRcSubId == rcSubId))
                        .ThenInclude(s => s.Process)
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<List<ProductionReturnCompVM>>(returnLogs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Failed to load Production Return Component Logs for RCSubId : {rcSubId}");
                throw;
            }
        }

        public async Task<List<ProductionSCNCompVM>> GetProductionSCNLogsAsync(int rcSubId)
        {
            try
            {
                var returnLogs = await _unitOfWork.ProductionSCNComps
                    .GetQueryable()
                    .Include(x => x.ProductionSCNCompSubs
                        .Where(s => s.RcSubId == rcSubId))
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<List<ProductionSCNCompVM>>(returnLogs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Failed to load Production SCN Component Logs for RCSubId : {rcSubId}");
                throw;
            }
        }


    }
}
