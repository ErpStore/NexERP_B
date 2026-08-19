using Microsoft.EntityFrameworkCore;
using Moq;
using V.SMART.Shared.BusinessLayer.BusinessService.InventoryService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Repository.InventoryStockRepository;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Tests.Infrastructure;

/// <summary>
/// Harness for characterisation tests over
/// V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/InventoryService/StockManagerService.cs.
///
/// HARNESS SHAPE - option A of task M0-13, chosen over option B. Rationale:
///   * A <see cref="Mock{IUnitOfWork}"/> configures only the four members
///     StockManagerService actually uses - StockAdds / StockIssues /
///     StockIssueTracks (IUnitOfWork.cs:270-272) and SaveAsync (IUnitOfWork.cs:84).
///     The remaining members of the 194-member interface cost nothing.
///   * The three repositories are REAL (StockAddRepository.cs:20 etc.), constructed
///     over a real <see cref="ApplicationDbContext"/>, so
///     IRepository&lt;T&gt;.GetQueryable() (Repository.cs:323-326) yields a genuine EF
///     Core IAsyncQueryProvider. The service's .ToListAsync() /
///     .FirstOrDefaultAsync() calls therefore execute rather than throw. A
///     hand-written stub returning list.AsQueryable() would throw at runtime.
///   * Option B - the real UnitOfWork over a fake ITenantDbContextFactory - would
///     instantiate roughly 190 repositories per test (UnitOfWork.cs:485 onward,
///     StockAdds at :672) and additionally needs IPasswordHasher&lt;User&gt;. It couples
///     this suite to unrelated repositories for no added coverage of the FIFO path.
///
/// CRITICAL: <see cref="Repository{T}"/> never saves - CreateAsync only calls
/// _dbSet.AddAsync (Repository.cs:70-83), UpdateAsync only _dbSet.Update (:103-115),
/// DeleteAsync only _dbSet.Remove (:137-149). Persistence happens solely through
/// IUnitOfWork.SaveAsync, so the mock's SaveAsync MUST forward to
/// SaveChangesAsync(). All three repositories and every assertion query share the
/// SAME context instance, or change tracking and the service's in-flight entity
/// state diverge.
///
/// Provider is EF Core InMemory - see INV-031 (KB-003) and TestDbContextFactory.
/// </summary>
public sealed class StockTestHarness : IDisposable
{
    private readonly TestDbContextFactory _factory;
    private int _saveCount;
    private int _refCounter;

    public StockTestHarness()
    {
        _factory = new TestDbContextFactory();
        Context = _factory.CreateContext();

        ItemId = TestDbContextFactory.SeedMasterData(Context);

        // Store and Screens rows arrive with the model's HasData seeds
        // (INV-031: Stores 9 rows, Screens 152 rows). Read them rather than
        // hardcoding literals - R-10 (KB-060) is about magic ScreenCode numbers.
        var stores = Context.Stores.OrderBy(s => s.StoreId).Take(2).ToList();
        StoreId = stores[0].StoreId;
        OtherStoreId = stores[1].StoreId;

        var screens = Context.Screens.OrderBy(s => s.Id).Take(2).ToList();
        ScreenCode = screens[0].Id;
        OtherScreenCode = screens[1].Id;

        Logs = new FakeLoggingService();

        var stockAdds = new StockAddRepository(Context, Logs);
        var stockIssues = new StockIssueRepository(Context, Logs);
        var stockIssueTracks = new StockIssueTrackRepository(Context, Logs);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.StockAdds).Returns(stockAdds);
        unitOfWork.Setup(u => u.StockIssues).Returns(stockIssues);
        unitOfWork.Setup(u => u.StockIssueTracks).Returns(stockIssueTracks);
        unitOfWork.Setup(u => u.SaveAsync()).Returns(SaveAsyncImpl);

        UnitOfWork = unitOfWork;

