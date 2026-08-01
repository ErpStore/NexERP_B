using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Data.SalesAndLabour.ContractReview;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ContractReviewVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService
{
    public interface IContractReviewService
    {
        #region These are ContractReviewMaster
        Task<List<ContractReviewMaster>> GetMasterDescription();

        Task<ContractReviewMaster?> GetMasterBySlNoAsync(int slNo);

        Task<bool> DeleteMasterBySlNoAsync(int slNo);

        Task<(bool CanDelete, string Message)> ValidateBeforeDeleteBySlNoAsync(int slNo);

        Task<bool> ExistsByDescriptionAsync(string description);

        Task<ContractReviewMaster> UpsertContractMasterAsync(ContractReviewMaster master);

        Task<(List<ContractReviewMasterVM> reviewMasterVMs, int TotalCount)>
                    SearchWithDynamicReviewMasterFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<(bool CanDelete, string Message)> CanDeleteReviewMasterAsync(int reviewId);

        Task<bool> DeleteReviewMasterByReviewIdAsync(int reviewId);

        #endregion



        #region These are ContractReviewchecklist
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<string> GetContractReviewNumberAsync(string suffix);

        Task<(List<ContractReviewVM> Review, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<bool> DeleteContractReviewByIdAsync(int Id);

        Task<List<User>> GetRevisedByUsersAsync();


        Task<List<ContractReviewMasterVM>> GetMasterAsync();

        Task<ContractReviewVM> GetContractReviewByIdAsync(int Id);

        Task<ContractReviewVM> GetPoDetailsByPoIdAsync(int refPoId);

        Task<IEnumerable<MfgPoVM>> GetAvailableMfgPosAsync();

        System.Threading.Tasks.Task UpdateOADetails(string contractNo, string generatedOANo, DateTime? OADate, int PoId);
        Task<string> GetNextOANumberAsync(string suffix);

        Task<ContractReviewVM> UpsertContractReview(ContractReviewVM reviewVM);
        Task<List<UserAuthorityVM>> GetApprovedByUsersAsync();

        #endregion
    }
}
