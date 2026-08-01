using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.HumanResource.StaffLoan;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IEmployeeRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.HumanResourceRepository.EmployeeLoanRepository
{
    public class StaffLoanRepository : Repository<StaffLoan>, IStaffLoanRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public StaffLoanRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<string> GetLastLoanNoAsync()
        {
            var lastLoanNo = await _db.StaffLoan
                .OrderByDescending(x => x.LoanId)   // safest ordering
                .Select(x => x.LoanNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastLoanNo) &&
                int.TryParse(lastLoanNo, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }

            return nextNumber.ToString();
        }
    }
}
