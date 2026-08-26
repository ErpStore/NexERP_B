namespace V.SMART.Api.Reporting
{
    /// <summary>
    /// Which of <c>ReportService</c>'s public entry points a print action calls. The two
    /// members share an identical <c>(int, string, string, bool, string, string)</c> signature
    /// (M2-B08.md §Why This Task Exists), so the registry can name either without a third
    /// abstraction. <c>Generate_Attendance_Report</c> is deliberately absent — its signature has
    /// no <c>id</c> and no <c>screenName</c>, so it does not fit this registry's shape at all
    /// (M2-B08.md, same section) and is left for a future task to give its own route.
    /// </summary>
    public enum PrintGenerator
    {
        /// <summary><c>ReportService.Generate_Report</c> (:40) — 78 of the 79 real callers.</summary>
        GenerateReport,

        /// <summary><c>ReportService.GenerateSalarySlipReport</c> (:169) — same signature, 2 callers.</summary>
        GenerateSalarySlipReport,
    }

    /// <summary>
    /// One printable document. Every field here is the exact argument a real Blazor call site
    /// passes to <c>Generate_Report</c>/<c>GenerateSalarySlipReport</c> today — transcribed from
    /// the call site, not invented. See the task's investigation record (INV row, KB-081) for
    /// the full print map this was sampled from.
    /// </summary>
    /// <param name="Resource">The kebab-case URL segment: <c>GET /api/v1/{Resource}/{id:int}/print</c>.</param>
    /// <param name="ScreenName">
    /// The seeded <c>Screens.ScreenName</c>, byte for byte — matched ordinally and
    /// case-sensitively (KB-105 D-1), verified present in <c>ScreenCatalogue.SeededScreenNames</c>
    /// before this entry was added.
    /// </param>
    /// <param name="Template">The <c>.frx</c> file name, unqualified — <c>ReportService</c> resolves
    /// the tenant/default folder itself.</param>
    /// <param name="Parameter">The SQL parameter name the report's main data source binds <c>id</c> to.</param>
    /// <param name="ProcedureName">The stored procedure supplying the main data source.</param>
    /// <param name="Generator">Which <c>ReportService</c> method produces the PDF.</param>
    /// <param name="SourceCallSite">Where this entry's values were transcribed from — <c>file:line</c>,
    /// for anyone auditing the registry against the Blazor call site it mirrors.</param>
    public sealed record PrintRegistryEntry(
        string Resource,
        string ScreenName,
        string Template,
        string Parameter,
        string ProcedureName,
        PrintGenerator Generator,
        string SourceCallSite);

    /// <summary>
    /// The print allow-list (M2-B08 §Target Result item 4). <c>ReportExecutor</c>/<c>ReportService</c>
    /// interpolate a procedure name into <c>EXEC dbo.{procedureName}</c> with no parameterisation
    /// of the name itself (ReportExecutor.cs:27) — a route that let a caller name the procedure
    /// would be a SQL-injection surface. This table is what makes <c>{resource}</c> in the route
    /// safe: it is looked up here, never passed through.
    /// </summary>
    /// <remarks>
    /// <b>Seeded with 3 entries, not the ceiling of 5</b> the task allows ("at most 5 print
    /// entries — enough to prove every branch"). Three is enough to exercise both
    /// <see cref="PrintGenerator"/> members (Purchase Order and Job Order use
    /// <c>GenerateReport</c>; Salary Slip uses <c>GenerateSalarySlipReport</c>) and the
    /// per-tenant/default template fallback (all three resolve through the same
    /// <see cref="ApiPathProvider"/>). Filling in the remaining ~76 print call sites is module-wave
    /// work (M3/M4), not M2 — same boundary the task file itself draws.
    /// </remarks>
    public static class PrintRegistry
    {
        public static IReadOnlyList<PrintRegistryEntry> Entries { get; } = new[]
        {
            new PrintRegistryEntry(
                Resource: "purchase-pos",
                ScreenName: "Purchase Order",
                Template: "PurchaseOrder.frx",
                Parameter: "PoId",
                ProcedureName: "Sp_Print_PurchasePo",
                Generator: PrintGenerator.GenerateReport,
                SourceCallSite: "V.SMART.Shared/Pages/OutSourcing_Module_pages/PurchOrSubConPO_Pages/PurchasePoDetails.razor:306"),

            new PrintRegistryEntry(
                Resource: "job-orders",
                ScreenName: "Job Order",
                Template: "JobOrder.frx",
                Parameter: "JobId",
                ProcedureName: "Sp_Print_JobOrder",
                Generator: PrintGenerator.GenerateReport,
                SourceCallSite: "V.SMART.Shared/Pages/Planning_Module_Pages/JobOrder_Pages/JobOrderDetails.razor:290"),

            new PrintRegistryEntry(
                Resource: "salary-slips",
                ScreenName: "Salary",
                Template: "SalarySlip.frx",
                Parameter: "RowId",
                ProcedureName: "Sp_Print_Salary",
                Generator: PrintGenerator.GenerateSalarySlipReport,
                SourceCallSite: "V.SMART.Shared/Pages/HumanResource_Pages/Payroll_Pages/SalaryDetails.razor:310"),
        };

        /// <summary>Case-sensitive lookup by URL resource segment, or <c>null</c> if unregistered.</summary>
        public static PrintRegistryEntry? Find(string resource)
            => Entries.FirstOrDefault(e => string.Equals(e.Resource, resource, StringComparison.Ordinal));
    }
}
