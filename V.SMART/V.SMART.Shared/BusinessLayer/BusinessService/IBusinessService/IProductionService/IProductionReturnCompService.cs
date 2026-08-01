using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService
{
    public interface IProductionReturnCompService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<TermsAndConditions>> GetTermsAsync();
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);

        Task<IEnumerable<CustomerVM>> SearchCustomers(string searchText);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<int> GetDecimalPlacesAsync();

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        Task<List<Machine>> GetAllActiveMachinesAsync();

        // 🔹 Production Component - specific logic

        Task<RouteCardSub?> GetRcDetailsByRcSubIdAsync(int rcSubId);
        Task<List<Dictionary<string, object>>> GetAllOpenRcDetailsAsync(int? custId);
        Task<ProductionReturnCompVM> UpsertProductionReturnAsync(ProductionReturnCompVM prodReturnCompVM, int screenCode);
        Task<List<ProductionIssueCompSubVM>> GetIssuedQtyByIssueIdsAsync(List<int> issueIds);
        Task<List<int>> GetIssueIdsByPoSubIdAsync(int poSubId);
        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId);
        Task<ProductionReturnCompVM?> GetProductionByReturnIdAsync(int returnId);
        Task<List<int>> GetIssueIdsByRcSubIdAsync(int rcSubId);
        Task<List<ProductionReturnCompTrackVM>> GetAllExistingRouteCardAsync();
        Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                        int screenCode,
                        int storeId,
                        int itemId,
                        int subItemRefId);
        Task<(bool CanDelete, string Message)> CanDeleteProductionReturnCompAsync(int returnId, int screenCode);
        Task<int> GetPendingJobOrdersCountAsync();
        Task<(bool IsValid, string Message)> ValidateDeleteAsync(int jobId, int itemId, decimal qtyReturned, int addStoreId);
        Task<(List<ProductionReturnCompVM> returnCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);

        Task<decimal?> GetIssueBalQtyByIssueSubId(int issueSubId);
        Task<List<Dictionary<string, object>>> GetAllProductionIssuedItemsAsync();
        Task<string> GetProductionReturnNumberAsync(string suffix);
        Task<List<ProductionReturnCompSubVM>> GetProductionReturnSubByReturnIdAsync(int returnId);
        Task<decimal> GetJobOrderBalQtyFromJobId(int jobId);
        Task<ProductionReturnCompSubVM?> GetProductionReturnSubItemDetailByReturnSubIdAsync(int returnSubId);
        Task<ProductionReturnCompSubVM?> GetProdReturnSubItemDetailByReturnSubIdAsync(int returnSubId);
        Task<bool> DeleteProdAssyReturnByReturnIdAsync(int returnId, int screenCode);

        Task<List<ProductionReturnCompStatusVM>> GetProductionReturnComponentStatusListAsync(string status);
        Task<bool> IsPOWiseProductionDcOutEnabledAsync();
        Task<bool> GetIsSameItemasInAsync();
        Task<List<ProductionIssueCompVM>> LoadDcOutNumbersPoWiseAsync(List<int> poIds, string check, bool receivedReturn);
        Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialPoIds(List<int> poIds, List<int> DcoutIds, int storeIssId);
        Task<int> GetPendingPoCountAsync(int CustId);
        Task<int> GetPendingDcOutsPoCountAsync(int CustId);
        Task<List<MfgPoVM>> GetOpenPurchPosByReturnMaterialVendor(int CustId);
        Task<List<Dictionary<string, object>>> GetPoDetailsBySelectedPoidCustId(List<int> poIds);
        Task<List<ProductionIssueCompSubVM>> GetProductionIssueQtyByDcIdsAsync(List<int> IssueIds);
        Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId);
        Task<decimal> GetRawMaterialWeightAsync(int compItemId);
        Task<List<ItemVM>> GetRawMaterialItemsAsync(int compItemId);
        Task<List<ProductionIssueCompSubVM>> GetProductionCompQtyByIssueIdsAsync(List<int> IssueIds);
        Task<List<Dictionary<string, object>>> GetProductionDetailsByWithOutPoCustId(int CustId);
        Task<List<Dictionary<string, object>>> GetDCDetailsByCustId(int CustId);
        Task<List<Dictionary<string, object>>> GetProductionDetailsByReturnCustId(int CustId);
        Task<List<int>> GetProductionConDcOutIdsByDcSubIdAsync(int IssueSubId);
        Task<List<int>> GetIssueIdsByIssueSubIdsAsync(List<int> IssueSubIds);
        Task<List<ProductionReturnCompSubVM>> GetproductionGrnSubVMsByIssueIdsAsync(List<int> dcIds);

        Task<bool> CheckIsSameAsOutItemByPoSubId(int poSubId);
        Task<(bool CanDelete, string Message)> CanRemoveProductionReturnAsync(int ReturnId, int ReturnSubId);

        Task UpdatedCancelStatusAndAddOrRevertQtyPoWiseAync(ProductionReturnCompVM dcVM, int screenCode);
        Task UpsertProductionReturnShortCloseAsync(ProductionReturnCompVM ProductionReturnCompVMs);
        Task<List<Dictionary<string, object>>> GetDCDetailsBy();

        Task<bool> CheckLabourGrnExist(int? refIssueSubId);
        Task<List<ProductionReturnCompSub>> GetProductionReturnByReturnidAsync(int ReturnId);
        Task ValidateProductionIssueBalanceBeforeRevertAsync(ProductionReturnCompSub sub, ProductionReturnCompVM ProductionReturnCompVMs);
        Task<bool> DeleteByProductionReturnSubIdAsync(int returnSubId, int screenCode);

        Task<List<ShiftAllocation>> GetAllShifts();

        Task<bool> IsProductionLogAsync();

        Task<List<Process>> GetAllProcess();



    }
}
