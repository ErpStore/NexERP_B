using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Services.MultiCompanyService;
using V.SMART.Shared.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Shared.Tests;

/// <summary>
/// The INV-031 spike, kept as permanent regression tests. Each one pins an
/// empirically observed fact recorded in docs/kb/investigation-registry.md (KB-003).
/// If one of these starts failing, the recorded finding has changed and KB-003
/// must be updated - that is the point of keeping them.
/// </summary>
public class DbFixtureTests
{
    [Fact]
    public void Spike_InMemoryProvider_ModelBuilds()
    {
        // INV-031, Confirmed: contrary to the pre-spike inference, the InMemory
        // provider DOES build this model, despite the 65 relational-only
        // ToView(null) calls in ApplicationDbContext.OnModelCreating.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        using var context = new ApplicationDbContext(options);

        var created = context.Database.EnsureCreated();

        Assert.True(created);
    }

    [Fact]
    public void Spike_SqliteInMemory_FailsOnSqlServerOnlyColumnType()
    {
        // INV-031, Confirmed NEGATIVE result. Sqlite cannot host this model:
        // nine [Column(TypeName = "nvarchar(max)")] attributes emit a
        // SQL-Server-only type into the CREATE TABLE script, which SQLite rejects.
        // Evidence: V.SMART/V.SMART.Shared/Data/HumanResource/Attendance/Attendance.cs:27,30,42,45
        // and five Inspection entities. Do not retry Sqlite without first removing
        // those attributes - which is production code this task may not touch.
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new ApplicationDbContext(options);

        var ex = Assert.Throws<SqliteException>(() => context.Database.EnsureCreated());

        Assert.Contains("near \"MAX\": syntax error", ex.Message);
    }

    [Fact]
    public void Fixture_SeedsAreApplied()
    {
        // INV-031, Confirmed: EnsureCreated() DOES apply the model's HasData seeds
        // under the InMemory provider. Observed counts, 2026-08-19.
        // M0-06 and M0-13 both depend on this answer.
        //
        // AMENDED BY M0-06, 2026-08-19: the two User assertions that stood here
        //     Assert.Equal(1, context.Users.Count());
        //     Assert.Equal("Administrator", context.Users.Single().UserName);
        // were removed because M0-06 removed that seed from the model (risk R-09 -
        // a published default credential in every tenant database). The Users table
        // is now expected to be EMPTY on a freshly created database; that expectation
        // is asserted positively in Data/SeedDataTests.cs. The remaining counts are
        // unchanged and still pin INV-031's finding that HasData seeds are applied.
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        Assert.Equal(152, context.Screens.Count());
        Assert.Empty(context.Users);
        Assert.Equal(9, context.Stores.Count());
        Assert.Equal(49, context.UOM.Count());
        Assert.Equal(16, context.Category.Count());
    }

    [Fact]
    public async Task Fixture_SupportsEfCoreAsyncOperators()
    {
        // The constraint M0-13 depends on: IRepository<T>.GetQueryable() results
        // have EF async operators applied to them (StockManagerService.cs:114-116,
        // :205-207), which require an IAsyncQueryProvider. A plain
        // list.AsQueryable() double throws. This proves the InMemory provider
        // satisfies that requirement.
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var screens = await context.Screens.Where(s => s.Id <= 5).ToListAsync();

        Assert.Equal(5, screens.Count);
    }

    [Fact]
    public void Fixture_SeedMasterData_SupportsStockAddInsert()
    {
        // The foreign-key answer for M0-13. StockAdd has three FKs; Store and
        // Screens arrive with the seeds, so only Item must be created.
        // Note also: the InMemory provider does not enforce FK constraints, so the
        // parents are seeded for readability and for parity with SQL Server, not
        // because InMemory would reject the insert.
        using var factory = new TestDbContextFactory();
        using var context = factory.CreateContext();

        var itemId = TestDbContextFactory.SeedMasterData(context);
        var storeId = context.Stores.OrderBy(s => s.StoreId).First().StoreId;
        var screenId = context.Screens.OrderBy(s => s.Id).First().Id;

        context.StockAdd.Add(new StockAdd
        {
            ItemId = itemId,
            StoreId = storeId,
            ScreenCode = screenId,
            AddQty = 10m,
            BalQty = 10m,
            UnitPrice = 100m,
            SubItemRefID = 1,
            RefNo = "TEST-REF-1",
            RefDate = new DateTime(2026, 1, 1),
        });
        context.SaveChanges();

        Assert.Equal(1, context.StockAdd.Count());
    }

    [Fact]
    public void Fixture_FakeTenantDbContextFactory_ReturnsAUsableContext()
    {
        using var factory = new TestDbContextFactory();
        ITenantDbContextFactory tenantFactory = new FakeTenantDbContextFactory(factory);

        using var context = tenantFactory.CreateDbContext();

        Assert.NotNull(context);
        Assert.Equal(152, context.Screens.Count());
    }
}
