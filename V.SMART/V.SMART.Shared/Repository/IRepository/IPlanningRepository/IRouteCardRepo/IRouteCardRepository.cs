using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IPlanningRepository.IRouteCardRepo
{
    public interface IRouteCardRepository : IRepository<RouteCard>
    {
        Task<string> GetLastRcNoAsync(string suffix);
    }
}
