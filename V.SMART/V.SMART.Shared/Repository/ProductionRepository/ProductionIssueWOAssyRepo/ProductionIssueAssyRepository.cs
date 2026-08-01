using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Repository.IRepository.IPlanningRepository.IJobOrderRepo;
using V.SMART.Shared.Repository.IRepository.IProductionRepository.IProductionIssueWOAssyRepo;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.ProductionRepository.ProductionIssueWOAssyRepo
{
    public class ProductionIssueAssyRepository : Repository<ProductionIssueAssy>, IProductionIssueAssyRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _currentUserService;
        public ProductionIssueAssyRepository(ApplicationDbContext db, ILoggingService logs,CurrentUserService userService) : base(db, logs)
        {
            _db = db;
            _logs = logs;
            _currentUserService = userService;
        }

        public async Task<string> GetLastIssueNoAsync(string suffix)
        {
            var lastNumberStr = await _db.ProductionIssueAssy
                .FromSqlRaw(@"
            SELECT TOP 1 * 
            FROM ProductionIssueAssy WITH (UPDLOCK, ROWLOCK)
            WHERE Suffix = {0}
            ORDER BY TRY_CAST(IssueNo AS INT) DESC", suffix)
                .Select(q => q.IssueNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastNumberStr) &&
                int.TryParse(lastNumberStr, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }
        public async Task<string> GetMonthWiseProductionIssueNumberAsync(string suffix)
        {
            try
            {
                string deptCode = "AD";

                int userId = await _currentUserService.GetUserIdAsync();

                var user = await _db.Users
                    .Include(u => u.Staff)
                    .Where(u => u.UserId == userId)
                    .Select(u => new { u.Staff.DepartmentCode })
                    .FirstOrDefaultAsync();


                if (user?.DepartmentCode != null)
                {
                    deptCode = user.DepartmentCode;
                }

                var today = DateTime.Now;
                string monthCode = today.Month.ToString("D2");

                // Get only last IssueNo using RAW SQL (FAST)
                var lastIssueNo = await _db.Database
                    .SqlQuery<string>($@"
                 SELECT TOP 1 IssueNo AS Value
                 FROM ProductionIssueAssy
                 WHERE DepartmentCode = {deptCode}
                   AND MonthCode = {monthCode}
                   AND Suffix = {suffix}
                 ORDER BY TRY_CAST(IssueNo AS INT) DESC")
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (!string.IsNullOrEmpty(lastIssueNo) &&
                    int.TryParse(lastIssueNo, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }

                return nextNumber.ToString();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error generating Production Issue number for suffix: {suffix}");

                throw new InvalidOperationException("Failed to generate Production Issue number.");
            }
        }

    }
}