using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IToolCribReturnRepo
{
    public interface IToolCribReturnSubRepository : IRepository<ToolCribReturnSub>
    {
        //Task<List<ToolCribReturnSub>> GetIssueDetailsByTCIdAsync(int tcId);
    }

}
