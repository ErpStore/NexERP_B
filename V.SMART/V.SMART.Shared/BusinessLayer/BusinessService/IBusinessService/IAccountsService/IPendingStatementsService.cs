using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IPendingStatementsService 
    {

        Task<List<PendingStatementsVM>> GetPendingStatements(bool detailsview, string partyType, DateTime? fromDate, DateTime? toDate);
    }
}
