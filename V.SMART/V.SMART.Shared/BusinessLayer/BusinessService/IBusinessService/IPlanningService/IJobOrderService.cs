using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.JobOrderStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService
{
    public interface IJobOrderService
    {
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<IEnumerable<ItemVM>> SaerchAssyOrSubAssy(string searchText);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<decimal> GetAvailableStockByItemIdAsync(int itemId, int? storeId);



        // Job Order-specific logic

        Task<List<Dictionary<string, object>>> GetLatestBOMFromAssemblyDefAsync(int AssyId);
        Task<decimal?> GetMfgPoWOBalQtyByRefPoSubId(int poSubId);
        Task<JobOrderVM?> GetJobQtyAndJobBalQtyByJobId(int jobId);
        Task<bool> UpdatedCancelStatusAsync(JobOrderVM jobVM, string cancelReason);
        Task<bool> DeleteJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM? parentJobVM);
        Task<List<JobOrder>> GetReferenceJobListByParentJobId(int parentJobId, int refPoSubId);
        Task<bool> CancelJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM ParentJobOrderVM, string cancelReason);
        Task AdjustPoWorkOrderBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context);
        Task<(List<JobOrderVM> jobOrderVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber, int pageSize,Dictionary<string, object>? filters);
       // Task<List<int>> GetBomItemIdsWithCategory7Async(List<int> itemIds);
        //Task<int?> GetJobIdByMfgPoAndItemIdAsync(int poId, int poSubId, int itemId);
        Task<bool> HasProductionIssueForJobAsync(int itemId, int jobId);
        Task<JobOrderVM?> GetJobOrderByJobIdAsync(int jobId);
        Task UpdateItemCancelAsync(JobOrderSubVM subItem);
        Task<List<JobOrderSubVM>> GetJobOrderSubByJobIdAsync(int jobId);
        Task<JobOrderVM> UpsertJobOrderAsync(JobOrderVM jobOrderVM);
        Task<List<MfgPoVM>> GetPendingPOsByCustomerAsync(int custId);
        Task<List<JobOrderAssyPoItemVM>> GetAssemblyItemsByPoIdAsync(int poId);
        Task<List<JobOrderPreviewVM>> GetBOMHierarchyAsync(List<int> assyIds, List<JobOrderAssyPoItemVM> selectedAssys);
        Task<List<BOMItem>> GetBOMItemsByAssyIdAsync(int assyId);
        Task<int> GetPreviewJobNoAsync(string suffix);
        Task<List<JobOrderSubVM>> GetItemDetailsByAssySubAssyIdAsync(int assyId, decimal joQty);
        Task<bool> DeleteJobOrderByJobIdAsync(int jobId);
        Task<bool> IsPoExists(int jobId, int? assyId);
        Task<(bool CanDelete, string Message)> CanDeleteJobOrderAsync(int jobId);
        //Task<List<BOMItem>> GetBOMSubAssyByAssyIdAsync(int assyId);
        Task<List<Dictionary<string, object>>> GetExistingBOMDetailsAsync(int jobId);
        Task<int?> GetStaffIdByCurrentuserAsync();
        Task<List<StaffVM>> GetStaffOnlyProductionDepartment();
        Task<bool> IsAssignJobOrderDepartmenWise();

        Task<List<JobOrderStatusListVM>> GetJobOrderStatusListAsync(string status);

        Task<bool> IsManualyJobOrderHiden();



    }
}
