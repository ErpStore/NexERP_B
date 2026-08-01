using V.SMART.Shared.Data.Master.Inventory_module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems
{
    public interface IAssemblyChargeRepository : IRepository<AssemblyCharge>
    {
        Task<AssemblyCharge?> GetByAssmblyIdAsync(int assmblyId);

        Task<bool> ExistsAsync(int assemblyId, int itemId);
        Task<int> GetNextSlNoAsync(int assemblyId);
    }
}
