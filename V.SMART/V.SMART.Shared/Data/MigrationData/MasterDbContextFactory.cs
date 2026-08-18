using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace V.SMART.Shared.Data.MigrationData
{
    /// <summary>
    /// Design-time factory for <see cref="MasterDbContext"/>. Used only by the
    /// <c>dotnet ef</c> tooling; never used by an application host.
    /// </summary>
    public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
    {
        // Same key the runtime hosts use (M0-03-01, docs/CONFIGURATION.md): there is only
        // one master database, at design time as at run time.
        private const string ConnectionStringKey = "ConnectionStrings:MasterDb";

        public MasterDbContext CreateDbContext(string[] args)
        {
            var connectionString = DesignTimeConnectionString.Resolve(
                ConnectionStringKey,
                "the master database 'dotnet ef' should target for MasterDbContext");

            var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new MasterDbContext(optionsBuilder.Options);
        }
    }
}
