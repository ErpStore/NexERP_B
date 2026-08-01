using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ILabourGRN_Repository
{
    public interface ILabourGRNRepository: IRepository<LabourGRN>
    {
        Task<string> GetLastGRNNoAsync(string suffix);
        Task<IEnumerable<LabourGRN>> GetAllWithItemsAsync();
    }
}
