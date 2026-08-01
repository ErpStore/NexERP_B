using AutoMapper;
using FastReport;


using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IRatingService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ReportViewModel.Ratings;

namespace IQSMART.Shared.BusinessLayer.BusinessService.ReportService.RatingService
{
	public class PrPoRatingService : IPrPoRatingService
	{

		private readonly IUnitOfWork _unitOfWork;
		private readonly ICommonService _commonService;
		private readonly CurrentUserService _currentUserService;
		private readonly ILoggingService _logs;
		private readonly IMapper _mapper;
		private readonly IReportExecutor _report;

		private readonly IStockManagerService _stockManagerService;

		public PrPoRatingService(
			IUnitOfWork unitOfWork,
			ICommonService commonService,
			CurrentUserService userService,
			IStockManagerService stockManagerService,
			ILoggingService logs,
			IMapper mapper,
			IReportExecutor report)
		{
			_unitOfWork = unitOfWork;
			_commonService = commonService;
			_currentUserService = userService;
			_stockManagerService = stockManagerService;
			_logs = logs;
			_mapper = mapper;
			_report = report;
		}


		public async Task<List<Vendor>> GetAllActiveVendorsAsync(DateTime fromDate, DateTime toDate)
		{
			try
			{
				var purchPoSubsData = await _unitOfWork.PurchPoSubs.GetQueryable().ToListAsync();

				var materialReqSubsData = await _unitOfWork.MaterialReqSubs.GetQueryable().ToListAsync();

				var purchPosData = await _unitOfWork.PurchPos.GetQueryable().ToListAsync();

				var vendorIds = (
					from p in purchPoSubsData
					join m in materialReqSubsData
						on p.RefMReqSubId equals m.MreqSubId

					join ps in purchPosData
						on p.PoId equals ps.PoId

					where ps.PODate >= fromDate &&
						  ps.PODate <= toDate.AddDays(1)

					select ps.VendorCode
				)
				.Distinct()
				.ToList();

				var vendors = await _unitOfWork.Vendors.GetQueryable()
					.Where(v => vendorIds.Contains(v.VendorCode))
					.ToListAsync();

				return vendors;
			}
			catch (Exception ex)
			{
				await _logs.LogDeveloperError(ex, "Error in GetAllActiveVendorsAsync()");
				return new List<Vendor>();
			}
		}


		public async Task<List<PrPoratingVM>> GetVendorRatingData(DateTime? fromDate, DateTime? toDate, int? vendorcode)
		{
			try
			{
				var parameters = new[]
				{
				new SqlParameter("@fromDate", fromDate ?? (object)DBNull.Value),
				new SqlParameter("@toDate", toDate ?? (object)DBNull.Value),
				new SqlParameter("@vendorcode", vendorcode ?? (object)DBNull.Value)
				
				};

				var result = await _report.ExecuteAsync<PrPoratingVM>("Sp_VendorPRRating", parameters);
				return result ?? new List<PrPoratingVM>();
			}
			catch (Exception ex)
			{
				await _logs.LogDeveloperError(ex, "Error executing stored procedure GetAccountsAsync");
				return new List<PrPoratingVM>();
			}
		}
	}
}
