using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.MasterService.GeneralService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Mappings;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.MasterRepository.GeneralRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Tests.Infrastructure;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Xunit;

namespace V.SMART.Shared.Tests.Services;

/// <summary>
/// The *Testing Requirements* integration row of task M2-D02-01: a round trip
/// <c>GetCustomerByIdAsync</c> -> <c>UpsertCustomerAsync</c> must preserve the consignees
/// (BR-CUST-013) and the contact persons, which is what the BR-CUST-017 mapping fix bought.
///
/// Why this is not the same assertion as CustomerMappingChildCollectionTests: that one proves
/// AutoMapper carries the child collections across the VM boundary. This one drives the real
/// <see cref="CustomerService"/> over a real <see cref="ApplicationDbContext"/> and the real
/// repository set, so it also exercises the update path's EF change tracking - the sequence at
/// CustomerService.cs:707-736 where <c>_mapper.Map(vm, existing)</c> transiently replaces the
/// tracked navigation collections and the service puts the tracked instances back. That
/// sequence cannot be reached from the static helpers, and <c>UpsertCustomerAsync</c> catches
/// every exception and returns (false, "An error occurred while saving the customer."), so a
/// tracking failure surfaces here as a failed Success assertion, not as a thrown exception.
///
/// PROVIDER: Microsoft.EntityFrameworkCore.InMemory, per INV-031 (KB-003) - Sqlite cannot host
/// this model. InMemory has no transactions, so <c>IUnitOfWork.BeginTransactionAsync</c> is the
/// one member faked here; everything else is the production type.
///
/// This is NOT a substitute for the manual Blazor scenarios in KB-031 section 9, which still
/// need a tenant database: InMemory does not enforce foreign keys and does not translate LINQ
/// to SQL, so it cannot prove the SQL-Server-side behaviour.
/// </summary>
public class CustomerServiceRoundTripTests
{
    private sealed class Harness : IDisposable
    {
        private readonly TestDbContextFactory _factory = new();

