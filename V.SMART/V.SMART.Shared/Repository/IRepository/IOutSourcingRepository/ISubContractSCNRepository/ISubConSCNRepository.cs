using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractSCNRepository
{
    public interface ISubConSCNRepository : IRepository<SubConSCN>
    {
        Task<string> GetLastSCNNoAsync(string suffix);
    }
}
