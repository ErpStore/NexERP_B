using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.GSTITCVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IGSTITC
{
    public interface IGSTITCService
    {
        Task<byte[]> ExportITCReport(DateTime fromDate, DateTime toDate, int reportType);

        Task<List<ITC04OutwardVM>> GetITC04OutwardDetailsAsync(DateTime fromDate, DateTime toDate);

        Task<List<ITC04InwardVM>> GetITC04InwardDetailsAsync(DateTime fromDate,DateTime toDate);
    }
}
