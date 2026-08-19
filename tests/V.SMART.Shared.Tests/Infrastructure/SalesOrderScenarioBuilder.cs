using AutoMapper;
using Moq;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.SalesService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.OutSourcingRepository.MaterialRequsiationRepo;
using V.SMART.Shared.Repository.PlanningRepository.JobOrderRepo;
using V.SMART.Shared.Repository.PlanningRepository.RouteCardRepo;
using V.SMART.Shared.Repository.ProductionRepository.ProuctionCompRepo;
using V.SMART.Shared.Repository.SalesAndLabourRepository.ExportInvoiceRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.MfgInvoice;
using V.SMART.Shared.Repository.SalesAndLabourRepository.PerformaInvoiceRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesDCRepository;
using V.SMART.Shared.Repository.SalesAndLabourRepository.SalesPoRepository;
using V.SMART.Shared.Services;
using ContractReviewEntity = V.SMART.Shared.Data.SalesAndLabour.ContractReview.ContractReview;
using ContractReviewRepo = V.SMART.Shared.Repository.SalesAndLabourRepository.ContractReviewRepository.ContractReviewRepository;

namespace V.SMART.Shared.Tests.Infrastructure;

/// <summary>
/// Harness for <c>MfgPoService.CanDeleteSalesOrderAsync</c> (task M0-09, risk R-08).
///
/// HARNESS SHAPE - the shape M0-13 established and INV-036 (KB-003) records: a
/// <see cref="Mock{IUnitOfWork}"/> whose touched repository properties are backed by REAL
/// repositories over ONE real EF Core InMemory <see cref="ApplicationDbContext"/>
/// (INV-031, KB-003). CanDeleteSalesOrderAsync calls .Include(), .FirstOrDefaultAsync()
/// and .AnyAsync() on IRepository&lt;T&gt;.GetQueryable() results, so the queryable must
/// come from a genuine EF Core IAsyncQueryProvider - a stub returning list.AsQueryable()
/// would throw.
///
/// The method is READ-ONLY: it never calls IUnitOfWork.SaveAsync. Seeding therefore goes
/// through the context directly and no SaveAsync forwarding is configured.
///
/// The service's other constructor dependencies (MfgPoService.cs:39-45) are inert:
/// CanDeleteSalesOrderAsync touches only _unitOfWork and _logs. CurrentUserService is a
/// concrete class with non-virtual members, so it is constructed - not mocked - over
/// <see cref="TestAuthenticationStateProvider"/>.
///
/// SEEDING REQUIREMENTS discovered while writing these tests (M0-09). The InMemory
/// provider does not enforce foreign keys but DOES reject null [Required] strings on
/// SaveChanges - see TestDbContextFactory:
///   * MfgPo          - PONo, Suffix, PODate required (MfgPo.cs:53-61); CurrId is set to
///                      the HasData-seeded Currency 1 (ApplicationDbContext.cs:1828-1831)
///                      because MfgPo.Currency is a non-nullable navigation (MfgPo.cs:39-41).
///   * MfgPoSub       - PoId, SlNo, ItemId, DueDate, PoDate required (MfgPoSub.cs:21-31, 75-79).
///   * ExpInvSub      - ExpInvId (non-nullable int FK) and ItemId (ExpInvSub.cs:23, 40-41);
///                      the ExpInv header row is NOT seeded and is not needed.
///   * MfgInvSub      - InvId and ItemId (MfgInvSub.cs:24, 40-41); no MfgInv header seeded.
///   * ContractReview - ContractNo, Suffix, RevisedBy, ApprovedBy, ReviewJson,
///                      ContractDate, ContractDateNow, PoId (ContractReview.cs:16-46).
///   * RouteCard      - RCNo, Suffix, CreatedBy, RCDate, RcQty, CompItemId
///                      (RouteCard.cs:20-33, 53-54, 106).
/// Item rows come from TestDbContextFactory.SeedMasterData.
/// </summary>
public sealed class SalesOrderDeleteGuardHarness : IDisposable
{
    private readonly TestDbContextFactory _factory;

