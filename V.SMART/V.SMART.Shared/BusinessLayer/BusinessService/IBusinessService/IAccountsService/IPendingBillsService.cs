using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;


namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IPendingBillsService
    {

        Task<List<PendingBillVM>> GetPendingBills(string? billType,DateTime? fromDate,DateTime? toDate);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

    }
}
