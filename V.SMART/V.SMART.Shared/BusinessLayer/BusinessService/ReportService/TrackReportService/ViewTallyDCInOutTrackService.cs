using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class ViewTallyDCInOutTrackService: IViewTallyDCInOutTrackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;


        public ViewTallyDCInOutTrackService(
            IUnitOfWork unitOfWork,
            CurrentUserService userService,
            ICommonService commonService,
            ILoggingService logs, IReportExecutor report)
        {

            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _unitOfWork = unitOfWork;
            _logs = logs;
            _report = report;
        }



        public async Task<List<CustomerVM>> GetCustomersByDate(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var result = await
                (
                    from g in _unitOfWork.LabourGRNs.GetQueryable()
                    join c in _unitOfWork.Customers.GetQueryable()
                        on g.CustId equals c.CustId

                    where g.GRNDate.Date >= fromDate.Date
                       && g.GRNDate.Date <= toDate.Date

                    select new CustomerVM
                    {
                        CustId = c.CustId,
                        CustName = c.CustName
                    }

                ).Distinct().ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load GetCustomersByDate");
                return null;
            }
        }

        public async Task<List<VendorVM>> GetSupplierByDate(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var result = await
                (
                    from g in _unitOfWork.SubConDCOuts.GetQueryable()
                    join c in _unitOfWork.Vendors.GetQueryable()
                        on g.VendorCode equals c.VendorCode

                    where g.DcDate.Date >= fromDate.Date
                       && g.DcDate.Date <= toDate.Date && c.IsitSubCon

                    select new VendorVM
                    {
                        VendorCode = c.VendorCode,
                        VendorName = c.VendorName
                    }

                ).Distinct().ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load GetSupplierByDate");
                return null;
            }
        }
        public async Task<List<ItemVM>> GetItemsByCustomer(DateTime fromDate, DateTime toDate, int custId)
        {
            try
            {
                var result = await
                (
                    from g in _unitOfWork.LabourGRNs.GetQueryable()

                    join gs in _unitOfWork.LabourGRNSubs.GetQueryable()
                        on g.GRNId equals gs.GRNId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on gs.ItemId equals i.ItemId

                    where
                        gs.TransType == "Out" && g.GRNDate.Date >= fromDate.Date
                                              && g.GRNDate.Date <= toDate.Date
                                              && (custId == 0 || g.CustId == custId)


                    select new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode + " - " + i.ItemName,
                    }

                )
                .Distinct()
                .OrderBy(x => x.ItemCode)
                .ToListAsync();

                return result;
            }
            catch(Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load GetItemsByCustomer");
                return null;
            }
        }

        public async Task<List<ItemVM>> GetItemsBySupplier(DateTime fromDate, DateTime toDate,int VendorCode)
        {
            try
            {
                var result = await (
                    from g in _unitOfWork.SubConDCOuts.GetQueryable()

                    join gs in _unitOfWork.SubConDCOutSubs.GetQueryable()
                        on g.DcId equals gs.DcId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on gs.ItemId equals i.ItemId

                    where gs.TransType == "In"
                          && g.DcDate.Date >= fromDate.Date
                          && g.DcDate.Date <= toDate.Date
                          && (VendorCode == 0 || g.VendorCode == VendorCode)

                    select new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode + " - " + i.ItemName
                    }
                )
                .Distinct()
                .OrderBy(x => x.ItemCode)
                .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load GetItemsBySupplier");
                return null;
            }
        }
        public async Task<List<LabourDcInOutTallyVM>> GetViewTallyDcInOutByCustomerAsync(DateTime fromDate, DateTime toDate, int? custId, int? itemId)
        {
            try
            {
                var result = await _report.ExecuteAsync<LabourDcInOutTallyVM>(
                    "Sp_GetLabourDcInOutTrack",

                    new SqlParameter("@FromDate", fromDate),

                    new SqlParameter("@ToDate", toDate),

                    new SqlParameter("@CustId",
                        custId.HasValue ? custId : 0),

                    new SqlParameter("@ItemId",
                        itemId.HasValue ? itemId : 0)
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    "Failed to load GetViewTallyDcInOutAsync");

                return new List<LabourDcInOutTallyVM>();
            }
        }
        public async Task<List<SubContractDcInOutVM>> GetViewTallyDcInOutBySupplierAsync(DateTime fromDate, DateTime toDate, int? SupplyId, int? itemId)
        {
            try
            {
                var result = await _report.ExecuteAsync<SubContractDcInOutVM>(
                    "Sp_GetSubContractDcInOutTrack",

                    new SqlParameter("@FromDate", fromDate),

                    new SqlParameter("@ToDate", toDate),

                    new SqlParameter("@SupplyId",
                        SupplyId.HasValue ? SupplyId : 0),

                    new SqlParameter("@ItemId",
                        itemId.HasValue ? itemId : 0)
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    "Failed to load GetViewTallyDcInOutBySupplierAsync");

                return new List<SubContractDcInOutVM>();
            }
        }
    }
}
