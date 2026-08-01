using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.InkML;
using FastReport;
using FastReport.Export.PdfSimple;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.Data.Enum;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using ZXing;
using ZXing.Common;


namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AdminService
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ICommonService _commonService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IJSRuntime _JS;
        private readonly IPathProvider _pathProvider;

        public UserService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService, 
                            IMapper mapper,ICommonService commonService,IPasswordHasher<User> passwordHasher, IPathProvider pathProvider, IJSRuntime JS)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _commonService = commonService;
            _passwordHasher = passwordHasher;
            _pathProvider = pathProvider;
            _JS = JS;
            
        }


        public async Task<bool> IsProductionLogEnabledAsync(int userId)
        {
            try
            {
                return await _unitOfWork.Users
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.UserId == userId)
                    .Select(x => x.IsProductionBased)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    $"Error checking production status for UserId : {userId}");

                return false;
            }
        }

        public async Task<List<State>> GetStatesAsync()
        {
            try
            {
                var states = await _commonService.GetStatesAsync();
                return states.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error loading states");
                return new List<State>();
            }
        }


        public async Task<bool> IsAssignStatesFeatureEnabledAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.IsFeatureEnabledAsync("Users", "Assign State Leads");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,"Error checking if 'Assign State Leads' feature is enabled");
                return false;
            }
        }




        public async Task<(List<UserVM> userVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Users
                    .GetQueryable()
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
                    .OrderByDescending(x => x.UserId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<UserVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (MfgPo)");
                throw new InvalidOperationException("Failed to load manufacturing PO list.", ex);
            }
        }

        public static class UserFilterBuilder
        {
            public static IQueryable<User> ApplyFilter(
                IQueryable<User> query,
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
                            return query.Where(x => x.UserName.Contains(val));

                        case "Email":
                            return query.Where(x => x.UserName.Contains(val));

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

                        case "Role":
                            return ApplyRoleFilter(query, val);

                        case "Status":
                            return ApplyStatusFilter(query, val);
                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }

            private static IQueryable<User> ApplyRoleFilter(
                IQueryable<User> query,
                string status)
            {
                try
                {
                    return status switch
                    {
                        "Administrator" =>
                            query.Where(x => x.Role == UserRole.Administrator),

                        "User" =>
                              query.Where(x => x.Role == UserRole.User),

                        _ => query
                    };
                }
                catch
                {
                    return query;
                }
            }

            private static IQueryable<User> ApplyStatusFilter(
                IQueryable<User> query,
                string status)
            {
                try
                {
                    return status switch
                    {
                        "Active" =>
                            query.Where(x => x.IsActive),

                        "InActive" =>
                              query.Where(x => !x.IsActive),

                        "Readonly" =>
                              query.Where(x => x.IsViewOnly),

                        _ => query
                    };
                }
                catch
                {
                    return query;
                }
            }

        }


        public async Task<(bool CanDelete, string Message)> CanDeleteUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users
                    .GetQueryable()
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                    return (false, "User not found or already deleted.");

                bool hasUserAuthority = await _unitOfWork.UserAuthorities
                    .GetQueryable()
                    .AnyAsync(r => r.UserId == userId);
                if (hasUserAuthority)
                    return (false, "Cannot delete this User because Authorization Premissions are assigned.");

                if (!user.IsActive)
                    return (false, "Inactive user cannot be deleted. Activate the user before deletion.");

                return (true, "User can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error while validating delete eligibility for UserId: {userId}");
                return (false, "An error occurred while validating the user.");
            }
        }

        public async Task<bool> DeleteUserbyUserIdAsync(int userId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _unitOfWork.Users
                        .GetQueryable()
                        .FirstOrDefaultAsync(e => e.UserId == userId);

                if (user == null)
                    return false;

                var changes = new StringBuilder();


                if(await _unitOfWork.UserAuthorities.GetQueryable().AnyAsync(r => r.UserId == userId))
                {
                    var authorities = await _unitOfWork.UserAuthorities.GetQueryable()
                        .Where(r => r.UserId == userId)
                        .ToListAsync();
                    await _unitOfWork.UserAuthorities.DeleteRangeAsync(authorities);
                    await _unitOfWork.SaveAsync();
                    changes.AppendLine($"Deleted {authorities.Count} related User Authorities.");
                }

                await _unitOfWork.Users.DeleteAsync(user);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "User List",
                    action: $"Deleted User: {user.UserName}",
                    additionalInfo: $"User Id: {user.UserId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete User: {userId}");
                throw;
            }
        }


        public async Task<UserRole> GetCurrentUserRoleAsync(ClaimsPrincipal user)
        {
            try
            {
                if (user.Identity?.IsAuthenticated != true)
                    return UserRole.User;

                var currentUser = await _unitOfWork.Users
                    .GetQueryable().Where(x=> x.UserName == user.Identity.Name).FirstOrDefaultAsync();

                return currentUser?.Role ?? UserRole.User;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error getting current user role");
                return UserRole.User;
            }
        }

        public async Task<UserVM?> GetUserForEditAsync(int userId)
        {
            try
            {
                var userData = await _unitOfWork.Users.GetAsync(userId);
                if (userData == null) return null;

                return _mapper.Map<UserVM>(userData);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error loading user for edit. UserId: {userId}");
                return null;
            }
        }

        public async Task<UserVM> GetNewUserAsync()
        {
            try
            {
                return new UserVM
                {
                    Role = UserRole.User,
                    StateCodes = new List<int>(),
                    LevelAuthorization = false,
                    StaffId = null
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error creating new user model");
                return new UserVM();
            }
        }

        public async Task<List<Staff>> GetAvailableStaffAsync()
        {
            try
            {
                var allStaffs = await _unitOfWork.Staffs.GetAllAsync();

                var assignedStaffIds = (await _unitOfWork.Users.GetAllAsync())
                    .Where(u => u.StaffId != null)
                    .Select(u => u.StaffId!.Value)
                    .ToHashSet();

                return allStaffs
                    .Where(s => s.IsUser && s.Status=="Active" && !assignedStaffIds.Contains(s.StaffID))
                    .ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error loading available staff list");
                return new List<Staff>();
            }
        }

        public async Task<UserVM> UpsertUserAsync(UserVM vm)
        {
            if (vm == null)
                throw new ArgumentNullException(nameof(vm));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                User entity;

                // ----------------- CREATE NEW USER -----------------
                if (vm.UserId == 0)
                {
                    var existing = await _unitOfWork.Users.GetQueryable()
                        .FirstOrDefaultAsync(u => u.UserName == vm.UserName);
                    if (existing != null)
                        throw new InvalidOperationException("User already exists with this username.");

                    entity = _mapper.Map<User>(vm);
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    if (!string.IsNullOrWhiteSpace(vm.UserPassword))
                        entity.UserPassword = _passwordHasher.HashPassword(null!, vm.UserPassword);

                    entity.StateCodesCsv = vm.StateCodes != null ? string.Join(",", vm.StateCodes) : null;

                    await _unitOfWork.Users.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    if (vm.IsViewOnly)
                    {
                        var screens = await _unitOfWork.Screens.GetQueryable()
                            .Select(s => s.Id)
                            .ToListAsync();

                        foreach (var Id in screens)
                        {
                            var right = new UserRight
                            {
                                UserId = entity.UserId,
                                ScreenId = Id,
                                CanView = true,
                                CanCreate = false,
                                CanEdit = false,
                                CanDelete = false,
                                IsHide = false,
                                CreatedDate = now,
                                CreatedBy = currentUser,
                                Remarks = "Default rights assigned on user creation"
                            };

                            await _unitOfWork.UserRights.CreateAsync(right);
                        }
                        await _unitOfWork.SaveAsync();
                    }

                    changes.AppendLine("User Created.");
                }
                else
                {
                    entity = await _unitOfWork.Users.GetQueryable()
                        .FirstOrDefaultAsync(u => u.UserId == vm.UserId)
                        ?? throw new InvalidOperationException("User not found for update.");

                    var propertyChanges = GetPropertyChanges(entity, vm);
                    if (!string.IsNullOrEmpty(propertyChanges))
                        changes.AppendLine(propertyChanges);

                    _mapper.Map(vm, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    if (vm.ChangePassword)
                    {
                        entity.UserPassword = _passwordHasher.HashPassword(entity, vm.UserPassword);
                        changes.AppendLine("Password: '[PROTECTED]' → '[PROTECTED]'");
                    }

                    entity.StateCodesCsv = vm.StateCodes != null ? string.Join(",", vm.StateCodes) : null;

                    await _unitOfWork.Users.UpdateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("User Updated.");
                }

                await transaction.CommitAsync();

                await LogChangesAsync(changes, vm.UserId == 0 ? "User Created" : "User Updated");

                var savedEntity = await _unitOfWork.Users.GetQueryable()
                    .FirstOrDefaultAsync(u => u.UserId == entity.UserId);

                return _mapper.Map<UserVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to upsert User: {vm.UserName}");
                throw new InvalidOperationException("Failed to save User. Please try again.");
            }
        }

        private string GetPropertyChanges(User entity, UserVM vm)
        {
            var sb = new StringBuilder();

            foreach (var prop in typeof(User).GetProperties())
            {
                var vmProp = typeof(UserVM).GetProperty(prop.Name);
                if (vmProp == null) continue;

                var oldVal = prop.GetValue(entity)?.ToString() ?? "null";
                var newVal = vmProp.GetValue(vm)?.ToString() ?? "null";

                if (oldVal != newVal)
                    sb.AppendLine($"{prop.Name}: '{oldVal}' → '{newVal}'");
            }

            var oldStateCsv = entity.StateCodesCsv ?? "";
            var newStateCsv = vm.StateCodes != null ? string.Join(",", vm.StateCodes) : "";
            if (oldStateCsv != newStateCsv)
                sb.AppendLine($"StateCodesCsv: '{oldStateCsv}' → '{newStateCsv}'");

            return sb.ToString();
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "User Master",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<List<UserVM>> GetQrUsers(int? userId = null)
        {
            try
            {
                var query = _unitOfWork.Users.GetQueryable()
                    .Join(
                        _unitOfWork.Staffs.GetQueryable(),
                        u => u.StaffId,
                        s => s.StaffID,
                        (u, s) => new { u, s }
                    )
                    .Where(x => x.s.Status == "Active");

                if (userId.HasValue && userId.Value > 0)
                {
                    query = query.Where(x => x.u.UserId == userId.Value);
                }

                return await query
                    .OrderBy(x => x.u.UserName)
                    .Select(x => new UserVM
                    {
                       
                        UserName = x.u.UserName,
                        StaffRefId = x.s.StaffRefId,
                        QrToken = x.u.QrToken
                    })
                    .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }




        public async Task<byte[]> PrintQrCards(int UserId)
        {
            try
            {
                return await PrintBulkQrExcel(UserId);
            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(ex, "Error generating PrintQrCards");
                return Array.Empty<byte>();
            }
        }

        private byte[] GenerateQrImage(string text)
        {
           
            using QRCodeGenerator qrGenerator = new QRCodeGenerator();

            using QRCodeData qrCodeData =
                qrGenerator.CreateQrCode(text,QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }

        public async Task<byte[]> PrintBulkQrExcel(int UserId)
        {
            try
            {

                var users = await GetQrUsers(UserId);

                var companyName = (await _commonService.GetCompanyDetailsAsync())?.CompanyName;


                string templatePath = Path.Combine(_pathProvider.GetReportTemplatePath(), "Bin_QR-Code.xlsx");

                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException("Excel template not found.", templatePath);
                }

                using XLWorkbook workbook = new XLWorkbook(templatePath);

                var ws = workbook.Worksheet(1);

                int row = 2;
                int col = 1;

                

                foreach (var user in users)
                {
                    if (!user.QrToken.HasValue)
                        continue;

                    if (companyName != null)
                    {
                        ws.Cell(row-1, col).Value = companyName;

                        ws.Cell(row -1, col).Style.Alignment.Horizontal =
                            XLAlignmentHorizontalValues.Center;

                        ws.Cell(row -1, col).Style.Alignment.Vertical =
                            XLAlignmentVerticalValues.Center;

                        ws.Cell(row - 1, col).Style.Alignment.WrapText = true;

                        ws.Cell(row - 1, col).Style.Font.FontSize = 6;

                        ws.Row(row - 1).Height = 30;
                    }

                    byte[] qr = GenerateQrImage(user.QrToken.Value.ToString());

                    using MemoryStream imgStream = new MemoryStream(qr);

                    // QR Code
                    ws.AddPicture(imgStream)
                      .MoveTo(ws.Cell(row, col), 30, 10) 
                      .WithSize(60, 60);

                    // Employee ID + User Name in same cell
                    ws.Cell(row + 1, col).Value = $"Emp Id:{user.StaffRefId}\n Name:{user.UserName}";

                    ws.Cell(row + 1, col).Style.Alignment.Horizontal =
                        XLAlignmentHorizontalValues.Center;

                    ws.Cell(row + 1, col).Style.Alignment.Vertical =
                        XLAlignmentVerticalValues.Center;

                    ws.Cell(row + 1, col).Style.Alignment.WrapText = true;

                    ws.Cell(row + 1, col).Style.Font.FontSize = 7;

                    ws.Row(row + 1).Height = 30;

                    col++;

                    // 3 cards per row
                    if (col > 5)
                    {
                        col = 1;
                        row += 3;
                    }
                }

                using MemoryStream output = new MemoryStream();

                workbook.SaveAs(output);

                return output.ToArray();

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task UpdateUserDeviceAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetAsync(userId);

                if (user == null)
                    return;

                var deviceId = await _JS.InvokeAsync<string>("deviceHelper.getDeviceId");
                var isMobile = await _JS.InvokeAsync<bool>("deviceHelper.isMobile");
                var platform = await _JS.InvokeAsync<string>("deviceHelper.getPlatform");
                var userAgent = await _JS.InvokeAsync<string>("deviceHelper.getUserAgent");

                if (isMobile)
                {
                    // Mobile Login
                    if (string.IsNullOrWhiteSpace(user.MobileDeviceId))
                    {
                        user.MobileDeviceId = deviceId;
                    }

                    user.MobileName = platform;
                    user.MobileIpAddress = _currentUserService.IpAddress;
                }
                else
                {
                    // Desktop Login
                    if (string.IsNullOrWhiteSpace(user.DesktopDeviceId))
                    {
                        user.DesktopDeviceId = deviceId;
                    }

                    user.DesktopName = _currentUserService.MachineName;
                    user.IpAddress = _currentUserService.IpAddress;
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "UpdateUserDeviceAsync");
                throw;
            }
        }
    }
}
