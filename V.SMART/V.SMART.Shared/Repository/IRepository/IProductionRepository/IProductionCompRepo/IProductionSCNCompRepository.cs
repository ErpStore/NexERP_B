using V.SMART.Shared.Data.Production.ProductionComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionCompRepo
{
    public interface IProductionSCNCompRepository : IRepository<ProductionSCNComp>
    {
        Task<string> GetLastSCNNoAsync(string suffix);
    }
}
