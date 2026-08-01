using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractDCOutRepository
{
    public interface ISubConDCOutRepository : IRepository<SubConDcOut>
    {
        Task<string> GetLastDcNoAsync(string suffix);
    }
}
