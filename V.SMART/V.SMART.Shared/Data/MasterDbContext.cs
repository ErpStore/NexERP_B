using Microsoft.EntityFrameworkCore;

namespace V.SMART.Shared.Data
{
    public class MasterDbContext : DbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options) { }
        public DbSet<TenantInfo> Tenants { get; set; }
    }

}
