using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseInvoice_Repository
{
    public interface IPurchaseInvoiceRepository: IRepository<PurchaseInvoice>
    {
       
        Task<IEnumerable<PurchaseInvoice>> GetAllWithItemsAsync();

        //Task<List<PurchaseInvoiceVM>> SearchPurchaseInvAsync(PurchaseInvoiceVM search);
    }
}
