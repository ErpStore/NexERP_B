using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IPaidBillsService
    {


        Task<List<PaidBillsVM>> GetPaidBills(string? billType, DateTime? fromDate, DateTime? toDate);

    }
}
