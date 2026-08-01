using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository
{
    public interface ILabInvSubRepository: IRepository<LabInvSub>
    {
        Task<List<LabInvSub>> GetLabInvSubDataByLabInvId(int LabInvId);
    }
}
