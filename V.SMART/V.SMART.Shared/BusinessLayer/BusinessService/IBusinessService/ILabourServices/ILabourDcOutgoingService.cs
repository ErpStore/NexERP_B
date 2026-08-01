using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices
{
    public interface ILabourDcOutgoingService
    {
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        Task<CustomerVM?> GetCustomerByIdAsync(int CustId);

        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<List<ContactPerson>> GetContactPersonsCustomerAsync(int CustId);

        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);

        Task<IEnumerable<Store>> GetAllActiveStoresAsync();

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);

        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<Companydetails> GetCompanyDetailsAsync();

        Task<int> GetDecimalPlacesAsync();

        Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId);
        Task<decimal> GetStockForItemRcSubIdAsync(int ItemId, int StoreId, int? RcSubId);


        Task<bool> GetPriceLockStatusAsync();
        Task<bool> CheckIsSameAsOutItemByPoSubId(int poSubId);
        Task<List<int>> GetReceivedSCNIdsByRefPoSubId(int refPoSubId);
        Task<Dictionary<int, int?>> GetPoSubIdByScnSubIdsAsync(List<int> scnSubIds);
        Task<List<LabourGRNVM>> LoadGrnNumbersAsync(List<int> poIds);


        Task<(List<LabourDcOutgoingVM> Grns, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);

        Task<bool> HasAnyItemOrLabourDcCancelAsync(int DcId);

        Task<(bool CanDelete, string Message)> CanDeleteLabourDcOutgoingAsync(int DcId, int screenCode, string refNo);

        Task<bool> DeleteLabourDcByIdAsync(int DcId, int screenCode);

        //---------------------------------------Dc outgoing operation--------------------------

        Task<decimal> GetLabourGrnItemBalQtyByGrnSubId(int grnSubId);
        Task<List<int>> GetSCNIdsByGRNSubIdAsync(int refGRNSubId);

        Task<List<int>> GetSCNIdsByGRNIds(List<int> grnIds);
        Task<List<Dictionary<string, object>>> GetPOGRNSCNItemDetailsByCustId(int CustId, int StoreId);
        Task<string> GetLastDcNumberAsync(string suffix);

        Task<LabourDcOutgoingVM> GetLabourDcByIdAsync(int DcId);

        Task<List<MfgPoVM>> GetOpenMfgPosByCustomer(int custId);

        Task<int> GetPendingScnGrnPoCountAsync(int CustId);

        Task<bool> GetLabourDcByIdIsCancelAsync(int DcId);

        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int CustId);

        Task<bool> IsOpenPoAsync(int poSubId);

        Task<decimal> GetScnItemBalQtyFromSCNSubId(int ScnSubId);

        Task<LabourDcOutgoingSubVM?> GetDcSubItemDetailByDcSubIdAsync(int dcSubId);

        Task UpdateItemCancelAsync(LabourDcOutgoingSubVM subItem);

        Task<List<LabourDcOutgoingSubVM>> GetDcSubByDcIdAsync(int dcId);

        Task UpdatedCancelStatusAndAddOrRevertQty(LabourDcOutgoingVM dcVM, int screenCode);

        Task UpdateItemCancelAndAddorRevertAsync(LabourDcOutgoingSubVM subItem, int screenCode, LabourDcOutgoingVM dcVM);

        Task DeleteAndResequenceAsync(LabourDcOutgoingSubVM subitem, LabourDcOutgoingVM dcVM,int screencode);

        //Task<List<Dictionary<string, object>>> ScnGrnPoDetailsByCustomer(int CustId, int StoreIssId);
        Task<List<Dictionary<string, object>>> ScnGrnPoDetailsByCustomer(int CustId, List<int> grnIds, int StoreIssId);

        Task<bool> IsDuplicateDcNoAsync(int CustId, string dcNo, string suffix);

        Task<LabourDcOutgoingSubVM?> GetSCNSubItemDetailByDcSubIdAsync(int DcSubId);

        Task<LabourDcOutgoingVM> UpsertDcAsync(LabourDcOutgoingVM labourDcVM, int screenCode);

        Task<string> GetNonReturnableDcNo(string Suffix);

        Task<string> GetPrefixFromDbAsync();


        Task<List<Dictionary<string, object>>> LoadPoSubsByPoIds(List<int> poIds, int StoreIssId);

        
        Task<List<LabourSCNVM>> LoadScnNumbersAsync(List<int> poIds, string check,bool ReceivedReturn);

        Task<List<LabourSCNSubVM>> GetReceivedQtyByScnIdsAsync(List<int> scnIds);

        Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId);

        Task<decimal> GetRawMaterialWeightAsync(int compItemId);

        Task<List<LabourSCNSubVM>> GetReceivedQtyByScnSubIdsAsync(List<int> scnSubIds);

        Task<List<LabourGRNVM>> GeLabourDCByCustomer(int custId);

        Task<List<LabourSCNSubVM>> GetLabourSCNQtyByReceivedIdsAsync(List<int> ReceivedIds);

        Task<decimal?> GetSCNBalQtyBySCNSubId(int SCNSubId,bool SCnRejRewReturn);

        Task<decimal?> GetLaboourGRnBalQtyByGRNSubId(int GRNSubId);
        Task<LabourSCNSubVM?> GetScnDetailsBySCNSubIdAsync(int SCnSubId);

        Task<List<LabourGRNVM>> GeLabourDCWithOutPoByCustomer(int custId);

        Task UpsertlabourDcOutgoingShortCloseAsync(LabourDcOutgoingVM LabourDcOutgoingVMs);

        Task<(bool CanDelete, string Message)> CanCancelAndRevokelabourDcOutgoingAsync(int Dcid, int DcSubId);

        Task<List<LabourDcOutgoingSub>> GetDcOutgoingSubDetailsBydcIdAsync(int DcId);

        Task ValidateLabourSCNBalanceBeforeRevertAsync(LabourDcOutgoingSub sub, LabourDcOutgoingVM LabourDcOutgoingVMs);

        Task<bool> GetLabourDcByIdIsCancelOutItemByAsync(int dcId);
        Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialPoIds(List<int> poIds,List<int>scnid, int StoreIssId,bool ScnRejRewReturn );
        Task<List<MfgPoVM>> GetOpenMfgPosByReturnMaterialCustomer(int custId);

        Task<bool> HasRejectReworkPoAsync(List<int> Posubids);
        Task<bool> HasRejectReworkPoByPoSubidAsync(int PosubId);

        Task<bool> CanlabourDcOutgoingTrasByDcsubIdAsync(List<int> dcSubIds);

        Task<List<ItemVM>> GetRawMaterialItemsAsync(int compItemId);

        Task<List<Dictionary<string, object>>> ScnGrnbyreturnItemRetyurnByCustomer(int custId, List<int> grnIds, int storeIssId,bool SCNRejRewReturn);

        Task<LabourDcOutgoingVM> GetPreviousDataAsync(int custId);
        Task<List<LabourSCNSubVM>> GetReceivedQtyByScnIdsByDetailsAsync(List<int> scnIds);

        Task<List<int>> GetReceivedSCNSubIdsByRefRefGrnSubId(int refGrnSubId);

        Task<bool> GetLabourDcByIdIsReduceInvoicevalueAsync(int dcId);
        Task<int> GetCountAsync();
        Task<List<LabourSCNSubVM>> GetReceivedQtyreturnByScnIdsAsync(List<int> scnIds);
        Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialRejRewPoIds(List<int> poIds, List<int> scnIds, int storeIssId);



        //------------**********Dc Details for EWAY *****-----------------------\\

        Task<List<EWayDocument>> GetLabourDcByCustidAsync(int custId);
        Task<LabourDcOutgoingVM> UpsertPoWiseDcAsync(LabourDcOutgoingVM labourDcVM, int screenCode);
        Task<bool> IsPOWiseLabDcOutEnabledAsync();
        Task DeleteAndResequencePoWiseAsync(LabourDcOutgoingSubVM subitem, LabourDcOutgoingVM dcVM, int ScreenCode);
        Task<bool> DeleteLabourDcPoWiseByIdAsync(int DcId, int screenCode);
        Task UpdatedCancelStatusAndAddOrRevertQtyPoWiseAync(LabourDcOutgoingVM dcVM, int screenCode);
        Task<decimal?> GetLaboourPoBalQtyByPoSubId(int PoSubId);
        Task<List<int>> GetReceivedSCNSubIdsByRefPoSubId(int refPoSubId);
        Task<List<MfgPoVM>> GetOpenMfgPosWiseByCustomer(int custId);
        Task ValidateLabourSCNBalanceBeforeRevertPoWiseAsync(LabourDcOutgoingSub sub, LabourDcOutgoingVM LabourDcOutgoingVMs);

        Task<List<LabourDcOutgoingStatusVM>> GetLabourDcStatusListAsync(string status);
        Task<List<LabourSCNVM>> LoadScnNumbersPoWiseAsync(List<int> poIds, string check, bool ReceivedReturn);

        Task<bool> IsDocumentUploaded(int dcId);

    }
}
