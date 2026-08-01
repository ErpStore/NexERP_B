using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.StaffLoan;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.StaffLoanViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IEmployeeLoanService
{
    public interface IStaffLoanService
    {
        Task<bool> IsLoanTransactionsMatchedAsync(int loanId, StaffLoanVM loanVMs);
      
        Task<(bool CanDelete, string Message)> CanDeleteStaffLoanAsync(int loanId);
        Task<List<StaffLoan>> GetLoanByLoanIdAsync(int loanId);

        Task<StaffLoanVM?> GetStaffLoanByLoanIdAsync(int loanId);
        Task<IEnumerable<StaffLoanVM>> GetAllLoanAsync();
        Task<StaffLoan?> GetLastLoanAsync(int staffId);
        Task<Staff?> GetStaffByStaffIDAsync(int staffId);
        Task<StaffLoanVM> UpsertLoanAsync(StaffLoanVM loan);
        Task<StaffLoanVM?> GetStaffLoanByStaffIdAsync(int staffId);
        Task<bool> HasAnySalaryTransactionMadeAsync(int loanId);
        Task<string> GetLoanNumberAsync();
        Task<bool> DeleteStaffLoanByLoanIdAsync(int LoanId);
        Task<List<StaffLoanVM>> GetRecoveriesByLoanIdAsync(int loanId, int staffId);
  
    }
}
