using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.ViewModels.ReportViewModel.AccountsReportViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.Ratings;
using V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel;

namespace V.SMART.Api.Reporting
{
    /// <summary>One typed parameter a report slug accepts, for the catalogue response.</summary>
    /// <param name="Name">The wire query-parameter name.</param>
    /// <param name="Type">A short type label for the catalogue — <c>"string"</c>, <c>"date"</c>
    /// or <c>"int"</c>. Not a .NET type name; this is a client-facing hint, not a contract.</param>
    /// <param name="Required">Whether the report can run without it.</param>
    public sealed record ReportParameterDescriptor(string Name, string Type, bool Required);

    /// <summary>
    /// One analytical report. <c>ReportExecutor.ExecuteAsync&lt;T&gt;</c> is generic per call, so
    /// each entry closes over its own <c>T</c> inside <see cref="Execute"/> rather than the
    /// registry trying to be generic over a heterogeneous list — the executor lambda is the
    /// allow-list's only place that needs to know the concrete result type.
    /// </summary>
    /// <param name="Slug">The kebab-case URL segment: <c>GET /api/v1/reports/{Slug}</c>. Never a
    /// procedure name — see the class remarks.</param>
    /// <param name="DisplayName">Human-readable name for the catalogue.</param>
    /// <param name="ScreenName">Seeded <c>Screens.ScreenName</c>, verified against
    /// <c>ScreenCatalogue.SeededScreenNames</c> before this entry was added.</param>
    /// <param name="ProcedureName">The stored procedure. Never derived from caller input.</param>
    /// <param name="Parameters">Declared for the catalogue response and for building the
    /// <see cref="SqlParameter"/> array from the bound query DTO.</param>
    /// <param name="Execute">
    /// Runs the procedure via <see cref="IReportExecutor.ExecuteAsync{T}"/> against the closed-over
    /// result type and returns the rows boxed as <c>object</c>. <c>System.Text.Json</c> serialises
    /// each element by its runtime type, so the concrete VM's real shape reaches the wire even
    /// though the static element type here is <c>object</c> (proven by
    /// <c>tests/V.SMART.Api.Tests/Reporting/ReportRegistryTests.cs</c>).
    /// </param>
    public sealed record ReportRegistryEntry(
        string Slug,
        string DisplayName,
        string ScreenName,
        string ProcedureName,
        IReadOnlyList<ReportParameterDescriptor> Parameters,
        Func<IReportExecutor, SqlParameter[], Task<IReadOnlyList<object>>> Execute,
        string SourceCallSite);

    /// <summary>
    /// The report allow-list (M2-B08 §Target Result item 4) — the same role as
    /// <see cref="PrintRegistry"/>, for <c>GET /api/v1/reports/{slug}</c>. <c>{slug}</c> is looked
    /// up here and only here; it is never interpolated from caller input into
    /// <c>EXEC dbo.{procedureName}</c> (that string concatenation happens inside
    /// <c>ReportExecutor.cs:27</c>, unchanged by this task, over a name this table already fixed).
    /// </summary>
    /// <remarks>
    /// <b>Seeded with 3 entries, not the ceiling of 5</b> the task allows. Three is enough to
    /// prove the general mechanism — a typed query DTO per slug, <see cref="SqlParameter"/>
    /// construction, the generic <c>ExecuteAsync&lt;T&gt;</c> call, and in-memory paging of the
    /// result — across three genuinely different parameter shapes (one string + two dates; two
    /// dates + a customer id; two dates + a vendor code). Filling in the remaining ~37
    /// <c>IReportExecutor</c> call sites is module-wave work (M3/M4), not M2.
    /// <para>
    /// <b>Paging is in memory, not server-side, and every entry below says so via the shared
    /// catalogue response — see <c>ReportsController.GetCatalogue</c>.</b> None of these
    /// procedures accept <c>@Skip</c>/<c>@Take</c>; <c>ReportExecutor.cs:48-86</c> is a
    /// commented-out attempt at exactly that, abandoned. Adding those parameters to a procedure
    /// is a schema change and out of this task's scope.
    /// </para>
    /// </remarks>
    public static class ReportRegistry
    {
        public static IReadOnlyList<ReportRegistryEntry> Entries { get; } = new[]
        {
            new ReportRegistryEntry(
                Slug: "hsn-summary",
                DisplayName: "HSN Summary",
                ScreenName: "HSNSummary Report",
                ProcedureName: "Sp_GetHSNSummaryReport",
                Parameters: new[]
                {
                    new ReportParameterDescriptor("reportType", "string", Required: true),
                    new ReportParameterDescriptor("fromDate", "date", Required: false),
                    new ReportParameterDescriptor("toDate", "date", Required: false),
                },
                Execute: async (executor, parameters) =>
                    (await executor.ExecuteAsync<HSNSummaryVM>("Sp_GetHSNSummaryReport", parameters))
                        .Cast<object>()
                        .ToList(),
                SourceCallSite: "V.SMART.Shared/BusinessLayer/BusinessService/AccountsService/HSNSummaryService.cs:77"),

            new ReportRegistryEntry(
                Slug: "sales-track",
                DisplayName: "Sales Track",
                ScreenName: "Sales Track Report",
                ProcedureName: "sp_Sales_Track",
                Parameters: new[]
                {
                    new ReportParameterDescriptor("fromDate", "date", Required: false),
                    new ReportParameterDescriptor("toDate", "date", Required: false),
                    new ReportParameterDescriptor("customerId", "int", Required: false),
                },
                Execute: async (executor, parameters) =>
                    (await executor.ExecuteAsync<SalesTrackVM>("sp_Sales_Track", parameters))
                        .Cast<object>()
                        .ToList(),
                SourceCallSite: "V.SMART.Shared/BusinessLayer/BusinessService/ReportService/TrackReportService/SalesTrackReportService.cs:67"),

            new ReportRegistryEntry(
                Slug: "vendor-pr-rating",
                DisplayName: "Vendor PR/PO Rating",
                ScreenName: "PR PO Rating Report",
                ProcedureName: "Sp_VendorPRRating",
                Parameters: new[]
                {
                    new ReportParameterDescriptor("fromDate", "date", Required: false),
                    new ReportParameterDescriptor("toDate", "date", Required: false),
                    new ReportParameterDescriptor("vendorCode", "int", Required: false),
                },
                Execute: async (executor, parameters) =>
                    (await executor.ExecuteAsync<PrPoratingVM>("Sp_VendorPRRating", parameters))
                        .Cast<object>()
                        .ToList(),
                SourceCallSite: "V.SMART.Shared/BusinessLayer/BusinessService/ReportService/Rating_Services/PrPoRatingService.cs:106"),
        };

        /// <summary>Case-sensitive lookup by URL slug, or <c>null</c> if unregistered.</summary>
        public static ReportRegistryEntry? Find(string slug)
            => Entries.FirstOrDefault(e => string.Equals(e.Slug, slug, StringComparison.Ordinal));
    }
}
