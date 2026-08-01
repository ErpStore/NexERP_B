using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;


namespace V.SMART.Shared.BusinessLayer.BusinessService.MaintenanceService
{
    public class MaintenanceScheduleService : IMaintenanceScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        public MaintenanceScheduleService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            ILoggingService js)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        #region Get MaintenanceSchedule By Id
        public async Task<MaintenanceScheduleVM> GetMaintenanceScheduleById(int ScheduleId)
        {
            try
            {
                var entity = await _unitOfWork.MaintenanceSchedules.GetQueryable()
                    .Where(i => i.ScheduleId == ScheduleId).FirstOrDefaultAsync();
                var MaintenanceVMs = _mapper.Map<MaintenanceScheduleVM>(entity);
                return MaintenanceVMs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetMaintenanceScheduleById");
                return new MaintenanceScheduleVM();
            }
        }
        #endregion

        #region Delete Maintenance Schedule
        public async Task<bool> DeleteMaintenanceScheduleAsync(int scheduleId)
        {
            try
            {
                var result = await _unitOfWork.MaintenanceSchedules.DeleteAsync(scheduleId);
                if (result)
                {
                    await _unitOfWork.SaveAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in DeleteMaintenanceScheduleAsync for ID : {scheduleId}");
                return false;
            }
        }
        #endregion

        #region Create Maintenance Schedule
        public async Task CreateMaintenanceSchedule(MaintenanceScheduleVM maintenanceSchedule)
        {
            try
            {
                if (maintenanceSchedule == null)
                {
                    throw new ArgumentNullException(nameof(maintenanceSchedule));
                }
                MaintenanceSchedule scheduleEntity = _mapper.Map<MaintenanceSchedule>(maintenanceSchedule);
                await _unitOfWork.MaintenanceSchedules.CreateAsync(scheduleEntity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in CreateMaintenanceSchedule: ");
            }
        }

        #endregion

        #region Update Maintenance Schedule
        public async Task UpdateMaintenanceScheduleAsync(MaintenanceScheduleVM maintenanceSchedule)
        {
            try
            {
                if (maintenanceSchedule == null)
                {
                    throw new ArgumentNullException(nameof(maintenanceSchedule));
                }
                MaintenanceSchedule scheduleEntity = _mapper.Map<MaintenanceSchedule>(maintenanceSchedule);
                await _unitOfWork.MaintenanceSchedules.UpdateAsync(scheduleEntity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in UpdateMaintenanceScheduleAsync: ");
            }
        }
        #endregion

        #region Get All Maintenance Schedule VMs
        public async Task<List<MaintenanceScheduleVM>> GetAllMaintenanceVMsAsync()
        {
            try
            {
                var query = _unitOfWork?.MaintenanceSchedules?.GetQueryable();

                if (query == null)
                    return new List<MaintenanceScheduleVM>();

                var ScheduleEntities = await query
                                        .Include(ms => ms.Machine)
                                        .ToListAsync();

                var MaintenanceVMs = _mapper.Map<List<MaintenanceScheduleVM>>(ScheduleEntities);
                return MaintenanceVMs ?? new List<MaintenanceScheduleVM>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetAllMaintenanceVMsAsync: ");
                return new List<MaintenanceScheduleVM>();
            }
        }

        #endregion

        #region Find Maintenance Schedules by Machine ID
        public async Task<List<MaintenanceScheduleVM>> FindMachineIdById(int MachId)
        {
            try
            {
                var entity = await _unitOfWork.MaintenanceSchedules.GetQueryable()
                              .Include(ms => ms.Machine)
                              .Where(m => m.MachineId == MachId)
                              .OrderBy(m => m.MaintenanceId)
                              .ToListAsync();
                var MaintenanceVMs = _mapper.Map<List<MaintenanceScheduleVM>>(entity);
                return MaintenanceVMs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in FindMachineIdById: ");
                return new List<MaintenanceScheduleVM>();
            }
        }
        #endregion

        #region Get Document No
        public async Task<string> GenerateDocumentNo()
        {
            try
            {
                var query = _unitOfWork?.MaintenanceSchedules?.GetQueryable();

                if (query == null)
                    return "1";

                var lastDocNo = await query
                    .OrderByDescending(x => x.DocNo)
                    .Select(x => x.DocNo)
                    .FirstOrDefaultAsync();

                int docNo = 1;

                if (!string.IsNullOrEmpty(lastDocNo) && int.TryParse(lastDocNo, out var parsed))
                    docNo = parsed + 1;

                return docNo.ToString();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GenerateDocumentNo in Maintainance Schedule");
                return "1";
            }
        }

        #endregion

    }
}
