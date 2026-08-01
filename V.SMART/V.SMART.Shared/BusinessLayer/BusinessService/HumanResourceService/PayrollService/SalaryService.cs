using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IPayrollService;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.HumanResource.Salary;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.SalaryViewModel;
using V.SMART.Shared.ViewModels.Settings;

namespace V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService.PayrollService
{
    public class SalaryService : ISalaryService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        


        public SalaryService(
            IUnitOfWork unitOfWork,
            CurrentUserService userService,
            ICommonService commonService,
            ILoggingService logs,
            IMapper mapper
           )
        {
            _unitOfWork = unitOfWork;
            _currentUserService = userService;
            _commonService = commonService;
            _logs = logs;
            _mapper = mapper;
        }

        public async Task<Salary> GetSalaryAsync(int id)
        {
            return await _unitOfWork.Salaries.GetByIdAsync(id);
        }

        public async Task<List<Salary>> GetAllSalariesAsync()
        {
            return await _unitOfWork.Salaries
                .GetQueryable()
                .OrderByDescending(x => x.RowId)
                .ToListAsync();
        }

        //Fast Report Print
        //public async Task<string> GetReportNameBySceenNameAsync(string screenName)
        //    => await _commonService.GetReportNameBySceenNameAsync(screenName);

        //Companydetails
        public async Task<Companydetails> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();

