using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.Salary;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.SalaryViewModel;
using V.SMART.Shared.ViewModels.Settings;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IPayrollService
{
    public interface ISalaryService
    {
        Task SaveSalaryAsync(Salary model);
        Task<Salary> GetSalaryAsync(int id);
        Task<List<Salary>> GetAllSalariesAsync();
        Task<(bool CanDelete, string Message)> CanDeleteSalaryAsync(int rowId);
        Task<bool> DeleteSalaryByRowIdAsync(int RowId);
        Task<SalaryVM?> GetSalaryByRowIdAsync(int rowId);
        Task<Staff?> GetStaffByStaffIdAsync(int staffId);
        void CalculateEarnings(SalaryVM vm, HRMasterSettingVM hr);

        void CalculateDeductions(SalaryVM vm, HRMasterSettingVM hr);
        decimal CalculateESIWage(SalaryVM vm,HRMasterSettingVM hr);
        decimal CalculatePFWage(SalaryVM vm, HRMasterSettingVM hr);
        Task<SalaryVM?> GetSalaryByStaffMonthYearAsync(int? staffId, int? month, int? year);

        Task<Companydetails> GetCompanyDetailsAsync();
    }
}