        // CurrentUserService is a concrete class with non-virtual members whose only
        // constructor parameter is AuthenticationStateProvider, so it is constructed,
        // not mocked (CurrentUserService.cs:6-16).
        Service = new StockManagerService(
            unitOfWork.Object,
            new CurrentUserService(new TestAuthenticationStateProvider(UserName)),
            Logs);
    }

    public const string UserName = "CharacterisationUser";

    public ApplicationDbContext Context { get; }
    public FakeLoggingService Logs { get; }
    public StockManagerService Service { get; }
    public Mock<IUnitOfWork> UnitOfWork { get; }

    public int ItemId { get; }
    public int StoreId { get; }
    public int OtherStoreId { get; }
    public int ScreenCode { get; }
    public int OtherScreenCode { get; }

    /// <summary>Number of IUnitOfWork.SaveAsync calls the service has made.</summary>
    public int SaveCount => _saveCount;

    /// <summary>
    /// Optional hook: given the 1-based SaveAsync call number, return an exception to
    /// throw instead of persisting. Used only by the exception-translation tests
    /// (statement 12), which need a NON-InvalidOperationException to escape from a
    /// specific point in the call sequence.
    /// </summary>
    public Func<int, Exception?>? SaveFailure { get; set; }

    private async Task<int> SaveAsyncImpl()
    {
        var call = Interlocked.Increment(ref _saveCount);
        var failure = SaveFailure?.Invoke(call);
        if (failure is not null)
        {
            throw failure;
        }

        return await Context.SaveChangesAsync();
    }

    /// <summary>
    /// Inserts a StockAdd batch directly (bypassing the service) so a scenario's
    /// starting ledger is exact.
    ///
    /// AddDate is REQUIRED here and never defaulted: StockAdd.AddDate defaults to
    /// DateTime.Now (StockAdd.cs:20) and TrackStockUsageAsync orders by it with no
    /// secondary key (StockManagerService.cs:206). Identical AddDate values would
    /// make FIFO order undefined - see the "tie-breaking" note in
    /// StockManagerServiceCharacterisationTests.
    ///
    /// SubItemRefID, RefNo and RefDate are [Required] on StockAdd (:56-64) and the
    /// InMemory provider enforces required properties, so all three are always set.
    /// </summary>
    public StockAdd AddBatch(
        DateTime addDate,
        decimal addQty,
        decimal? balQty = null,
        int? storeId = null,
        int? rcSubId = null,
        int? itemId = null,
        decimal unitPrice = 10m,
        string? refNo = null,
        int subItemRefId = 1,
        int? screenCode = null)
    {
        var batch = new StockAdd
        {
            AddDate = addDate,
            ItemId = itemId ?? ItemId,
            StoreId = storeId ?? StoreId,
            AddQty = addQty,
            BalQty = balQty ?? addQty,
            UnitPrice = unitPrice,
            ScreenCode = screenCode ?? ScreenCode,
            SubItemRefID = subItemRefId,
            RefNo = refNo ?? $"ADD-{++_refCounter}",
            RefDate = addDate,
            RcSubID = rcSubId,
        };

        Context.StockAdd.Add(batch);
        Context.SaveChanges();
        return batch;
    }

    // ---- read-back helpers; all query the same tracked context ----

    public StockAdd Batch(int addId) => Context.StockAdd.Single(a => a.AddId == addId);

    public decimal BalanceOf(int addId) => Batch(addId).BalQty;

    public List<StockAdd> AllBatches() => Context.StockAdd.OrderBy(a => a.AddId).ToList();

    public List<StockIssue> AllIssues() => Context.StockIssue.OrderBy(i => i.IssueId).ToList();

    public StockIssue SingleIssue() => Context.StockIssue.Single();

    /// <summary>
    /// Tracks for an issue in ALLOCATION order. StockIssueTrack rows are created one
    /// per allocated batch inside the FIFO loop (StockManagerService.cs:223-230), so
    /// ascending IssueTrackId is the order the loop visited the batches in.
    /// </summary>
    public List<StockIssueTrack> TracksFor(int issueId) =>
        Context.StockIssueTrack.Where(t => t.IssueId == issueId).OrderBy(t => t.IssueTrackId).ToList();

    public List<StockIssueTrack> AllTracks() =>
        Context.StockIssueTrack.OrderBy(t => t.IssueTrackId).ToList();

    public decimal TrackedQtyFor(int issueId) =>
        Context.StockIssueTrack.Where(t => t.IssueId == issueId).Sum(t => t.UsedQty);

    public void Dispose()
    {
        _factory.Dispose();
    }
}
