using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConRepository
{
    public interface IEnquiryPurchaseSubRepository : IRepository<EnquiryPurchaseSub>
    {
        Task RemoveEnquirySubItemAsync(int itemId, int enquiryId, int EnquirySubId, CancellationToken cancellationToken = default);
    }
}
