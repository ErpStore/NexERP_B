using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Pages.Report_Module_Pages.PoPending_Pages;
using V.SMART.Shared.Repository;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.RatingsVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class GetRatingsService : IRatingService
    {

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICommonService _commonService;
    private readonly CurrentUserService _currentUserService;
    private readonly IMapper _mapper;
    private readonly ILoggingService _logs;
    private readonly IReportExecutor _report;


    public GetRatingsService(
        IUnitOfWork unitOfWork,
        ICommonService commonService,
        CurrentUserService userService,
        ILoggingService logs,
        IMapper mapper,
        IReportExecutor report
        )
    {
        _unitOfWork = unitOfWork;
        _commonService = commonService;
        _currentUserService = userService;
        _logs = logs;
        _mapper = mapper;
        _report = report;

    }


    public async Task<List<RatingVM>> GetRatings(
    bool DetailsView,
    string partyType,
    string partyId,
    DateTime? fromDate,
    DateTime? toDate)
    {
        try
        {
            var parameters = new[]
            {
                    new SqlParameter("@DetailsView", DetailsView != null ? (object)DetailsView : DBNull.Value),
                    new SqlParameter("@partyType", partyType ?? (object)DBNull.Value),
                    new SqlParameter("@PartyId", partyId ?? (object)DBNull.Value),
                    new SqlParameter("@FromDate", fromDate ?? (object)DBNull.Value),
                    new SqlParameter("@ToDate", toDate ?? (object)DBNull.Value)
                 

            };

            var result = await _report.ExecuteAsync<RatingVM>(
                "sp_GetRatingsReport",
                parameters
            );

            return result ?? new List<RatingVM>();
        }
        catch (Exception ex)
        {
            _logs.LogDeveloperError(
                ex,
                "Error executing stored procedure SP_BillPending"
            );

            return new List<RatingVM>();
        }
    }

        public async Task<List<CustomerVM>> GetAllCustomersAsync(string type, DateTime fromdate, DateTime todate)
        {
            try
            {
                IQueryable<Customer> query = null;

                if (type == "SALES")
                {
                    query = _unitOfWork.MfgDcs.GetQueryable()
                            .Where(x => x.DcDate.Date >= fromdate.Date && x.DcDate.Date <= todate.Date)
                            .Select(x => x.Customer);
                }
                else if (type == "LABOUR WORK")
                {
                    query = _unitOfWork.LabourDcOutgoings.GetQueryable()
                            .Where(x => x.DcDate.Date >= fromdate.Date && x.DcDate.Date <= todate.Date)
                            .Select(x => x.Customer);
                }

                if (query == null)
                    return new List<CustomerVM>();

                var customers = await query
                    .Distinct()
                    .ToListAsync();

                if (customers == null || !customers.Any())
                {
                    return new List<CustomerVM>();
                }

                return _mapper.Map<List<CustomerVM>>(customers);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetAllCustomersAsync");
                return new List<CustomerVM>();
            }
        }

        public async Task<List<VendorVM>> GetAllVendorsAsync(string type, DateTime fromdate, DateTime todate)
        {
            try
            {
                IQueryable<Vendor> query = null;

                if (type == "PURCHASE")
                {
                    query = _unitOfWork.PurchaseGRNs.GetQueryable()
                            .Where(x => x.GRNDate.Date >= fromdate.Date && x.GRNDate.Date <= todate.Date)
                            .Select(x => x.Vendor);
                }
                else if (type == "SUBCONTRACT")
                {
                    query = _unitOfWork.SubConGRNs.GetQueryable()
                            .Where(x => x.GRNDate.Date >= fromdate.Date && x.GRNDate.Date <= todate.Date)
                            .Select(x => x.vendor);
                }

                if (query == null)
                    return new List<VendorVM>();

                var vendors = await query
                    .Distinct()
                    .ToListAsync();

                if (vendors == null || !vendors.Any())
                {
                    return new List<VendorVM>();
                }

                return _mapper.Map<List<VendorVM>>(vendors);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error executing GetAllVendorsAsync");
                return new List<VendorVM>();
            }
        }

       
    }

}
    
   
