using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AdminService
{
    public class UserAuthorityservice : IUserAuthorityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;

        public UserAuthorityservice(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService,
                            IMapper mapper, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _commonService = commonService;
        }

        public async Task<List<UserAuthorityVM>> GetAllUserAuthoritiesAsync()
        {
            try
            {
                var entities = await _unitOfWork.UserAuthorities.GetAllAsync();

                var result = _mapper.Map<List<UserAuthorityVM>>(entities);

                return result ?? new List<UserAuthorityVM>();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while fetching all user authorities.");
                return new List<UserAuthorityVM>();
            }
        }

        public async Task<(List<UserAuthorityVM> userAuthVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.UserAuthorities
                    .GetQueryable()
                    .Include(x=> x.User)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = UserFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<UserAuthorityVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (User Authority)");
                throw new InvalidOperationException("Failed to load User Authority list.", ex);
            }
        }

        public static class UserFilterBuilder
        {
            public static IQueryable<UserAuthority> ApplyFilter(
                IQueryable<UserAuthority> query,
                string field,
                object value)
            {
                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

                    switch (field)
                    {
                        case "UserName":
                            return query.Where(x => x.User.UserName.Contains(val));


                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.CreatedDate >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.CreatedDate <= toDate.Date.AddDays(1).AddTicks(-1));
                            return query;
                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }
        }

        public async Task<bool> DeleteUserAuthByIdAsync(int id)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var userAuth = await _unitOfWork.UserAuthorities
                        .GetQueryable()
                        .FirstOrDefaultAsync(e => e.Id == id);

                if (userAuth == null)
                    return false;

                var changes = new StringBuilder();

                await _unitOfWork.UserAuthorities.DeleteAsync(userAuth);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "User Authority List",
                    action: $"Deleted User Authority: {userAuth.Id}",
                    additionalInfo: $"User Id: {userAuth.Id}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete User Authority Level: {id}");
                throw;
            }
        }


        public async Task<List<UserVM>> GetLevelAuthorizedUsersAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetQueryable()
                    .Where(u => u.LevelAuthorization == true)
                    .Select(u => new UserVM
                    {
                        UserId = u.UserId,
                        UserName = u.UserName
                    })
                    .OrderBy(u => u.UserName)
                    .ToListAsync();

                return users;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error in {nameof(GetLevelAuthorizedUsersAsync)} : {ex.Message}");
                throw;
            }
        }

        public async Task<UserAuthorityVM?> GetUserLevelAuthByIdAsync(int id)
        {
            try
            {
                var entity = await _unitOfWork.UserAuthorities.GetQueryable()
                            .Include(x=> x.User)
                            .FirstOrDefaultAsync(x => x.Id == id);
                return _mapper.Map<UserAuthorityVM>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error in {nameof(GetUserLevelAuthByIdAsync)} Id:{id}");
                throw;
            }
        }

        public async Task<UserAuthorityVM?> GetUserLevelAuthByUserIdAsync(int userId)
        {
            try
            {
                var entity = await _unitOfWork.UserAuthorities
                            .GetQueryable()
                            .Include(x=> x.User)
                            .FirstOrDefaultAsync(x => x.UserId == userId);

                if (entity == null)
                    return null;

                return _mapper.Map<UserAuthorityVM>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error in {nameof(GetUserLevelAuthByUserIdAsync)} UserId:{userId}");
                throw;
            }
        }


        public async Task<UserAuthorityVM> UpsertUserAuthAsync(UserAuthorityVM vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                UserAuthority entity;

                // ----------------- CREATE NEW USER Auth -----------------
                if (vm.Id == 0)
                {

                    entity = _mapper.Map<UserAuthority>(vm);
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    await _unitOfWork.UserAuthorities.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    changes.AppendLine("User level Authority Created.");
                }
                else
                {
                    entity = await _unitOfWork.UserAuthorities.GetQueryable()
                        .FirstOrDefaultAsync(u => u.Id == vm.Id)
                        ?? throw new InvalidOperationException("User Authority not found for update.");

                    var propertyChanges = GetPropertyChanges(entity, vm);
                    if (!string.IsNullOrEmpty(propertyChanges))
                        changes.AppendLine(propertyChanges);

                    _mapper.Map(vm, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await _unitOfWork.UserAuthorities.UpdateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("User Authority Updated.");
                }

                await transaction.CommitAsync();

                await LogChangesAsync(changes, vm.UserId == 0 ? "User Level Authority Created" : "User Level Authority Updated");

                var savedEntity = await _unitOfWork.UserAuthorities.GetQueryable()
                    .FirstOrDefaultAsync(u => u.Id == entity.Id);

                return _mapper.Map<UserAuthorityVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to upsert User: {vm.UserName}");
                throw new InvalidOperationException("Failed to save User. Please try again.");
            }
        }

        private string GetPropertyChanges(UserAuthority entity, UserAuthorityVM vm)
        {
            var sb = new StringBuilder();

            foreach (var prop in typeof(UserAuthority).GetProperties())
            {
                var vmProp = typeof(UserAuthorityVM).GetProperty(prop.Name);
                if (vmProp == null) continue;

                var oldVal = prop.GetValue(entity)?.ToString() ?? "null";
                var newVal = vmProp.GetValue(vm)?.ToString() ?? "null";

                if (oldVal != newVal)
                    sb.AppendLine($"{prop.Name}: '{oldVal}' → '{newVal}'");
            }
            return sb.ToString();
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "User Authority Master",
                action: action,
                additionalInfo: changes.ToString()
            );
        }






    }
}
