using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService
{
    public interface IProfit_LossAccountService
    {

        Task<List<Profit_LossAccountsVM>> GetProfitLossAsync(DateTime fromDate, DateTime toDate);

        Task<List<ProfitLossMonthlyVM>>  GetMonthlyProfitAsync(DateTime fromDate,DateTime toDate);


    }
}
