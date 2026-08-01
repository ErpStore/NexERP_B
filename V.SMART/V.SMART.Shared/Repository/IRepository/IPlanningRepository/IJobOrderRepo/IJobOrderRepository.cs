using V.SMART.Shared.Data.Planning.AssyJobOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IPlanningRepository.IJobOrderRepo
{
    public interface IJobOrderRepository :IRepository<JobOrder>
    {
        Task<string> GetLastJobNoAsync(string suffix);
    }
}
