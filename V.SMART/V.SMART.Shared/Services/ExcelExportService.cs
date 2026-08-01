using ClosedXML.Excel;
using MudBlazor;
using System.Data;
using System.Reflection;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class ExcelExportService
{
    private readonly ILoggingService _logger;
    private readonly ICommonService _commonService;

    private Companydetails _company;

    public ExcelExportService(ILoggingService logger, ICommonService commonService)
    {
        _logger = logger;
        _commonService = commonService;

    }

    public byte[] ExportListToExcel<T>( List<T> dataList,   string sheetName = "Sheet1",  params object[] lockColumns)
    {
        try
        {
            var dataTable = ToDataTable(dataList);

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add(dataTable, sheetName);

            // Auto-fit columns
            worksheet.Columns().AdjustToContents();

            // Header formatting
            var headerRow = worksheet.Row(1);

            headerRow.Style.Font.Bold = true;
            headerRow.Style.Font.FontColor = XLColor.White;
            headerRow.Style.Fill.BackgroundColor = XLColor.SteelBlue;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            // Borders & alignment
            var usedRange = worksheet.RangeUsed();

            if (usedRange != null)
            {
                usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                usedRange.Style.Alignment.Vertical =
                    XLAlignmentVerticalValues.Center;
            }

            // Freeze first row
            worksheet.SheetView.FreezeRows(1);

            // Make all cells editable by default
            worksheet.Cells().Style.Protection.SetLocked(false);

            // Lock specific columns
            if (lockColumns != null && lockColumns.Length > 0)
            {
                foreach (var colNameObj in lockColumns)
                {
                    string colName =
                        Convert.ToString(colNameObj)?.Trim() ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(colName))
                        continue;

                    var headerCell = headerRow.Cells()
                        .FirstOrDefault(c =>
                            string.Equals(
                                c.GetString().Trim(),
                                colName,
                                StringComparison.OrdinalIgnoreCase));

                    if (headerCell == null)
                        continue;

                    int colIndex = headerCell.Address.ColumnNumber;

                    worksheet.Column(colIndex)
                             .Cells()
                             .Style
                             .Protection
                             .SetLocked(true);
                }

                // Protect worksheet
                worksheet.Protect("Admin@123");
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogDeveloperError(
                ex,
                "Error in ExcelExportService on ExportListToExcel method");

            return null;
        }
    }


    public async Task<byte[]> ExportPendingListToExcel<T>(List<T> dataList, string sheetName = "Sheet1", params object[] Name1)
    {
        try
        {

            var dataTable = ToDataTable(dataList);
            _company = await _commonService.GetCompanyDetailsAsync();
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(sheetName);



                if (_company != null)
                {
                    var cell1 = !(Name1 != null && Name1.Length > 1 && Name1[1] != null)? worksheet.Cell(1, 4): worksheet.Cell(1, 2); 
                    cell1.Value = _company.CompanyName;
                    cell1.Style.Font.FontSize = 16;
                    cell1.Style.Font.Bold = true;
                    cell1.Style.Font.FontColor = XLColor.Blue;

                    var cell2 = !(Name1 != null && Name1.Length > 1 && Name1[1] != null)
                     ? worksheet.Cell(2, 4)
                     : worksheet.Cell(2, 2);

                    // Merge cells first
                    if (!(Name1 != null && Name1.Length > 1 && Name1[1] != null))
                    {
                        worksheet.Range(2, 4, 2, 8).Merge(); // D2:L2
                    }
                    else
                    {
                        worksheet.Range(2, 2, 2, 6).Merge();  // B2:F2
                    }

                    // Set value after merge
                    cell2.SetValue(_company.RegisteredAddress);

                    // Apply formatting
                    cell2.Style.Alignment.WrapText = true;
                    cell2.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    cell2.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;

                    cell2.Style.Font.FontSize = 12;
                    cell2.Style.Font.Italic = true;
                    cell2.Style.Font.FontColor = XLColor.DarkGreen;

                    // Set row height
                    worksheet.Row(2).Height = 40;
                }

                // ✅ OPTIONAL: HEADER CONTENT ABOVE TABLE
               

                if (Name1 != null && Name1.Length > 1 &&  Name1[1] != null)
                {
                    worksheet.Cell(6, 2).Value = "Assembly Details: " + Name1[1].ToString();

                    worksheet.Cell(5, 2).Value = "Generated On: " + DateTime.Now;
                }
                else
                {
                    worksheet.Cell(4, 4).Value = $"{Name1[0]}";
                    worksheet.Cell(5, 4).Value = "Generated On: " + DateTime.Now;
                }

                // ✅ INSERT TABLE STARTING FROM ROW 8
                // worksheet.Cell(8, 1).InsertTable(dataTable);
                var table = worksheet.Cell(8, 1).InsertTable(dataTable);

                var imageColumn = dataTable.Columns["DrawingImage"];

                if (imageColumn != null)
                {
                    int imageColIndex = imageColumn.Ordinal + 1;

                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        int excelRow = i + 9;

                        worksheet.Cell(excelRow, imageColIndex).Value = "";

                        var imageObj = dataTable.Rows[i]["DrawingImage"];

                        if (imageObj is byte[] imageBytes && imageBytes.Length > 0)
                        {
                            worksheet.Row(excelRow).Height = 60;

                            using var stream = new MemoryStream(imageBytes);

                            worksheet.AddPicture(stream)
                                .MoveTo(worksheet.Cell(excelRow, imageColIndex))
                                .WithSize(80, 80);
                        }
                    }

                    worksheet.Column(imageColIndex).Width = 15;
                }

                var range = worksheet.RangeUsed();

                foreach (var column in table.DataRange.Columns())
                {
                    bool isNumericColumn = true;

                    foreach (var cell in column.Cells())
                    {
                        if (cell.IsEmpty())
                            continue;

                        object value = cell.Value;

                        if (value == null)
                            continue;

                        if (!decimal.TryParse(value.ToString(), out _))
                        {
                            isNumericColumn = false;
                            break;
                        }
                    }

                    if (isNumericColumn)
                    {
                        foreach (var cell in column.Cells())
                        {
                            if (cell.IsEmpty())
                                continue;

                            if (decimal.TryParse(cell.Value.ToString(), out decimal num))
                            {
                                cell.Value = num;
                            }
                        }

                        //column.Style.NumberFormat.Format = "#,##0.00";
                    }
                }

                // ✅ AUTO FIT COLUMNS
                worksheet.Columns().AdjustToContents();

                // ✅ APPLY BORDERS
                if (range != null)
                {
                    range.Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                }

                // ✅ HEADER ROW IS NOW ROW 8
                var headerRow = worksheet.Row(8);
                headerRow.Style.Font.Bold = true;
                //headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

                //---------------------------------------------------------------------------  
                // ✅ LOCK REQUIRED COLUMNS
                if (Name1 != null && Name1.Length > 0)
                {
                    // Unlock all cells first
                    //worksheet.Cells().Style.Protection.SetLocked(false);

                    foreach (var colNameObj in Name1)
                    {
                        string colNameStr = Convert.ToString(colNameObj)?.Trim() ?? string.Empty;
                        if (string.IsNullOrEmpty(colNameStr)) continue;

                        var headerCell = headerRow.Cells().FirstOrDefault(c => string.Equals(c.GetString().Trim(), colNameStr,StringComparison.OrdinalIgnoreCase));

                        if (headerCell == null) continue;

                        int colIndex = headerCell.Address.ColumnNumber;

                        // Lock full column
                      //  worksheet.Column(colIndex).Cells().Style.Protection.SetLocked(false);
                    }

                }
                //---------------------------------------------------------------------------

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDeveloperError(ex, "Error in ExcelExportService on ExportPendingListToExcel method");
            return null;
        }
    }


    //private DataTable ToDataTable<T>(List<T> items)
    //{
    //    DataTable dataTable = new DataTable();

    //    if (items == null || !items.Any())
    //        return dataTable;

    //    if (items.First() is Dictionary<string, object>)
    //    {
    //        var dictItems = items
    //            .Cast<Dictionary<string, object>>()
    //            .ToList();

    //        // CREATE COLUMNS
    //        foreach (var key in dictItems.First().Keys)
    //        {
    //            var firstValue = dictItems.First()[key];

    //            Type columnType = firstValue?.GetType() ?? typeof(string);

    //            dataTable.Columns.Add(key, columnType);
    //        }

    //        foreach (var key in dictItems.First().Keys)
    //        {
    //            var value = dictItems.First()[key];

    //            dataTable.Columns.Add(
    //                key,
    //                value == null || value == DBNull.Value
    //                    ? typeof(string)              
    //                    : value.GetType());
    //        }

    //        return dataTable;
    //    }


    //    PropertyInfo[] properties = typeof(T).GetProperties();

    //    foreach (PropertyInfo prop in properties)
    //    {
    //        Type colType = Nullable.GetUnderlyingType(prop.PropertyType)
    //                       ?? prop.PropertyType;

    //        dataTable.Columns.Add(prop.Name, colType);
    //    }

    //    foreach (T item in items)
    //    {
    //        var values = properties
    //            .Select(p => p.GetValue(item) ?? DBNull.Value)
    //            .ToArray();

    //        dataTable.Rows.Add(values);
    //    }

    //    return dataTable;
    //}

    private DataTable ToDataTable<T>(List<T> items)
    {
        DataTable dataTable = new DataTable();

        if (items == null || !items.Any())
            return dataTable;

        // Dictionary<string, object> support
        if (items.First() is Dictionary<string, object>)
        {
            var dictItems = items
                .Cast<Dictionary<string, object>>()
                .ToList();

            // CREATE COLUMNS
            foreach (var key in dictItems.First().Keys)
            {
                var value = dictItems.First()[key];

                Type columnType = (value == null || value == DBNull.Value)
                    ? typeof(object)
                    : value.GetType();

                dataTable.Columns.Add(key, columnType);
            }

            // CREATE ROWS
            foreach (var dict in dictItems)
            {
                var row = dataTable.NewRow();

                foreach (var key in dict.Keys)
                {
                    row[key] = dict[key] ?? DBNull.Value;
                }

                dataTable.Rows.Add(row);
            }

            return dataTable;
        }

        // Strongly typed objects
        PropertyInfo[] properties = typeof(T).GetProperties();

        foreach (PropertyInfo prop in properties)
        {
            Type colType = Nullable.GetUnderlyingType(prop.PropertyType)
                           ?? prop.PropertyType;

            dataTable.Columns.Add(prop.Name, colType);
        }

        foreach (T item in items)
        {
            DataRow row = dataTable.NewRow();

            foreach (PropertyInfo prop in properties)
            {
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            }

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }
}
