using DocumentFormat.OpenXml.Bibliography;
using FastReport;
using FastReport.Data;
using FastReport.Export.PdfSimple;
using FastReport.Table;
using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Services.MultiCompanyService;

namespace V.SMART.Shared.Services.ReportViewer
{
    public class ReportService
    {
        private readonly string _reportFolder;
        private readonly ILoggingService _logger;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICommonService _commonService;
        private readonly ISalaryHeadPrintSettingService _salaryHeadPrintSettingService;


        public ReportService(ITenantProvider tenantProvider,ICommonService common, IPathProvider pathProvider,ILoggingService logger, ISalaryHeadPrintSettingService salaryHeadPrintSettingService)
        {
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
            _commonService = common ?? throw new ArgumentNullException(nameof(common));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _salaryHeadPrintSettingService = salaryHeadPrintSettingService;
            // ✅ IMPORTANT: Assign report folder
            _reportFolder = pathProvider?.GetReportTemplatePath()
                ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public async Task<byte[]> Generate_Report(int id,string fileName,string parameter, bool cancel, string screenName,string procedureName)
        {
            try
            {
                // Tenant Validation
                var tenant = _tenantProvider.GetCurrentTenant();
                if (tenant == null || string.IsNullOrWhiteSpace(tenant.ConnectionString))
                    throw new InvalidOperationException("Tenant connection is not configured.");

                // File Path Check
                string tenantFolder = tenant.Hostname ?? "default";
                string frxFile = Path.Combine(_reportFolder, tenantFolder, fileName);

                if (!File.Exists(frxFile))
                {
                    frxFile = Path.Combine(_reportFolder, "default", fileName);

                    if (!File.Exists(frxFile))
                        throw new InvalidOperationException($"Report template '{fileName}' does not exist in tenant or default folder.");
                }

                // Register FastReport Connection
                RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));

                using Report report = new Report();
                report.Load(frxFile);

                // DB Connection Check
                if (report.Dictionary.Connections.Count == 0)
                    throw new InvalidOperationException("Database connection is missing in report template.");

                report.Dictionary.Connections[0].ConnectionString = tenant.ConnectionString;

                // Company Details Check  
                if (report.GetDataSource("Sp_Print_CompanyDetails") is not TableDataSource dsCompany)
                    throw new InvalidOperationException("Company Details data source is not available in report.");

                dsCompany.SelectCommand = "Sp_Print_CompanyDetails";
                dsCompany.Parameters.Clear();

                //Stored Procedure Check
                if (report.GetDataSource(procedureName) is not TableDataSource dsMain)
                    throw new InvalidOperationException($"Stored procedure '{procedureName}' is not mapped in report.");

                dsMain.SelectCommand = procedureName;
                dsMain.Parameters.Clear();

                dsMain.Parameters.Add(new CommandParameter()
                {
                    Name = $"@{parameter}",
                    DataType = (int)DbType.String,
                    Value = id.ToString()
                });

                using var ms = new MemoryStream();

                // Print Settings Check
                var printSettings = await _commonService.GetPrintSettingsDetailsAsync(screenName);

                var page = report.Pages.OfType<ReportPage>().FirstOrDefault();

                if (printSettings != null && printSettings.Count > 0 && page != null)
                {
                    int totalSettings = printSettings.Count;

                    foreach (var setting in printSettings)
                    {
                        bool active = Convert.ToBoolean(setting.IsActive);
                        bool watermark = Convert.ToBoolean(setting.WatermarkRequired);
                        bool logoRequired = Convert.ToBoolean(setting.LogoRequired);
                        bool isoLogoRequired = Convert.ToBoolean(setting.IsoLogoRequired);
                        int copies = Convert.ToInt32(setting.NoOfCopies);
                        copies = copies == 0 ? 1 : copies;

                        if (!active) continue;

                        for (int i = 0; i < copies; i++)
                        {


                            report.SetParameterValue("CopyName",
                                totalSettings == 1 ? "" : setting.CopyName);

                            // Watermark
                            if (cancel)
                            {
                                page.Watermark.Enabled = true;
                                page.Watermark.Text = "CANCELLED";
                            }
                            else if (watermark)
                            {
                                page.Watermark.Enabled = true;
                                page.Watermark.Text = setting.WatermarkLable?.ToUpper();
                            }
                            else
                            {
                                page.Watermark.Enabled = false;
                            }

                            page.Watermark.Font = new Font("Times New Roman", 72, FontStyle.Bold);

                            // Logos
                            if (report.FindObject("Logo") is FastReport.PictureObject logo)
                                logo.Visible = logoRequired;

                            if (report.FindObject("IsoLogo") is FastReport.PictureObject isoLogo)
                                isoLogo.Visible = isoLogoRequired;

                            report.Prepare(true);
                        }
                    }
                }
                else
                {
                    report.Prepare(true);
                }

                report.Export(new PDFSimpleExport(), ms);
                return ms.ToArray();
            }
            catch (Exception ex)
            {
                await _logger.LogDeveloperError(ex, "Error in ReportService.Generate_Report()");
                throw;
            }
        }



