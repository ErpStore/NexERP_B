using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchOrSubConRepository
{
    public interface IEnquiryPurchaseRepository : IRepository<EnquiryPurchase>
    {
        Task<string> GetLastEnqNoAsync(string suffix);
    }
}
