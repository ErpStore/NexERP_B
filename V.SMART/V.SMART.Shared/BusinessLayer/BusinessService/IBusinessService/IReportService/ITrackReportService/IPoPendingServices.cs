
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;

using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IPoPendingServices
    {
        // 🔹 Customer Dropdown
        //Task<List<ComboModelVM>> GetCustomerList(int? custId);

        //// 🔹 Vendor Dropdown
        //Task<List<ComboModelVM>> GetVendorList(int? vendorId);

        //// 🔥 PO Pending Details
        //Task<List<PoPendingDetailsVM>> GetPoPendingDetails(string type, int id);

        Task<List<ComboModelVM>> GetCustomerList(int? custId, DateTime? fromDate, DateTime? toDate);

        Task<List<ComboModelVM>> GetVendorList(int? vendorId, DateTime? fromDate, DateTime? toDate);

        Task<List<PoPendingDetailsVM>> GetPoPendingDetails(string type, int id, DateTime? fromDate, DateTime? toDate);

    }
}