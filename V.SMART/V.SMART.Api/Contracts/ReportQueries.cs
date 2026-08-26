using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace V.SMART.Api.Contracts
{
    /// <summary>
    /// Query DTO for <c>GET /api/v1/reports/hsn-summary</c> — <c>Sp_GetHSNSummaryReport</c>'s
    /// three parameters, named exactly as
    /// <c>HSNSummaryService.cs:77</c> passes them (<c>@ReportType</c>, <c>@FromDate</c>,
    /// <c>@ToDate</c>), just camel-cased on the wire per ADR-002 §2.
    /// </summary>
    public sealed record HsnSummaryReportQuery : PagedQuery
    {
        [FromQuery(Name = "reportType")]
        [Required(ErrorMessage = "reportType is required.")]
        public string ReportType { get; init; } = string.Empty;

        [FromQuery(Name = "fromDate")]
        public DateTime? FromDate { get; init; }

        [FromQuery(Name = "toDate")]
        public DateTime? ToDate { get; init; }

        // No sortable fields: ExecuteAsync<T> returns whatever order the procedure emits, and
        // paging is applied in memory (ReportRegistry remarks) — there is nothing to sort by
        // that the procedure itself did not already order. An explicit sort request 400s with
        // this empty list, rather than silently doing nothing.
        protected override IReadOnlyList<string> SortableFields { get; } = Array.Empty<string>();
    }

    /// <summary>
    /// Query DTO for <c>GET /api/v1/reports/sales-track</c> — <c>sp_Sales_Track</c>'s three
    /// parameters, named as <c>SalesTrackReportService.cs:57-63</c> passes them.
    /// </summary>
    public sealed record SalesTrackReportQuery : PagedQuery
    {
        [FromQuery(Name = "fromDate")]
        public DateTime? FromDate { get; init; }

        [FromQuery(Name = "toDate")]
        public DateTime? ToDate { get; init; }

        [FromQuery(Name = "customerId")]
        public int? CustomerId { get; init; }

        protected override IReadOnlyList<string> SortableFields { get; } = Array.Empty<string>();
    }

    /// <summary>
    /// Query DTO for <c>GET /api/v1/reports/vendor-pr-rating</c> — <c>Sp_VendorPRRating</c>'s
    /// three parameters, named as <c>PrPoRatingService.cs:99-101</c> passes them.
    /// </summary>
    public sealed record VendorPrRatingReportQuery : PagedQuery
    {
        [FromQuery(Name = "fromDate")]
        public DateTime? FromDate { get; init; }

        [FromQuery(Name = "toDate")]
        public DateTime? ToDate { get; init; }

        [FromQuery(Name = "vendorCode")]
        public int? VendorCode { get; init; }

        protected override IReadOnlyList<string> SortableFields { get; } = Array.Empty<string>();
    }
}
