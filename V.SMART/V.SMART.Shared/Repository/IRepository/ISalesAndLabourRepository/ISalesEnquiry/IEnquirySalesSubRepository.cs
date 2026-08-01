using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.ISalesEnquiry
{
    public interface IEnquirySalesSubRepository : IRepository<EnquirySalesSub>
    {
        Task RemoveEnquirySubItemAsync(int itemId, int enquiryId, int EnquirySubId, CancellationToken cancellationToken = default);
    }
}
