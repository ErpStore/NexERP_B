using V.SMART.Shared.Data;

namespace V.SMART.Shared.Services.MultiCompanyService
{
    public interface ITenantDbContextFactory
    {
        ApplicationDbContext CreateDbContext();
    }
}
