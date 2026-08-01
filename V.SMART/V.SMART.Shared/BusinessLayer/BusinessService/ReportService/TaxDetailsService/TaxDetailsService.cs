
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITaxDetailsReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TaxDetailsReportViewModel;

namespace IQSMART.Shared.BusinessLayer.BusinessService.ReportService.TaxDeatilsReportService
{
    public class TaxDetailsService : ITaxDetailsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
      
        private readonly IReportExecutor _report;


        public TaxDetailsService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            
            IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
           
            _report = report;
        }

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        //screens

        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
                 => await _commonService.GetScreenCodeByScreenNameAsync(screenName);
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<List<Customer>> GetSalesCustomersAsync()
        {
            return await (
                from mi in _unitOfWork.MfgInvs.GetQueryable()
                join c in _unitOfWork.Customers.GetQueryable()
                    on mi.CustId equals c.CustId

                select c
            )
            .Distinct()
            .OrderBy(x => x.CustName)
            .ToListAsync();
        }

        public async Task<List<TaxDetailsVM>> GetTaxDetailsReportAsync(int? custId,int? vendorCode, string topic, DateTime? fromDate,DateTime? toDate)
        {
            
            try
            {
                var result = await _report.ExecuteAsync<TaxDetailsVM>(
                    "Sp_GetTaxDetailsReport",

                    new SqlParameter("@CustId",
                        custId.HasValue ? custId : 0),

                    new SqlParameter("@VendorCode",
                        vendorCode.HasValue ? vendorCode : 0),

                    new SqlParameter("@Topic", topic),

                    new SqlParameter("@FromDate", fromDate),

                    new SqlParameter("@ToDate", toDate)

                );

                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

       

        public async Task<List<Customer>> GetPartiesByTopicAsync(string topic)
        {
            var customers = _unitOfWork.Customers
                .GetQueryable()
                .AsNoTracking();

            var vendors = _unitOfWork.Vendors
                .GetQueryable()
                .AsNoTracking();

            if (topic == "Sales")
            {
                customers = customers.Where(c =>
                    _unitOfWork.MfgInvs.GetQueryable().Any(x => x.CustId == c.CustId)
                    ||
                    _unitOfWork.LabInvs.GetQueryable().Any(x => x.CustId == c.CustId)
                );

                return await customers
                    .OrderBy(x => x.CustName)
                    .ToListAsync();
            }
            else if (topic == "Exports")
            {
                customers = customers.Where(c =>
                    _unitOfWork.ExpInvs.GetQueryable().Any(x => x.CustId == c.CustId)
                );

                return await customers
                    .OrderBy(x => x.CustName)
                    .ToListAsync();
            }
            else if (topic == "Purchase")
            {
                var vendorList = await vendors
                    .Where(v =>
                        _unitOfWork.PurchaseInvoices.GetQueryable().Any(x => x.VendorCode == v.VendorCode)
                        ||
                        _unitOfWork.SubConInvs.GetQueryable().Any(x => x.VendorCode == v.VendorCode)
                    )
                    .OrderBy(v => v.VendorName)
                    .ToListAsync();

                return vendorList.Select(v => new Customer
                {
                    CustId = v.VendorCode,
                    CustName = v.VendorName
                }).ToList();
            }

            return new List<Customer>();
        }

        public async Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable().Where(c => c.ReferenceId == refId && c.ReferenceType == refType).CountAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error counting attachments for Invno={refId}, RefType={refType}.");
                return 0;
            }
        }
    }
}
