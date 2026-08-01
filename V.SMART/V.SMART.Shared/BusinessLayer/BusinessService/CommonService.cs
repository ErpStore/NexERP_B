
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.InkML;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.DcAutoRunning;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.InvoiceAutoRunning;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Data.Utilities;
using V.SMART.Shared.Repository.IRepository;

using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;

using static MudBlazor.Icons;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService
{
    public class CommonService : ICommonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CurrentUserService _userService;
        private readonly ILoggingService _loggingService;
        private readonly IMapper _mapper;
        private readonly V.SMART.Shared.Services.ReportViewer.ReportService _reportService;

        private readonly IReportExecutor _report;
        
        // Cache decimal places for performance
        private static int? _cachedDecimalPlaces;

        public CommonService(IUnitOfWork unitOfWork,
            CurrentUserService userService,
            ILoggingService loggingService,
            IMapper mapper, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _loggingService = loggingService;
            _mapper = mapper; 
            _report = report;
            
        }

        #region Users
        public async Task<List<string>> GetAllActiveUserNamesAsync()
        {
            var userNames = await _unitOfWork.Users
                .GetQueryable()
                .Where(u => u.IsActive)
                .Select(u => u.UserName)
                .ToListAsync();

            return userNames;
        }

        #endregion

        #region Decimal Places
        public async Task<int> GetDecimalPlacesAsync()
        {
            if (_cachedDecimalPlaces.HasValue)
                return _cachedDecimalPlaces.Value;

            try
            {
                var decimalPlaces = await _unitOfWork.Companies
                    .GetQueryable()
                    .Select(c => c.DecimalPlaces)
                    .FirstOrDefaultAsync();

                _cachedDecimalPlaces = decimalPlaces != 0 ? decimalPlaces : 2;
                return _cachedDecimalPlaces.Value;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching decimal places.");
                return 2; // fallback default
            }
        }
        #endregion

        #region User Authority
        public async Task<UserAuthority?> GetUserAuthorityAsync(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return null;

            try
            {
                return await _unitOfWork.UserAuthorities
                    .GetQueryable()
                    .FirstOrDefaultAsync(ua => ua.User.UserName == userName);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching user authority for username='{userName}'.");
                return null;
            }
        }
        #endregion

        #region States & Currency
        public async Task<IEnumerable<State>> GetStatesAsync()
        {
            try
            {
                return await _unitOfWork.States.GetAllAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching states.");
                return Enumerable.Empty<State>();
            }
        }

        public async Task<IEnumerable<Currency>> GetCurrenciesAsync()
        {
            try
            {
                return await _unitOfWork.Currencyis.GetAllAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching currencies.");
                return Enumerable.Empty<Currency>();
            }
        }

        public async Task<Currency?> GetCurrencyByIdAsync(int currId)
        {
            if (currId <= 0) return null;

            try
            {
                return await _unitOfWork.Currencyis.GetAsync(currId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching currency with CurrId={currId}.");
                return null;
            }
        }

        public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
        {
            if (currId <= 0) return null;

            try
            {
                var latestCurrency = await _unitOfWork.CurrencyTodays
                    .GetLatestAsync(c => c.CurrId == currId, c => c.Id);

                return latestCurrency?.CurrValueInRs;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching latest currency value for CurrId={currId}.");
                return null;
            }
        }
        #endregion

        #region Items
        public async Task<List<ItemVM>> GetAllActiveItemsAsync()
        {
            try
            {
                var items = await _unitOfWork.ItemRepositories.FindAllProjectedAsync(
                    c => !c.Hide,
                    c => new ItemVM
                    {
                        ItemId = c.ItemId,
                        ItemCode = c.ItemCode,
                        ItemName = c.ItemName,
                        Rate = c.Rate,
                        MeasureUnit = c.MeasureUnit,
                        HSNCode = c.HSNCode,
                        SACCode = c.SACCode,
                        CGST = c.CGST,
                        SGST = c.SGST,
                        IGST = c.IGST
                    });

                return items.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active items.");
                return new List<ItemVM>();
            }
        }


        public async Task<List<ItemVM>> GetAllItemsByCategoryCode(int Id)
        {
            try
            {
                var items = await _unitOfWork.ItemRepositories.GetQueryable()
                            .Where(i => i.CategoryCode == Id).ToListAsync();
                return items.Select(x => new ItemVM
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    Rate = x.Rate,
                    MeasureUnit = x.MeasureUnit,
                    HSNCode = x.HSNCode,
                    SACCode = x.SACCode,
                    CGST = x.CGST,
                    SGST = x.SGST,
                    IGST = x.IGST
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error occured in GetAllItemsByCategoryCode");
                return new List<ItemVM>();
            }
        }

        public async Task<List<ItemVM>> GetAllActiveItemsByCategooryAsync(string category)
        {
            try
            {
                var items = await _unitOfWork.ItemRepositories.GetQueryable()
                    .Include(i => i.Category)
                    .Where(i => !i.Hide && i.Category.CategoryName == category)
                    .Select(c => new ItemVM
                    {
                        ItemId = c.ItemId,
                        ItemCode = c.ItemCode,
                        ItemName = c.ItemName,
                        Rate = c.Rate,
                        MeasureUnit = c.MeasureUnit,
                        HSNCode = c.HSNCode,
                        SACCode = c.SACCode,
                        CGST = c.CGST,
                        SGST = c.SGST,
                        IGST = c.IGST
                    })
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active items.");
                return new List<ItemVM>();
            }
        }


        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
        {
            if (itemId == null || itemId <= 0) return null;

            try
            {
                return await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(x => x.ItemId == itemId.Value)
                    .Select(x => new ItemVM
                    {
                        ItemId = x.ItemId,
                        ItemCode = x.ItemCode,
                        ItemName = x.ItemName,
                        MeasureUnit = x.MeasureUnit,
                        HSNCode = x.HSNCode,
                        SACCode = x.SACCode,
                        Rate = x.Rate,
                        LabourPrice = x.LabourPrice,
                        AssemblyPrice = x.AssemblyPrice,
                        IsLabourItem = x.IsLabourItem,
                        CGST = x.CGST,
                        SGST = x.SGST,
                        IGST = x.IGST,
                        HandlingCharges = x.HandlingCharges,
                        ROL = x.ROL,
                        MOL = x.MOL,
                        LotSize = x.LotSize

                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching item details for ItemId={itemId}.");
                return null;
            }
        }

        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? customerId = null)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = searchText.Trim();

                // Get screen setting
                bool isRestrictionEnabled = await _unitOfWork.ScreenManagements
                    .GetQueryable()
                    .Where(x => x.Display == "Customer-Based Item Restriction"
                             && x.ScreenName == "Items")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();

                IQueryable<Item> query = _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(i => !i.Hide);

                // Apply customer restriction
                if (isRestrictionEnabled && customerId.HasValue)
                {
                    var assignedQuery = _unitOfWork.ItemCustomerAssigns
                        .GetQueryable()
                        .Where(x => x.CustomerId == customerId.Value);

                    bool hasAssignments = await assignedQuery.AnyAsync();

                    if (hasAssignments)
                    {
                        query = from i in query
                                join ica in assignedQuery
                                    on i.ItemId equals ica.ItemId
                                select i;
                    }
                }

                var items = await query
                    .Where(i =>
                        EF.Functions.Like(i.ItemCode, $"%{searchText}%") ||
                        EF.Functions.Like(i.ItemName, $"%{searchText}%") ||
                        EF.Functions.Like(i.Specification, $"%{searchText}%"))

                    .OrderBy(i => i.ItemCode)
                    .Take(50)
                    .Select(i => new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        Specification = i.Specification,
                        Rate = i.Rate,
                        MeasureUnit = i.MeasureUnit,
                        CategoryCode = i.CategoryCode,
                        CategoryName = i.Category.CategoryName,
                        HSNCode = i.HSNCode,
                        SACCode = i.SACCode,
                        CGST = i.CGST,
                        SGST = i.SGST,
                        IGST = i.IGST,
                        IsLabourItem = i.IsLabourItem,
                        AssemblyPrice = i.AssemblyPrice,
                        ROL = i.ROL,
                        DeliveryDays = i.DeliveryDays
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching items with text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }

        public async Task<IEnumerable<ItemVM>> SearchOnlyAssyItemsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = System.Text.RegularExpressions.Regex.Replace(searchText.Trim(), @"\s+", " ").ToLower();

                var query = _unitOfWork.ItemRepositories.GetQueryable()
                    .Where(i => !i.Hide &&
                                i.CategoryCode == 3 &&
                                (EF.Functions.Like(i.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(i.ItemName.ToLower(), $"%{searchText}%")))
                    .Select(i => new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        IsLabourItem = i.IsLabourItem,
                        AssemblyPrice = i.AssemblyPrice
                    });

                // Print raw SQL to console (parameterized)
                Console.WriteLine(query.ToQueryString());

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching Assembly/SubAssembly items for text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }

        public async Task<IEnumerable<ItemVM>> SearchRawMaterialItemsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = System.Text.RegularExpressions.Regex.Replace(searchText.Trim(), @"\s+", " ").ToLower();

                var query = _unitOfWork.ItemRepositories.GetQueryable()
                    .Where(i => !i.Hide &&
                                i.CategoryCode == 1 &&
                                (EF.Functions.Like(i.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(i.ItemName.ToLower(), $"%{searchText}%")))
                    .Select(i => new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        IsLabourItem = i.IsLabourItem,
                        AssemblyPrice = i.AssemblyPrice
                    });

                // Print raw SQL to console (parameterized)
                Console.WriteLine(query.ToQueryString());

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching Component items for text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }
        public async Task<IEnumerable<ItemVM>> SearchComponentItemsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = System.Text.RegularExpressions.Regex.Replace(searchText.Trim(), @"\s+", " ").ToLower();

                var query = _unitOfWork.ItemRepositories.GetQueryable()
                    .Where(i => !i.Hide &&
                                i.CategoryCode == 2 &&
                                (EF.Functions.Like(i.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(i.ItemName.ToLower(), $"%{searchText}%")))
                    .Select(i => new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        IsLabourItem = i.IsLabourItem,
                        AssemblyPrice = i.AssemblyPrice
                    });

                // Print raw SQL to console (parameterized)
                Console.WriteLine(query.ToQueryString());

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching Component items for text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }
       

        public async Task<IEnumerable<ItemVM>> SearchAssyAndSubAssyItemsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = System.Text.RegularExpressions.Regex.Replace(searchText.Trim(), @"\s+", " ").ToLower();

                var query = _unitOfWork.ItemRepositories.GetQueryable()
                    .Where(i => !i.Hide &&
                                (i.CategoryCode == 3 || i.CategoryCode == 7) &&
                                (EF.Functions.Like(i.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(i.ItemName.ToLower(), $"%{searchText}%")))
                    .Select(i => new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        IsLabourItem = i.IsLabourItem,
                        AssemblyPrice = i.AssemblyPrice
                    });

                Console.WriteLine(query.ToQueryString());

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching Assembly/SubAssembly items for text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }

        public async Task<IEnumerable<ItemVM>> SearchSubAssyAndItemsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<ItemVM>();

            try
            {
                searchText = searchText.Trim().ToLower();

                var query = _unitOfWork.ItemRepositories.GetQueryable()
                    .Where(i => !i.Hide &&
                                (i.CategoryCode != 3) &&
                                (EF.Functions.Like(i.ItemCode.ToLower(), $"%{searchText}%") ||
                                 EF.Functions.Like(i.ItemName.ToLower(), $"%{searchText}%")))
                    .Select(i => new ItemVM
                    {
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        Rate = i.Rate,
                        MeasureUnit = i.MeasureUnit,
                        HSNCode = i.HSNCode,
                        SACCode = i.SACCode,
                        CGST = i.CGST,
                        SGST = i.SGST,
                        IGST = i.IGST,
                        IsLabourItem = i.IsLabourItem,
                        AssemblyPrice = i.AssemblyPrice
                    });

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching Assembly/SubAssembly items for text='{searchText}'.");
                return Enumerable.Empty<ItemVM>();
            }
        }

        public async Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes)
        {
            if (itemCodes == null || !itemCodes.Any())
                return new Dictionary<string, int>();

            var items = await _unitOfWork.ItemRepositories
                .GetQueryable()
                .Where(i => itemCodes.Contains(i.ItemCode))
                .Select(i => new { i.ItemCode, i.ItemId })
                .ToListAsync();

            return items.ToDictionary(i => i.ItemCode, i => i.ItemId);
        }

        public async Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds)
        {
            if (itemIds == null || !itemIds.Any())
                return new List<ItemVM>();

            var items = await _unitOfWork.ItemRepositories
                .GetQueryable()
                .Where(i => itemIds.Contains(i.ItemId))
                .Select(i => new ItemVM
                {
                    ItemId = i.ItemId,
                    ItemName = i.ItemName,
                    MeasureUnit = i.MeasureUnit,
                    ItemCode = i.ItemCode,
                    HSNCode = i.HSNCode,
                    SACCode = i.SACCode,
                    IsLabourItem = i.IsLabourItem,
                    AssemblyPrice = i.AssemblyPrice

                })
                .ToListAsync();

            return items;
        }

        #endregion

        #region Categories
        public async Task<List<Category>> GetAllActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.Categories
                .GetQueryable()
                .Select(c => new Category
                {
                    CategoryCode = c.CategoryCode,
                    CategoryName = c.CategoryName
                })
                .ToListAsync();

            return categories;
        }

        public async Task<int?> GetItemCategoryCodeByItemIdAsync(int itemId)
        {
            return await _unitOfWork.ItemRepositories.GetQueryable()
                .Where(i => i.ItemId == itemId)
                .Select(i => (int?)i.CategoryCode)
                .FirstOrDefaultAsync();
        }


        #endregion

        #region Staff
        public async Task<List<Staff>> GetAllStaffAsync()
        {
            try
            {
                var staffs = await _unitOfWork.Staffs.GetQueryable()
                            .Where(s => s.Status=="Active")
                            .ToListAsync();
                return staffs.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching All Staffs.");
                return new List<Staff>();
            }
        }
        #endregion


        #region Defects
        public async Task<List<DefectInfo>> GetAllActiveDefectsAsync()
        {
            try
            {
                var defects = await _unitOfWork.DefectInfos.GetAllAsync();
                return defects.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error Occured in GetAllActiveDefectsAsync()");
                return new List<DefectInfo>();
            }
        }
        #endregion


        #region Customer

        public async Task<bool> GetLineIdByCustomerAsync(int custId)
        {
            var customer = await _unitOfWork.Customers.GetQueryable()
                .Where(c => c.CustId == custId)
                .Select(c => new { c.LineId })
                .FirstOrDefaultAsync();

            return customer.LineId;
        }
        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<CustomerVM>();
            try
            {
                searchText = searchText.Trim().ToLower();
                var customers = await _unitOfWork.Customers.GetQueryable()
                    .Where(c => !c.Inactive &&
                                (EF.Functions.Like(c.CustName.ToLower(), $"%{searchText}%")))
                    .Select(c => new CustomerVM
                    {
                        CustId = c.CustId,
                        CustName = c.CustName,
                        CustAddr = c.CustAddr,
                        ContactNo = c.ContactNo,
                        GSTNo = c.GSTNo,
                        BusiType = c.BusiType,
                        TaxValidate = c.TaxValidate,
                        CurrId = c.CurrId
                    })
                    .ToListAsync();
                return customers;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching customers with text='{searchText}'.");
                return Enumerable.Empty<CustomerVM>();
            }
        }
        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
        {
            try
            {
                var cust = await _unitOfWork.Customers.GetAsync(custId);
                if (cust == null) return null;

                return new CustomerVM
                {
                    CustId = cust.CustId,
                    CustName = cust.CustName,
                    CustAddr = cust.CustAddr,
                    ContactNo = cust.ContactNo,
                    GSTNo = cust.GSTNo,
                    StateName = cust.StateName,
                    StateId = cust.StateId,
                    PinCode = cust.PinCode,
                    Distance = cust.Distance,
                    Location = cust.Location,
                    SupTyp = cust.SupTyp,
                    BusiType = cust.BusiType,
                    CurrId = cust.CurrId,

                    TaxValidate = cust.TaxValidate,
                    LineId = cust.LineId,
                    IsWoNoReq = cust.IsWoNoReq,
                    RejectRetrac = cust.RejectRetrac,


                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching customer details for CustId={custId}.");
                return null;
            }
        }
        public async Task<List<CustomerVM>> GetAllActiveCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.Customers.FindAllProjectedAsync(
                    c => !c.Inactive,
                    c => new CustomerVM
                    {
                        CustId = c.CustId,
                        CustName = c.CustName,
                        CustAddr = c.CustAddr,
                        ContactNo = c.ContactNo,
                        GSTNo = c.GSTNo,
                        StateName = c.StateName,
                        StateId = c.StateId,
                        PinCode = c.PinCode,
                        Distance = c.Distance,
                        Location = c.Location,
                        SupTyp = c.SupTyp,
                        BusiType = c.BusiType,
                        CurrId = c.CurrId,

                        TaxValidate = c.TaxValidate,
                        LineId = c.LineId,
                        IsWoNoReq = c.IsWoNoReq,
                        RejectRetrac = c.RejectRetrac,
                    });

                return customers.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active customers.");
                return new List<CustomerVM>();
            }
        }

        public async Task<List<CustomerVM>> GetAllActiveNonExportCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.Customers.FindAllProjectedAsync(
                    c => !c.Inactive && c.BusiType != "Exports" && c.BusiType != "Imports",
                    c => new CustomerVM
                    {
                        CustId = c.CustId,
                        CustName = c.CustName,
                        CustAddr = c.CustAddr,
                        ContactNo = c.ContactNo,
                        GSTNo = c.GSTNo,
                        StateName = c.StateName,
                        StateId = c.StateId,
                        PinCode = c.PinCode,
                        Distance = c.Distance,
                        Location = c.Location,
                        SupTyp = c.SupTyp,
                        BusiType = c.BusiType,
                        CurrId = c.CurrId,

                        TaxValidate = c.TaxValidate,
                        LineId = c.LineId,
                        IsWoNoReq = c.IsWoNoReq,
                        RejectRetrac = c.RejectRetrac,
                    });

                return customers.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active customers.");
                return new List<CustomerVM>();
            }
        }

        public async Task<IEnumerable<CustomerVM>> SearchExportCustomersAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<CustomerVM>();
            try
            {
                searchText = searchText.Trim().ToLower();
                var customers = await _unitOfWork.Customers.GetQueryable()
                    .Where(c => !c.Inactive && c.BusiType == "Exports" &&
                                (EF.Functions.Like(c.CustName.ToLower(), $"%{searchText}%")))
                    .Select(c => new CustomerVM
                    {
                        CustId = c.CustId,
                        CustName = c.CustName,
                        CustAddr = c.CustAddr,
                        ContactNo = c.ContactNo,
                        GSTNo = c.GSTNo,
                        BusiType = c.BusiType,
                        TaxValidate = c.TaxValidate,
                        CurrId = c.CurrId
                    })
                    .ToListAsync();
                return customers;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching customers with text='{searchText}'.");
                return Enumerable.Empty<CustomerVM>();
            }
        }

        public async Task<CustomerVM?> GetExportCustomerByIdAsync(int custId)
        {
            try
            {
                var cust = await _unitOfWork.Customers.GetQueryable()
                    .Where(x => x.CustId == custId && x.BusiType == "Exports")
                    .Select(x => new CustomerVM
                    {
                        CustId = x.CustId,
                        CustName = x.CustName,
                        CustAddr = x.CustAddr,
                        ContactNo = x.ContactNo,
                        GSTNo = x.GSTNo,
                        StateName = x.StateName,
                        StateId = x.StateId,
                        PinCode = x.PinCode,
                        Distance = x.Distance,
                        Location = x.Location,
                        SupTyp = x.SupTyp,
                        BusiType = x.BusiType,
                        CurrId = x.CurrId,
                        TaxValidate = x.TaxValidate,
                        LineId = x.LineId,
                        IsWoNoReq = x.IsWoNoReq,
                        RejectRetrac = x.RejectRetrac
                    })
                    .FirstOrDefaultAsync();

                return cust;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex, $"Error fetching export customer details for CustId={custId}."
                );
                return null;
            }
        }

        public async Task<List<CustomerVM>> GetExportAllActiveCustomersAsync()
        {
            try
            {
                var customers = await _unitOfWork.Customers.FindAllProjectedAsync(
                    c => !c.Inactive && c.BusiType == "Exports",
                    c => new CustomerVM
                    {
                        CustId = c.CustId,
                        CustName = c.CustName,
                        CustAddr = c.CustAddr,
                        ContactNo = c.ContactNo,
                        GSTNo = c.GSTNo,
                        StateName = c.StateName,
                        StateId = c.StateId,
                        PinCode = c.PinCode,
                        Distance = c.Distance,
                        Location = c.Location,
                        SupTyp = c.SupTyp,
                        BusiType = c.BusiType,
                        CurrId = c.CurrId,

                        TaxValidate = c.TaxValidate,
                        LineId = c.LineId,
                        IsWoNoReq = c.IsWoNoReq,
                        RejectRetrac = c.RejectRetrac,
                    });

                return customers.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active customers.");
                return new List<CustomerVM>();
            }
        }

        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
        {
            try
            {
                var persons = await _unitOfWork.ContactPersons.FindAsync(cp => cp.CustId == custId);
                return persons
                    .GroupBy(cp => cp.Id)
                    .Select(g => g.First())
                    .OrderBy(cp => cp.ContactPersonName)
                    .ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching contact persons for CustId={custId}.");
                return new List<ContactPerson>();
            }
        }
        public async Task<CustomerIndirect?> GetConsigneeByAltCustIdAsync(int altCustId)
        {
            try
            {
                return await _unitOfWork.CustomerIndirects.GetAsync(altCustId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching consignee for AltCustId={altCustId}.");
                return null;
            }
        }
        public async Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId)
        {
            try
            {
                var consignees = await _unitOfWork.CustomerIndirects.GetAllAsync(c => c.CustId == custId);
                return consignees.DistinctBy(c => c.AltCustId).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching consignee addresses for CustId={custId}.");
                return new List<CustomerIndirect>();
            }
        }

        #endregion


        #region Vendor
        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<VendorVM>();

            try
            {
                searchText = searchText.Trim().ToLower();

                var vendors = await _unitOfWork.Vendors.GetQueryable()
                    .Where(c => !c.Inactive && EF.Functions.Like(c.VendorName.ToLower(), $"%{searchText}%"))
                    .ProjectTo<VendorVM>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return vendors;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching Vendors with text='{searchText}'.");
                return Enumerable.Empty<VendorVM>();
            }
        }

        public async Task<VendorVM?> GetVendorByVenerCodeAsync(int vendorCode)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors.GetQueryable()
                    .Where(v => v.VendorCode == vendorCode)
                    .ProjectTo<VendorVM>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

                return vendor;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Vendor details for VendorCode={vendorCode}.");
                return null;
            }
        }

        public async Task<List<VendorVM>> GetAllActiveVendorsAsync()
        {
            try
            {
                var vendors = await _unitOfWork.Vendors.GetQueryable()
                    .Where(v => !v.Inactive)
                    .ProjectTo<VendorVM>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return vendors;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active vendors.");
                return new List<VendorVM>();
            }
        }

        public async Task<VendorInDirect?> GetConsigneeByAltvendorCodeAsync(int AltVendorCode)
        {
            try
            {
                return await _unitOfWork.VendorInDirects.GetQueryable()
                    .Where(x => x.AltVendorCode == AltVendorCode).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching consignee for AltVendorCode={AltVendorCode}.");
                return null;
            }
        }
        public async Task<List<VendorContact>> GetContactPersonsVendorAsync(int VendorCode)
        {
            try
            {
                var persons = await _unitOfWork.VendorContacts.FindAsync(cp => cp.VendorCode == VendorCode);
                return persons
                    .GroupBy(cp => cp.Id)
                    .Select(g => g.First())
                    .OrderBy(cp => cp.ContactPerson)
                    .ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching contact persons for VendorCode={VendorCode}.");
                return new List<VendorContact>();
            }
        }
        public async Task<List<VendorInDirect>> GetConsigneeAddressesVendorAsync(int VendorCode)
        {
            try
            {
                var consignees = await _unitOfWork.VendorInDirects.GetAllAsync(c => c.VendorCode == VendorCode);
                return consignees.DistinctBy(c => c.AltVendorCode).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching consignee addresses for VendorCode={VendorCode}.");
                return new List<VendorInDirect>();
            }
        }

        #endregion

        #region Company
        public async Task<Companydetails?> GetCompanyDetailsAsync()
        {
            try
            {
                var company = (await _unitOfWork.Companies.GetAllAsync()).FirstOrDefault();
                if (company == null) return null;

                return new Companydetails
                {
                    CompanyId = company.CompanyId,
                    CompanyName = company.CompanyName,
                    RegisteredAddress = company.RegisteredAddress,
                    GSTIN = company.GSTIN,
                    LogoUrl = company.LogoUrl
                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching company details.");
                return null;
            }
        }
        #endregion

        #region Terms & Cost Centers
        public async Task<List<TermsAndConditions>> GetAllActiveTermsAsync()
        {
            try
            {
                var terms = await _unitOfWork.TermsAndConditions.GetAllAsync(c => c.IsActive);
                return terms.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active terms.");
                return new List<TermsAndConditions>();
            }
        }

        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
        {
            try
            {
                var result = await _unitOfWork.CostCenters.GetQueryable()
                    .Where(x =>
                        x.CustId == custId &&
                        ((!x.AssToEnq && !x.AssToQuote && !x.AssToMfgPO && !x.IsClosed && !x.IsDispatch) ||
                         usedCostCenterIds.Contains(x.Id)))
                    .Select(x => new CostCenterVM
                    {
                        Id = x.Id,
                        ProjectNo = x.ProjectNo,
                        CostCenterName = x.CostCenterName
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching cost centers for CustId={custId}.");
                return new List<CostCenterVM>();
            }
        }

        public async Task<List<CostCenterVM>> GetAllCostCenterDetails()
        {
            try
            {
                var result = await _unitOfWork.CostCenters
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(cc => !cc.IsClosed && !cc.IsDispatch)
                    .Select(cc => new CostCenterVM
                    {
                        Id = cc.Id,
                        ProjectNo = cc.ProjectNo,
                        CostCenterName = cc.CostCenterName,
                    })
                    .ToListAsync();

                return result ?? new List<CostCenterVM>();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in {nameof(GetAllCostCenterDetails)}()");
                return new List<CostCenterVM>();
            }
        }


        #endregion

        #region Correspondence & Attachments
        public async Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .Where(c => c.ReferenceId == refId && c.ReferenceType == refType)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error counting attachments for RefId={refId}, RefType={refType}.");
                return 0;
            }
        }
        #endregion

        #region Stores
        public async Task<IEnumerable<Store>> GetAllIssueStoresAsync()
        {
            try
            {
                return await _unitOfWork.Stores.GetQueryable()
                        .Where(x => x.CanIssue == true)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching stores");
                return Enumerable.Empty<Store>();
            }
        }

        public async Task<IEnumerable<Store>> GetAllAddStoresAsync()
        {
            try
            {
                return await _unitOfWork.Stores.GetQueryable()
                        .Where(x => x.CanAdd == true)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching stores");
                return Enumerable.Empty<Store>();
            }
        }


        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
        {
            try
            {
                return await _unitOfWork.Stores.GetQueryable().Where(x => x.CanAdd == true &&  x.CanIssue == true).ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching GetAllActiveStoresAsync");
                return Enumerable.Empty<Store>();
            }
        }
        public async Task<IEnumerable<Process>> GetAllProcessDetailsAsync()
        {
            try
            {
                var processList = await _unitOfWork.Processes.GetQueryable().ToListAsync();

                return processList;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in LoadAllProcessAsync()");
                return Enumerable.Empty<Process>();
            }
        }



        public async Task<List<(int StoreId, string StoreName)>> GetAllStoreNameAndIdAsync()
        {
            var data = await _unitOfWork.Stores.GetQueryable()
                                              .OrderBy(s => s.StoreName)
                                              .Select(s => new { s.StoreId, s.StoreName })
                                              .ToListAsync();

            return data.Select(x => (x.StoreId, x.StoreName)).ToList();
        }

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
        {
            var storeMap = await _unitOfWork.StoreMaps
                .GetQueryable()
                .FirstOrDefaultAsync(sm => sm.FormName == formName);

            if (storeMap == null)
                return (0, string.Empty);

            var store = await _unitOfWork.Stores.GetAsync(storeMap.StoreId.Value);

            return (storeMap.StoreId.Value, store?.StoreName ?? string.Empty);
        }

        #endregion

        #region Banks

        public async Task<List<Banks>> GetAllActiveBanksDetailaAsync()
        {
            var allBanks = await _unitOfWork.Banks.GetAllAsync();
            return allBanks
                .OrderBy(b => b.BankName)
                .ToList();
        }


        #endregion

        #region Assembly defaults

        public async Task<List<ItemVM>> GetAllAssembliesAsync()
        {
            return await _unitOfWork.AssmblyDefs
                .GetQueryable()
                .Where(a => a.AssemblyItem != null
                            && !a.AssemblyItem.Hide
                            && a.AssemblyItem.CategoryCode == 3)
                .Select(a => new
                {
                    a.AssemblyItem.ItemId,
                    a.AssemblyItem.ItemCode,
                    a.AssemblyItem.ItemName
                })
                .Distinct()
                .Select(x => new ItemVM
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName
                })
                .ToListAsync();
        }

        public async Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId)
        {
            return await _unitOfWork.AssmblyDefs
                .GetQueryable()
                .Where(a => a.AssemblyItem != null
                            && !a.AssemblyItem.Hide
                            && a.PartItem.CategoryCode == 7
                            && a.AssmblyID == assyId)
                .Select(a => new
                {
                    a.PartItem.ItemId,
                    a.PartItem.ItemCode,
                    a.PartItem.ItemName
                })
                .Distinct()
                .Select(x => new ItemVM
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName
                })
                .ToListAsync();
        }

        public async Task<List<ItemVM>> GetAllSubAssembliesAsync()
        {
            return await _unitOfWork.AssmblyDefs
                .GetQueryable()
                .Where(a => a.AssemblyItem != null
                            && !a.AssemblyItem.Hide
                            && a.AssemblyItem.CategoryCode == 7)
                .Select(a => new
                {
                    a.AssemblyItem.ItemId,
                    a.AssemblyItem.ItemCode,
                    a.AssemblyItem.ItemName
                })
                .Distinct()
                .Select(x => new ItemVM
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName
                })
                .ToListAsync();
        }



        #endregion

        #region Screens

        public async Task<List<Screens>> GetAllScreenAsync()
        {
            try
            {
                var screens = await _unitOfWork.Screens
                    .GetQueryable()
                    .Select(c => new Screens
                    {
                        ScreenCode = c.ScreenCode,
                        ScreenName = c.ScreenName
                    })
                    .ToListAsync();

                return screens;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching screens in GetAllScreenAsync.");
                return new List<Screens>();
            }
        }

        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
        {

            try
            {
                var screenCode = await _unitOfWork.Screens
                    .GetQueryable()
                    .Where(s => s.ScreenName == screenName)
                    .Select(c => c.ScreenCode)
                    .FirstOrDefaultAsync();

                return screenCode;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching GetScreenCodeByScreenNameAsync.");
                return 0;
            }
        }

        #endregion

        #region UOMS

        public async Task<List<UOM>> GetUOMsAsync()
        {
            return await _unitOfWork.UOMs.GetQueryable()
                .OrderBy(u => u.UnitCode)
                .ToListAsync();
        }

        #endregion

        #region RawMaterials

        public async Task<List<RawMaterial>> GetRMAsync()
        {
            return await _unitOfWork.RawMaterial.GetQueryable()
                .OrderBy(u => u.RMName)
                .ToListAsync();
        }

        #endregion

        #region HSNMaster

        public async Task<List<HSNMaster>> GetAllHSNAsynce()
        {
            return await _unitOfWork.HSNMasters.GetQueryable()
                .Where(h => h.Required)
                .ToListAsync();
        }

        public async Task<bool> HSNExistsAsync(string hsnCode)
        {
            if (string.IsNullOrWhiteSpace(hsnCode))
                return false;

            return await _unitOfWork.HSNMasters
                .GetQueryable()
                .AnyAsync(h => h.HSNcode == hsnCode);
        }

        #endregion

        #region Process

        public async Task<IEnumerable<Process>> GetAllProcessAsync()
        {
            try
            {
                var processList = await _unitOfWork.Processes
                    .GetQueryable()
                    .Select(p => new Process
                    {
                        ProcessId = p.ProcessId,
                        ProcessName = p.ProcessName
                    })
                    .ToListAsync();

                return processList;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in LoadAllProcessAsync()");
                return Enumerable.Empty<Process>();
            }
        }


        #endregion

        #region Print settings
        public async Task<List<PrintSetting>> GetPrintSettingsDetailsAsync(string screenName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(screenName))
                {
                    return new List<PrintSetting>();
                }

                var entities = await _unitOfWork.PrintSettings
                    .GetQueryable()
                    .Where(q => q.ScreenName == screenName)
                    .ToListAsync();

                return entities ?? new List<PrintSetting>();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching print settings details.");
                return new List<PrintSetting>();
            }
        }

        #endregion

        #region Machines

        public async Task<List<Machine>> GetAllMachineAsync()
        {
            return await _unitOfWork.Machines.GetQueryable()
                .Select(p => new Machine
                {
                    MachineId = p.MachineId,
                    MachineName = p.MachineName
                })
                .ToListAsync();
        }


        #endregion

        #region Shifts
        public async Task<IEnumerable<ShiftAllocation>> GetAllShiftsAsync()
        {
            try
            {
                return await _unitOfWork.ShiftAllocations.GetAllAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching shift allocations");
                return Enumerable.Empty<ShiftAllocation>();
            }
        }


        #endregion

        #region Screen Managements

        public async Task<bool> GetScreenPermissionsAsync(string screenName, string displayName)
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .AnyAsync(s =>
                        s.ScreenName == screenName &&
                        s.Display == displayName &&
                        s.Required);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error checking screen permissions");
                return false;
            }
        }

        #endregion

        #region Inspection
        public async Task<List<InspectionRowVM>> GetInspectionRowVMsAsync(int ItemId, int ProcessId)
        {
            try
            {
                var json = await _unitOfWork.InspectionRefs.GetQueryable()
                                            .Where(i => i.ItemID == ItemId && i.ProcessId == ProcessId)
                                            .Select(i => i.InsRefData)
                                            .FirstOrDefaultAsync();

                List<InspectionRowVM> inspectionRows = string.IsNullOrWhiteSpace(json)
                    ? new List<InspectionRowVM>()
                    : JsonSerializer.Deserialize<List<InspectionRowVM>>(json)
                        ?? new List<InspectionRowVM>();
                return inspectionRows;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while getting the previous InspectionData in CommonService");
                return new List<InspectionRowVM>();
            }
        }

        public async Task<List<InspectionRowVM>> GetFinalInspectionRowVMsAsync(int ItemId)
        {
            try
            {
                var json = await _unitOfWork.FinalInspectionRefs.GetQueryable()
                                            .Where(i => i.ItemID == ItemId)
                                            .Select(i => i.InsRefData)
                                            .FirstOrDefaultAsync();

                List<InspectionRowVM> inspectionRows = string.IsNullOrWhiteSpace(json)
                    ? new List<InspectionRowVM>()
                    : JsonSerializer.Deserialize<List<InspectionRowVM>>(json)
                        ?? new List<InspectionRowVM>();
                return inspectionRows;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while getting the previous InspectionData in CommonService");
                return new List<InspectionRowVM>();
            }
        }

        public async Task<List<InspectionRowVM>> GetIncomingInspectionRowVMsAsync(int ItemId, int ProcessId)
        {
            try
            {
                var json = await _unitOfWork.IncomingInspectionRefs.GetQueryable()
                                            .Where(i => i.ItemID == ItemId && i.ProcessId == ProcessId)
                                            .Select(i => i.InsRefData)
                                            .FirstOrDefaultAsync();

                List<InspectionRowVM> inspectionRows = string.IsNullOrWhiteSpace(json)
                    ? new List<InspectionRowVM>()
                    : JsonSerializer.Deserialize<List<InspectionRowVM>>(json)
                        ?? new List<InspectionRowVM>();
                return inspectionRows;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while getting the previous InspectionData in CommonService");
                return new List<InspectionRowVM>();
            }
        }

        public async Task<int> GetNoOfSamples()
        {
            try
            {
                return await _unitOfWork.InspectSettings.GetQueryable()
                                 .Select(i => i.SamplesPerSheet)
                                 .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while getting the previous InspectionData in CommonService");
                return 0;
            }
        }
        #endregion



        #region DC Outgoing Type(For Generating Auto Running No.)

        //public async Task<string> GenerateAutoRunningNoAsync(string dcType, string Suffix)
        //{
        //    try
        //    {
        //        DateTime now = DateTime.Now;
        //        int currentYear = now.Month > 3 ? now.Year : now.Year - 1;

        //        DateTime startDate = new DateTime(currentYear, 4, 1);
        //        DateTime endDate = new DateTime(currentYear + 1, 3, 31, 23, 59, 59);

        //        byte bookTypeDc = await _unitOfWork.Companies
        //                                .GetQueryable()
        //                                .Select(c => c.BookTypeDc)
        //                                .FirstOrDefaultAsync();

        //        dcType = dcType.ToUpper();

        //        string mfg = await _unitOfWork.MfgDcs.GetLastDcNoAsync(Suffix);
        //        long mfgNumber = 0;
        //        if (!string.IsNullOrWhiteSpace(mfg))
        //        {
        //            var numberPart = mfg.Split('/')[0];
        //            long.TryParse(numberPart, out mfgNumber);
        //        }

        //        string labour = await _unitOfWork.LabourDcOutgoings.GetLastDcNoAsync(Suffix);
        //        long labNumber = 0;
        //        if (!string.IsNullOrWhiteSpace(labour))
        //        {
        //            var numberPart = labour.Split('/')[0];
        //            long.TryParse(numberPart, out labNumber);
        //        }


        //        string subcon = await _unitOfWork.LabourDcOutgoings.GetLastDcNoAsync(Suffix);
        //        long subconNumber = 0;
        //        if (!string.IsNullOrWhiteSpace(labour))
        //        {
        //            var numberPart = subcon.Split('/')[0];
        //            long.TryParse(numberPart, out subconNumber);
        //        }

        //        long result = 0;

        //        switch (bookTypeDc)
        //        {
        //            // Sales, Labour, SubContract - ALL SEPARATE BOOKS
        //            case 1:
        //                result = dcType switch
        //                {
        //                    "MFGDC" => mfgNumber,
        //                    "LABOURDCOUTGOING" => labNumber,
        //                    "SUBCONDCOUT" => subconNumber,
        //                    _ => 0
        //                };

        //                break;

        //            // Sales + Labour - ONE COMBINED BOOK
        //            // SubContract - Separate
        //            case 2:

        //                result = dcType switch
        //                {
        //                    "MFGDC" => Math.Max(mfgNumber, labNumber),
        //                    "LABOURDCOUTGOING" => Math.Max(mfgNumber, labNumber),
        //                    "SUBCONDCOUT" => subconNumber,
        //                    _ => 0
        //                };

        //                break;

        //            // Sales, Labour, SubContract - ALL IN ONE BOOK
        //            case 3:
        //                result = Math.Max(Math.Max(mfgNumber, labNumber), subconNumber);
        //                break;
        //        }

        //        return result == 0 ? (result + 1).ToString() : result.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, "Error generating auto running number.");
        //        return null;
        //    }
        //}


        #endregion


        #region Outgoing Invoice and Export Invoice (For Generating auto Running No)
        //public async Task<string> GenerateInvoiceAutoRunningNoAsync(string invType, string Suffix)
        //{
        //    try
        //    {
        //        DateTime now = DateTime.Now;
        //        int currentYear = now.Month > 3 ? now.Year : now.Year - 1;

        //        DateTime startDate = new DateTime(currentYear, 4, 1);
        //        DateTime endDate = new DateTime(currentYear + 1, 3, 31, 23, 59, 59);

        //        byte bookTypeDc = await _unitOfWork.Companies
        //                                .GetQueryable()
        //                                .Select(c => c.BookTypeInvoice)
        //                                .FirstOrDefaultAsync();

        //        invType = invType.ToUpper();

        //        string mfg = await _unitOfWork.MfgInvs.GetLastInvNoAsync(Suffix);

        //        long mfgNumber = 0;
        //        if (!string.IsNullOrWhiteSpace(mfg))
        //        {
        //            var numberPart = mfg.Split('/')[0];
        //            long.TryParse(numberPart, out mfgNumber);
        //        }

        //        string labour = await _unitOfWork.LabInvs.GetLastLabInvoiceNoAsync(Suffix);
        //        long labNumber = 0;
        //        if (!string.IsNullOrWhiteSpace(labour))
        //        {
        //            var numberPart = labour.Split('/')[0];
        //            long.TryParse(numberPart, out labNumber);
        //        }


        //        string expinv = await _unitOfWork.ExpInvs.GetLastExpInvNoAsync(Suffix);
        //        long expNumber = 0;
        //        if (!string.IsNullOrWhiteSpace(labour))
        //        {
        //            var numberPart = expinv.Split('/')[0];
        //            long.TryParse(numberPart, out expNumber);
        //        }

        //        long result = 0;

        //        switch (bookTypeDc)
        //        {
        //            // Sales, Labour, Export - ALL SEPARATE BOOKS
        //            case 2:
        //                result = invType switch
        //                {
        //                    "MFGINV" => mfgNumber,
        //                    "LABINV" => labNumber,
        //                    "EXPINV" => expNumber,
        //                    _ => 0
        //                };

        //                break;

        //            // Sales + Labour - ONE COMBINED BOOK
        //            // SubContract - Separate
        //            case 1:

        //                result = invType switch
        //                {
        //                    "MFGINV" => Math.Max(mfgNumber, labNumber),
        //                    "LABINV" => Math.Max(mfgNumber, labNumber),
        //                    "EXPINV" => expNumber,
        //                    _ => 0
        //                };

        //                break;

        //            // Sales, Labour, Export - ALL IN ONE BOOK
        //            case 3:
        //                result = Math.Max(Math.Max(mfgNumber, labNumber), expNumber);
        //                break;
        //        }

        //        return result == 0 ? (result + 1).ToString() : result.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, "Error generating Invoice auto running number.");
        //        return null;
        //    }
        //}

        #endregion

        public async Task<string> GenerateAutoRunningNoAsync(string dcType, string suffix)
        {
            try
            {
                DateTime now = DateTime.Now;
                int currentYear = now.Month > 3 ? now.Year : now.Year - 1;
                string financialYear = $"{currentYear % 100:D2}-{(currentYear + 1) % 100:D2}";

                dcType = dcType.ToUpper();

                byte bookTypeDc = await _unitOfWork.Companies
                                        .GetQueryable()
                                        .Select(c => c.BookTypeDc)
                                        .FirstOrDefaultAsync();

                long startNumber = 1;

                var mfg = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .Where(x => x.DcType == "MFGDC"
                                     && x.Suffix == suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                var lab = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .Where(x => x.DcType == "LABOURDCOUTGOING"
                                     && x.Suffix == suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                var subcon = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .Where(x => x.DcType == "SUBCONDCOUT"
                                     && x.Suffix == suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                if (bookTypeDc == 1 && dcType == "MFGDC")
                {
                    startNumber = mfg;
                }
                else if (bookTypeDc == 1 && dcType == "LABOURDCOUTGOING")
                {
                    startNumber = lab;
                }
                else if (bookTypeDc == 1 && dcType == "SUBCONDCOUT")
                {
                    startNumber = subcon;
                }
                else if (bookTypeDc == 2 && (dcType == "MFGDC" || dcType == "LABOURDCOUTGOING"))
                {
                    startNumber = Math.Max(mfg, lab) + 1;
                }
                else if (bookTypeDc == 2 && dcType == "SUBCONDCOUT")
                {
                    startNumber = subcon + 1;
                }
                else if (bookTypeDc == 3)
                {

                    startNumber = Math.Max(Math.Max(mfg, lab), subcon) + 1;
                }

                var runningRow = await _unitOfWork.DcRunningNumbers
                    .GetQueryable()
                    .FirstOrDefaultAsync(x =>
                        x.DcType == dcType &&
                        x.Suffix == suffix);

                if (runningRow == null)
                {
                    runningRow = new DcRunningNumber
                    {
                        DcType = dcType,
                        Suffix = suffix,
                        LastNumber = startNumber
                    };

                    await _unitOfWork.DcRunningNumbers.CreateAsync(runningRow);
                }
                else
                {
                    if (bookTypeDc <= 0)
                    {
                        if (dcType == "MFGDC")
                        {
                            startNumber = mfg + 1;
                        }
                        else if (dcType == "LABOURDCOUTGOING")
                        {
                            startNumber = lab + 1;
                        }
                        else if (dcType == "SUBCONDCOUT")
                        {
                            startNumber = subcon + 1;
                        }
                        runningRow.LastNumber = startNumber;
                        await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                    }
                    else
                    {
                        runningRow.LastNumber = startNumber;
                        await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                    }

                }

                await _unitOfWork.SaveAsync();

                return $"{runningRow.LastNumber}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error generating auto running number.");
                return null;
            }

        }

        public async Task<string> GeneratePreviewAutoDcRunningNoAsync(string dcType, string suffix)
        {
            try
            {
                DateTime now = DateTime.Now;
                int currentYear = now.Month > 3 ? now.Year : now.Year - 1;
                string financialYear = $"{currentYear % 100:D2}-{(currentYear + 1) % 100:D2}";

                dcType = dcType.ToUpper();

                byte bookTypeDc = await _unitOfWork.Companies
                                        .GetQueryable()
                                        .Select(c => c.BookTypeDc)
                                        .FirstOrDefaultAsync();

                long startNumber = 1;

                var mfg = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .Where(x => x.DcType == "MFGDC"
                                     && x.Suffix == suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                var lab = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .Where(x => x.DcType == "LABOURDCOUTGOING"
                                     && x.Suffix == suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                var subcon = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .Where(x => x.DcType == "SUBCONDCOUT"
                                     && x.Suffix == suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                if (bookTypeDc == 1 && dcType == "MFGDC")
                {
                    startNumber = mfg;
                }
                else if (bookTypeDc == 1 && dcType == "LABOURDCOUTGOING")
                {
                    startNumber = lab;
                }
                else if (bookTypeDc == 1 && dcType == "SUBCONDCOUT")
                {
                    startNumber = subcon;
                }
                else if (bookTypeDc == 2 && (dcType == "MFGDC" || dcType == "LABOURDCOUTGOING"))
                {
                    startNumber = Math.Max(mfg, lab) + 1;
                }
                else if (bookTypeDc == 2 && dcType == "SUBCONDCOUT")
                {
                    startNumber = subcon + 1;
                }
                else if (bookTypeDc == 3)
                {

                    startNumber = Math.Max(Math.Max(mfg, lab), subcon) + 1;
                }

                var runningRow = await _unitOfWork.DcRunningNumbers
                    .GetQueryable()
                    .FirstOrDefaultAsync(x =>
                        x.DcType == dcType &&
                        x.Suffix == suffix);

                if (runningRow == null)
                {
                    runningRow = new DcRunningNumber
                    {
                        DcType = dcType,
                        Suffix = suffix,
                        LastNumber = startNumber
                    };

                }
                else
                {
                    if (bookTypeDc <= 0)
                    {
                        if (dcType == "MFGDC")
                        {
                            startNumber = mfg + 1;
                        }
                        else if (dcType == "LABOURDCOUTGOING")
                        {
                            startNumber = lab + 1;
                        }
                        else if (dcType == "SUBCONDCOUT")
                        {
                            startNumber = subcon + 1;
                        }

                    }

                }

                return $"{startNumber}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error GeneratePreviewAutoDcRunningNoAsync auto running number.");
                return null;
            }
        }


        #region Outgoing Invoice and Export Invoice (For Generating auto Running No)

        public async Task<string> GenerateInvoiceAutoRunningNoAsync(string invType, string Suffix)
        {
            try
            {
                DateTime now = DateTime.Now;
                int currentYear = now.Month > 3 ? now.Year : now.Year - 1;

                DateTime startDate = new DateTime(currentYear, 4, 1);
                DateTime endDate = new DateTime(currentYear + 1, 3, 31, 23, 59, 59);

                byte bookTypeInv = await _unitOfWork.Companies
                                        .GetQueryable()
                                        .Select(c => c.BookTypeInvoice)
                                        .FirstOrDefaultAsync();

                invType = invType.ToUpper();

                long startNumber = 1;

                var mfg = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .Where(x => x.InvoiceType == "MFGINV"
                                     && x.Suffix == Suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                var lab = await _unitOfWork.InvoiceAutoRunningNumbers
                           .GetQueryable()
                           .Where(x => x.InvoiceType == "LABINV"
                                    && x.Suffix == Suffix)
                           .Select(x => x.LastNumber)
                           .FirstOrDefaultAsync();

                var expinv = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .Where(x => x.InvoiceType == "EXPINV"
                                     && x.Suffix == Suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();


                if (bookTypeInv == 2 && invType == "MFGINV")
                {
                    startNumber = mfg;
                }
                else if (bookTypeInv == 2 && invType == "LABINV")
                {
                    startNumber = lab;
                }
                else if (bookTypeInv == 2 && invType == "EXPINV")
                {
                    startNumber = expinv;
                }
                else if (bookTypeInv == 1 && (invType == "MFGINV" || invType == "LABINV"))
                {
                    startNumber = Math.Max(mfg, lab) + 1;
                }
                else if (bookTypeInv == 1 && invType == "EXPINV")
                {
                    startNumber = expinv + 1;
                }
                else if (bookTypeInv == 3)
                {

                    startNumber = Math.Max(Math.Max(mfg, lab), expinv) + 1;
                }

                var runningRow = await _unitOfWork.InvoiceAutoRunningNumbers
                    .GetQueryable()
                    .FirstOrDefaultAsync(x =>
                        x.InvoiceType == invType &&
                        x.Suffix == Suffix);

                if (runningRow == null)
                {
                    runningRow = new InvoiceAutoRunningNumber
                    {
                        InvoiceType = invType,
                        Suffix = Suffix,
                        LastNumber = startNumber
                    };

                    await _unitOfWork.InvoiceAutoRunningNumbers.CreateAsync(runningRow);
                }
                else
                {
                    if (bookTypeInv <= 0)
                    {
                        if (invType == "MFGINV")
                        {
                            startNumber = mfg + 1;
                        }
                        else if (invType == "LABINV")
                        {
                            startNumber = lab + 1;
                        }
                        else if (invType == "EXPINV")
                        {
                            startNumber = expinv + 1;
                        }
                        runningRow.LastNumber = startNumber;
                        await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                    }
                    else
                    {
                        runningRow.LastNumber = startNumber;
                        await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                    }

                }

                await _unitOfWork.SaveAsync();

                return $"{runningRow.LastNumber}";


            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error generating Invoice auto running number.");
                return null;
            }

        }


        public async Task<string> GeneratePreviewInvoiceAutoRunningNoAsync(string invType, string Suffix)
        {
            try
            {
                DateTime now = DateTime.Now;
                int currentYear = now.Month > 3 ? now.Year : now.Year - 1;

                DateTime startDate = new DateTime(currentYear, 4, 1);
                DateTime endDate = new DateTime(currentYear + 1, 3, 31, 23, 59, 59);

                byte bookTypeDc = await _unitOfWork.Companies
                                        .GetQueryable()
                                        .Select(c => c.BookTypeInvoice)
                                        .FirstOrDefaultAsync();

                invType = invType.ToUpper();

                long startNumber = 1;

                var mfg = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .Where(x => x.InvoiceType == "MFGINV"
                                     && x.Suffix == Suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();

                var lab = await _unitOfWork.InvoiceAutoRunningNumbers
                           .GetQueryable()
                           .Where(x => x.InvoiceType == "LABINV"
                                    && x.Suffix == Suffix)
                           .Select(x => x.LastNumber)
                           .FirstOrDefaultAsync();

                var expinv = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .Where(x => x.InvoiceType == "EXPINV"
                                     && x.Suffix == Suffix)
                            .Select(x => x.LastNumber)
                            .FirstOrDefaultAsync();


                if (bookTypeDc == 2 && invType == "MFGINV")
                {
                    startNumber = mfg;
                }
                else if (bookTypeDc == 2 && invType == "LABINV")
                {
                    startNumber = lab;
                }
                else if (bookTypeDc == 2 && invType == "EXPINV")
                {
                    startNumber = expinv;
                }
                else if (bookTypeDc == 1 && (invType == "MFGINV" || invType == "LABINV"))
                {
                    startNumber = Math.Max(mfg, lab) + 1;
                }
                else if (bookTypeDc == 1 && invType == "EXPINV")
                {
                    startNumber = expinv + 1;
                }
                else if (bookTypeDc == 3)
                {

                    startNumber = Math.Max(Math.Max(mfg, lab), expinv) + 1;
                }

                var runningRow = await _unitOfWork.InvoiceAutoRunningNumbers
                    .GetQueryable()
                    .FirstOrDefaultAsync(x =>
                        x.InvoiceType == invType &&
                        x.Suffix == Suffix);

                if (runningRow == null)
                {
                    runningRow = new InvoiceAutoRunningNumber
                    {
                        InvoiceType = invType,
                        Suffix = Suffix,
                        LastNumber = startNumber
                    };


                }
                else
                {
                    if (bookTypeDc <= 0)
                    {
                        if (invType == "MFGINV")
                        {
                            startNumber = mfg + 1;
                        }
                        else if (invType == "LABINV")
                        {
                            startNumber = lab + 1;
                        }
                        else if (invType == "EXPINV")
                        {
                            startNumber = expinv + 1;
                        }

                    }

                }

                return $"{startNumber}";


            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error  GeneratePreviewInvoiceAutoRunningNoAsync auto running number.");
                return null;
            }
        }

        #endregion


        #region Converting Base64 to QR Code
        public async Task<string> GenerateQrBase64(string signedQrText)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(signedQrText))
                    return null;

                using var qrGenerator = new QRCodeGenerator();
                using var qrData = qrGenerator.CreateQrCode(
                    signedQrText,
                    QRCodeGenerator.ECCLevel.Q);

                var qrCode = new PngByteQRCode(qrData);

                byte[] qrBytes = qrCode.GetGraphic(25); // clean + sharp

                return Convert.ToBase64String(qrBytes);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> CheckPrefixValid(string ScreenName)
        {
            bool IsPrefix = await _unitOfWork.ScreenManagements.GetQueryable()
                                .Where(x => x.ScreenName == ScreenName && x.Display == "Prefix")
                                .Select(x => x.Required)
                                .FirstOrDefaultAsync();
            return IsPrefix;
        }

        public async Task<bool> CheckEwayRequired(string ScreenName)
        {
            bool IsEway = await _unitOfWork.ScreenManagements.GetQueryable()
                                 .Where(x => x.ScreenName == ScreenName && x.Display == "E-Way Bill")
                                 .Select(x => x.Required)
                                 .FirstOrDefaultAsync();
            return IsEway;
        }
        #endregion
        public async Task<string> GetBasicDirectory()
        {
            var baseDir = AppContext.BaseDirectory;
            DirectoryInfo dir = new DirectoryInfo(baseDir);
            string filePath = "";
            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "wwwroot")))
            {
                dir = dir.Parent;
            }

            if (dir != null)
            {
                filePath = Path.Combine(dir.FullName, "wwwroot");
            }
            else
            {
                filePath = Path.Combine(baseDir, "wwwroot");
            }

            return filePath;
        }


        #region Common method for exporting status records (all screens) to Excel
        public async Task<IEnumerable<T>> ExecuteStatusSPAsync<T>(string spName, string? status) where T : class  
        {
            try
            {
                var param = new SqlParameter("@Status",
                string.IsNullOrWhiteSpace(status) ? DBNull.Value : status);

                return await _report.ExecuteAsync<T>(spName, param);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        #endregion
        #region Rejection Master
        public async Task<bool> GetRejectionSelectionEnableAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                 .AnyAsync(s =>
                     s.ScreenName == "RejectionMaster" &&
                     s.Display == "Enable Rejection Reason" &&
                     s.Required);

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error GetRejectionSelectionEnableAsync.");
                return false;
            }

        }
        public async Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync()
        {
            try
            {
                return await _unitOfWork.RejectionMasters.GetQueryable()
                    .Where(x => x.IsActive)
                    .Select(x => new RejectionMasterVM
                    {
                        RejectionId = x.RejectionId,
                        RejectionCode = x.RejectionCode,
                        RejectionDescription = x.RejectionDescription,
                        IsActive = x.IsActive
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetAllRejectionReasonAsync()");
                return new List<RejectionMasterVM>();
            }
        }
        public async Task<RejectionMasterVM?> GetRejectionReasonByIdAsync(int? rejectionId)
        {
            try
            {

                return await _unitOfWork.RejectionMasters.GetQueryable()
                    .Where(x => x.RejectionId == rejectionId)
                    .Select(x => new RejectionMasterVM
                    {
                        RejectionId = x.RejectionId,
                        RejectionCode = x.RejectionCode,
                        RejectionDescription = x.RejectionDescription
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetAllRejectionReasonAsync()");
                return null;
            }

        }
        #endregion


        public async Task<IEnumerable<CandidateVM>> SearchCandidatesAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<CandidateVM>();
            try
            {
                searchText = searchText.Trim().ToLower();
                var candidates = await _unitOfWork.Candidate.GetQueryable()
                   .Where(c => EF.Functions.Like(c.CandidateName, $"%{searchText}%"))
                    .Select(c => new CandidateVM
                    {
                        CandidateID = c.CandidateID,
                        CandidateName = c.CandidateName,
                        Address = c.Address,
                        PhoneNo = c.PhoneNo,
                        Gmail = c.Gmail,
                        Position = c.Position,
                        Qualification = c.Qualification,
                        Status = c.Status,
                        Skills = c.Skills,
                        Notice_Period = c.Notice_Period,
                        Current_Salary = c.Current_Salary,
                        Expected_Salary = c.Expected_Salary,
                        Experience = c.Experience,
                        Source = c.Source,

                    })
                    .ToListAsync();
                return candidates;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error searching customers with text='{searchText}'.");
                return Enumerable.Empty<CandidateVM>();
            }
        }



        public async Task<CandidateVM?> GetCandidateByIdAsync(int CandidateId)
        {
            try
            {
                var cust = await _unitOfWork.Candidate.GetAsync(CandidateId);
                if (cust == null) return null;

                return new CandidateVM
                {
                    CandidateID = cust.CandidateID,
                    CandidateName = cust.CandidateName,
                    Address = cust.Address,
                    PhoneNo = cust.PhoneNo,
                    Gmail = cust.Gmail,
                    Position = cust.Position,
                    Qualification = cust.Qualification,
                    Status = cust.Status,
                    Skills = cust.Skills,
                    Notice_Period = cust.Notice_Period,
                    Current_Salary = cust.Current_Salary,
                    Expected_Salary = cust.Expected_Salary,
                    Experience = cust.Experience,
                    Source = cust.Source,

                };
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Candidate details for CandidateID={CandidateId}.");
                return null;
            }
        }

        public async Task<List<CandidateVM>> GetAllActiveCadidatesAsync()
        {
            try
            {
                var candidate = await _unitOfWork.Candidate.FindAllProjectedAsync(
                   c => true,
                    c => new CandidateVM
                    {
                        CandidateID = c.CandidateID,
                        CandidateName = c.CandidateName,
                        Address = c.Address,
                        PhoneNo = c.PhoneNo,
                        Gmail = c.Gmail,
                        Position = c.Position,
                        Qualification = c.Qualification,
                        Status = c.Status,
                        Skills = c.Skills,
                        Notice_Period = c.Notice_Period,
                        Current_Salary = c.Current_Salary,
                        Expected_Salary = c.Expected_Salary,
                        Experience = c.Experience,
                        Source = c.Source,
                    });

                return candidate.ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching active customers.");
                return new List<CandidateVM>();
            }
        }
    }
}