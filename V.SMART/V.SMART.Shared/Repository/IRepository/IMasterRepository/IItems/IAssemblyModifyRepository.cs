using V.SMART.Shared.Data.Master.Inventory_module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems
{
    public interface IAssemblyModifyRepository :IRepository<AssemblyModify>
    {
        Task<int> GetNextRecNoAsync(int? assyId);
    }
}
