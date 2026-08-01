using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices
{
    public interface ILabourSCNService
    {
        //Companydetails
        Task<Companydetails> GetCompanyDetailsAsync();

        //ScreenCode
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        Task<CustomerVM?> GetCustomerByIdAsync(int CustId);

        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<List<ContactPerson>> GetContactPersonsCustomerAsync(int CustId);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<List<Store>> GetAllIssueStoresAsync();

        Task<IEnumerable<Store>> GetAllAddStoresAsync();

        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId);

        Task<int> GetDecimalPlacesAsync();

        //-------------------------SCNList Operation-------------------------

        Task ValidateBeforeRevertAsync(int scnSubId);
        Task<bool> IsSCNTransactionsMatchedAsync(int scnId, LabourSCNVM scnVms);
        Task<(List<LabourSCNVM> Scns, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<bool> HasAnyItemOrLabourSCNCancelAsync(int SCNId);

        Task<(bool CanDelete, string Message)> NeedTocheckRejection(int refGRNSubId, decimal rejQty);

        Task<(bool CanDelete, string Message)> CanDeleteLabourSCNAsync(int SCNId, int screenCode, string refNo);

        Task<(bool CanDelete, string Message)> CanCancelAndRevokelabourSCNAsync(int ReturnId, int ReturnSubId);

        Task<bool> DeleteLabourSCNByIdAsync(int SCNId, int screenCode);
        //-------------------------------------------------------------------------

        //-----------------------SCN Upsert Operation---------------------------
        Task<string> GetLastSCNNumberAsync(string suffix);

        Task<LabourSCNVM> GetLabourSCNByIdAsync(int SCNId);

        Task<bool> GetLabourSCNByIdIsCancelAsync(int SCNId);

        Task<int> GetPendingGRNCountAsync(int CustId);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int CustId);

        Task<decimal> GetGRNItemBalQtyFromGRNSubId(int GRNSubId);

        Task<LabourSCNSubVM?> GetSCNSubItemDetailBySCNSubIdAsync(int SCNSubId);

        Task UpdateItemCancelAndAddorRevertAsync(LabourSCNSubVM subItem, int screenCode, string SCNNo, int AddStoreId, DateTime SCNDate);

        Task<List<LabourSCNSubVM>> GetSCNSubBySCNIdAsync(int scnId);

        Task UpdatedCancelStatusAndAddOrRevertQty(LabourSCNVM scnVM, int screenCode);

        Task DeleteAndResequenceAsync(LabourSCNSubVM subitem, LabourSCNVM scnVM, int screenCode);

        Task<List<Dictionary<string, object>>> GetGRNDetailsByCustId(int CustId, int ScreenCode, int StoreId);

        Task<LabourSCNVM> UpsertSCNAsync(LabourSCNVM labourscnVM, int screenCode);
        Task UpsertlabourSCNShortCloseAsync(LabourSCNVM LabourSCNVms);

        Task ValidateGRNBalanceBeforeRevertAsync(LabourSCNSub sub);

        Task<List<LabourSCNSub>> GetSCNSubDetailsBySCNIdAsync(int scnId);

        Task<bool> GetParamemterLisAsync();

        Task<List<LabourSCNStatusListVM>> GetLabourSCNStatusListAsync(string status);

        Task<bool> IsDocumentUploaded(int scnId);
        Task<bool> GetRejectionSelectionEnableAsync();

        Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync();
    }
}