        public async Task<SalaryVM?> GetSalaryByStaffMonthYearAsync(int? staffId, int? month, int? year)
        {
            return await _unitOfWork.Salaries
                .GetQueryable()
                .Where(s => s.StaffId == staffId && s.SalMonth == month && s.SalYear == year)
                .Select(s => new SalaryVM
                {
                    RowId = s.RowId,
                    StaffId = s.StaffId,
                    SalMonth = s.SalMonth,
                    SalYear = s.SalYear,
                    SalDate = s.SalDate,

                    Dept = s.Dept,

                    PayableDays = s.PayableDays,
                    WorkingDays = s.WorkingDays,
                    LeaveDays = s.LeaveDays,
                    OT = s.OT,

                    Basic = s.Basic,
                    EarnedBasic = s.EarnedBasic,

                    DA = s.DA,
                    EarnedDA = s.EarnedDA,

                    IsOTunderBasic = s.IsOTunderBasic,
                    OT_times_now = s.OT_times_now,
                    EarnedOT = s.EarnedOT,

                    IsAttBonus = s.IsAttBonus,
                    EarnedAttBonus = s.EarnedAttBonus,

                    IsHRAPerDay = s.IsHRAPerDay,
                    IsHRAPerMonth = s.IsHRAPerMonth,
                    HRA = s.HRA,
                    EarnedHRA = s.EarnedHRA,

                    IsTAPerDay = s.IsTAPerDay,
                    IsTAPerMonth = s.IsTAPerMonth,
                    TA = s.TA,
                    EarnedTA = s.EarnedTA,

                    EA = s.EA,

                    SplAllow = s.SplAllow,
                    EarnedSplAllow = s.EarnedSplAllow,

                    LTA = s.LTA,
                    MR = s.MR,

                    TotalEarnings = s.TotalEarnings,

                    IsPF = s.IsPF,
                    EarnedPF = s.EarnedPF,

                    IsESI = s.IsESI,
                    EarnedESI = s.EarnedESI,

                    PT = s.PT,
                    TDS = s.TDS,

                    Society = s.Society,
                    Insurance = s.Insurance,
                    SalAdvance = s.SalAdvance,
                    Fines = s.Fines,
                    DamageLoss = s.DamageLoss,
                    OtherDed = s.OtherDed,

                    LoanId = s.LoanId,
                    LoanAmtDed = s.LoanAmtDed,
                    LoanBalB4 = s.LoanBalB4,

                    TotalDed = s.TotalDed,
                    NetPay = s.NetPay,

                    OldBal = s.OldBal,
                    paidAmt = s.paidAmt,

                    AttBonus = s.AttBonus,
                    LoanDedId = s.LoanDedId,

                    IsDAtoPF = s.IsDAtoPF,
                    IsPT = s.IsPT,

                    ESIBankCode = s.ESIBankCode,
                    ESIBankName = s.ESIBankName,
                    ESICQDate = s.ESICQDate,
                    ESICQNo = s.ESICQNo,
                    ESIDate = s.ESIDate,
                    ESIPytMode = s.ESIPytMode,

                    PFBankName = s.PFBankName,
                    PFCQDate = s.PFCQDate,
                    PFCQNo = s.PFCQNo,
                    PFDate = s.PFDate,
                    PFDepositor = s.PFDepositor,
                    PFPytMode = s.PFPytMode,

                    Createdby = s.Createdby,
                    Createddate = s.Createddate,
                    Modifiedby = s.Modifiedby,
                    Modifieddate = s.Modifieddate,

                    chckOT = s.IsOTunderBasic,

                    PH = s.PH,
                    W = s.W,
                    LOP = s.LOP,
                    P = s.P,
                    O = s.O,

                    Miscellaneous = s.Miscellaneous,
                    Others = s.Others,

                    IsSplAllowPerDay = s.IsSplAllowPerDay,
                    IsSplAllowPerMonth = s.IsSplAllowPerMonth,
                    IsMRPerDay = s.IsMRPerDay,
                    IsMRPerMonth = s.IsMRPerMonth,
                    IsLTAPerDay = s.IsLTAPerDay,
                    IsLTAPerMonth = s.IsLTAPerMonth,
                    IsEAPerDay = s.IsEAPerDay,
                    IsEAPerMonth = s.IsEAPerMonth,

                    EarnedLTA = s.EarnedLTA,
                    EarnedMR = s.EarnedMR,
                    EarnedEA = s.EarnedEA,

                    PresentDays = s.PresentDays,
                    AbsentDays = s.AbsentDays,
                    Holidays = s.Holidays,

                    // Add any other fields from your entity similarly
                })
                .FirstOrDefaultAsync();
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteSalaryAsync(int rowId)
        {

            try
            {
                var loan = await _unitOfWork.Salaries
                                .GetQueryable()
                                .Where(e => e.RowId == rowId).FirstOrDefaultAsync();

                if (loan == null)
                    return (true, "Salary can be safely deleted.");

                //var loanSubIds = loan.StaffLoan
                //    .Select(es => es.QuoteSubId)
                //    .ToList();

                //bool hasSalary = await _unitOfWork.Salaries
                //    .GetQueryable()
                //    .AnyAsync(qs =>
                //        qs.LoanId.HasValue &&
                //        loanSubIds.Contains(qs.LoanId.Value));

                //if (hasSalary)
                //    return (false, "Cannot delete this StaffLoan as a Salary transaction exists.");


                return (true, "Salary can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSalaryAsync for RowId: {rowId}");
                throw new Exception("Error checking Salary delete eligibility", ex);
            }


        }

        public async Task<bool> DeleteSalaryByRowIdAsync(int RowId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var loan = await _unitOfWork.Salaries
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.RowId == RowId);

                if (loan == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the quotation
                await _unitOfWork.Salaries.DeleteAsync(loan);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Salary List",
                    action: $"Deleted Salary: {loan.RowId}",
                    additionalInfo: $"Salary Id: {loan.RowId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Salary: {RowId}");
                throw;
            }
        }

        //public void CalculateEarnings(SalaryVM vm)
        //{
        //    decimal wdays = vm.WorkingDays;
        //    decimal paydays = vm.PayableDays;

        //    if (wdays <= 0)
        //        return;

        //    // BASIC
        //    vm.EarnedBasic = Math.Round((vm.Basic / wdays) * paydays, 2);

        //    // DA
        //    vm.EarnedDA = Math.Round((vm.DA / wdays) * paydays, 2);

        //    // HRA
        //    vm.EarnedHRA = vm.IsHRAPerDay
        //        ? Math.Round((vm.HRA / wdays) * paydays, 2)
        //        : vm.HRA;

        //    // TA
        //    vm.EarnedTA = vm.IsTAPerDay
        //            ? Math.Round((vm.TA / wdays) * paydays, 2)
        //            : vm.TA;

        //    // SPECIAL ALLOWANCE
        //    vm.EarnedSplAllow = vm.IsSplAllowPerDay
        //        ? Math.Round((vm.SplAllow / wdays) * paydays, 2)
        //        : vm.SplAllow;

        //    // MEDICAL REIMBURSEMENT
        //    vm.EarnedMR = vm.IsMRPerDay
        //        ? Math.Round((vm.MR / wdays) * paydays, 2)
        //        : vm.MR;

        //    // LTA
        //    vm.EarnedLTA = vm.IsLTAPerDay
        //        ? Math.Round((vm.LTA / wdays) * paydays, 2)
        //        : vm.LTA;

        //    // EA
        //    vm.EarnedEA = vm.IsEAPerDay
        //        ? Math.Round((vm.EA / wdays) * paydays, 2)
        //                 : vm.EA;

        //    // Attendance Bonus
        //    vm.EarnedAttBonus = (paydays >= wdays && vm.IsAttBonus)
        //        ? vm.AttBonus
        //        : 0;

        //    // Misc & Others
        //    vm.Miscellaneous = vm.Miscellaneous;
        //    vm.Others = vm.Others;

        //    // OT Calculation
        //    if (vm.WorkingDays > 0 && vm.WorkingHrs > 0)
        //    {
        //        if (!vm.IsOTunderBasic)
        //        {
        //            decimal grossSalary = vm.Basic + vm.AttBonus + vm.DA +vm.LTA + vm.SplAllow + vm.EA + vm.HRA + vm.Others + vm.Miscellaneous + vm.TA + vm.MR;
        //            vm.EarnedOT =
        //                (vm.OT * vm.OT_times_now) *
        //                ((grossSalary / vm.WorkingDays) / vm.WorkingHrs);
        //        }
        //        else if (!vm.IsOTPerHour)
        //        {
        //            vm.EarnedOT = vm.OT * vm.OT_times_now;
        //        }
        //        else
        //        {
        //            vm.EarnedOT = 0;
        //        }
        //    }
        //    else
        //    {
        //        vm.EarnedOT = 0;
        //    }

        //    // TOTAL EARNINGS
        //    vm.TotalEarnings =
        //        vm.EarnedBasic +
        //        vm.EarnedDA +
        //        vm.EarnedHRA +
        //        vm.EarnedTA +
        //        vm.EarnedLTA +
        //        vm.EarnedSplAllow +
        //        vm.EarnedMR +
        //        vm.EarnedEA +
        //        vm.EarnedAttBonus +
        //        vm.Miscellaneous +
        //        vm.Others +
        //        vm.EarnedOT;
        //}

        //22/03/26

        //public void CalculateEarnings(SalaryVM vm, HRMasterSettingVM hr)
        //{
        //    decimal wdays = vm.WorkingDays;
        //    decimal paydays = vm.PayableDays;

        //    if (wdays <= 0)
        //    {
        //        vm.EarnedBasic = 0;
        //        vm.EarnedDA = 0;
        //        vm.EarnedHRA = 0;
        //        vm.EarnedTA = 0;
        //        vm.EarnedLTA = 0;
        //        vm.EarnedSplAllow = 0;
        //        vm.EarnedMR = 0;
        //        vm.EarnedEA = 0;
        //        vm.EarnedAttBonus = 0;
        //        vm.EarnedOT = 0;
        //        vm.TotalEarnings = 0;
        //        return;
        //    }

        //    // BASIC
        //    vm.EarnedBasic = Math.Round((vm.Basic / wdays) * paydays, 2);

        //    // DA
        //    vm.EarnedDA = Math.Round((vm.DA / wdays) * paydays, 2);

        //    // HRA
        //    vm.EarnedHRA = vm.IsHRAPerDay
        //        ? Math.Round((vm.HRA / wdays) * paydays, 2)
        //        : vm.HRA;

        //    // TA
        //    vm.EarnedTA = vm.IsTAPerDay
        //        ? Math.Round((vm.TA / wdays) * paydays, 2)
        //        : vm.TA;

        //    // LTA
        //    vm.EarnedLTA = vm.IsLTAPerDay
        //        ? Math.Round((vm.LTA / wdays) * paydays, 2)
        //        : vm.LTA;

        //    // SPECIAL ALLOWANCE
        //    vm.EarnedSplAllow = vm.IsSplAllowPerDay
        //        ? Math.Round((vm.SplAllow / wdays) * paydays, 2)
        //        : vm.SplAllow;

        //    // EA
        //    vm.EarnedEA = vm.IsEAPerDay
        //        ? Math.Round((vm.EA / wdays) * paydays, 2)
        //        : vm.EA;

        //    // MR
        //    vm.EarnedMR = vm.IsMRPerDay
        //        ? Math.Round((vm.MR / wdays) * paydays, 2)
        //        : vm.MR;

        //    // Attendance Bonus
        //    vm.EarnedAttBonus =
        //        (vm.IsAttBonus && paydays >= wdays)
        //        ? vm.AttBonus
        //        : 0;

        //    // OT
        //    vm.EarnedOT = 0;

        //    if (!vm.OtCount)
        //    {
        //        vm.EarnedOT = 0;
        //        return;
        //    }


        //    if (vm.WorkingDays > 0 && vm.WorkingHrs > 0)
        //    {
        //        if (!vm.IsOTunderBasic)
        //        {
        //            decimal otSalary = 0;

        //            if (hr.OTBasic) otSalary += vm.Basic;
        //            if (hr.OTDA) otSalary += vm.DA;
        //            if (hr.OTHRA) otSalary += vm.HRA;
        //            if (hr.OTConveyance) otSalary += vm.TA;
        //            if (hr.OTLTA) otSalary += vm.LTA;
        //            if (hr.OTSplAllow) otSalary += vm.SplAllow;
        //            if (hr.OTEA) otSalary += vm.EA;
        //            if (hr.OTMedicalAllow) otSalary += vm.MR;
        //            if (hr.OTAttendBonus) otSalary += vm.AttBonus;
        //            if (hr.OTMiscEarning) otSalary += vm.Miscellaneous;
        //            if (hr.OTOtherAllow) otSalary += vm.Others;

        //            vm.EarnedOT = Math.Round(
        //                (vm.OT * vm.OT_times_now) *
        //                ((otSalary / vm.WorkingDays) / vm.WorkingHrs),
        //                2);
        //        }
        //        else if (!vm.IsOTPerHour)
        //        {
        //            vm.EarnedOT = Math.Round(vm.OT * vm.OT_times_now, 2);
        //        }
        //    }



        //    // TOTAL EARNINGS
        //    vm.TotalEarnings =
        //        vm.EarnedBasic +
        //        vm.EarnedDA +
        //        vm.EarnedHRA +
        //        vm.EarnedTA +
        //        vm.EarnedLTA +
        //        vm.EarnedSplAllow +
        //        vm.EarnedEA +
        //        vm.EarnedMR +
        //        vm.EarnedAttBonus +
        //        vm.Miscellaneous +
        //        vm.Others +
        //        vm.EarnedOT;
        //}

        public void CalculateEarnings(SalaryVM vm, HRMasterSettingVM hr)
        {
            if (vm.EnableAttendanceEdit)
            {
                // User entered attendance manually
                // vm.PayableDays = vm.PresentDays + vm.LeaveDays + vm.Holidays;
                decimal total = vm.PresentDays + vm.LeaveDays + vm.Holidays;

                if (total > vm.WorkingDays)
                {
                    // Reduce holidays so total never exceeds working days
                    vm.Holidays = Math.Max(
                        0,
                        vm.WorkingDays - vm.PresentDays - vm.LeaveDays);

                    total = vm.PresentDays + vm.LeaveDays + vm.Holidays;
                }

                vm.PayableDays = total;
                vm.AbsentDays = vm.WorkingDays - vm.PayableDays;
            }
    

            decimal wdays = vm.WorkingDays;
            decimal paydays = vm.PayableDays;

            if (wdays <= 0)
            {
                vm.EarnedBasic = 0;
                vm.EarnedDA = 0;
                vm.EarnedHRA = 0;
                vm.EarnedTA = 0;
                vm.EarnedLTA = 0;
                vm.EarnedSplAllow = 0;
                vm.EarnedMR = 0;
                vm.EarnedEA = 0;
                vm.EarnedAttBonus = 0;
                vm.EarnedOT = 0;
                vm.TotalEarnings = 0;
                return;
            }

            // BASIC
            vm.EarnedBasic = Math.Round((vm.Basic / wdays) * paydays, 2);

            // DA
            vm.EarnedDA = Math.Round((vm.DA / wdays) * paydays, 2);

            // HRA
            vm.EarnedHRA = vm.IsHRAPerDay
                ? Math.Round((vm.HRA / wdays) * paydays, 2)
                : vm.HRA;

            // TA
            vm.EarnedTA = vm.IsTAPerDay
                ? Math.Round((vm.TA / wdays) * paydays, 2)
                : vm.TA;

            // LTA
            vm.EarnedLTA = vm.IsLTAPerDay
                ? Math.Round((vm.LTA / wdays) * paydays, 2)
                : vm.LTA;

            // SPECIAL ALLOWANCE
            vm.EarnedSplAllow = vm.IsSplAllowPerDay
                ? Math.Round((vm.SplAllow / wdays) * paydays, 2)
                : vm.SplAllow;

            // EA
            vm.EarnedEA = vm.IsEAPerDay
                ? Math.Round((vm.EA / wdays) * paydays, 2)
                : vm.EA;

            // MR
            vm.EarnedMR = vm.IsMRPerDay
                ? Math.Round((vm.MR / wdays) * paydays, 2)
                : vm.MR;

            // Attendance Bonus
            vm.EarnedAttBonus =
                (vm.IsAttBonus && paydays >= wdays)
                ? vm.AttBonus
                : 0;

            // OT
            vm.EarnedOT = 0;

            if (!vm.OtCount)
            {
                vm.EarnedOT = 0;
            }
            else if (vm.WorkingDays > 0 && vm.WorkingHrs > 0)
            {
                if (!vm.IsOTunderBasic)
                {
                    decimal otSalary = 0;

                    if (hr.OTBasic) otSalary += vm.Basic;
                    if (hr.OTDA) otSalary += vm.DA;
                    if (hr.OTHRA) otSalary += vm.HRA;
                    if (hr.OTConveyance) otSalary += vm.TA;
                    if (hr.OTLTA) otSalary += vm.LTA;
                    if (hr.OTSplAllow) otSalary += vm.SplAllow;
                    if (hr.OTEA) otSalary += vm.EA;
                    if (hr.OTMedicalAllow) otSalary += vm.MR;
                    if (hr.OTAttendBonus) otSalary += vm.AttBonus;
                    if (hr.OTMiscEarning) otSalary += vm.Miscellaneous;
                    if (hr.OTOtherAllow) otSalary += vm.Others;

                    vm.EarnedOT = Math.Round(
                        (vm.OT * vm.OT_times_now) *
                        ((otSalary / vm.WorkingDays) / vm.WorkingHrs),
                        2);
                }
                else if (!vm.IsOTPerHour)
                {
                    vm.EarnedOT = Math.Round(vm.OT * vm.OT_times_now, 2);
                }
            }

            // TOTAL EARNINGS
            vm.TotalEarnings =
                vm.EarnedBasic +
                vm.EarnedDA +
                vm.EarnedHRA +
                vm.EarnedTA +
                vm.EarnedLTA +
                vm.EarnedSplAllow +
                vm.EarnedEA +
                vm.EarnedMR +
                vm.EarnedAttBonus +
                vm.Miscellaneous +
                vm.Others +
                vm.EarnedOT;
        }


        public void CalculateDeductions(SalaryVM vm, HRMasterSettingVM hr)
        {
            decimal pf = 0;
            decimal esi = 0;

            // ================= PF =================
            decimal pfWage = 0;

            if (hr.PFBasic) pfWage += vm.Basic;
            if (hr.PFDA) pfWage += vm.DA;
            if (hr.PFHRA) pfWage += vm.HRA;
            if (hr.PFConveyance) pfWage += vm.TA;
            if (hr.PFSplAllow) pfWage += vm.SplAllow;
            if (hr.PFMedicalAllow) pfWage += vm.MR;
            if (hr.PFLTA) pfWage += vm.LTA;
            if (hr.PFEA) pfWage += vm.EA;
            if (hr.PFMiscEarning) pfWage += vm.Miscellaneous;
            if (hr.PFOtherAllow) pfWage += vm.Others;
            if (hr.PFAttendBonus) pfWage += vm.AttBonus;

            if (hr.PFOTAmount)
                pfWage += vm.OTBaseAmount;

            decimal pfBase = Math.Min(pfWage, hr.PFSealing);

            if (vm.IsPF)
            {
                pf = Math.Round(pfBase * hr.EmpPF / 100, 2, MidpointRounding.AwayFromZero);
            }

            // ================= ESI =================
            decimal esiWage = 0;

            if (hr.ESIBasic) esiWage += vm.Basic;
            if (hr.ESIDA) esiWage += vm.DA;
            if (hr.ESIHRA) esiWage += vm.HRA;
            if (hr.ESIConveyance) esiWage += vm.TA;
            if (hr.ESISplAllow) esiWage += vm.SplAllow;
            if (hr.ESIMedicalAllow) esiWage += vm.MR;
            if (hr.ESILTA) esiWage += vm.LTA;
            if (hr.ESIEA) esiWage += vm.EA;
            if (hr.ESIMiscEarning) esiWage += vm.Miscellaneous;
            if (hr.ESIOtherAllow) esiWage += vm.Others;
            if (hr.ESIAttBonus) esiWage += vm.AttBonus;

            if (hr.ESIOTAmt)
                esiWage += vm.OTBaseAmount;

            bool esiEligible;

            if (hr.ESIBasicDA)
                esiEligible = (vm.Basic + vm.DA) <= hr.ESISealing;
            else
                esiEligible = esiWage <= hr.ESISealing;

            if (vm.IsESI && esiEligible)
            {
                esi = Math.Round(esiWage * hr.EmpESI / 100, 2, MidpointRounding.AwayFromZero);
            }

            // ================= TOTAL =================
            vm.EarnedPF = pf;
            vm.EarnedESI = esi;

            vm.TotalDed =
                  vm.EarnedPF
                + vm.EarnedESI
                + vm.PT
                + vm.TDS
                + vm.Society
                + vm.Insurance
                + vm.SalAdvance
                + vm.Fines
                + vm.DamageLoss
                + vm.OtherDed
                + vm.LoanAmtDed;

            vm.NetPay = vm.TotalEarnings - vm.TotalDed;
        }

        public decimal CalculatePFWage(SalaryVM vm,HRMasterSettingVM hr)
        {
            decimal grossAmountSalary = 0;

            if (hr.PFBasic) grossAmountSalary += vm.Basic;
            if (hr.PFDA) grossAmountSalary += vm.DA;
            if (hr.PFHRA) grossAmountSalary += vm.HRA;
            if (hr.PFConveyance) grossAmountSalary += vm.TA;
            if (hr.PFSplAllow) grossAmountSalary += vm.SplAllow;
            if (hr.PFMedicalAllow) grossAmountSalary += vm.MR;
            if (hr.PFLTA) grossAmountSalary += vm.LTA;
            if (hr.PFEA) grossAmountSalary += vm.EA;
            if (hr.PFMiscEarning) grossAmountSalary += vm.Miscellaneous;
            if (hr.PFOtherAllow) grossAmountSalary += vm.Others;
            if (hr.PFAttendBonus) grossAmountSalary += vm.AttBonus;

            if (hr.PFOTAmount)
                grossAmountSalary += vm.OTBaseAmount;

            return grossAmountSalary;
        }

        public decimal CalculateESIWage(SalaryVM vm,HRMasterSettingVM hr)
        {
            decimal grossAmountSalary = 0;

            if (hr.ESIBasic) grossAmountSalary += vm.Basic;
            if (hr.ESIDA) grossAmountSalary += vm.DA;
            if (hr.ESIHRA) grossAmountSalary += vm.HRA;
            if (hr.ESIConveyance) grossAmountSalary += vm.TA;
            if (hr.ESISplAllow) grossAmountSalary += vm.SplAllow;
            if (hr.ESIMedicalAllow) grossAmountSalary += vm.MR;
            if (hr.ESILTA) grossAmountSalary += vm.LTA;
            if (hr.ESIEA) grossAmountSalary += vm.EA;
            if (hr.ESIMiscEarning) grossAmountSalary += vm.Miscellaneous;
            if (hr.ESIOtherAllow) grossAmountSalary += vm.Others;
            if (hr.ESIAttBonus) grossAmountSalary += vm.AttBonus;

            if (hr.ESIOTAmt)
                grossAmountSalary += vm.OTBaseAmount;

            return grossAmountSalary;
        }

        public async Task<HRMasterSetting> GetHRMSettingAsync()
        {
            return await _unitOfWork.HRMasterSettings.GetFirstAsync()
                   ?? new HRMasterSetting();
        }

        public async Task<SalaryVM?> GetSalaryByRowIdAsync(int rowId)
        {
            try
            {
                var entity = await _unitOfWork.Salaries.GetQueryable()
                    .FirstOrDefaultAsync(q => q.RowId == rowId);

                return _mapper.Map<SalaryVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetSalaryByRowIdAsync({rowId})");
                return null;
            }
        }

        public async Task<Staff?> GetStaffByStaffIdAsync(int staffId)
        {
            try
            {
                var entity = await _unitOfWork.Staffs.GetQueryable()
                    .FirstOrDefaultAsync(q => q.StaffID == staffId);
                return entity; // Return Staff entity, not SalaryVM
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetStaffByStaffIdAsync({staffId})");
                return null;
            }
        }

        public async Task SaveSalaryAsync(Salary model)
        {
            // ===== CALCULATIONS ====

            model.TotalEarnings =
                model.EarnedBasic +
                model.EarnedDA +
                model.EarnedHRA +
                model.EarnedTA +
                model.EarnedOT +
                model.EarnedSplAllow +
                model.EarnedEA +
                model.EarnedMR +
                model.EarnedLTA +
                model.EarnedAttBonus;

            model.TotalDed =
                model.EarnedPF +
                model.EarnedESI +
                model.PT +
                model.TDS +
                model.Society +
                model.Insurance +
                model.SalAdvance +
                model.Fines +
                model.DamageLoss +
                model.OtherDed +
                model.LoanAmtDed;

            model.NetPay = model.TotalEarnings - model.TotalDed;

            // ===== UPDATE LOAN BALANCE =====

            if (model.LoanId != null && model.LoanAmtDed > 0)
            {
                var loan = await _unitOfWork.StaffLoans.GetAsync(model.LoanId.Value);

                if (loan != null)
                {
                    loan.BalanceAmount -= model.LoanAmtDed;

                    if (loan.BalanceAmount <= 0)
                    {
                        loan.BalanceAmount = 0;
                        loan.IsFinished = false;
                    }

                    _unitOfWork.StaffLoans.UpdateAsync(loan);
                }
            }

            await _unitOfWork.Salaries.UpsertAsync(model);
            await _unitOfWork.SaveAsync();
        }
    }
}
