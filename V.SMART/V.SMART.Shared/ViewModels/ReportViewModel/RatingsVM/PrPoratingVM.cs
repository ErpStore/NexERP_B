using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.Ratings
{
	public  class PrPoratingVM
	{

		public long? SlNo { get; set; }

		public string? VendorName { get; set; }

		public string? PRNo { get; set; }

		public string? PRDate { get; set; }

		public decimal? PRQty { get; set; }

		public bool? PRItemCancl { get; set; }

		public string? PRCreatedBy { get; set; }

		public string? PRCreatedDate { get; set; }

		public string? PONo { get; set; }

		public string? PODate { get; set; }

		public decimal? POQty { get; set; }

		public bool? ItemCancel { get; set; }

		public string? POCreatedBy { get; set; }

		public string? POCreatedDate { get; set; }

		public string? ItemCode { get; set; }

		public string? ItemName { get; set; }

		public int? DelayDays { get; set; }

		public string? Rating { get; set; }
	}
}
