using Microsoft.Data.SqlClient;
using V.SMART.Api.Authorization;
using V.SMART.Api.Reporting;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using Xunit;

namespace V.SMART.Api.Tests.Reporting
{
    /// <summary>
    /// M2-B08 — registry integrity. The authorization gating and OpenAPI-naming correctness of
    /// every action built on these registries is already swept automatically by
    /// <c>PermissionMatrix.EndpointDiscoveryTests</c> and <c>OpenApiConformanceTests</c> — this
    /// file checks the registries themselves, which those two sweeps have no reason to know
    /// about: no duplicate route keys, and every declared <c>ScreenName</c> actually exists in
    /// the compiled catalogue <c>ScreenRightStartupValidator</c> checks controllers against at
    /// startup (a mismatch there would otherwise only surface as the whole host refusing to
    /// start, not as a clear assertion failure naming the offending entry).
    /// </summary>
    public class ReportRegistryTests
    {
        [Fact]
        public void PrintRegistry_has_no_duplicate_resource_keys()
        {
            var resources = PrintRegistry.Entries.Select(e => e.Resource).ToList();
            Assert.Equal(resources.Distinct(StringComparer.Ordinal).Count(), resources.Count);
        }

        [Fact]
        public void ReportRegistry_has_no_duplicate_slugs()
        {
            var slugs = ReportRegistry.Entries.Select(e => e.Slug).ToList();
            Assert.Equal(slugs.Distinct(StringComparer.Ordinal).Count(), slugs.Count);
        }

        [Fact]
        public void Every_PrintRegistry_screen_name_is_in_the_seeded_catalogue()
        {
            foreach (var entry in PrintRegistry.Entries)
            {
                Assert.True(
                    ScreenCatalogue.SeededScreenNames.Contains(entry.ScreenName),
                    $"PrintRegistry entry '{entry.Resource}' declares ScreenName '{entry.ScreenName}', " +
                    "which is not in ScreenCatalogue.SeededScreenNames — the API would refuse to start.");
            }
        }

        [Fact]
        public void Every_ReportRegistry_screen_name_is_in_the_seeded_catalogue()
        {
            foreach (var entry in ReportRegistry.Entries)
            {
                Assert.True(
                    ScreenCatalogue.SeededScreenNames.Contains(entry.ScreenName),
                    $"ReportRegistry entry '{entry.Slug}' declares ScreenName '{entry.ScreenName}', " +
                    "which is not in ScreenCatalogue.SeededScreenNames — the API would refuse to start.");
            }
        }

        [Fact]
        public void PrintRegistry_Find_is_case_sensitive_and_returns_null_for_unknown_resource()
        {
            Assert.Null(PrintRegistry.Find("PURCHASE-POS"));
            Assert.Null(PrintRegistry.Find("Sp_Print_PurchasePo")); // the allow-list probe (M2-B08 Testing §8)
            Assert.NotNull(PrintRegistry.Find("purchase-pos"));
        }

        [Fact]
        public void ReportRegistry_Find_is_case_sensitive_and_returns_null_for_unknown_slug()
        {
            Assert.Null(ReportRegistry.Find("HSN-SUMMARY"));
            Assert.Null(ReportRegistry.Find("Sp_GetHSNSummaryReport")); // the allow-list probe (M2-B08 Testing §8)
            Assert.NotNull(ReportRegistry.Find("hsn-summary"));
        }

        [Fact]
        public async Task Every_ReportRegistry_Execute_delegate_is_wired_to_its_declared_procedure()
        {
            // A fake IReportExecutor that records what procedure name and parameters it was
            // called with, so this test proves the closure captured in each entry's Execute
            // delegate actually calls ExecuteAsync<T> with the entry's own ProcedureName rather
            // than a typo'd or stale one — the one thing that would otherwise only surface as a
            // "Could not find stored procedure" SqlException at request time.
            foreach (var entry in ReportRegistry.Entries)
            {
                var fake = new RecordingReportExecutor();
                await entry.Execute(fake, Array.Empty<SqlParameter>());

                Assert.Equal(entry.ProcedureName, fake.CalledProcedureName);
            }
        }

        private sealed class RecordingReportExecutor : IReportExecutor
        {
            public string? CalledProcedureName { get; private set; }

            public Task<List<T>> ExecuteAsync<T>(string procedureName, params SqlParameter[] parameters) where T : class
            {
                CalledProcedureName = procedureName;
                return Task.FromResult(new List<T>());
            }

            public Task<System.Data.DataTable> ExecuteDataTableAsync(string procedureName, params SqlParameter[] parameters)
                => throw new NotImplementedException("Not exercised by the registry's Execute delegates.");
        }
    }
}
