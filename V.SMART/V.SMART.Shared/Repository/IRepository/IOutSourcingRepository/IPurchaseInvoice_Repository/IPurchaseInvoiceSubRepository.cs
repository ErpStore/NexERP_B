using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseInvoice_Repository
{
    public interface IPurchaseInvoiceSubRepository : IRepository<PurchaseInvoiceSub>
    {
        Task<List<PurchaseInvoiceSub>> GetPurchaseINVSubDataByInvId(int InvId);
    }
}
