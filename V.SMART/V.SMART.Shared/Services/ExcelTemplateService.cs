using ClosedXML.Excel;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public class ExcelTemplateService : IExcelTemplateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;

        public ExcelTemplateService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;

        }

        #region First Step- Enter the Columns Headings
        public async Task<string[]> LoadHeadings(string uploadType)
        {
            try
            {

                var CommonHeaders = uploadType.Trim() switch
                {
                    UploadTypes.EquirySales => new[]
                    {
                    "ItemType","Category","Item Code / Part No.", "Item Descriptions","Item Specification",
                    "UOM","PurchaseUnit","HSN Code","Sac Code","Item Weight","Item Area","Item Remarks","Qty","Remarks"
                    },

                    UploadTypes.MfgQuote => new[]
                    {
                    "Ref. No", "ItemType","Category","Item Code / Part No.", "Item Descriptions","Item Specification",
                    "UOM","PurchaseUnit","HSN Code","Sac Code","Item Weight","Item Area","Item Remarks","Qty","Rate","Discount %","Discount Amount",
                    "CGST", "SGST", "IGST", "Remarks"
                    },

                    UploadTypes.SalesPo => new[]
                    {
                    "LineId","ItemType","Category","Item Code / Part No.", "Item Descriptions",
                    "Item Specification","UOM","PurchaseUnit", "HSN Code", "Sac Code","Item Weight","Item Area","Item Remarks",
                    "Qty", "Rate", "Discount %","Discount Amount","CGST", "SGST", "IGST", "Due Date","PODate"
                    },

                    UploadTypes.LabourGRN => new[]
                    {
                    "Trans Type","ItemType","Category","Item Code / Part No.","Item Descriptions","Item Specification",
                    "UOM","PurchaseUnit","HSN Code","Sac Code","Item Weight","Item Area","Item Remarks",
                    "Dc Qty", "Recd.Qty", "Rate", "Wo No.","Remarks","Batch No.", "Heat No."
                    },

                    _ => Array.Empty<string>()
                };
                return CommonHeaders;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in ExcelUpload LoadHeadings Method");
                return Array.Empty<string>();
            }
        }
        #endregion

        #region Template Downloading
        public async Task<byte[]> DownloadTemplate(string uploadType)
        {
            try
            {
                string[] headers = await LoadHeadings(uploadType);

                //// ✅ Fetch CostCenters from DB
                //var costcenterEntities=await _unitOfWork.CostCenters.GetAllAsync();
                //var costcenterlist = costcenterEntities.ToList();

                // ✅ Fetch UOMs from DB
                var uomEntities = await _unitOfWork.UOMs.GetAllAsync();
                var uomList = uomEntities.ToList();

                var categoryEntities = await _unitOfWork.Categories.GetAllAsync();
                var categoryList = categoryEntities
                                    .Select(u => $"{u.CategoryCode} - {u.CategoryName}")
                                    .ToList();

                // Generate Excel
                var bytes = await CreateTemplateAsync($"{uploadType}", headers, uomList: uomList, categoryList: categoryList);

                return bytes;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in DownloadTemplate for UploadType: {uploadType}");
                return Array.Empty<byte>();
            }

        }
        #endregion

        #region Common Service Methods
        private string GetValue(string header, List<string> row, Dictionary<string, int> columnIndex)
        {
            if (columnIndex.TryGetValue(header.ToLower().Trim(), out var index) && index < row.Count)
                return row[index];
            return "";
        }

        public async Task<Item> GetOrCreateItemByCodeAsync(string itemCode, string itemName, string speification, string uom, string hsn,
                                                           string scn, string itemType, int categorycode = 0, decimal weight = 0, decimal Area = 0, string Remarks = "")
        {
            try
            {
                // 1️⃣ Basic validation
                if (string.IsNullOrWhiteSpace(itemCode))
                    throw new ArgumentException("ItemCode is required");

                itemCode = itemCode.Trim();
                uom = string.IsNullOrWhiteSpace(uom) ? "NOS" : uom.Trim().ToUpper();

                // 2️⃣ Check if item already exists
                var item = await _unitOfWork.ItemRepositories.FirstOrDefaultAsync(x => x.ItemCode == itemCode.Trim());

                if (item != null)
                    return item;

                // 3️⃣ Validate UOM exists (IMPORTANT – FK safety)
                var uomExists = await _unitOfWork.UOMs.AnyAsync(u => u.UnitCode == uom);

                if (!uomExists)
                    throw new Exception($"Invalid UOM '{uom}' for ItemCode '{itemCode}'");

                // 4️⃣ Create item
                item = new Item
                {
                    ItemCode = itemCode,
                    ItemName = string.IsNullOrWhiteSpace(itemName) ? itemCode : itemName.Trim(),
                    Specification = speification?.Trim(),
                    PartWeight = weight,
                    TotalArea = Area,
                    BtOtorMfg = itemType == "Manufacturing" ? "Mfg" : "BtOt",
                    MeasureUnit = uom,
                    PurchaseUnit = uom,
                    HSNCode = hsn?.Trim() ?? "",
                    SACCode = scn?.Trim() ?? "",
                    //------------
                    IsServcL = hsn?.StartsWith("99") == true ? "Y" : "N",
                    IsServcS = scn?.StartsWith("99") == true ? "Y" : "N",
                    ///---------
                    CategoryCode = categorycode,
                    Remarks = Remarks?.Trim(),
                    CreatedDate = DateTime.Now
                };

                // 5️⃣ DataAnnotation validation
                var results = new List<ValidationResult>();
                var context = new ValidationContext(item);

                if (!Validator.TryValidateObject(item, context, results, true))
                {
                    var errors = string.Join(", ", results.Select(r => r.ErrorMessage));
                    throw new Exception($"Item validation failed: {errors}");
                }

                // 6️⃣ Save
                await _unitOfWork.ItemRepositories.CreateAsync(item);
                await _unitOfWork.SaveAsync();

                return item;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetOrCreateItemByCodeAsync for ItemCode: {itemCode}");
                // ❌ DO NOT return empty Item
                throw;
            }
        }

        public async Task AssignItemToCustomer(int customerId, int itemId)
        {
            var itemAssign = await _unitOfWork.ScreenManagements.GetQueryable()
                               .Where(x => x.Id == 8)
                               .Select(x => x.Required)
                               .FirstOrDefaultAsync();
            if (itemAssign)
            {
                var exists = await _unitOfWork.ItemCustomerAssigns
                             .AnyAsync(x => x.CustomerId == customerId && x.ItemId == itemId);

                if (!exists)
                {
                    var custItem = new ItemCustomerAssign
                    {
                        CustomerId = customerId,
                        ItemId = itemId
                    };
                    await _unitOfWork.ItemCustomerAssigns.CreateAsync(custItem);
                    await _unitOfWork.SaveAsync();
                }
            }
        }

        public async Task<List<Item>> GetItemsByCodesAsync(List<string> itemCodes)
        {
            try
            {
                var items = await _unitOfWork.ItemRepositories.FindAsync(x => itemCodes.Contains(x.ItemCode));
                return items.ToList();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetItemsByCodesAsync");
                return null;
            }
        }

        public async Task<Category> EnsureCategoryExists(string Category)
        {
            //Checking the Category Exists or not, if not then create new category and return the category object
            try
            {
                if (string.IsNullOrWhiteSpace(Category))
                    return null;

                var cate = await _unitOfWork.Categories.FirstOrDefaultAsync(s => s.CategoryName == Category.Trim());

                if (cate == null)
                {
                    cate = new Category { CategoryName = Category.Trim() };
                    await _unitOfWork.Categories.CreateAsync(cate);
                    await _unitOfWork.SaveAsync(); // Save changes

                }

                return cate;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in EnsureCategoryExists for Category: {Category}");
                return null;
            }
        }

        public async Task<HSNMaster> EnsureHsnExists(string HSN)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(HSN))
                    return null;

                var code = await _unitOfWork.HSNMasters.FirstOrDefaultAsync(s => s.HSNcode == HSN.Trim());

                if (code == null)
                {
                    HSN = HSN.Trim();

                    if (HSN.Length < 2 || !HSN.All(char.IsDigit))
                        throw new ArgumentException("Invalid HSN code");

                    code = new HSNMaster
                    {
                        HSNcode = HSN.Trim(),
                        HSNCategoryCode = HSN.Substring(0, 2),
                        Required = true
                    };
                    await _unitOfWork.HSNMasters.CreateAsync(code);
                    await _unitOfWork.SaveAsync(); // Save changes
                }
                return code;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in EnsureHsnExists for HSN Code: {HSN}");
                return null;
            }
        }

        public async Task<Dictionary<string, string>> ItemValidation(ExcelUploadVM Model)
        {
            var errors = new Dictionary<string, string>();
            try
            {
                if (string.IsNullOrWhiteSpace(Model.ItemCode))
                {
                    errors["Item Code / Part No."] = "required";
                    return errors;
                }

                var itemCode = Model.ItemCode.Trim();

                var existingItem = await _unitOfWork.ItemRepositories
                    .FirstOrDefaultAsync(x => x.ItemCode == itemCode.Trim());

                if (existingItem != null)
                    return errors; // no errors

                if (string.IsNullOrWhiteSpace(Model.ItemName))
                    errors["Item Descriptions"] = "required";

                if (string.IsNullOrWhiteSpace(Model.MeasureUnit))
                    errors["UOM"] = "required";

                if (string.IsNullOrWhiteSpace(Model.PurchaseUnits))
                    errors["PurchaseUnit"] = "required";

                if (string.IsNullOrWhiteSpace(Model.ItemType))
                    errors["ItemType"] = "required";

                if (string.IsNullOrWhiteSpace(Model.Category))
                    errors["Category"] = "required";

                if (string.IsNullOrWhiteSpace(Model.HSNCode))
                {
                    errors["HSN Code"] = "required";
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(Model.HSNCode, @"^\d{4,}$"))
                {
                    errors["HSN Code"] = "HSN Code must be greater than or equal to 4 digits";
                }

                if (string.IsNullOrWhiteSpace(Model.SACCode))
                {
                    errors["Sac Code"] = "required";
                }
                else if (!System.Text.RegularExpressions.Regex.IsMatch(Model.SACCode, @"^\d{4,}$"))
                {
                    errors["Sac Code"] = "SAC Code must be greater than or equal to 4 digits";
                }

                if (!string.IsNullOrWhiteSpace(Model.MeasureUnit))
                {
                    var uomExists = await _unitOfWork.UOMs
                        .AnyAsync(u => u.UnitCode == Model.MeasureUnit.Trim().ToUpper());

                    if (!uomExists)
                        errors["UOM"] = $"Invalid UOM ({Model.MeasureUnit})";
                }

                if (string.IsNullOrWhiteSpace(Model.Category))
                {
                    var categoryCodeExists = await _unitOfWork.Categories
                        .AnyAsync(u => u.CategoryName == Model.Category);

                    if (!categoryCodeExists)
                        errors["Category"] = $"Invalid CategoryCode ({Model.Category})";
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ItemValidation for ItemCode: {Model.ItemCode}");
                errors["Item Code / Part No."] = "Unexpected validation error";
            }

            return errors;
        }

        public async Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes)
              => await _commonService.GetItemIdsByItemCodesAsync(itemCodes);

        public async Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds)
            => await _commonService.GetItemVMsByItemIdsAsync(itemIds);
        #endregion

        public async Task<byte[]> CreateTemplateAsync(
            string sheetName,
            string[] headers,
            IEnumerable<object>? sampleData = null,
            bool freezeHeader = true,
            XLColor? headerColor = null,
            IEnumerable<UOM>? uomList = null,
            IEnumerable<string>? categoryList = null
        )

        {
            using var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet(sheetName);

            int totalColumns = headers.Length;

            // 🔹 Add headers
            for (int i = 0; i < totalColumns; i++)
                ws.Cell(1, i + 1).Value = headers[i];

            // 🔹 Header styling
            var headerRange = ws.Range(1, 1, 1, totalColumns);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.WrapText = true; // ✅ Wrap text in headers
            headerRange.Style.Fill.BackgroundColor = headerColor ?? XLColor.Yellow;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 🔹 Optional sample data
            int row = 2;
            if (sampleData != null)
            {
                foreach (var data in sampleData)
                {
                    var props = data.GetType().GetProperties();
                    for (int col = 0; col < totalColumns && col < props.Length; col++)
                    {
                        var value = props[col].GetValue(data);

                        if (value == null)
                        {
                            ws.Cell(row, col + 1).Value = string.Empty;
                        }
                        else
                        {
                            switch (value)
                            {
                                case string s:
                                    ws.Cell(row, col + 1).Value = s;
                                    break;
                                case int iVal:
                                    ws.Cell(row, col + 1).Value = iVal;
                                    break;
                                case long lVal:
                                    ws.Cell(row, col + 1).Value = lVal;
                                    break;
                                case decimal dVal:
                                    ws.Cell(row, col + 1).Value = dVal;
                                    break;
                                case double dblVal:
                                    ws.Cell(row, col + 1).Value = dblVal;
                                    break;
                                case float fVal:
                                    ws.Cell(row, col + 1).Value = fVal;
                                    break;
                                case DateTime dt:
                                    ws.Cell(row, col + 1).Value = dt;
                                    ws.Cell(row, col + 1).Style.DateFormat.Format = "dd/MMM/yyyy";
                                    break;
                                case bool b:
                                    ws.Cell(row, col + 1).Value = b;
                                    break;
                                default:
                                    ws.Cell(row, col + 1).Value = value.ToString();
                                    break;
                            }
                        }

                        // ✅ Wrap text for each data cell
                        ws.Cell(row, col + 1).Style.Alignment.WrapText = true;
                    }
                    row++;
                }
            }

            // 🔹 Apply borders
            var firstRowsRange = ws.Range(1, 1, Math.Max(10, row - 1), totalColumns);
            firstRowsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            firstRowsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // 🔹 Create RefData sheet for dropdowns (UOM + Category)
            var refSheet = workbook.Worksheets.FirstOrDefault(s => s.Name == "RefData") ?? workbook.AddWorksheet("RefData");
            int refRow = 1;

            if (uomList != null && uomList.Any())
            {
                foreach (var uom in uomList.Distinct())
                {
                    refSheet.Cell(refRow, 1).Value = uom.UnitCode;
                    refRow++;
                }
                workbook.NamedRanges.Add("UOMList", refSheet.Range($"A1:A{refRow - 1}"));
            }

            if (categoryList != null && categoryList.Any())
            {
                int startRow = refRow + 1;
                int catRow = startRow;
                foreach (var cat in categoryList.Distinct())
                {
                    refSheet.Cell(catRow, 2).Value = cat;
                    catRow++;
                }
                workbook.NamedRanges.Add("CategoryList", refSheet.Range($"B{startRow}:B{catRow - 1}"));
            }

            refSheet.Visibility = XLWorksheetVisibility.VeryHidden;

            // --- Apply dropdowns based on header name ---
            for (int i = 0; i < totalColumns; i++)
            {
                string header = headers[i].Trim();

                if (string.Equals(header, "ItemType", StringComparison.OrdinalIgnoreCase))
                {
                    var allowedValues = new List<string> { "Brought-Out", "Manufacturing" };
                    var validation = ws.Range(2, i + 1, 500, i + 1).CreateDataValidation();
                    validation.List($"\"{string.Join(",", allowedValues)}\"");
                    validation.IgnoreBlanks = true;
                    validation.InCellDropdown = true;
                    validation.ShowErrorMessage = true;
                    validation.ErrorTitle = "Invalid Item Type";
                    validation.ErrorMessage = "Select 'Brought-Out' or 'Manufacturing'.";
                }

                //new Code
                if (string.Equals(header, "Trans Type", StringComparison.OrdinalIgnoreCase))
                {
                    var allowedValues = new List<string> { "In", "Out" };
                    var validation = ws.Range(2, i + 1, 500, i + 1).CreateDataValidation();
                    validation.List($"\"{string.Join(",", allowedValues)}\"");
                    validation.IgnoreBlanks = true;
                    validation.InCellDropdown = true;
                    validation.ShowErrorMessage = true;
                    validation.ErrorTitle = "Invalid Transaction Type";
                    validation.ErrorMessage = "Select 'In' or 'Out'.";
                }
                //End New Code

                else if (string.Equals(header, "UOM", StringComparison.OrdinalIgnoreCase) && uomList != null && uomList.Any())
                {
                    var validation = ws.Range(2, i + 1, 500, i + 1).CreateDataValidation();
                    validation.List("=UOMList");
                    validation.IgnoreBlanks = true;
                    validation.InCellDropdown = true;
                    validation.ShowErrorMessage = true;
                    validation.ErrorTitle = "Invalid UOM";
                    validation.ErrorMessage = "Select a valid UOM from the dropdown.";
                }

                //new Code 
                else if (string.Equals(header, "PurchaseUnit", StringComparison.OrdinalIgnoreCase) && uomList != null && uomList.Any())
                {
                    var validation = ws.Range(2, i + 1, 500, i + 1).CreateDataValidation();
                    validation.List("=UOMList");
                    validation.IgnoreBlanks = true;
                    validation.InCellDropdown = true;
                    validation.ShowErrorMessage = true;
                    validation.ErrorTitle = "Invalid UOM";
                    validation.ErrorMessage = "Select a valid Units from the dropdown.";
                }
                //end

                else if (string.Equals(header, "Category", StringComparison.OrdinalIgnoreCase) && categoryList != null && categoryList.Any())
                {
                    var validation = ws.Range(2, i + 1, 500, i + 1).CreateDataValidation();
                    validation.List("=CategoryList");
                    validation.IgnoreBlanks = true;
                    validation.InCellDropdown = true;
                    validation.ShowErrorMessage = true;
                    validation.ErrorTitle = "Invalid Category";
                    validation.ErrorMessage = "Select a valid Category from the dropdown.";
                }
            }

            // Freeze header
            if (freezeHeader)
                ws.SheetView.FreezeRows(1);

            ws.Range(1, 1, 1, totalColumns).SetAutoFilter();
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }

        public async Task<Dictionary<string, byte[]>> ExtractImagesFromExcelAsync(Stream excelStream, string sheetName)
        {
            ExcelPackage.License.SetNonCommercialOrganization("MyCompany"); // Set EPPlus license context

            var imageMap = new Dictionary<string, byte[]>();

            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[sheetName]; // first sheet

                foreach (var drawing in worksheet.Drawings)
                {
                    if (drawing is ExcelPicture pic) // Must cast to ExcelPicture
                    {
                        int row = pic.From.Row + 1; // EPPlus rows are 0-based
                        string itemCode = worksheet.Cells[row, 1].Text; // Assuming ItemCode in column 1

                        if (!string.IsNullOrEmpty(itemCode))
                        {
                            // Use ImageBytes property from ExcelImageReadOnly
                            var imageBytes = pic.Image?.ImageBytes;
                            if (imageBytes != null && imageBytes.Length > 0)
                            {
                                imageMap[itemCode] = imageBytes;
                            }
                        }
                    }
                }
            }

            return imageMap;
        }

    }

}
