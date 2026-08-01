using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractSCNService
{
    public interface ISubConSCNService
    {
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<VendorVM?> GetVendorByVenerCodeAsync(int vendorCode);
        Task<int> GetDecimalPlacesAsync();
        Task<IEnumerable<VendorVM>> SearchVendors(string value, CancellationToken token);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllIssueStoresAsync();
        Task<List<Store>> GetAllAddStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);


        // 🔹 Quotation-specific logic

        Task<List<Dictionary<string, object>>> GetProductionCompReturnDetails(int vendorcode);
        Task<SubConSCNVM> UpsertproductionSCNAsync(SubConSCNVM prodCompVM, int screenCode);
        Task<SubConSCNVM?> GetProductionSCNByScnIdAsync(int scnId);
        Task<(List<SubConSCNVM> scnCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        //Task<bool> CheckIfTransactionsMadeAsync(
        //        List<int> itemIds,
        //        int storeId,
        //        int screenCode,
        //        List<int> refSubItemIds);
        Task<bool> IsSCNTransactionsMatchedAsync(
                                         int scnId,
                                         SubConSCNVM scnVms,
                                         int storeId,
                                         int screenCode);
        Task<bool> DeleteProdAssySCNBySCNIdAsync(int scnId, int screenCode);
        //Task<(bool CanDelete, string Message)> CanDeleteProductionSCNAssyAsync(int scnId, int screenCode);
        Task<SubConSCNSubVM?> GetSCNCompItemDetailByScnSubIdAsync(int scnSubId);
        Task DeleteAndResequenceAsync(SubConSCNSubVM subitem, SubConSCNVM prodScn, int screenCode);
        Task<List<SubConSCNSubVM>> GetProdSCNSubBySCNIdAsync(int scnId);
        Task<decimal> GetProdReturnItemBalQtyFromReturnSubId(int returnSubId);
        Task<SubConSCNSubVM?> GetProdScnSubItemDetailByScnSubIdAsync(int scnSubId);
        Task<string> GetSCNNumberAsync(string suffix);
        Task<int> GetReturnPendingCountAsync();

        Task<List<SubConSCNSubVM>> GetDistinctRefGRNNOsbySCNIdAsync(int SCNId);
        Task<SubConSCNVM> GetSubconSCNByIdAsync(int SCNId);

        Task<VendorVM?> GetVendorByIdAsync(int vendorCode);
        Task UpdatedCancelStatusAndAddOrRevertQty(SubConSCNVM scnVM, int screenCode);
        Task UpsertSubConSCNShortCloseAsync(SubConSCNVM SubConSCNVMs);
        Task ValidateGRNBalanceBeforeRevertAsync(SubConSCNSub sub);
        Task<List<SubConSCNSub>> GetSCNSubDetailsBySCNIdAsync(int scnId);

        Task<(bool CanDelete, string Message)> CanDeleteSubConSCNAsync(int SCNId, int screenCode, string refNo);
        Task<bool> GetRejectionSelectionEnableAsync();
        Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync();

        Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId);


        Task<List<SubContractSCNPendingVM>> GetSubContractScnPendingList(string status);//Shankar

        Task<bool> IsDocumentUploaded(int invId);
    }
}
