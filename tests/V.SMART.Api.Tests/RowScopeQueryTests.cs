using Microsoft.EntityFrameworkCore;
using V.SMART.Api.Authorization;
using V.SMART.Api.Contracts;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.SalesAndLabour.Leads;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A08 — the leak tests. These are the ones that prove a scoped caller cannot reach an
    /// out-of-scope row, and they are written against a real EF Core query pipeline
    /// (<c>ApplicationDbContext</c> on the InMemory provider), not against a list.
    ///
    /// <para><b>What they cannot prove.</b> The InMemory provider does not translate LINQ to SQL
    /// (INV-031, KB-003), so "the predicate is part of the query and no out-of-scope row comes back"
    /// is proven here; "it renders as <c>WHERE StateId IN (...)</c>" is not. That needs a SQL Server
    /// and is recorded as such rather than claimed.</para>
    /// </summary>
    public class RowScopeQueryTests : IDisposable
    {
        private const int StateA = 11;
        private const int StateB = 22;
        private const int StateC = 33;

        private readonly ApplicationDbContext _db;

        public RowScopeQueryTests()
        {
            _db = new ApplicationDbContext(
                new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                    .Options);

            _db.Database.EnsureCreated();

            for (var i = 1; i <= 12; i++)
            {
                _db.Leads.Add(new Leads
                {
                    LeadId = i,
                    LeadName = $"Lead {i:D2}",
                    StateId = i % 3 == 0 ? StateC : (i % 3 == 1 ? StateA : StateB)
                });
            }

            _db.SaveChanges();
        }

        public void Dispose() => _db.Dispose();

        // ---- test 1: the leak, closed ------------------------------------------------------

        [Fact]
        public async Task A_caller_scoped_to_one_state_never_sees_another_states_rows()
        {
            var scoped = await _db.Leads.ApplyRowScope(RowScope.ForStateCodes(StateA)).ToListAsync();

            Assert.NotEmpty(scoped);
            Assert.All(scoped, lead => Assert.Equal(StateA, lead.StateId));
            Assert.Equal(4, scoped.Count);
        }

        [Fact]
        public async Task No_page_sort_or_filter_combination_reaches_an_out_of_scope_row()
        {
            // Test 1/2 combined over the shapes M2-B02's paging contract can produce. Scope is
            // applied FIRST and everything else composes on top of it, which is the property that
            // makes "across every page, filter and sort combination" true by construction rather
            // than by enumeration.
            var scoped = _db.Leads.ApplyRowScope(RowScope.ForStateCodes(StateA, StateB));

            foreach (var pageSize in new[] { 1, 3, PagedQuery.DefaultPageSize, PagedQuery.MaxPageSize })
            {
                for (var page = 1; page <= 3; page++)
                {
                    var ascending = await scoped
                        .OrderBy(l => l.LeadId)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    var descending = await scoped
                        .OrderByDescending(l => l.LeadName)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();

                    Assert.All(ascending, l => Assert.NotEqual(StateC, l.StateId));
                    Assert.All(descending, l => Assert.NotEqual(StateC, l.StateId));
                }
            }
        }

        [Fact]
        public async Task A_hostile_filter_cannot_widen_the_scope()
        {
            // A caller-supplied predicate composes with AND, never with OR: once the scope is in
            // the expression tree there is no query parameter that can remove it. Asking explicitly
            // for the state the caller is not scoped to returns nothing, not an error and not the row.
            var scoped = _db.Leads.ApplyRowScope(RowScope.ForStateCodes(StateA));

            var hostile = await scoped
                .Where(l => l.StateId == StateC || l.LeadId > 0)
                .ToListAsync();

            Assert.All(hostile, l => Assert.Equal(StateA, l.StateId));
            Assert.Empty(await scoped.Where(l => l.StateId == StateC).ToListAsync());
        }

        // ---- test 3: fail-closed, preserved ------------------------------------------------

        [Fact]
        public async Task An_empty_scope_yields_zero_rows_and_no_error()
        {
            // LeadService.cs:136-144 — a blank StateCodesCsv produces an empty token list and
            // Contains() is false for every lead. KB-108 decision P2: preserved, deliberately.
            // Zero rows, NOT everything, and NOT an exception (which a controller would turn into
            // a 500 rather than the 200-with-an-empty-page the contract requires).
            var scoped = await _db.Leads.ApplyRowScope(RowScope.Empty).ToListAsync();

            Assert.Empty(scoped);
        }

        [Fact]
        public async Task An_empty_scope_also_makes_the_paged_total_zero()
        {
            // KB-108 decision P7. LeadsList.razor:396-401 computes TotalStaffCount and totalPage
            // from the UNSCOPED GetAllLeadsAsync() for every user, so a scoped user's page count
            // today reflects leads they cannot see. The API counts within scope instead — a
            // deliberate, recorded behaviour change, and this test is what pins it.
            var scoped = _db.Leads.ApplyRowScope(RowScope.Empty);

            var result = new PagedResult<Leads>(
                await scoped.Take(20).ToListAsync(),
                await scoped.CountAsync(),
                1,
                20);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task A_scoped_paged_total_counts_only_rows_in_scope()
        {
            var scoped = _db.Leads.ApplyRowScope(RowScope.ForStateCodes(StateA));

            Assert.Equal(4, await scoped.CountAsync());
            Assert.Equal(12, await _db.Leads.CountAsync());
        }

        // ---- the ported carve-out ----------------------------------------------------------

        [Fact]
        public async Task An_unrestricted_scope_returns_every_row()
        {
            // LeadsList.razor:470-484 — user 1 calls the unscoped GetAllLeadsAsync() today. The
            // carve-out is ported so that account does not lose data on the day the API ships.
            var all = await _db.Leads.ApplyRowScope(RowScope.Unrestricted).ToListAsync();

            Assert.Equal(12, all.Count);
        }

        // ---- the mechanism's own guarantees -------------------------------------------------

        [Fact]
        public void An_unregistered_entity_type_throws_rather_than_returning_everything()
        {
            // Fail loud, never fail open. Currency is not row-scoped, so asking for its scope is a
            // programming error; silently returning the unfiltered query is how a scoped entity
            // quietly becomes unscoped.
            var error = Assert.Throws<InvalidOperationException>(
                () => _db.Currency.ApplyRowScope(RowScope.ForStateCodes(StateA)));

            Assert.Contains(nameof(Currency), error.Message, StringComparison.Ordinal);
            Assert.Contains("ScopedEntityCatalogue", error.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void An_unregistered_entity_type_throws_even_for_the_unrestricted_caller()
        {
            // Resolved before the unrestricted short-circuit on purpose: otherwise the bug would
            // hide from user 1 and surface in production for everyone else.
            Assert.Throws<InvalidOperationException>(
                () => _db.Currency.ApplyRowScope(RowScope.Unrestricted));
        }

        [Fact]
        public void Leads_is_the_only_scoped_entity_and_that_is_a_finding()
        {
            // INV-028's central negative result, pinned as a test: StateCodesCsv scopes nothing but
            // Leads anywhere in V.SMART/. If this starts failing, someone has begun filtering rows a
            // live ERP shows today (KB-108 decision P1) and the decision must be recorded first.
            Assert.Equal(new[] { typeof(Leads) }, ScopedEntityCatalogue.ScopedEntityTypes);
        }
    }
}
