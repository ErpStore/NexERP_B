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


        public async Task<User?> GetUserByQrToken(Guid token)
        {
            return await _db.Users
                                 .Where(x =>
                                    x.QrToken == token &&
                                    x.IsQrEnabled &&
                                    x.IsActive)
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
