using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice
{
    public interface ICandidateService
    {
        Task<(List<CandidateVM> CandidateVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<bool> DeleteCandidateAsync(int cadidateid);
        Task<(bool CanDelete, string Message)> CanDeleteCandidateAsync(int candidateId);
    }
}
