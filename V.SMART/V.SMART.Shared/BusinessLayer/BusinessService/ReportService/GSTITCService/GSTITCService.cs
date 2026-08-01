
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System.Data;
using System.Globalization;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IGSTITC;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.ReportViewModel.GSTITCVM;


namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.GSTITC
{
    public class GSTITCService : IGSTITCService
    {
        private readonly IReportExecutor _report;
        private readonly IPathProvider _pathProvider;
        private readonly IUnitOfWork _unitOfWork;

        public GSTITCService(IReportExecutor report, IPathProvider pathProvider, IUnitOfWork unitOfWork)
        {
            this._report = report;
            this._pathProvider = pathProvider;
            this._unitOfWork = unitOfWork;
        }

        public async Task<List<ITC04OutwardVM>> GetITC04OutwardDetailsAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return await (
                from dc in _unitOfWork.SubConDCOuts.GetQueryable()

                where dc.DcDate >= fromDate.Date
                   && dc.DcDate <= toDate.Date

                join dcs in _unitOfWork.SubConDCOutSubs.GetQueryable()
                    on dc.DcId equals dcs.DcId

                join i in _unitOfWork.ItemRepositories.GetQueryable()
                    on dcs.ItemId equals i.ItemId

                join v in _unitOfWork.Vendors.GetQueryable()
                    on dc.VendorCode equals v.VendorCode

                group new { dcs } by new
                {
                    v.GSTNo,
                    v.VendorName,
                    v.StateName,
                    dc.DcNo,
                    dc.Suffix,
                    dc.DcDate,
                    i.ItemCode,
                    i.ItemName,
                    i.MeasureUnit,
                    i.IGST,
                    i.CGST,
                    i.SGST
                }
                into g

                select new ITC04OutwardVM
                {
                    GSTNo = g.Key.GSTNo,
                    VendorName = g.Key.VendorName,
                    State = g.Key.StateName,

                    Type = "SUBCONTRACT",

                    DcNo = g.Key.DcNo + " " + g.Key.Suffix,

                    DcDate = g.Key.DcDate.ToString("dd-MM-yyyy"),

                    ItemCode = g.Key.ItemCode,
                    ItemName = g.Key.ItemName,
                    MeasureUnit = g.Key.MeasureUnit,

                    Qty = g.Sum(x => x.dcs.Qty),

                    TaxAmt = g.Sum(x => (x.dcs.UnitPrice) * (x.dcs.Qty)),

                    IGST = g.Key.IGST,
                    CGST = g.Key.CGST,
                    SGST = g.Key.SGST
                }
            ).ToListAsync();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<ITC04InwardVM>> GetITC04InwardDetailsAsync(DateTime fromDate, DateTime toDate)
        {
            return await (
                from dc in _unitOfWork.SubConGRNs.GetQueryable()

                where dc.GRNDate >= fromDate.Date
                   && dc.GRNDate <= toDate.Date

                join sub in _unitOfWork.SubConGRNSubs.GetQueryable()
                    on dc.GRNId equals sub.GRNId

                join v in _unitOfWork.Vendors.GetQueryable()
                    on dc.VendorCode equals v.VendorCode

                join i in _unitOfWork.ItemRepositories.GetQueryable()
                    on sub.ItemId equals i.ItemId

                join dos in _unitOfWork.SubConDCOutSubs.GetQueryable()
                    on sub.RefDcSubId equals dos.DcSubId into dosJoin
                from dos in dosJoin.DefaultIfEmpty()

                join o in _unitOfWork.SubConDCOuts.GetQueryable()
                    on dos.DcId equals o.DcId into oJoin
                from o in oJoin.DefaultIfEmpty()

                group new { sub } by new
                {
                    v.GSTNo,
                    v.VendorName,
                    v.StateName,

                    RefDcNo = o != null
                        ? o.DcNo + " " + o.Suffix
                        : "",

                    RefDcDate = dc.GRNDate,

                    i.ItemCode,
                    i.ItemName,
                    i.MeasureUnit,

                    i.IGST,
                    i.CGST,
                    i.SGST
                }
                into g

                select new ITC04InwardVM
                {
                    GSTNo = g.Key.GSTNo,
                    VendorName = g.Key.VendorName,
                    State = g.Key.StateName,

                    RefDcNo = g.Key.RefDcNo,
                    RefDcDate = g.Key.RefDcDate,

                    ItemCode = g.Key.ItemCode,
                    ItemName = g.Key.ItemName,
                    MeasureUnit = g.Key.MeasureUnit,

                    Qty = g.Sum(x => x.sub.Qty ?? 0m),

                    TaxAmt = g.Sum(x =>
                        x.sub.UnitPrice * (x.sub.Qty ?? 0m)),

                    IGST = g.Key.IGST,
                    CGST = g.Key.CGST,
                    SGST = g.Key.SGST
                }
            ).ToListAsync();
        }

        public async Task<byte[]> ExportITCReport(DateTime fromDate, DateTime toDate, int reportType)
        {
            ExcelPackage.License.SetNonCommercialOrganization("Bhargavi Soft Tech");

            string templatePath = Path.Combine(_pathProvider.GetReportTemplatePath(), "ITC_04_Offline_Utility.xlsx");

            if (!File.Exists(templatePath))
            {
                throw new Exception($"Template file not found : {templatePath}");
            }

            using var package = new ExcelPackage(new FileInfo(templatePath));

            var ws = package.Workbook.Worksheets
                .FirstOrDefault(x =>
                    x.Name.Trim().Equals("Table 4",
                    StringComparison.OrdinalIgnoreCase));

            if (ws == null)
            {
                throw new Exception("Worksheet 'Table 4' not found in template.");
            }

            var outwardList = await (
            from dc in _unitOfWork.SubConDCOuts.GetQueryable()

            where dc.DcDate >= fromDate.Date
               && dc.DcDate <= toDate.Date

            join dcs in _unitOfWork.SubConDCOutSubs.GetQueryable()
                on dc.DcId equals dcs.DcId

            join i in _unitOfWork.ItemRepositories.GetQueryable()
                on dcs.ItemId equals i.ItemId

            join v in _unitOfWork.Vendors.GetQueryable()
                on dc.VendorCode equals v.VendorCode

            group new { dcs } by new
            {
                v.GSTNo,
                v.VendorName,
                v.StateName,
                dc.DcNo,
                dc.Suffix,
                dc.DcDate,
                i.ItemCode,
                i.ItemName,
                i.MeasureUnit,
                i.IGST,
                i.CGST,
                i.SGST
            }
            into g

            select new ITC04OutwardVM
            {
                GSTNo = g.Key.GSTNo,
                VendorName = g.Key.VendorName,
                State = g.Key.StateName,

                Type = "SUBCONTRACT",

                DcNo = g.Key.DcNo + " " + g.Key.Suffix,

                DcDate = g.Key.DcDate.ToString("dd-MM-yyyy"),

                ItemCode = g.Key.ItemCode,
                ItemName = g.Key.ItemName,
                MeasureUnit = g.Key.MeasureUnit,

                Qty = g.Sum(x => x.dcs.Qty),

                TaxAmt = g.Sum(x => (x.dcs.UnitPrice) * (x.dcs.Qty)),

                IGST = g.Key.IGST,
                CGST = g.Key.CGST,
                SGST = g.Key.SGST
            }
        ).ToListAsync();

            if (!outwardList.Any())
            {
                return Array.Empty<byte>();
            }

            int rowNo = 7;

            foreach (var dr in outwardList)
            {
                decimal taxableValue = Convert.ToDecimal(dr.TaxAmt);

                decimal igstRate = Convert.ToDecimal(dr.IGST);
                decimal cgstRate = Convert.ToDecimal(dr.CGST);
                decimal sgstRate = Convert.ToDecimal(dr.SGST);

                decimal igstAmt = taxableValue * igstRate / 100;
                decimal cgstAmt = taxableValue * cgstRate / 100;
                decimal sgstAmt = taxableValue * sgstRate / 100;

                // Column A - GSTIN of Job Worker
                ws.Cells[rowNo, 2].Value = dr.GSTNo;

                // Column B - Supplier
                ws.Cells[rowNo, 3].Value = dr.VendorName;

                // Column C - State
                ws.Cells[rowNo, 4].Value = dr.State;

                // Column D - Job Worker Type
                ws.Cells[rowNo, 5].Value = dr.Type;

                // Column E - Challan Number
                ws.Cells[rowNo, 6].Value = dr.DcNo;

                // Column F - Challan Date
                if (!string.IsNullOrWhiteSpace(dr.DcDate))
                {
                    if (DateTime.TryParseExact(
                            dr.DcDate,
                            "dd-MM-yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime dcDate))
                    {
                        ws.Cells[rowNo, 7].Value = dcDate;
                        ws.Cells[rowNo, 7].Style.Numberformat.Format = "dd-MM-yyyy";
                    }
                }

                // Column G - Types of Goods
                ws.Cells[rowNo, 8].Value = "Inputs";

                // Column H - Item Code
                ws.Cells[rowNo, 9].Value = dr.ItemCode;

                // Column I - Description of Goods
                ws.Cells[rowNo, 10].Value = dr.ItemName;

                // Column J - UQC
                ws.Cells[rowNo, 11].Value = dr.MeasureUnit;

                // Column K - Quantity
                ws.Cells[rowNo, 12].Value = dr.Qty;

                // Column L - Taxable Value
                ws.Cells[rowNo, 13].Value = taxableValue;

                // Column M - IGST %
                ws.Cells[rowNo, 14].Value = igstRate;

                // Column N - IGST Amount
                ws.Cells[rowNo, 15].Value = igstAmt;

                // Column O - CGST %
                ws.Cells[rowNo, 16].Value = cgstRate;

                // Column P - CGST Amount
                ws.Cells[rowNo, 17].Value = cgstAmt;

                // Column Q - SGST %
                ws.Cells[rowNo, 18].Value = sgstRate;

                // Column R - SGST Amount
                ws.Cells[rowNo, 19].Value = sgstAmt;
                rowNo++;

            }

            var ws5 = package.Workbook.Worksheets
                .FirstOrDefault(x =>
                    x.Name.Trim().Equals("Table 5A",
                    StringComparison.OrdinalIgnoreCase));

            if (ws5 == null)
            {
                throw new Exception("Worksheet 'Table 5A' not found in template.");
            }

            var inwardList = await (
            from dc in _unitOfWork.SubConGRNs.GetQueryable()

            where dc.GRNDate >= fromDate.Date
               && dc.GRNDate <= toDate.Date

            join sub in _unitOfWork.SubConGRNSubs.GetQueryable()
                on dc.GRNId equals sub.GRNId

            join v in _unitOfWork.Vendors.GetQueryable()
                on dc.VendorCode equals v.VendorCode

            join i in _unitOfWork.ItemRepositories.GetQueryable()
                on sub.ItemId equals i.ItemId

            join dos in _unitOfWork.SubConDCOutSubs.GetQueryable()
                on sub.RefDcSubId equals dos.DcSubId into dosJoin
            from dos in dosJoin.DefaultIfEmpty()

            join o in _unitOfWork.SubConDCOuts.GetQueryable()
                on dos.DcId equals o.DcId into oJoin
            from o in oJoin.DefaultIfEmpty()

            group new { sub } by new
            {
                v.GSTNo,
                v.VendorName,
                v.StateName,
                RefDcNo = o != null ? o.DcNo + " " + o.Suffix : "",
                RefDcDate = dc.GRNDate,
                i.ItemCode,
                i.ItemName,
                i.MeasureUnit,
                i.IGST,
                i.CGST,
                i.SGST
            }
            into g

            select new ITC04InwardVM
            {
                GSTNo = g.Key.GSTNo,
                VendorName = g.Key.VendorName,
                State = g.Key.StateName,

                RefDcNo = g.Key.RefDcNo,
                RefDcDate = g.Key.RefDcDate,

                ItemCode = g.Key.ItemCode,
                ItemName = g.Key.ItemName,
                MeasureUnit = g.Key.MeasureUnit,

                Qty = g.Sum(x => x.sub.Qty ?? 0m),

                TaxAmt = g.Sum(x =>
                    (x.sub.UnitPrice) * (x.sub.Qty ?? 0m)),

                IGST = g.Key.IGST,
                CGST = g.Key.CGST,
                SGST = g.Key.SGST
            })
            .ToListAsync();

            rowNo = 7;

            rowNo = 7;

            foreach (var dr in inwardList)
            {
                ws5.Cells[rowNo, 2].Value = dr.GSTNo;          // B
                ws5.Cells[rowNo, 3].Value = dr.VendorName;     // C
                ws5.Cells[rowNo, 4].Value = dr.State;          // D

                // Original Challan No
                ws5.Cells[rowNo, 5].Value = dr.RefDcNo;        // E

                // Original Challan Date
                if (dr.RefDcDate.HasValue)
                {
                    ws5.Cells[rowNo, 6].Value = dr.RefDcDate.Value;
                    ws5.Cells[rowNo, 6].Style.Numberformat.Format = "dd-MM-yyyy";
                }

                // Challan issued by Job Worker
                ws5.Cells[rowNo, 7].Value = "";                // G
                ws5.Cells[rowNo, 8].Value = "";                // H

                // Nature of Job Work
                ws5.Cells[rowNo, 9].Value = "Sub Contract";    // I

                // Description of Goods
                ws5.Cells[rowNo, 10].Value = dr.ItemName;      // J

                // UQC
                ws5.Cells[rowNo, 11].Value = dr.MeasureUnit;   // K

                // Quantity
                ws5.Cells[rowNo, 12].Value = dr.Qty;           // L

                rowNo++;
            }

            return package.GetAsByteArray();
        }
    }
}



