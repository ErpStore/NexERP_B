using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.GeneralRepository
{
    public class ContactPersonRepository : Repository<ContactPerson>, IContactPersonRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public ContactPersonRepository(ApplicationDbContext db, ILoggingService loggingService, CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task DeleteByCustIdAsync(int custId)
        {
            var contactPersons = await _db.ContactPerson
                .Where(e => e.CustId == custId)
                .ToListAsync();

            if (contactPersons.Any())
            {
                _db.ContactPerson.RemoveRange(contactPersons);
            }
        }


        public override async Task<ContactPerson> UpdateAsync(ContactPerson obj)
        {
            try
            {
                var tracked = await _db.ContactPerson
                    .FirstOrDefaultAsync(c => c.CustId == obj.CustId && c.Id == obj.Id);

                if (tracked == null)
                    return new ContactPerson();

                var entry = _db.Entry(tracked);
                var originalValues = entry.OriginalValues.Clone();

                // Apply new values
                entry.CurrentValues.SetValues(obj);

                var changes = new StringBuilder();

                foreach (var prop in entry.Properties)
                {
                    if (prop.IsModified)
                    {
                        var oldValue = originalValues[prop.Metadata.Name]?.ToString() ?? "null";
                        var newValue = prop.CurrentValue?.ToString() ?? "null";

                        if (oldValue != newValue)
                        {
                            changes.AppendLine($"{prop.Metadata.Name}: '{oldValue}' → '{newValue}'");
                        }
                    }
                }

                // No changes → skip save/log
                if (changes.Length == 0)
                    return tracked;

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Contact Person Master",
                    action: "Contact Person data modified",
                    additionalInfo: changes.ToString()
                );

                return tracked;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ContactPersonRepository.UpdateAsync");
                return new ContactPerson();
            }
        }

    }
}