        public async Task<byte[]> GenerateSalarySlipReport(int id, string fileName, string parameter, bool cancel, string screenName, string procedureName)
        {
            try
            {
                var tenant = _tenantProvider.GetCurrentTenant();

                if (tenant == null || string.IsNullOrWhiteSpace(tenant.ConnectionString))
                    throw new InvalidOperationException("Tenant connection is not configured.");

                string tenantFolder = tenant.Hostname ?? "default";

                string frxFile = Path.Combine(_reportFolder, tenantFolder, fileName);

                if (!File.Exists(frxFile))
                {
                    frxFile = Path.Combine(_reportFolder, "default", fileName);

                    if (!File.Exists(frxFile))
                        throw new Exception($"Report {fileName} not found.");
                }

                RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));

                using Report report = new Report();

                report.Load(frxFile);

                report.Dictionary.Connections[0].ConnectionString =
                    tenant.ConnectionString;

                var dsCompany =
                    report.GetDataSource("Sp_Print_CompanyDetails") as TableDataSource;

                dsCompany.SelectCommand = "Sp_Print_CompanyDetails";

                var dsMain =
                    report.GetDataSource(procedureName) as TableDataSource;

                dsMain.SelectCommand = procedureName;

                dsMain.Parameters.Clear();

                dsMain.Parameters.Add(new CommandParameter()
                {
                    Name = $"@{parameter}",
                    DataType = (int)DbType.String,
                    Value = id.ToString()
                });

                //--------------------------------------------------
                // SALARY HEAD SETTINGS
                //--------------------------------------------------

                var salaryHeads =
                    await _salaryHeadPrintSettingService
                        .GetAllSalaryPrintHeadSettingsAsync();

                foreach (var item in salaryHeads)
                {
                    string fieldName = item.FieldName
                        .Replace(" ", "")
                        .Replace(".", "")
                        .Replace("-", "");

                    // Change label
                    var lbl = report.FindObject("txt" + fieldName) as FastReport.TextObject;
                    if (lbl != null)
                    {
                        lbl.Text = item.PrintName;
                    }

                    // Get table
                    var table = report.FindObject("tblEarnings") as TableObject;

                    if (table != null)
                    {
                        foreach (TableRow row in table.Rows)
                        {
                            if (row.Name == "row" + fieldName)
                            {
                                row.Height = item.IsPrint ? row.Height : 0;
                                break;
                            }
                        }
                    }

                    // ---------------- Deduction Table ----------------
                    var deductionTable = report.FindObject("tblDeductions") as TableObject;

                    if (deductionTable != null)
                    {
                        foreach (TableRow row in deductionTable.Rows)
                        {
                            if (row.Name == "row" + fieldName)
                            {
                                row.Height = item.IsPrint ? row.Height : 0;
                                break;
                            }
                        }
                    }
                }

                //--------------------------------------------------

                report.Prepare();

                using MemoryStream ms = new();

                report.Export(new PDFSimpleExport(), ms);

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                await _logger.LogDeveloperError(
                    ex,
                    "Error in GenerateSalarySlipReport()");

                throw;
            }
        }


        public async Task<byte[]> Generate_Attendance_Report(int year, int monthId, int weekNo, string fileName, string procedureName)
        {
            try
            {
                var tenant = _tenantProvider.GetCurrentTenant();

                if (tenant == null || string.IsNullOrWhiteSpace(tenant.ConnectionString))
                    throw new InvalidOperationException("Tenant connection is not configured.");

                string tenantFolder = tenant.Hostname ?? "default";

                string frxFile = Path.Combine(_reportFolder, tenantFolder, fileName);

                if (!File.Exists(frxFile))
                {
                    frxFile = Path.Combine(_reportFolder, "default", fileName);

                    if (!File.Exists(frxFile))
                        throw new InvalidOperationException(
                            $"Report template '{fileName}' does not exist.");
                }

                RegisteredObjects.AddConnection(typeof(MsSqlDataConnection));

                using Report report = new();

                report.Load(frxFile);

                if (report.Dictionary.Connections.Count == 0)
                    throw new InvalidOperationException(
                        "Database connection is missing in report template.");

                report.Dictionary.Connections[0].ConnectionString =
                    tenant.ConnectionString;

                // Company Details
                if (report.GetDataSource("Sp_Print_CompanyDetails")
                    is TableDataSource dsCompany)
                {
                    dsCompany.SelectCommand = "Sp_Print_CompanyDetails";
                    dsCompany.Parameters.Clear();
                }

                // Attendance Datasource
                if (report.GetDataSource(procedureName)
                    is not TableDataSource dsAttendance)
                {
                    throw new InvalidOperationException(
                        $"Stored procedure '{procedureName}' is not mapped in report.");
                }

                dsAttendance.SelectCommand = procedureName;

                foreach (CommandParameter p in dsAttendance.Parameters)
                {
                    if (p.Name == "@Year")
                        p.Value = year;

                    else if (p.Name == "@MonthId")
                        p.Value = monthId;

                    else if (p.Name == "@WeekNo")
                        p.Value = weekNo;
                }

                report.Dictionary.UpdateRelations();

                using var ms = new MemoryStream();

                report.Prepare();

                report.Export(new PDFSimpleExport(), ms);

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                await _logger.LogDeveloperError(ex, "Error in ReportService.Generate_Attendance_Report()");
                throw;
            }
        }


    }

}
