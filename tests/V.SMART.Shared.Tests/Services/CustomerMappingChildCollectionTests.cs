using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Mappings;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Xunit;

namespace V.SMART.Shared.Tests.Services;

/// <summary>
/// BR-CUST-017 — before M2-D02-01, <c>CustomerVM.CustomerIndirectVMs</c> and
/// <c>CustomerVM.ContactPersonVMs</c> were declared but mapped in neither direction, so a
/// customer's consignees and contact persons could not cross the VM boundary at all. That is the
/// one behaviour *addition* the task authorises, and these tests are what prove it.
///
/// The mapper is built the same way the application builds it — by scanning the assembly that
/// holds <see cref="MappingProfileMarker"/> — so the test exercises the real profile set,
/// including the internal <c>CustomerMapping</c>.
/// </summary>
public class CustomerMappingChildCollectionTests
{
    private static IMapper BuildMapper()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfileMarker).Assembly));

        return services.BuildServiceProvider().GetRequiredService<IMapper>();
    }

    private static Customer CustomerWithChildren() => new Customer
    {
        CustId = 7,
        CustName = "Acme Engineering",
        CustAddr = "1 Industrial Estate",
        GSTNo = "33AAACA1111A1Z5",
        PANNo = "AAACA1111A",
        VendorCode = "V-0007",
        StateId = 33,
        StateName = "Tamil Nadu",
        BusiType = "Local",
        SupTyp = "B2B",
        PinCode = "600001",
        Location = "Chennai",
        CustomerIndirects = new List<CustomerIndirect>
        {
            new CustomerIndirect { AltCustId = 11, CustId = 7, AltCustName = "Beta Works", GSTNo = "33AAACA2222A1Z5" },
            new CustomerIndirect { AltCustId = 12, CustId = 7, AltCustName = "Gamma Tools" }
        },
        ContactPersons = new List<ContactPerson>
        {
            new ContactPerson { Id = 21, CustId = 7, ContactPersonName = "Ravi", PhoneNo = "9000000000" }
        }
    };

    [Fact]
    public void Entity_to_vm_carries_the_consignees_and_the_contact_persons()
    {
        var vm = BuildMapper().Map<CustomerVM>(CustomerWithChildren());

        Assert.Equal(2, vm.CustomerIndirectVMs.Count);
        Assert.Equal(new[] { 11, 12 }, vm.CustomerIndirectVMs.Select(i => i.AltCustId).ToArray());
        Assert.Equal("Beta Works", vm.CustomerIndirectVMs[0].AltCustName);
        Assert.Equal("33AAACA2222A1Z5", vm.CustomerIndirectVMs[0].GSTNo);

        Assert.Single(vm.ContactPersonVMs);
        Assert.Equal(21, vm.ContactPersonVMs[0].Id);
        Assert.Equal("Ravi", vm.ContactPersonVMs[0].ContactPersonName);
    }

    [Fact]
    public void Vm_to_entity_carries_them_back()
    {
        var mapper = BuildMapper();

        var vm = mapper.Map<CustomerVM>(CustomerWithChildren());
        var entity = mapper.Map<Customer>(vm);

        Assert.Equal(2, entity.CustomerIndirects.Count);
        Assert.Equal(new[] { 11, 12 }, entity.CustomerIndirects.Select(i => i.AltCustId).ToArray());
        Assert.Single(entity.ContactPersons);
        Assert.Equal("Ravi", entity.ContactPersons.First().ContactPersonName);
    }

    [Fact]
    public void The_round_trip_preserves_vendor_code()
    {
        // CustomerUpsert.razor binds Customer.VendorCode; M2-D02-01 added it to CustomerVM so the
        // save path, which now runs through the VM, does not silently drop it on create.
        var mapper = BuildMapper();

        var vm = mapper.Map<CustomerVM>(CustomerWithChildren());
        var entity = mapper.Map<Customer>(vm);

        Assert.Equal("V-0007", vm.VendorCode);
        Assert.Equal("V-0007", entity.VendorCode);
    }
}
