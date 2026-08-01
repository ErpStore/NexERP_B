using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.StaffLoan;

namespace V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IEmployeeRepository
{
    public interface IStaffLoanRepository : IRepository<StaffLoan>
    {
        Task<string> GetLastLoanNoAsync();
    }
}
