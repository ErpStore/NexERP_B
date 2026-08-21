using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMaintenanceService;
using V.SMART.Shared.Data.Maintenance.Maintenance_Process;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Machine = V.SMART.Shared.Data.Master.Inventory.Machine;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MaintenanceService
{
    public class MaintenanceProcessService : IMaintenanceProcessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IMaintenanceScheduleService _maintenanceScheduleService;

        public MaintenanceProcessService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            ILoggingService js,
            IMaintenanceScheduleService maintenanceScheduleService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _maintenanceScheduleService = maintenanceScheduleService;
        }

        //private List<MaintenanceScheduleVM> MaintenanceProcessList = new();

        #region isValid
        public async Task<(bool CanSave, String Message)> IsValid(List<MaintenanceProcess> list)
        {
            try
            {
                return await Task.FromResult((true, "Validation successful"));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsValid");
                return (false, String.Empty);
            }
        }
        #endregion

        #region Show MaintenanceProcess List

        public async Task<List<MaintenanceScheduleVM>> ShowMaintenanceProcessList(DateTime SelectedDate)
        {
            try
            {
                List<MaintenanceScheduleVM> MaintenanceProcessList = new();
                var MaintenancetypeList = MaintenanceTypeHelper.GetMaintenanceTypes();
                String ScheduleDay;
                List<Machine> MachineList = new();
                MachineList = await _commonService.GetAllMachineAsync();
                List<MaintenanceScheduleVM> ScheduleList = new();
                foreach (var Machine in MachineList)
                {
                    ScheduleList = await _maintenanceScheduleService.FindMachineIdById(Machine.MachineId);
                    foreach (var Schedule in ScheduleList)
                    {
                        if (Schedule.CreatedDate.Date > SelectedDate.Date)
                        {
                            continue;
                        }
                        ScheduleDay = Schedule.ScheduleDay.ToString();
                        switch (Schedule.MaintenanceId)
                        {
                            //Daily
                            case 1:
                                // MaintenanceProcessList.Add(Schedule);
                                MaintenanceProcessList.Add(new MaintenanceScheduleVM
                                {
                                    ScheduleId = Schedule.ScheduleId,
                                    MachineId = Schedule.MachineId,
                                    MachineName = Schedule.MachineName,
                                    MaintenanceId = Schedule.MaintenanceId,
                                    Description = Schedule.Description,
                                    ScheduleDay = Schedule.ScheduleDay,
                                    DocNo = Schedule.DocNo,
                                    RevisionNo = Schedule.RevisionNo,
                                    RevisionDate = Schedule.RevisionDate,
                                    CreatedDate = Schedule.CreatedDate,
                                    CreatedBy = Schedule.CreatedBy,
                                    ModifiedBy = Schedule.ModifiedBy,
                                    ModifiedDate = SelectedDate,
                                    // not mapped UI only fields
                                    MaintenanceName = Schedule.MaintenanceName,
                                    ScheduleDate = SelectedDate
                                    // 💥 only UI-date changes

                                });

                                break;
                            //Weekly
                            case 2:
                                if (ScheduleDay == SelectedDate.DayOfWeek.ToString())
                                {
                                    Schedule.ScheduleDate = SelectedDate.Date;
                                    MaintenanceProcessList.Add(Schedule);
                                }
                                break;

                            //fortnightly
                            case 3:
                                //Day 1 => 1
                                var numeric = new string(ScheduleDay.Where(char.IsDigit).ToArray());

                                if (int.TryParse(numeric, out int scheduleDay))
                                {
                                    if (scheduleDay == SelectedDate.Day || scheduleDay + 15 == SelectedDate.Day)
                                    {
                                        Schedule.ScheduleDate = SelectedDate.Date;
                                        MaintenanceProcessList.Add(Schedule);
                                    }
                                }
                                break;

                            //Monthly
                            case 4:
                                //Day 1 => 1
                                var numeric1 = new string(ScheduleDay.Where(char.IsDigit).ToArray());
                                if (int.TryParse(numeric1, out int scheduleDay1))
                                {
                                    if (scheduleDay1 == SelectedDate.Day)
                                    {
                                        Schedule.ScheduleDate = SelectedDate.Date;
                                        MaintenanceProcessList.Add(Schedule);
                                    }
                                }
                                break;

                            //Quarterly
                            case 5:
                                //It will Convert month text to date Example : "January"=>01-01-0001"

                                if (DateTime.TryParseExact(ScheduleDay, "MMMM",
                                    System.Globalization.CultureInfo.InvariantCulture,
                                    System.Globalization.DateTimeStyles.None,
                                    out DateTime date))
                                {
                                    int ScheduleMonth = date.Month;
                                    int monthNumber = Convert.ToInt32(SelectedDate.Month);
                                    if (((ScheduleMonth == monthNumber) || (ScheduleMonth + 3 == monthNumber) || (ScheduleMonth + 6 == monthNumber) || (ScheduleMonth + 9 == monthNumber)) && (SelectedDate.Day == 1))
                                    {
                                        Schedule.ScheduleDate = SelectedDate.Date;
                                        MaintenanceProcessList.Add(Schedule);
                                    }
                                }

                                break;
                            case 6:
                                // Bi-Annual
                                //It will Convert month text to date  Example : "January"=>01-01-0001"

                                if (DateTime.TryParseExact(ScheduleDay, "MMMM",
                                       System.Globalization.CultureInfo.InvariantCulture,
                                       System.Globalization.DateTimeStyles.None,
                                       out DateTime datetemp))

                                {
                                    int ScheduleMonth = datetemp.Month;
                                    int monthNumber = SelectedDate.Month;
                                    if (((ScheduleMonth == monthNumber) || (ScheduleMonth + 6 == monthNumber)) && (SelectedDate.Day == 1))
                                    {
                                        Schedule.ScheduleDate = SelectedDate.Date;
                                        MaintenanceProcessList.Add(Schedule);
                                    }
                                }
                                break;


                            case 7:
                                //Annual
                                if ((ScheduleDay == SelectedDate.ToString("MMMM")) && (SelectedDate.Day == 1))
                                {
                                    Schedule.ScheduleDate = SelectedDate.Date;
                                    MaintenanceProcessList.Add(Schedule);
                                }

                                break;
                            default:

                                break;
                        }
                    }

                }
                foreach (var item in MaintenanceProcessList)
                {
                    var maintenanceName = MaintenancetypeList.FirstOrDefault(m => m.Id == item.MaintenanceId);
                    if (maintenanceName != null)
                    {
                        item.MaintenanceName = maintenanceName.Name;
                    }
                }
                return MaintenanceProcessList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ShowMaintenanceProcessList in MaintenanceProcessService");
                return new List<MaintenanceScheduleVM>();
            }
        }
        #endregion

        #region Get LatestMaintenance Date
        public async Task<String> GetLatestMaintenanceDate(int MachId)
        {
            try
            {
                var latestDate = await _unitOfWork.MaintenanceProcesses.GetQueryable()
                                    .Where(mp => mp.MachineId == MachId)
                                    .MaxAsync(mp => (DateTime?)mp.MaintenanceDate);
                return latestDate.HasValue ? latestDate.Value.ToString("dd-MM-yyyy") : "Not Done yet";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLatestMaintenanceDate in MaintenanceProcessService");
                return String.Empty;
            }
        }
        #endregion 

        #region Getting Next DueDate
        public async Task<DateTime> GetNextDueDate(int MainId, DateTime SelectedDate)
        {
            try
            {
                DateTime NextDueDate = SelectedDate;
                switch (MainId)
                {
                    case 1:
                        //Daily
                        NextDueDate = DateTime.Now.AddDays(1);
                        break;
                    case 2:
                        //Weekly
                        NextDueDate = DateTime.Now.AddDays(7);
                        break;
                    case 3:
                        //Fortnightly
                        NextDueDate = DateTime.Now.AddDays(15);
                        break;
                    case 4:
                        //Monthly
                        NextDueDate = DateTime.Now.AddMonths(1);
                        break;
                    case 5:
                        //Quarterly
                        NextDueDate = DateTime.Now.AddMonths(3);
                        break;
                    case 6:
                        //Bi-Annual
                        NextDueDate = DateTime.Now.AddMonths(6);
                        break;
                    case 7:
                        //Annual
                        NextDueDate = DateTime.Now.AddYears(1);
                        break;
                    default:
                        break;
                }
                return await Task.FromResult(NextDueDate);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetNextDueDate in MaintenanceProcessService");
                return DateTime.Now;
            }
        }
        #endregion

        #region Get MaintenaceNameByName
        public async Task<String> GetMaintenanceNameById(int MainId)
        {
            try
            {
                var MaintenancetypeList = MaintenanceTypeHelper.GetMaintenanceTypes();
                var item = MaintenancetypeList.FirstOrDefault(m => m.Id == MainId);
                return await Task.FromResult(item != null ? item.Name : String.Empty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetMaintenanceNameById in MaintenanceProcessService");
                return String.Empty;
            }
        }
        #endregion

        #region Get History of Machine By MachineId
        public async Task<List<MaintenanceProcess>> GetAllHistory(int MachId)
        {
            try
            {
                return _unitOfWork.MaintenanceProcesses.GetQueryable()
                               .Where(mp => mp.MachineId == MachId)
                               .OrderByDescending(mp => mp.Id).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetAllHistory in MaintenanceProcessService");
                return new List<MaintenanceProcess>();
            }

        }
        #endregion

        #region Get Maintenance Process By Id 
        public async Task<List<MaintenanceProcess>> GetMaintenanceProcessesById(int machId, int mainId, int ScheduleId, DateTime selecteddate)
        {
            try
            {
                return _unitOfWork.MaintenanceProcesses.GetQueryable()
                       .Where(mp => mp.MachineId == machId && mp.MaintenanceId == mainId && mp.ScheduleId == ScheduleId && mp.ModifiedDate.Value.Day == selecteddate.Day)
                       .ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetMaintenanceProcessesById in MaintenanceProcessService");
                return new List<MaintenanceProcess>();
            }
        }
        #endregion

        #region Finding Status of Machine
        public async Task<bool> FindStatus(int machId, int mainId, int ScheduleId, DateTime selecteddate)
        {
            try
            {
                var status = await _unitOfWork.MaintenanceProcesses.GetQueryable()
                              .Where(mp => mp.MachineId == machId && mp.MaintenanceId == mainId && mp.ScheduleId == ScheduleId && mp.ModifiedDate.Value.Day == selecteddate.Day)
                              .Select(mp => mp.MaintenanceStatus)
                              .FirstOrDefaultAsync();
                return status ?? false;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in FindStatus in MaintenanceProcessService");
                return false;
            }
        }
        #endregion

        #region Updating The Status
        public async Task UpdateStatus(int machId, int mainId, int ScheduleId, DateTime selecteddate)
        {
            try
            {
                var itemToRemove = await _unitOfWork.MaintenanceProcesses.FirstOrDefaultAsync(mp =>
                                           mp.MachineId == machId &&
                                           mp.MaintenanceId == mainId &&
                                           mp.ScheduleId == ScheduleId &&
                                           mp.ModifiedDate.HasValue &&
                                           mp.ModifiedDate.Value.Year == selecteddate.Year &&
                                           mp.ModifiedDate.Value.Month == selecteddate.Month &&
                                           mp.ModifiedDate.Value.Day == selecteddate.Day
                                            );

                if (itemToRemove != null)
                {
                    await _unitOfWork.MaintenanceProcesses.DeleteAsync(itemToRemove);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in UpdateStatus in MaintenanceProcessService");
            }
        }
        #endregion

        #region FindBetweenSchedules
        public async Task<List<MaintenanceScheduleVM>> FindSchedules(DateTime? fromdate, DateTime? todate)
        {
            try
            {
                if (fromdate == null || todate == null)
                    return new List<MaintenanceScheduleVM>();

                var maintenanceScheduleList = new List<MaintenanceScheduleVM>();


                    for (DateTime tempdate = fromdate.Value.Date;
                            tempdate <= todate.Value.Date;
                            tempdate = tempdate.AddDays(1))
                    {
                    var maintainList = await ShowMaintenanceProcessList(tempdate);

                    if (maintainList != null && maintainList.Any())
                    {
                        maintenanceScheduleList.AddRange(maintainList);
                    }
                }

                return maintenanceScheduleList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error in FindSchedules() in MaintenanceProcessService");

                return new List<MaintenanceScheduleVM>();
            }
        }

        #endregion

        #region find Process Between Date
        public async Task<Dictionary<(int MachineId, int ScheduleId, DateTime Date),
                               List<MaintenanceProcess>>>
         FindBetweenProcess(DateTime? fromdate, DateTime? todate)
        {
            try
            {
                var result = new Dictionary<(int, int, DateTime),
                                            List<MaintenanceProcess>>();

                if (fromdate == null || todate == null)
                    return result;

                // 🔑 FIX: Await the async call
                var maintenanceScheduleList = await FindSchedules(fromdate, todate);

                if (!maintenanceScheduleList.Any())
                    return result;

                for (DateTime tempdate = fromdate.Value.Date;
                    tempdate <= todate.Value.Date;
                    tempdate = tempdate.AddDays(1))
                {
                    foreach (var schedule in maintenanceScheduleList)
                    {
                        var key = (schedule.MachineId,
                                   schedule.ScheduleId,
                                   tempdate.Date);

                        var processList =
                            await GetMaintenanceProcessesById(
                                schedule.MachineId,
                                schedule.MaintenanceId,
                                schedule.ScheduleId,
                                tempdate);

                        if (!result.ContainsKey(key))
                        {
                            result[key] = new List<MaintenanceProcess>();
                        }

                        if (processList != null && processList.Any())
                        {
                            result[key].AddRange(processList);
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    "Error in FindBetweenProcess()");

                return new Dictionary<(int MachineId,
                                       int ScheduleId,
                                       DateTime Date),
                                       List<MaintenanceProcess>>();
            }
        }


        #endregion
    }
}

