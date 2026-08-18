using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace V.SMART.Shared.Data.MigrationData
{
    /// <summary>
    /// Design-time factory for the per-tenant <see cref="ApplicationDbContext"/>. Used only
    /// by the <c>dotnet ef</c> tooling; the runtime path is
    /// <c>Services/MultiCompanyService/TenantDbContextFactory</c>, which builds the same
    /// context from <c>TenantInfo.ConnectionString</c> and is unaffected by this file.
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        // A design-time run needs *some* tenant database. That is not the master database,
        // so it gets its own key rather than overloading ConnectionStrings:MasterDb.
        private const string ConnectionStringKey = "ConnectionStrings:DesignTimeTenantDb";

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = DesignTimeConnectionString.Resolve(
                ConnectionStringKey,
                "any reachable tenant database 'dotnet ef' should target for ApplicationDbContext");

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
