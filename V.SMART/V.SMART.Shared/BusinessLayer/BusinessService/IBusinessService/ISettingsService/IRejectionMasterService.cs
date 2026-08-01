using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface IRejectionMasterService
    {
        Task<List<RejectionMasterVM>> GetAllAsync();

        Task<RejectionMasterVM?> GetByIdAsync(int id);

        Task<RejectionMasterVM> Upsertrejection(RejectionMasterVM model);

        Task<RejectionMasterVM?> UpdateAsync(RejectionMasterVM model);

        Task<bool> DeleteAsync(int id);

        Task<bool> IsCodeExistsAsync(string code, int? excludeId = null);

        Task<string?> CheckRejectionUsageAsync(int rejectionId);


    }
}
