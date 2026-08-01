using V.SMART.Shared.Data;
using V.SMART.Shared.Data.SalesAndLabour.Leads;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgLeads;
using V.SMART.Shared.Repository.IRepository.ISalesAndLabourRepository.IMfgQuotation;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.SalesAndLabourRepository.MfgLeads
{

    public class LeadsRepository : Repository<Leads>, ILeadsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public LeadsRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public override async Task<Leads> UpdateAsync(Leads obj)
        {
            try
            {
                var tracked = await _db.Leads.FirstOrDefaultAsync(s => s.LeadId == obj.LeadId);

                if (tracked == null)
                    return new Leads();

                var originalValues = _db.Entry(tracked).OriginalValues.Clone();

                // Set updated values to the tracked entity
                _db.Entry(tracked).CurrentValues.SetValues(obj);

                var changes = new StringBuilder();

                foreach (var prop in _db.Entry(tracked).Properties)
                {
                    var oldVal = originalValues[prop.Metadata.Name]?.ToString() ?? "null";
                    var newVal = prop.CurrentValue?.ToString() ?? "null";

                    if (oldVal != newVal)
                    {
                        changes.AppendLine($"{prop.Metadata.Name}: '{oldVal}' → '{newVal}'");
                    }
                }

                if (changes.Length == 0)
                    return tracked; // No changes detected

                tracked.CreatedBy = obj.CreatedBy ?? tracked.CreatedBy;
                tracked.CreatedDate = obj.CreatedDate ?? tracked.CreatedDate;

                tracked.ModifiedBy = obj.ModifiedBy;
                tracked.ModifiedDate = DateTime.Now;

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Leads Master",
                    action: "Leads data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in LeadsRepository.UpdateAsync");
                return new Leads();
            }
        }
    }

}
