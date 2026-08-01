using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace V.SMART.Shared.Data.MigrationData
{
    public class MasterDbContextFactory : IDesignTimeDbContextFactory<MasterDbContext>
    {
        public MasterDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MasterDbContext>();
            //optionsBuilder.UseSqlServer("Server=154.61.76.112,1533;Database=IQSmartDb_Master;User Id=bspl;Password=U^b1p7j61;TrustServerCertificate=True;MultipleActiveResultSets=true;");
            optionsBuilder.UseSqlServer("Server=DESKTOP-R60MNGC\\SQLEXPRESS;Database=IQSmartDb_Master;User Id=sa;Password=aDMIN@123;TrustServerCertificate=True;MultipleActiveResultSets=true;");

            return new MasterDbContext(optionsBuilder.Options);
        }
    }
}
