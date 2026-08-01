using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices
{
    public interface ILabourGRNService
    {

        //Screens
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<CustomerVM?> GetCustomerByIdAsync(int CustId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<List<ContactPerson>> GetContactPersonsCustomerAsync(int CustId);
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<IEnumerable<Store>> GetAllActiveStoresAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<int> GetDecimalPlacesAsync();

        //GRN List Operation 

        Task<ItemVM?> GetRawMaterialItemAsync(int compItemId);
        Task ValidatePoBalanceBeforeRevertAsync(LabourGRNSub sub);
        Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId);

        Task<(List<LabourGRNVM> Grns, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<(bool CanDelete, string Message)> ToCheckStockQtyIssued(int GRNId, int screenCode, string refNo);

        Task<bool> DeleteLabourGRNByIdAsync(int GRNId, int screenCode);
        // -----------   Upsert    -----------
        Task<string> GetLastGRNNumberAsync(string suffix);

        Task<int> GetPendingPoCountAsync(int CustId);

        Task<bool> GetLabourGRNByIdIsCancelAsync(int GRNId);

        Task<LabourGRNVM> GetLabourGRNByIdAsync(int GRNId);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int CustId);

        Task<bool> IsOpenPoAsync(int poSubId);

        Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId);

        Task<LabourGRNSubVM?> GetDcSubItemDetailByDcSubIdAsync(int dcSubId);

        Task UpdateItemCancelAndAddorRevertAsync(LabourGRNSubVM subItem, int screenCode, string GRNNo, int AddStoreId, DateTime GRNDate);

        Task<List<LabourGRNSubVM>> GetGRNSubByGRNIdAsync(int grnId);

        Task UpdatedCancelStatusAndAddOrRevertQty(LabourGRNVM grnVM, int screenCode);

        Task DeleteAndResequenceAsync(LabourGRNSubVM subitem, LabourGRNVM dcVM, int screenCode);

        Task<LabourGRNSubVM?> GetGRNSubItemDetailByGRNSubIdAsync(int GRNSubId);

        Task<bool> IsDuplicateDcNoAsync(int CustId, string dcNo, string suffix);

        Task<LabourGRNVM> UpsertGRNAsync(LabourGRNVM labourgrnVM, int screenCode);

        Task<List<Dictionary<string, object>>> GetPoDetailsByVendorCode(int CustId);

        Task<(bool CanItemCancel, string Message)> CanLabourGRNItemCancelCheckAsync(LabourGRNSubVM subItem);

        Task UpsertlabourGRNShortCloseAsync(LabourGRNVM LabourGRnVms);
        Task<bool> IsLabourGRNTransactionsMatchedAsync(int GRNId, LabourGRNVM LabGrn);
        Task<(bool CanDelete, string Message)> CanDeleteLabourGRNAsync(int GRNId, int screenCode);
        Task<List<LabourGRNSub>> GetlabourGRNSubDetailsByDcIdAsync(int DCID);

        Task<byte[]> CreateTemplateAsync(string uploadtype);

        Task<bool> GetPriceLockStatusAsync();
        Task<bool> GetIsSameItemasInAsync();

        Task<List<LabourGRNStatusListVM>> GetLabourGRNStatusListAsync(string status);

        Task<bool> BatchNoOption();

        Task<bool> IsDocumentUploaded(int grnId);
    }
}
