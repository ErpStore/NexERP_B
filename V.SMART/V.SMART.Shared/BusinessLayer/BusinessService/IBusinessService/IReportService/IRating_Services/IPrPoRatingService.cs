
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels.ReportViewModel.Ratings;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IRatingService
{
	public interface IPrPoRatingService
	{

		Task<List<Vendor>> GetAllActiveVendorsAsync(DateTime fromdate, DateTime todate);

		Task<List<PrPoratingVM>> GetVendorRatingData(DateTime? fromDate, DateTime? toDate, int? vendorcode);

	}
}