    public SalesOrderDeleteGuardHarness()
    {
        _factory = new TestDbContextFactory();
        Context = _factory.CreateContext();

        ItemId = TestDbContextFactory.SeedMasterData(Context);
        CurrId = Context.Currency.OrderBy(c => c.CurrId).First().CurrId;

        Logs = new FakeLoggingService();
        var currentUser = new CurrentUserService(new TestAuthenticationStateProvider());

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.MfgPos).Returns(new MfgPoRepository(Context, Logs, currentUser));
        unitOfWork.Setup(u => u.MfgDcSubs).Returns(new MfgDcSubRepository(Context, Logs, currentUser));
        unitOfWork.Setup(u => u.MfgInvSubs).Returns(new MfgInvSubRepository(Context, Logs, currentUser));
        unitOfWork.Setup(u => u.ExpInvSubs).Returns(new ExpInvSubRepository(Context, Logs, currentUser));
        unitOfWork.Setup(u => u.PerformaInvSubs).Returns(new PerformaInvSubRepository(Context, Logs, currentUser));
        unitOfWork.Setup(u => u.RouteCards).Returns(new RouteCardRepository(Context, Logs));
        unitOfWork.Setup(u => u.ContractReviews).Returns(new ContractReviewRepo(Context, Logs, currentUser));
        unitOfWork.Setup(u => u.MaterialReqSubs).Returns(new MaterialReqSubRepository(Context, Logs));
        unitOfWork.Setup(u => u.JobOrders).Returns(new JobOrderRepository(Context, Logs));
        unitOfWork.Setup(u => u.ProductionIssueCompSubs).Returns(new ProductionIssueCompSubRepository(Context, Logs));

        UnitOfWork = unitOfWork;

        Service = new MfgPoService(
            unitOfWork.Object,
            new Mock<ICommonService>().Object,
            currentUser,
            Logs,
            new Mock<IMapper>().Object,
            new Mock<IExcelTemplateService>().Object);
    }

    public ApplicationDbContext Context { get; }
    public FakeLoggingService Logs { get; }
    public MfgPoService Service { get; }
    public Mock<IUnitOfWork> UnitOfWork { get; }
    public int ItemId { get; }
    public int CurrId { get; }

    /// <summary>
    /// Inserts one Sales Order header with a single line and returns it. The line's
    /// PoSubId is what the downstream-document guards match on through RefPoSubId.
    /// </summary>
    public MfgPo AddSalesOrder(string poNo = "SO-0001")
    {
        var po = new MfgPo
        {
            CustId = 1,
            CurrId = CurrId,
            PONo = poNo,
            Suffix = "A",
            PODate = new DateTime(2026, 1, 1),
            NoOfItems = 1,
            CreatedBy = "Tester",
            CreatedDate = new DateTime(2026, 1, 1),
        };

        po.MfgPoSubs.Add(new MfgPoSub
        {
            SlNo = 1,
            ItemId = ItemId,
            Qty = 10m,
            BalQty = 10m,
            UnitPrice = 100m,
            DueDate = new DateTime(2026, 2, 1),
            PoDate = new DateTime(2026, 1, 1),
        });

        Context.MfgPo.Add(po);
        Context.SaveChanges();
        return po;
    }

    /// <summary>Export invoice line referencing a Sales Order line (guard 3).</summary>
    public ExpInvSub AddExportInvoiceLine(int refPoSubId)
    {
        var row = new ExpInvSub
        {
            ExpInvId = 1,
            SlNo = 1,
            RefPoSubId = refPoSubId,
            ItemId = ItemId,
            Qty = 1m,
            BalQty = 1m,
            UnitPrice = 100m,
        };

        Context.ExpInvSub.Add(row);
        Context.SaveChanges();
        return row;
    }

    /// <summary>Tax invoice line referencing a Sales Order line (guard 2).</summary>
    public MfgInvSub AddTaxInvoiceLine(int refPoSubId)
    {
        var row = new MfgInvSub
        {
            InvId = 1,
            SlNo = 1,
            RefPoSubId = refPoSubId,
            ItemId = ItemId,
            Qty = 1m,
            BalQty = 1m,
            UnitPrice = 100m,
        };

        Context.MfgInvSub.Add(row);
        Context.SaveChanges();
        return row;
    }

    /// <summary>Contract review referencing the Sales Order HEADER id (guard 6).</summary>
    public ContractReviewEntity AddContractReview(int poId)
    {
        var row = new ContractReviewEntity
        {
            ContractNo = "CR-0001",
            Suffix = "A",
            ContractDate = new DateTime(2026, 1, 2),
            ContractDateNow = new DateTime(2026, 1, 2),
            PoId = poId,
            RevisedBy = "Tester",
            ApprovedBy = "Tester",
            ReviewJson = "{}",
        };

        Context.ContractReview.Add(row);
        Context.SaveChanges();
        return row;
    }

    /// <summary>Route card referencing the Sales Order HEADER id (guard 5).</summary>
    public RouteCard AddRouteCard(int refPoId)
    {
        var row = new RouteCard
        {
            RCNo = "RC-0001",
            Suffix = "A",
            RCDate = new DateTime(2026, 1, 3),
            RcQty = 1m,
            RcType = 1,
            RefPoId = refPoId,
            CompItemId = ItemId,
            CreatedBy = "Tester",
            CreatedDate = new DateTime(2026, 1, 3),
        };

        Context.RouteCard.Add(row);
        Context.SaveChanges();
        return row;
    }

    public void Dispose() => _factory.Dispose();
}
