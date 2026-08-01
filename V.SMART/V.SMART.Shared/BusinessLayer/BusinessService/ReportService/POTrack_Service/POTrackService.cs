using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IPOTrack_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.POTrackVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.POTrack_Service
{
    public class POTrackService: IPOTrackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;

        public POTrackService(IUnitOfWork unitOfWork,CurrentUserService userService,ICommonService commonService,ILoggingService logs, IReportExecutor report)
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
                    from g in _unitOfWork.MfgPos.GetQueryable()
                    join c in _unitOfWork.Customers.GetQueryable()
                        on g.CustId equals c.CustId

                    where g.PODate.Value.Date >= fromDate.Date
                       && g.PODate.Value.Date <= toDate.Date

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
                    from g in _unitOfWork.PurchPos.GetQueryable()
                    join c in _unitOfWork.Vendors.GetQueryable()
                        on g.VendorCode equals c.VendorCode

                    where g.PODate.Date >= fromDate.Date
                       && g.PODate.Date <= toDate.Date && c.IsitSubCon

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


        public async Task<List<InwardPOTrack>> GetInwordPOTrack(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new("@FromDate", fromDate),
                    new("@ToDate", toDate)
                };

                var result = await _report.ExecuteAsync<InwardPOTrack>("SP_Get_PO_TRACKING", parameters);

                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<PurchasePOTrack>> GePurchasePOTrack(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var parameters = new SqlParameter[]
                {
                    new("@FromDate", fromDate),
                    new("@ToDate", toDate)
                };

                var result = await _report.ExecuteAsync<PurchasePOTrack>("SP_Get_Purchase_PO_TRACKING", parameters);

                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
