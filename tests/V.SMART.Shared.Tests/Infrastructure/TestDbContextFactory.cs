using Microsoft.EntityFrameworkCore;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Inventory;

namespace V.SMART.Shared.Tests.Infrastructure;

/// <summary>
/// Produces a disposable <see cref="ApplicationDbContext"/> for tests.
///
/// PROVIDER CHOICE - see INV-031 in docs/kb/investigation-registry.md (KB-003).
/// The provider is Microsoft.EntityFrameworkCore.InMemory, NOT Sqlite. Both were
/// executed by task M0-12-01:
///   * InMemory  -> EnsureCreated() succeeds and the HasData seeds are applied.
///   * Sqlite    -> EnsureCreated() throws
///                  Microsoft.Data.Sqlite.SqliteException: SQLite Error 1:
///                  'near "MAX": syntax error'
///                  because nine [Column(TypeName = "nvarchar(max)")] attributes on
///                  entity classes emit a SQL-Server-only column type into the
///                  CREATE TABLE script. SpikeInv031Tests pins that behaviour.
///
/// CONSEQUENCES a caller must know about:
///   * InMemory IS a real EF Core provider, so IQueryable results expose an
///     IAsyncQueryProvider: ToListAsync()/FirstOrDefaultAsync() over
///     IRepository&lt;T&gt;.GetQueryable() work. That is the constraint M0-13 needs.
///   * InMemory does NOT enforce foreign keys. StockAdd's [ForeignKey] attributes
///     on ItemId, StoreId and ScreenCode are therefore not enforced here. Seed the
///     parents anyway when the test reads them back - use <see cref="SeedMasterData"/>.
///   * InMemory does not translate LINQ to SQL, so it cannot catch a
///     "could not be translated" regression. Anything depending on SQL semantics
///     needs a real SQL Server, which no test in this repository has yet.
/// </summary>
public sealed class TestDbContextFactory : IDisposable
{
    private readonly List<ApplicationDbContext> _contexts = new();
    private readonly string _databaseName = Guid.NewGuid().ToString("N");

    /// <summary>
    /// Creates a context over this factory's isolated in-memory database and calls
    /// EnsureCreated(), which applies the model's HasData seed data.
    /// Every call shares the same underlying store, so a second context sees rows
    /// saved through the first.
    /// </summary>
    public ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        _contexts.Add(context);
        return context;
    }

    /// <summary>
    /// Creates the minimum master rows a <see cref="StockAdd"/> row needs.
    ///
    /// StockAdd declares three foreign keys - [ForeignKey(nameof(ItemId))] -> Item,
    /// [ForeignKey(nameof(StoreId))] -> Store and [ForeignKey(nameof(ScreenCode))]
    /// -> Screens (V.SMART/V.SMART.Shared/Data/Inventory(Stock)/StockAdd.cs:22-54).
    /// Store, Screens, UOM and Category are ALREADY present after EnsureCreated()
    /// because the model seeds them with HasData, so only Item has to be created
    /// here. Item itself needs a UOM (Item.MeasureUnit is non-nullable and is an FK
    /// to UOM - Item.cs:50-54) and a Category (Item.cs:41-45); both are taken from
    /// the seeded rows.
    ///
    /// NOTE on Screens: its primary key is Screens.Id, not Screens.ScreenCode
    /// (Screens.cs:8-12). StockAdd.ScreenCode is an FK to Screens.Id. The seed data
    /// happens to set Id == ScreenCode for the low rows, which hides the difference.
    /// Pass a Screens.Id value.
    /// </summary>
    /// <returns>The ItemId of the created item.</returns>
    public static int SeedMasterData(ApplicationDbContext context)
    {
        var uom = context.UOM.OrderBy(u => u.UnitCode).First();
        var category = context.Category.OrderBy(c => c.CategoryCode).First();

        var item = new Item
        {
            ItemCode = "TEST-ITEM-001",
            ItemName = "Test Item",
            BtOtorMfg = "BT",
            CategoryCode = category.CategoryCode,
            MeasureUnit = uom.UnitCode,
            // Item.HSNCode (Item.cs:141-143) and Item.SACCode (:145-147) are
            // [Required] non-nullable strings. The InMemory provider DOES enforce
            // required properties on SaveChanges, even though it does not enforce
            // foreign keys - omitting them throws DbUpdateException.
            HSNCode = "00000000",
            SACCode = "000000",
        };

        context.Item.Add(item);
        context.SaveChanges();
        return item.ItemId;
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }
        _contexts.Clear();
    }
}
