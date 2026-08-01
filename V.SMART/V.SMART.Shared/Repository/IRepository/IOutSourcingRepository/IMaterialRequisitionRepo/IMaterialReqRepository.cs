using V.SMART.Shared.Data.OutSourcing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IMaterialRequisitionRepo
{
    public interface IMaterialReqRepository :IRepository<MaterialReq>
    {
        Task<string> GetLastMReqNoAsync(string suffix);
    }
}
