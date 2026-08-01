using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgQuotation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgQuotation
{
    public interface IMfgQuoteRepository: IRepository<MfgQuote>
    {
        Task<IEnumerable<MfgQuote>> GetAllWithItemsAsync();
        Task<string> GetLastQuoteNoAsync(string suffix);
    }
}
