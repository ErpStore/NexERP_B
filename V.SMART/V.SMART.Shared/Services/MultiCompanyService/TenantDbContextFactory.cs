using V.SMART.Shared.Data;
using V.SMART.Shared.Services.MultiCompanyService;
using Microsoft.EntityFrameworkCore;

public class TenantDbContextFactory : ITenantDbContextFactory
{
    private readonly ITenantProvider _tenantProvider;

    public TenantDbContextFactory(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public ApplicationDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseSqlServer(
            _tenantProvider.GetCurrentTenant().ConnectionString,
            sqlOptions =>
            {
                sqlOptions.CommandTimeout(60); // 👈 set timeout to 60 seconds
            });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

