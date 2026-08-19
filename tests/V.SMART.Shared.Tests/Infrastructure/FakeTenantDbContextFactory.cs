using V.SMART.Shared.Data;
using V.SMART.Shared.Services.MultiCompanyService;

namespace V.SMART.Shared.Tests.Infrastructure;

/// <summary>
/// Test double for <see cref="ITenantDbContextFactory"/>, whose single member is
/// <c>ApplicationDbContext CreateDbContext()</c>
/// (V.SMART/V.SMART.Shared/Services/MultiCompanyService/ITenantDbContextFactory.cs:5-8).
///
/// The production implementation, TenantDbContextFactory, hardcodes
/// <c>UseSqlServer(tenant.ConnectionString)</c> and is therefore unusable in a
/// test process. It is NOT modified; this fake replaces it.
/// </summary>
public sealed class FakeTenantDbContextFactory : ITenantDbContextFactory
{
    private readonly Func<ApplicationDbContext> _factory;

    public FakeTenantDbContextFactory(TestDbContextFactory factory)
        : this(factory.CreateContext) { }

    public FakeTenantDbContextFactory(Func<ApplicationDbContext> factory)
    {
        _factory = factory;
    }

    public ApplicationDbContext CreateDbContext() => _factory();
}
