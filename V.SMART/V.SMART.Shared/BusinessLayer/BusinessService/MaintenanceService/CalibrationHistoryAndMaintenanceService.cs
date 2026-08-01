using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService;
using V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MaintenanceService
{
    public class CalibrationHistoryAndMaintenanceService : ICalibrationHistoryAndMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        public CalibrationHistoryAndMaintenanceService(
                                                        IUnitOfWork unitOfWork,
                                                        ICommonService commonService,
                                                        CurrentUserService userService,
                                                        ILoggingService logs,
                                                        IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        //Get All Instruments
        public async Task<List<ItemVM>> GetAllInstrumentsAsync()
            => await _commonService.GetAllItemsByCategoryCode(5);

        #region Search Calibration Histories
        public async Task<(List<CalibrationHistoryVM> CalibrationVMs, int TotalCount)>
        SearchCalibrationHistoriesAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.CalibrationHistories
                    .GetQueryable()
                    .Include(x => x.Instrument)
                        .ThenInclude(i => i.Item)
                    .AsQueryable();

                // Apply dynamic filters
                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = CalibrationFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.CalibrateId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<CalibrationHistoryVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchCalibrationHistoriesAsync");
                throw new InvalidOperationException("Failed to load calibration history list.", ex);
            }
        }

        public static class CalibrationFilterBuilder
        {
            public static IQueryable<CalibrationHistory> ApplyFilter(
                IQueryable<CalibrationHistory> query,
                string field,
                object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString()!.Trim();

                switch (field)
                {
                    case "Instrument":
                        return query.Where(x =>
                            x.Instrument != null &&
                            x.Instrument.Item.ItemCode.Contains(val));

                    case "UniqueID":
                        return query.Where(x =>
                            x.Instrument != null &&
                            x.Instrument.UniqueId != null &&
                            x.Instrument.UniqueId.Contains(val));

                    case "Frequency":
                        return query.Where(x =>
                            x.Instrument != null &&
                            x.Instrument.Frequency != null &&
                            x.Instrument.Frequency.Contains(val));

                    case "Range":
                        return query.Where(x =>
                            x.Instrument != null &&
                            x.Instrument.Range != null &&
                            x.Instrument.Range.Contains(val));

                    case "LeastCount":
                        return query.Where(x =>
                            x.Instrument != null &&
                            x.Instrument.LeastCount!= null &&
                            x.Instrument.LeastCount.Contains(val));

                    case "ReportNo":
                        return query.Where(x =>
                            x.CalibrateNo != null &&
                            x.CalibrateNo.Contains(val));

                    case "Authorised":
                        return query.Where(x =>
                            x.Authorised != null &&
                            x.Authorised.Contains(val));
                        

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.PrevDate >= fromDate.Date);
                        return query;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x =>
                                x.PrevDate <= toDate.Date.AddDays(1).AddTicks(-1));
                        return query;

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            x.CreatedBy.Contains(val));
                }

                return query;
            }
        }

        #endregion


        #region Get Calibration By Id
        public async Task<CalibrationHistoryVM> GetCalbrationById(int id)
        {
            try
            {
                var history = await _unitOfWork.CalibrationHistories.GetQueryable()
                         .Include(x => x.Instrument)
                         .ThenInclude(i => i.Item)
                         .FirstOrDefaultAsync(i => i.CalibrateId == id);

                if (history == null)
                    return null;

                var historyVM = _mapper.Map<CalibrationHistoryVM>(history);

                if (historyVM != null)
                {
                    return historyVM;
                }
                else
                {
                    return new CalibrationHistoryVM();
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetCalbrationById()");
                return new CalibrationHistoryVM();
            }
        }
        #endregion

        #region Find All UID(Unique Identification Number)
        public async Task<List<InstrumentDetailsVM>> FindAllUID(int ItemId)
        {
            try
            {
                var instrumentDetails = await _unitOfWork.AllInstrumentDetails.GetQueryable()
                                        .Where(id => id.ItemId == ItemId).ToListAsync();
                var instrumentDetailsVM = _mapper.Map<List<InstrumentDetailsVM>>(instrumentDetails);
                if (instrumentDetailsVM != null)
                {
                    return instrumentDetailsVM;
                }
                else
                {
                    return new List<InstrumentDetailsVM>();
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in FindAllUID()");
                return new List<InstrumentDetailsVM>();
            }
        }
        #endregion

        #region IsUniqueIdExists
        public async Task<bool> IsUniqueIdExists(int ItemId, string uniqueId)
        {
            try
            {
                return await _unitOfWork.AllInstrumentDetails.GetQueryable()
                        .AnyAsync(id => id.ItemId == ItemId && id.UniqueId == uniqueId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsUniqueIdExists()");
                return false;
            }
        }
        #endregion

        #region IsReportNoExists
        public async Task<bool> IsReportNoExists(string calibrateno)
        {
            try
            {
                return await _unitOfWork.CalibrationHistories.GetQueryable()
                        .AnyAsync(id => id.CalibrateNo == calibrateno);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsReportNoExists()");
                return false;
            }
        }
        #endregion

        #region Creating Instrument Details
        public async Task<InstrumentDetailsVM> CreateInstrumentDetailsAsync(InstrumentDetailsVM instrumentDetails)
        {
            try
            {
                if (instrumentDetails == null)
                {
                    throw new ArgumentNullException(nameof(instrumentDetails));
                }
                InstrumentDetails instrument = _mapper.Map<InstrumentDetails>(instrumentDetails);
                await _unitOfWork.AllInstrumentDetails.CreateAsync(instrument);
                await _unitOfWork.SaveAsync();
                return _mapper.Map<InstrumentDetailsVM>(instrument);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in CreateInstrumentDetailsAsync()");
                return new InstrumentDetailsVM();
            }
        }
        #endregion

        #region Updating Instrument Details
        public async Task<InstrumentDetailsVM> UpdateInstrumentDetails(InstrumentDetailsVM instrumentDetails)
        {
            try
            {
                if (instrumentDetails == null)
                {
                    throw new ArgumentNullException(nameof(instrumentDetails));
                }

                // Map VM → Entity
                var instrumentEntity = _mapper.Map<InstrumentDetails>(instrumentDetails);

                await _unitOfWork.AllInstrumentDetails.UpdateAsync(instrumentEntity);
                await _unitOfWork.SaveAsync();

                // 🔥 RELOAD with Item included
                var updatedEntity = await _unitOfWork.AllInstrumentDetails
                    .GetQueryable()
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == instrumentEntity.Id);

                return _mapper.Map<InstrumentDetailsVM>(updatedEntity);

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in UpdateInstrumentDetails()");
                return new InstrumentDetailsVM();
            }
        }
        #endregion

        #region Create Calibration History
        public async Task CreateCalibrationAsync(CalibrationHistoryVM calibrationHistory)
        {
            try
            {
                if (calibrationHistory == null)
                {
                    throw new ArgumentNullException(nameof(calibrationHistory));
                }
                CalibrationHistory calibrationVM = _mapper.Map<CalibrationHistory>(calibrationHistory);
                await _unitOfWork.CalibrationHistories.CreateAsync(calibrationVM);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in CreateCalibrationAsync()");
            }
        }
        #endregion

        #region Delete Calibration History
        public async Task<bool> DeleteCalibrationHistory(int id)
        {
            try
            {
                var history = await GetCalbrationById(id);
                if (history == null)
                    return false;

                // Get all calibration records for same instrument + unique id
                var instrumentHistories = await _unitOfWork.CalibrationHistories
                    .GetQueryable()
                    .Where(i => i.InstrumentId == history.InstrumentId &&
                                i.Instrument.UniqueId == history.UniqueId)
                    .ToListAsync();

                // If this is the only calibration record, delete instrument master
                if (instrumentHistories.Count == 1)
                {
                    await _unitOfWork.AllInstrumentDetails.DeleteAsync(history.InstrumentId);
                }

                // Delete the calibration history
                var result = await _unitOfWork.CalibrationHistories.DeleteAsync(id);

                if (result)
                {
                    await _unitOfWork.SaveAsync();
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in DeleteCalibrationHistory()");
                return false;
            }
        }
        #endregion

        #region Get InstrumentDetails By Id 
        public async Task<InstrumentDetailsVM> GetInstrumentDetailsById(int id)
        {
            try
            {
                var entity = await _unitOfWork.AllInstrumentDetails.GetQueryable()
                    .Include(x => x.Item)
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (entity == null)
                    return null;

                var vm = _mapper.Map<InstrumentDetailsVM>(entity);
                return vm;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetInstrumentDetailsById()");
                return new InstrumentDetailsVM();
            }
        }
        #endregion

        #region Updating the Cancel Status
        public async Task UpdatedCancelStatus(CalibrationHistoryVM calibrate)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingcalibrate = await _unitOfWork.CalibrationHistories.GetAsync(calibrate.CalibrateId??0);
                if (existingcalibrate == null)
                    throw new InvalidOperationException("Calibration History not found.");

                existingcalibrate.CalibrateCancl = calibrate.CalibrateCancl;
                existingcalibrate.CancelReason = calibrate.CancelReason;
                await _unitOfWork.CalibrationHistories.UpdateAsync(existingcalibrate);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Unexpected error");
                throw new InvalidOperationException("Failed to update cancel/revert status. Please contact support.");
            }
        }
        #endregion
    }
}
