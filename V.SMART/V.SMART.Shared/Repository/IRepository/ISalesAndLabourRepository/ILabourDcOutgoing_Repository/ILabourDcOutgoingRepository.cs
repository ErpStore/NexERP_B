using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourDcOutgoing_Repository
{
    public interface ILabourDcOutgoingRepository: IRepository<LabourDcOutgoing>
    {
        Task<string> GetLastDcNoAsync(string suffix);
        Task<IEnumerable<LabourDcOutgoing>> GetAllWithItemsAsync();

        Task<string> GetNextNrDcNoAsync(string suffix);
    }
}
