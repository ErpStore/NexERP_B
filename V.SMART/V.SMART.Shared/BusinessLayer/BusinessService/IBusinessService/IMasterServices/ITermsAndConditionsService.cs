using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices
{
    public interface ITermsAndConditionsService
    {
        Task<IEnumerable<TermsAndConditions>> GetAllAsync();
        Task<TermsAndConditions> GetByIdAsync(int id);
        Task<TermsAndConditions> CreateAsync(TermsAndConditions model);
        Task<TermsAndConditions> UpdateAsync(TermsAndConditions model);
        Task<bool> DeleteAsync(int id);
        Task<(bool CanDelete, string Message)> CanDeleteTermsAsync(int termsId);
        Task<(List<TermsAndConditionsVM> termsAndConditionsVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<bool> DeleteTermsAndConditionsByTermsIdAsync(int id);
    }
}
