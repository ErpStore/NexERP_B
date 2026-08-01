using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractGRNRepository
{
    public interface ISubConGRNRepository : IRepository<SubConGRN>
    {
        Task<string> GetLastGRNNoAsync(string suffix);
    }
}
