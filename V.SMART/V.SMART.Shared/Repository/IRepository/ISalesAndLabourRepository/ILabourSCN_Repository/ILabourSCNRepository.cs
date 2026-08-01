using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourSCN_Repository
{
    public interface ILabourSCNRepository: IRepository<LabourSCN>
    {
        Task<string> GetLastSCNNoAsync(string suffix);
        Task<IEnumerable<LabourSCN>> GetAllWithItemsAsync();
    }
}
