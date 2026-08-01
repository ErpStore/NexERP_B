using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.Salary;

namespace V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IPayrollRepository
{
    public interface ISalaryRepository : IRepository<Salary>
    {
        Task<Salary> GetByIdAsync(int id);
        Task UpsertAsync(Salary model);
    }
}
