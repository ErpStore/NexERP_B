using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.AdminRepository
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly CurrentUserService _currentUserService;

        private readonly UserSession _session;

        public UserRepository(ApplicationDbContext db,IPasswordHasher<User> passwordHasher,ILoggingService loggingService,  CurrentUserService currentUserService)
            : base(db, loggingService)
        {
            _db = db;
            _passwordHasher = passwordHasher;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
           
        }


        // ===================== LOGIN =====================
        public async Task<User?> LoginAsync(string username, string password)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username && u.IsActive);
                if (user == null) return null;

                var result = _passwordHasher.VerifyHashedPassword(user, user.UserPassword, password);
                return result == PasswordVerificationResult.Success ? user : null;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in LoginAsync for username: {username}");
                return null;
            }
        }


        // M2-A08 (Q-05 / R-16): the QrExpiryDate predicate belongs HERE, in the query.
        //
        // Before this change the query filtered QrToken, IsQrEnabled and IsActive but NOT
        // QrExpiryDate, so it returned expired users and correctness depended on the caller
        // remembering to check. Both Blazor callers do — QrLogin.razor:50-56 ("QR Code Expired")
        // and Login.razor:422-429 ("QR Expired") — and they keep their checks; a redundant check
        // is not a bug and this task does not touch those files. Any THIRD caller (an API
        // endpoint, a background job, a mobile client) got a valid User for an expired token.
        //
        // Behaviour-preserving for both existing callers: they already rejected exactly what this
        // predicate now excludes. Verified there is no third caller — GetUserByQrToken has exactly
        // two call sites, Login.razor:404 and QrLogin.razor:34.
        //
        // The comparison mirrors the callers' test, "QrExpiryDate.HasValue && QrExpiryDate.Value <
        // DateTime.Now", including DateTime.Now (server-local, time-of-day significant) rather
        // than DateTime.Today. A NULL QrExpiryDate is NOT an expired one and still returns the
        // user, exactly as the callers' HasValue guard does.
        public async Task<User?> GetUserByQrToken(Guid token)
        {
            // Evaluated once, in the client, so the predicate is a parameter rather than a
            // GETDATE() re-evaluated per row.
            var now = DateTime.Now;

            return await _db.Users
                                 .Where(x =>
                                    x.QrToken == token &&
                                    x.IsQrEnabled &&
                                    x.IsActive &&
                                    (x.QrExpiryDate == null || x.QrExpiryDate.Value >= now))
                                 .FirstOrDefaultAsync();
        }


        public async Task<UserVM?> GetUserTrialAsync(int userId)
        {
            return await _db.Users
             .Where(x => x.UserId == userId)
             .Select(x => new UserVM
             {
                 TrialDays = x.TrialDays,
                 ExpiryDate = x.ExpiryDate
             }).SingleOrDefaultAsync();
        }

    }
}
