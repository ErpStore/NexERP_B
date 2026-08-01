using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace V.SMART.Shared.Data.MigrationData
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {

            #region Local / Testing ConnectionString Local/Cloud

            var connectionString = "Server=DESKTOP-R60MNGC\\SQLEXPRESS;Database=IQSmartDb_2025-26;User Id=sa;Password=aDMIN@123;TrustServerCertificate=True;MultipleActiveResultSets=true";
            //var connectionString = "Server=154.61.76.112,1533;Database=IQSMARTDEMO_DB_2025-26;User Id=bspl;Password=U^b1p7j61;TrustServerCertificate=True;MultipleActiveResultSets=true";
           
            #endregion


            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