        public Harness()
        {
            Context = _factory.CreateContext();

            var logs = new FakeLoggingService();
            var currentUser = new CurrentUserService(new TestAuthenticationStateProvider());

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfileMarker).Assembly));
            var mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();

            var transaction = new Mock<IDbContextTransaction>();
            transaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            transaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.Setup(u => u.Customers).Returns(new CustomerRepository(Context, currentUser, logs));
            unitOfWork.Setup(u => u.CustomerIndirects).Returns(new CustomerIndirectRepository(Context, logs, currentUser));
            unitOfWork.Setup(u => u.ContactPersons).Returns(new ContactPersonRepository(Context, logs, currentUser));
            unitOfWork.Setup(u => u.States).Returns(new StateRepository(Context, logs));
            unitOfWork.Setup(u => u.SaveAsync()).Returns(() => Context.SaveChangesAsync());
            unitOfWork.Setup(u => u.BeginTransactionAsync()).ReturnsAsync(transaction.Object);

            Service = new CustomerService(
                unitOfWork.Object,
                mapper,
                logs,
                currentUser,
                Mock.Of<ICommonService>(),
                new ForeignKeyUsageChecker(Context));
        }

        public ApplicationDbContext Context { get; }

        public CustomerService Service { get; }

        public void Dispose() => _factory.Dispose();
    }

    /// <summary>StateCode 33 is seeded by the model as "TAMIL NADU" (ApplicationDbContext HasData).</summary>
    private const int SeededStateCode = 33;

    private static CustomerVM NewCustomerVM() => new CustomerVM
    {
        CustName = "Acme Engineering",
        CustAddr = "1 Industrial Estate",
        GSTNo = "33AAACA1111A1Z5",
        PANNo = "AAACA1111A",
        StateId = SeededStateCode,
        BusiType = "Local",
        SupTyp = "B2B",
        PinCode = "600001",
        Location = "Chennai",
        Distance = 12,
        CurrId = 1,
        CustomerIndirectVMs = new List<CustomerIndirectVM>
        {
            new CustomerIndirectVM
            {
                AltCustName = "Beta Works",
                AltCustAddr1 = "2 Works Road",
                GSTNo = "33AAACA2222A1Z5",
                PANNo = "AAACA2222A",
            },
            new CustomerIndirectVM
            {
                AltCustName = "Gamma Tools",
                AltCustAddr1 = "3 Tools Lane",
                GSTNo = "33AAACA3333A1Z5",
                PANNo = "AAACA3333A",
            },
        },
        ContactPersonVMs = new List<ContactPersonVM>
        {
            new ContactPersonVM { ContactPersonName = "Ravi", PhoneNo = "9000000000" },
            new ContactPersonVM { ContactPersonName = "Meena", PhoneNo = "9000000001" },
        },
    };

    [Fact]
    public async Task Round_trip_through_the_service_preserves_consignees_and_contact_persons()
    {
        using var harness = new Harness();

        var created = await harness.Service.UpsertCustomerAsync(NewCustomerVM());
        Assert.True(created.Success, string.Join(" | ", created.Errors.Append(created.Message)));
        Assert.Equal("Customer Created Successfully", created.Message);

        var custId = created.Customer!.CustId;
        Assert.NotEqual(0, custId);

        // Read back exactly as the page does when it edits an existing customer.
        var loaded = await harness.Service.GetCustomerByIdAsync(custId);
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.CustomerIndirectVMs.Count);
        Assert.Equal(2, loaded.ContactPersonVMs.Count);

        var indirectIds = loaded.CustomerIndirectVMs.Select(i => i.AltCustId).OrderBy(i => i).ToArray();
        var contactIds = loaded.ContactPersonVMs.Select(c => c.Id).OrderBy(i => i).ToArray();
        Assert.DoesNotContain(0, indirectIds);
        Assert.DoesNotContain(0, contactIds);

        // Edit a parent field only; the children go back unchanged. This is the sequence that
        // could trip an EF tracking failure - see the class comment.
        loaded.CustAddr = "1 Industrial Estate, Unit B";

        var updated = await harness.Service.UpsertCustomerAsync(loaded);
        Assert.True(updated.Success, string.Join(" | ", updated.Errors.Append(updated.Message)));
        Assert.Equal("Customer Updated Successfully", updated.Message);

        var reloaded = await harness.Service.GetCustomerByIdAsync(custId);
        Assert.NotNull(reloaded);
        Assert.Equal("1 Industrial Estate, Unit B", reloaded!.CustAddr);

        // Same rows, same ids - nothing was deleted and re-inserted.
        Assert.Equal(indirectIds, reloaded.CustomerIndirectVMs.Select(i => i.AltCustId).OrderBy(i => i).ToArray());
        Assert.Equal(contactIds, reloaded.ContactPersonVMs.Select(c => c.Id).OrderBy(i => i).ToArray());

        Assert.Equal(
            new[] { "Beta Works", "Gamma Tools" },
            reloaded.CustomerIndirectVMs.Select(i => i.AltCustName).OrderBy(n => n).ToArray());
        Assert.Equal(
            new[] { "Meena", "Ravi" },
            reloaded.ContactPersonVMs.Select(c => c.ContactPersonName).OrderBy(n => n).ToArray());

        // The persisted rows, not only the mapped VM.
        Assert.Equal(2, await harness.Context.CustomerIndirect.CountAsync(i => i.CustId == custId));
        Assert.Equal(2, await harness.Context.ContactPerson.CountAsync(c => c.CustId == custId));
    }

    [Fact]
    public async Task Round_trip_applies_the_child_id_set_difference_on_update()
    {
        using var harness = new Harness();

        var created = await harness.Service.UpsertCustomerAsync(NewCustomerVM());
        Assert.True(created.Success, string.Join(" | ", created.Errors.Append(created.Message)));

        var custId = created.Customer!.CustId;
        var loaded = (await harness.Service.GetCustomerByIdAsync(custId))!;

        // BR-CUST-013: keep and edit one consignee, drop one, add a third.
        var kept = loaded.CustomerIndirectVMs.Single(i => i.AltCustName == "Beta Works");
        kept.City = "Coimbatore";
        loaded.CustomerIndirectVMs = new List<CustomerIndirectVM>
        {
            kept,
            new CustomerIndirectVM
            {
                AltCustName = "Delta Castings",
                GSTNo = "33AAACA4444A1Z5",
                PANNo = "AAACA4444A",
            },
        };

        // Drop one contact person.
        loaded.ContactPersonVMs = loaded.ContactPersonVMs
            .Where(c => c.ContactPersonName == "Ravi")
            .ToList();

        var updated = await harness.Service.UpsertCustomerAsync(loaded);
        Assert.True(updated.Success, string.Join(" | ", updated.Errors.Append(updated.Message)));

        var reloaded = (await harness.Service.GetCustomerByIdAsync(custId))!;

        Assert.Equal(
            new[] { "Beta Works", "Delta Castings" },
            reloaded.CustomerIndirectVMs.Select(i => i.AltCustName).OrderBy(n => n).ToArray());
        Assert.Equal("Coimbatore", reloaded.CustomerIndirectVMs.Single(i => i.AltCustName == "Beta Works").City);
        Assert.Equal(kept.AltCustId, reloaded.CustomerIndirectVMs.Single(i => i.AltCustName == "Beta Works").AltCustId);

        Assert.Equal(new[] { "Ravi" }, reloaded.ContactPersonVMs.Select(c => c.ContactPersonName).ToArray());

        Assert.Equal(2, await harness.Context.CustomerIndirect.CountAsync(i => i.CustId == custId));
        Assert.Equal(1, await harness.Context.ContactPerson.CountAsync(c => c.CustId == custId));
    }
}
