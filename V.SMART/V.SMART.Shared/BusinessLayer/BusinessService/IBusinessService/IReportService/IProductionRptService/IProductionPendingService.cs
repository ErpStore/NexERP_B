using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IProductionPendingService
    {
        Task<List<ProductionPendingVM>> GetPendingStatements(bool DetailsView,string partyType, DateTime? FromDate,
              DateTime? ToDate);
    }
}

