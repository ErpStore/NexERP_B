using DocumentFormat.OpenXml.Spreadsheet;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.TrackReportService
{
    public class RejectionService : IRejectionService
    {
        private readonly IReportExecutor _report;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;

        public RejectionService(IReportExecutor report, IUnitOfWork unitOfWork, ICommonService commonService)
        {
            _report = report;
            _unitOfWork = unitOfWork;
            _commonService = commonService;
        }

        // =========================
        // MAIN REPORT
        // =========================
        public async Task<List<RejectionVM>> GetRejectionAnalysisAsync(int reportType, DateTime fromDate, DateTime toDate, int partyId = 0, int itemId = 0)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReportType", reportType),
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate),
                new SqlParameter("@PartyId", partyId),
                new SqlParameter("@ItemId", itemId)
            };

            var result = await _report.ExecuteAsync<RejectionVM>(
                "SP_GetRejectionAnalysis",
                parameters);

            return result ?? new List<RejectionVM>();
        }

        public async Task<List<CustomerVM>> GetCustomersAsync(DateTime fromDate, DateTime toDate, string typeOfCustomer)
        {
            try
            {
                List<CustomerVM> customers;

                if (typeOfCustomer == "Sales")
                {
                    customers = await (from c in _unitOfWork.Customers.GetQueryable()
                                       join d in _unitOfWork.MfgDcs.GetQueryable()
                                           on c.CustId equals d.CustId
                                       join e in _unitOfWork.MfgDcSubs.GetQueryable()
                                           on d.DcId equals e.DcId
                                       where d.DcDate >= fromDate
                                             && d.DcDate <= toDate
                                       //  && (e.RejectQty > 0 || e.ReworkQty > 0)
                                       group c by new { c.CustId, c.CustName } into g
                                       orderby g.Key.CustName
                                       select new CustomerVM
                                       {
                                           CustId = g.Key.CustId,
                                           CustName = g.Key.CustName
                                       })
                               .ToListAsync();
                }
                else
                {
                    customers = await (
                        from c in _unitOfWork.Customers.GetQueryable()
                        join d in _unitOfWork.LabourDcOutgoings.GetQueryable()
                            on c.CustId equals d.CustId
                        join e in _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                            on d.DcId equals e.DcId
                        where d.DcDate >= fromDate
                              && d.DcDate <= toDate
                        //  && (e.RejectQty > 0 || e.ReworkQty > 0)
                        where d.DcDate >= fromDate && d.DcDate <= toDate
                        select new CustomerVM
                        {
                            CustId = c.CustId,
                            CustName = c.CustName
                        })
                        .Distinct()
                        .OrderBy(x => x.CustName)
                        .ToListAsync();
                }

                // Add "All" option at the beginning
                customers.Insert(0, new CustomerVM
                {
                    CustId = 0,
                    CustName = "All"
                });

                return customers;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<VendorVM>> GetVendorsAsync(DateTime fromDate, DateTime toDate, string typeOfVendor)
        {
            try
            {
                List<VendorVM> vendors;

                if (typeOfVendor == "Sub Contract")
                {
                    vendors = await (
                     from v in _unitOfWork.Vendors.GetQueryable()
                     join s in _unitOfWork.SubConSCNs.GetQueryable()
                         on v.VendorCode equals s.VendorCode
                     join sub in _unitOfWork.SubConSCNSubs.GetQueryable()
                         on s.SCNId equals sub.SCNId
                     where s.SCNDate >= fromDate
                           && s.SCNDate <= toDate
                     // && (sub.RejQty > 0 || sub.RewQty > 0)
                     select new VendorVM
                     {
                         VendorCode = v.VendorCode,
                         VendorName = v.VendorName
                     })
                     .Distinct()
                     .OrderBy(x => x.VendorName)
                     .ToListAsync();
                }
                else if (typeOfVendor == "Purchase")
                {
                    vendors = await (
                        from v in _unitOfWork.Vendors.GetQueryable()
                        join p in _unitOfWork.PurchaseSCNs.GetQueryable()
                            on v.VendorCode equals p.VendorCode
                        join ps in _unitOfWork.PurchaseSCNSubs.GetQueryable()
                            on p.SCNId equals ps.SCNId
                        where p.SCNDate >= fromDate
                              && p.SCNDate <= toDate
                        //  && (ps.RejectQty > 0 || ps.ReworkQty > 0)
                        select new VendorVM
                        {
                            VendorCode = v.VendorCode,
                            VendorName = v.VendorName
                        })
                        .Distinct()
                        .OrderBy(x => x.VendorName)
                        .ToListAsync();
                }
                else
                {
                    vendors = new List<VendorVM>();
                }

                vendors.Insert(0, new VendorVM
                {
                    VendorCode = 0,
                    VendorName = "All"
                });

                return vendors;
            }
            catch
            {
                throw;
            }
        }


        public async Task<List<ItemVM>> GetItemsAsync(DateTime fromDate, DateTime toDate, string typeOfCustomer, int custId)
        {
            try
            {
                List<int> itemIds;

                if (typeOfCustomer == "Sales")
                {
                    itemIds = await (from sub in _unitOfWork.MfgDcSubs.GetQueryable()
                                     join dc in _unitOfWork.MfgDcs.GetQueryable()
                                         on sub.DcId equals dc.DcId
                                     where dc.DcDate >= fromDate
                                           && dc.DcDate <= toDate
                                           && sub.ItemId.HasValue
                                           //    && (sub.RejectQty > 0 || sub.ReworkQty > 0)
                                           && (custId == 0 || dc.CustId == custId)
                                     select sub.ItemId.Value
                                      )
                                      .Distinct()
                                      .ToListAsync();
                }
                else if (typeOfCustomer == "Labour")
                {
                    itemIds = await (
                        from sub in _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                        join dc in _unitOfWork.LabourDcOutgoings.GetQueryable()
                            on sub.DcId equals dc.DcId
                        where dc.DcDate >= fromDate
                              && dc.DcDate <= toDate
                              && (custId == 0 || dc.CustId == custId)
                        //   && (sub.RejectQty > 0 || sub.ReworkQty > 0)
                        select sub.ItemId
                    )
                    .Distinct()
                    .ToListAsync();
                }
                else if (typeOfCustomer == "Sub Contract")
                {
                    itemIds = await (
                        from sub in _unitOfWork.SubConSCNSubs.GetQueryable()
                        join scn in _unitOfWork.SubConSCNs.GetQueryable()
                            on sub.SCNId equals scn.SCNId
                        where scn.SCNDate >= fromDate
                              && scn.SCNDate <= toDate
                              && (custId == 0 || scn.VendorCode == custId)
                        // && (sub.RejQty > 0 || sub.RewQty > 0)
                        select sub.ItemId
                    )
                    .Distinct()
                    .ToListAsync();
                }
                else if (typeOfCustomer == "Purchase")
                {
                    itemIds = await (
                        from sub in _unitOfWork.PurchaseSCNSubs.GetQueryable()
                        join scn in _unitOfWork.PurchaseSCNs.GetQueryable()
                            on sub.SCNId equals scn.SCNId
                        where scn.SCNDate >= fromDate
                              && scn.SCNDate <= toDate
                              && (custId == 0 || scn.VendorCode == custId)
                        //   && (sub.RejectQty > 0 || sub.ReworkQty > 0)
                        select sub.ItemId
                    )
                    .Distinct()
                    .ToListAsync();
                }
                else
                {
                    return new List<ItemVM>();
                }

                return await _commonService.GetItemVMsByItemIdsAsync(itemIds);
            }


            catch
            {
                throw;
            }
        }

    }
}