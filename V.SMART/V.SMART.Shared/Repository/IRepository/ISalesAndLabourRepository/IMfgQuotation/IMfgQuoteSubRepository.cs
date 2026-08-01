using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgQuotation
{
    public interface IMfgQuoteSubRepository: IRepository<MfgQuoteSub>
    {
        Task<List<MfgQuoteSub>> GetMfgSubDataByQuoteId(int quoteId);
    }
}
