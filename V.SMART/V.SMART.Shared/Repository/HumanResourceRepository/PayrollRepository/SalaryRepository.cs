using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.HumanResource.Salary;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IPayrollRepository;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.Repository.HumanResourceRepository.PayrollRepository
{
    public class SalaryRepository : Repository<Salary>, ISalaryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public SalaryRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }

        public async Task<Salary> GetByIdAsync(int id)
        {
            return await _db.Salary
                .FirstOrDefaultAsync(x => x.RowId == id);
        }

        public async Task UpsertAsync(Salary model)
        {
            try
            {
                // Prevent NULL string DB errors
                model.ESIBankCode ??= "";
                model.ESIBankName ??= "";
                model.ESIPytMode ??= "";
                model.PFBankName ??= "";
                model.PFPytMode ??= "";
                model.PFDepositor ??= "";
                model.Createdby ??= "";

                if (model.RowId == 0)
                {
                    model.Createddate = DateTime.Now;
                    await _db.Salary.AddAsync(model);
                }
                else
                {
                    var existing = await _db.Salary
                        .FirstOrDefaultAsync(x => x.RowId == model.RowId);

                    if (existing == null)
                        throw new Exception("Salary record not found");

                    _db.Entry(existing).CurrentValues.SetValues(model);
                    existing.Modifieddate = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Salary Upsert failed");
                throw;
            }
        }
    }
}
