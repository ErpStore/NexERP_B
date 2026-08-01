using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService
{
    public interface IVendorService
    {
        Task<(List<VendorVM> vendorVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteVendorAsync(int vendorId);

        Task<bool> DeleteVendorByVendorIdAsync(int vendorId);

        Task<List<VendorVM>> GetVendorListAsync(string topic, DateTime fromDate, DateTime toDate);

    }
}
