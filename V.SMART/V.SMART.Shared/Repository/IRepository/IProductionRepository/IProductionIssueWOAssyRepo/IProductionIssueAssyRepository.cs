using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionIssueWOAssyRepo
{
    public interface IProductionIssueAssyRepository : IRepository<ProductionIssueAssy>
    {
        Task<string> GetLastIssueNoAsync(string suffix);
        //sns
        Task<string> GetMonthWiseProductionIssueNumberAsync(string suffix);
    }
}
