using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IEInvoiceAPI;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.E_Invoice;
using V.SMART.Shared.E_Invoice.E_InvoiceHelper;
using V.SMART.Shared.E_Invoice.EwayHelper;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.Extensions;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZXing;
using static V.SMART.Shared.E_Invoice.Enums;

namespace V.SMART.Shared.BusinessLayer.BusinessService.EInvoiceAPIService
{
    public class EWayDatabaseService: IEWayDatabaseService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _logs;
        

        public EWayDatabaseService(IUnitOfWork unitOfWork ,ILoggingService logs)
        {
            _unitOfWork = unitOfWork;
            _logs = logs;
           
        }

        public async Task GetRequiredDetailsFromDb(string DcNo, string DcType, bool isPreView)
        {
            try
            {
                var productKey = await GetProductKeyAsync();

                if (string.IsNullOrWhiteSpace(productKey))
                    throw new InvalidOperationException("Your API product key is empty. Please contact ERP Admin.");

                var license = new LicenseProductKey(productKey);

                var auth = await GetUserNameandPassword(license);

                if (Enum.TryParse(DcType, out EwayType type))
                {
                    await GenerateEwayBillNo(DcNo, type, auth.UserName, auth.Password);
                }
                else
                {
                    throw new InvalidOperationException("Invalid DcType. Please Contact Vendor.");
                }
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch
            {
                throw; // preserves original stack trace
            }
        }

        public async Task<string?> GetProductKeyAsync()
        {
            return await _unitOfWork.Companies
                                    .GetQueryable()
                                    .Select(x => x.APIEinvoiceLicenseKey)
                                    .FirstOrDefaultAsync();
        }


        public async Task<bool> IsValid(DataSet dInvoiceDet)
        {
            var errors = new List<string>();

            DataRow dr = dInvoiceDet.Tables[0].Rows[0];
            DataRow r = dInvoiceDet.Tables[1].Rows[0];
            try
            {
                var validations = new List<(Func<DataRow, bool>, string)>
                {
                    //My Company Details validations
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("LegalName")), "LegalName Should not be Empty in My Company Details...!"),
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("GSTAddress")) && dr.Field<string>("GSTAddress").Trim().Length <= 240, "The Length of Your Company Address Should not be Greater Than or Equal to 240 OR It Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("City")), "Location Should not be Empty in My Company Details...!"),
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("ZipCode")), "Pincode Should not be Empty in My Company Details...!"),
                    ((row) => (dr.Field<int?>("StateCode")!=null), "StateCode Should not be Empty in My Company Details...!"),

                    //CUSTOMER Details validations
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("SubSuppType")), "Customer or Vendor Type Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("CustAddress")) && r.Field<string>("CustAddress").Trim().Length <= 240, "The Length of Customer or Vendor Address Should not be Greater Than or Equal to 240 OR It Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("DocType")), "Eway Document Type Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("CustGstNo")) && r.Field<string>("CustGstNo").Trim().Length == 15, "Customer GSTIN Should Not be Empty OR The Length should be Equal to 15"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("Location")), "Customer or Vendor Location Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("PinCode")), "Customer or Vendor Pincode Should Not be Empty"),
                  
                     ((row) => (r.Field<int?>("StateCode")!=null), "Customer or Vendor StateCode Should Not be Empty"),
                   
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("MeasureUnit")), "Unit of Measurement Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("HSNCode")), "HSNCode Should Not be Empty"),

                    ((row => ValidateHSNCode(r.Field<string>("HSNCode")), "HSNCode must be  6 or 8 digits")),

                    ((row) => !string.IsNullOrEmpty(r.Field<string>("ModeofTrasnport")), "Eway Mode of Transportation Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("VehicleNo")), "Vehicle No. Should Not be Empty")  ,  
                    ((row) => IsValidVehicleNo(r.Field<string>("VehicleNo")),"Please enter valid Vehicle No.")
                };

                var Messages = validations.Select(validation => new { IsValid = validation.Item1(dr), ErrorMessage = validation.Item2 }).Where(validationResult =>
                !validationResult.IsValid).Select(validationResult => validationResult.ErrorMessage).ToList();

                if (errors.Any())
                {
                    var message = string.Join("<br>", errors);
                    throw new InvalidOperationException(message);
                    //return false;
                }
                return true;
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ;
            }
        }

        private bool ValidateHSNCode(string hsn)
        {
            if (string.IsNullOrWhiteSpace(hsn))
                return false;

            if (!(hsn.Length == 6 || hsn.Length == 8))
                return false;

            return hsn.All(char.IsDigit);
        }

        public bool IsValidVehicleNo(string vehicleNo)
        {
            if (string.IsNullOrWhiteSpace(vehicleNo))
                return false;

            vehicleNo = vehicleNo.ToUpper().Replace(" ", "");

            var patterns = new[]
            {
                @"^[A-Z]{2}[0-9]{2}[A-Z]{1,2}[0-9]{4}$",   
                @"^[A-Z]{2}[0-9]{2}[A-Z]{1}[0-9]{4}$",     
                @"^[0-9]{2}BH[0-9]{4}[A-Z]{2}$"            
            };

            return patterns.Any(p => Regex.IsMatch(vehicleNo, p));
        }

        public async Task<AuthEWay> GetUserNameandPassword(ILicenseProductKey licenseProductKey)
        {
            try
            {
                var auth = new AuthEWay();
                if (await licenseProductKey.IsValidProductKey())
                {
                    auth = await licenseProductKey.GetUserNameEway();
                }
                else
                {
                    Environment.Exit(1);
                }

                return auth;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task GenerateEwayBillNo(string DcNo, Enum ewayType, string username, string password)
        {
            try
            {
                DataSet dinvoiceDet = await UpdateDcDetails(DcNo, ewayType);

                if (await IsValid(dinvoiceDet))
                {

                    IAPIService eWayPIService = APIFactory.GetAPIService("EWay", await GetEwayJsonData(dinvoiceDet, ewayType, DcNo));
                    var ewayResponse = await eWayPIService.GetResponseByJsonData(new AuthenticationDetails(await GetUserNameandPassword(username, password), await GetGstIn(dinvoiceDet))).ConfigureAwait(true);

                    var docList = DcNo.Split(',').Select(s => s.Trim()).ToList();


                    switch (ewayType)
                    {
                        case EwayType.MfgDc:
                            var Mfgrecords = await _unitOfWork.MfgDcs.GetQueryable().Where(x => docList.Contains(x.DcNo + x.Suffix)).ToArrayAsync();
                            foreach (var item in Mfgrecords)
                            {
                                item.EwayBillNo = ewayResponse.ewayBillNo;
                                item.EwayBillDate = Convert.ToDateTime(ewayResponse.ewayBillDate);
                                await _unitOfWork.MfgDcs.UpdateAsync(item);
                            }
                             await _unitOfWork.SaveAsync();
                            //_js.ToastrSuccess("E-Way Bill Successfully Updated, BHARGAVI SOFT-TECH PVT LTD");
                            break;
                        case EwayType.MfgInv:
                            var MfgInvrecords = await _unitOfWork.MfgInvs.GetQueryable().Where(x => docList.Contains(x.InvNo+x.Suffix)).ToArrayAsync();
                            foreach (var item in MfgInvrecords)
                            {
                                item.EWayNo = ewayResponse.ewayBillNo;
                                item.EWayDate = Convert.ToDateTime(ewayResponse.ewayBillDate);
                                await _unitOfWork.MfgInvs.UpdateAsync(item);
                            }
                            await _unitOfWork.SaveAsync();
                            break;


                        case EwayType.LabourDc:
                            var Labrecords = await _unitOfWork.LabourDcOutgoings.GetQueryable().Where(x => docList.Contains(x.DcNo + x.Suffix)).ToArrayAsync();
                            foreach (var item in Labrecords)
                            {
                                item.EwayNo = ewayResponse.ewayBillNo;
                                item.EwayDate = Convert.ToDateTime(ewayResponse.ewayBillDate);
                                 await _unitOfWork.LabourDcOutgoings.UpdateAsync(item);
                            }
                             await _unitOfWork.SaveAsync();
                           // await _js.ToastrSuccess("E-Way Bill Successfully Updated, BHARGAVI SOFT-TECH PVT LTD");
                            break;
                        
                        case EwayType.SubConDc:
                            var Subrecords = await _unitOfWork.SubConDCOuts.GetQueryable().Where(x => docList.Contains(x.DcNo + x.Suffix)).ToArrayAsync();
                            foreach (var item in Subrecords)
                            {
                                item.EwayBillNumber = ewayResponse.ewayBillNo;
                                item.EwayBillDate = Convert.ToDateTime(ewayResponse.ewayBillDate);
                                await _unitOfWork.SubConDCOuts.UpdateAsync(item);
                            }
                             await _unitOfWork.SaveAsync();
                            //await _js.ToastrSuccess("E-Way Bill Successfully Updated, BHARGAVI SOFT-TECH PVT LTD");
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<DataSet> UpdateDcDetails(string DcNo, Enum DcType)
        {
            try
            {
                DataSet dinvoiceDet = new DataSet();
                var company =  await _unitOfWork.Companies
                            .GetQueryable()
                            .Select(x => new Companydetails
                            {
                                LegalName = x.LegalName,
                                GSTAddress = x.GSTAddress,
                                City = x.City,
                                ZipCode = x.ZipCode,
                                StateCode = x.StateCode,
                                GSTIN = x.GSTIN
                            })
                            .FirstOrDefaultAsync();

                if (company != null)
                {
                    var dt = EWayDatabaseService.ToDataTable(
                        new List<Companydetails> { company });

                    dinvoiceDet.Tables.Add(dt);
                }

                switch (DcType)
                {
                    case EwayType.MfgDc:

                        var mfgResult = await GetMfgDcByDcNos(DcNo);
                        var mfgdt = EWayDatabaseService.ToDataTable<EWayDocument>((IEnumerable<EWayDocument>)mfgResult);
                        dinvoiceDet.Tables.Add(mfgdt);
                        break;
                    case EwayType.MfgInv:

                        var mfgInvResult = await GetMfgInvoiceByInvNos(DcNo);
                        var mfginvdt = EWayDatabaseService.ToDataTable<EWayDocument>((IEnumerable<EWayDocument>)mfgInvResult);
                        dinvoiceDet.Tables.Add(mfginvdt);
                        break;

                    case EwayType.LabourDc:
                        var labResult = await GetLabDcByDcNos(DcNo);
                        var labdt = EWayDatabaseService.ToDataTable<EWayDocument>((IEnumerable<EWayDocument>)labResult);
                        dinvoiceDet.Tables.Add(labdt);
                        break;
                   
                    case EwayType.SubConDc:
                        
                         var subconResult = await GetSubConDcByDcNos(DcNo);
                        var subcondt = EWayDatabaseService.ToDataTable<EWayDocument>((IEnumerable<EWayDocument>)subconResult);
                        dinvoiceDet.Tables.Add(subcondt);
                        break;
                    default:
                        break;
                }
                return dinvoiceDet;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable ToDataTable<T>(IEnumerable<T> data)
        {
            var dt = new DataTable(typeof(T).Name);

            if (data == null || !data.Any())
                return dt;

            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                dt.Columns.Add(prop.Name, type);
            }

            foreach (var item in data)
            {
                var row = dt.NewRow();

                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }

                dt.Rows.Add(row);
            }

            return dt;
        }

        private  async Task<string> GetEwayJsonData(DataSet dInvoiceDet, Enum EwayType, string Invnos)
        {
            try
            {
                IMainTaxCalculation mainTaxCalculation = new MainTaxCalculation();
                EwayJsonSerializer ewayJsonSerializer = new EwayJsonSerializer();
                DataRow dr = dInvoiceDet.Tables[0].Rows[0];
                DataRow row = dInvoiceDet.Tables[1].Rows[0];

                decimal Igst = 0, Cgst = 0, Sgst = 0, OtherCharges = 0, overallsubtot3 = 0, subtot3 = 0, cgstval = 0, sgstval = 0, igstval = 0, totcgstval = 0, totsgstval = 0, totigstval = 0,
                       overallTotAmt = 0, overallCgstAmt = 0, overallSgstAmt = 0, overallIgstAmt = 0, totalVal = 0;

                //OtherCharges = dInvoiceDet.Tables[1].AsEnumerable().GroupBy(rw => rw["id"]).Select(g => g.First()).Sum(rw => Math.Round(double.TryParse((rw["OtherCharges"]).ToString(), out var oth), 2));
                OtherCharges = dInvoiceDet.Tables[1].AsEnumerable().GroupBy(rw => rw["DcId"]).Select(g => g.First()).Sum(rw =>
                {
                    if (decimal.TryParse(rw["OtherCharges"].ToString(), out var otherCharges))
                    {
                        return Math.Round(otherCharges, 2);
                    }
                    return 0;
                });

                var TotInvNos = Invnos.Split(',');
                if (TotInvNos.Length == 1)
                {
                    EwayCalculations_For_Single_DocNo(dInvoiceDet, mainTaxCalculation, dr, row, out Igst, out Cgst, out Sgst, out subtot3, out cgstval, out sgstval, out igstval, out totcgstval, out totsgstval, out totigstval, out totalVal, TotInvNos);
                    await GetEwayDtls(ewayJsonSerializer, dr, row, OtherCharges, subtot3, totcgstval, totsgstval, totigstval, totalVal, Invnos, _unitOfWork);
                }
                else // for Calculations of Multiple Invoice's Mainly 
                {
                    EwayCalculations_For_Multi_DocNo(dInvoiceDet, mainTaxCalculation, dr, ref Igst, ref Cgst, ref Sgst, ref OtherCharges, ref overallsubtot3, ref subtot3, ref cgstval, ref sgstval, ref igstval, ref totcgstval, ref totsgstval, ref totigstval, ref overallTotAmt, ref overallCgstAmt, ref overallSgstAmt, ref overallIgstAmt, ref totalVal, TotInvNos);
                    await GetEwayDtls(ewayJsonSerializer, dr, row, OtherCharges, overallsubtot3, overallCgstAmt, overallSgstAmt, overallIgstAmt, overallTotAmt, Invnos, _unitOfWork);
                }

               await GetEwayItemDtls(dInvoiceDet, ewayJsonSerializer, Igst, Cgst, Sgst); // Item Details

                
                return Newtonsoft.Json.JsonConvert.SerializeObject(ewayJsonSerializer, Newtonsoft.Json.Formatting.Indented);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void EwayCalculations_For_Multi_DocNo(DataSet dInvoiceDet, IMainTaxCalculation mainTaxCalculation, DataRow dr, ref decimal Igst, ref decimal Cgst, ref decimal Sgst, ref decimal OtherCharges, ref decimal overallsubtot3, ref decimal subtot3, ref decimal cgstval, ref decimal sgstval, ref decimal igstval, ref decimal totcgstval, ref decimal totsgstval, ref decimal totigstval, ref decimal overallTotAmt, ref decimal overallCgstAmt, ref decimal overallSgstAmt, ref decimal overallIgstAmt, ref decimal totalVal, string[] TotInvNos)
        {
            try
            {
                long dcinvid = 0;
                foreach (DataRow row in dInvoiceDet.Tables[1].Rows)
                {
                    if (row["DcId"].ToString() != dcinvid.ToString())
                    {
                        decimal FreightAmt, PandF, Insurance, TCS, totalBasicAmt;
                        string PandAmtorPer, InsAmtorPer, TcsAmtorPer;
                        long.TryParse(row["DcId"].ToString(), out dcinvid);
                        ExtractingValues(dInvoiceDet, dr, out Igst, out Cgst, out Sgst, row, out FreightAmt, out PandF, out Insurance, out TCS, out totalBasicAmt, out PandAmtorPer, out InsAmtorPer, out TcsAmtorPer, TotInvNos);
                        cgstval = 0; sgstval = 0; igstval = 0; decimal TcsAmt = 0; totalVal = 0;

                        if (Cgst > 0 || Igst > 0)
                        {
                            overallsubtot3 = MainTaxCal_For_MultiDocNo(mainTaxCalculation, Igst, Cgst, Sgst, overallsubtot3, out subtot3, out totcgstval, out totsgstval, out totigstval, out totalVal, FreightAmt, PandF, Insurance, TCS, totalBasicAmt, PandAmtorPer, InsAmtorPer, TcsAmtorPer, out TcsAmt, TotInvNos);
                        }
                        else
                        {
                            ItemwiseTaxCal_For_MultiDocNo(mainTaxCalculation, dInvoiceDet, ref overallsubtot3, out subtot3, ref cgstval, ref sgstval, ref igstval, out totcgstval, out totsgstval, out totigstval, out totalVal, dcinvid, FreightAmt, PandF, Insurance, TCS, PandAmtorPer, InsAmtorPer, TcsAmtorPer, out TcsAmt);
                        }

                        overallTotAmt = Math.Round(overallTotAmt + totalVal, 2);
                        overallCgstAmt = Math.Round(overallCgstAmt + totcgstval, 2);
                        overallSgstAmt = Math.Round(overallSgstAmt + totsgstval, 2);
                        overallIgstAmt = Math.Round(overallIgstAmt + totigstval, 2);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ItemwiseTaxCal_For_MultiDocNo(IMainTaxCalculation mainTaxCalculation, DataSet dInvoiceDet, ref decimal overallsubtot3, out decimal subtot3, ref decimal cgstval, ref decimal sgstval, ref decimal igstval, out decimal totcgstval, out decimal totsgstval, out decimal totigstval, out decimal totalVal, long dcinvid, decimal FreightAmt, decimal PandF, decimal Insurance, decimal TCS, string PandAmtorPer, string InsAmtorPer, string TcsAmtorPer, out decimal TcsAmt)
        {
            try
            {
                DataRow[] drow = dInvoiceDet.Tables[1].Select($"DcId = '{dcinvid}'");
                decimal totcgst = 0 , totval = 0,  totsgst = 0,  totigst = 0,  totalvalAmt = 0;
                foreach (DataRow row in drow)
                {

                    ItemRowWiseTaxCal(mainTaxCalculation, out cgstval, out sgstval, out igstval, ref totcgst, out totval, ref totsgst, ref totigst, ref totalvalAmt, row);
                }
                ItemRowWiseSubTotCal(mainTaxCalculation, out subtot3, out totcgstval, out totsgstval, out totigstval, out totalVal, totcgst, totsgst, totigst, totalvalAmt, FreightAmt, PandF, Insurance, TCS, PandAmtorPer, InsAmtorPer, out TcsAmt);
                overallsubtot3 = Math.Round(overallsubtot3 + subtot3, 2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
       
        private static void ItemRowWiseSubTotCal(IMainTaxCalculation mainTaxCalculation, out decimal subtot3, out decimal totcgstval, out decimal totsgstval, out decimal totigstval, out decimal totalVal, decimal totcgst, decimal totsgst, decimal totigst, decimal totalvalAmt, decimal FreightAmt, decimal PandF, decimal Insurance, decimal TCS, string PandAmtorPer, string InsAmtorPer, out decimal TcsAmt)
        {
            try
            {
                //commited on 14032026

                //var subtot4 = FreightAmt > 0 ? totalvalAmt + FreightAmt : totalvalAmt;

               // var subtot5 = PandF > 0 ? (PandAmtorPer == "True" ? subtot4 + (subtot4 * PandF / 100) : subtot4 + PandF) : subtot4;

                var subtot5 = PandF > 0 ? (PandAmtorPer == "True" ? (totalvalAmt * PandF / 100) :  PandF) : 0;

                //subtot3 = Math.Round(Insurance > 0 ? (InsAmtorPer == "True" ? subtot5 + (subtot5 * Insurance / 100) : subtot5 + Insurance) : subtot5, 2);

                subtot3 = Math.Round(Insurance > 0 ? (InsAmtorPer == "True" ? (totalvalAmt * Insurance / 100) : Insurance) : 0, 2);


                totcgstval = Math.Round(totcgst, 2);
                totsgstval = Math.Round(totsgst, 2);
                totigstval = Math.Round(totigst, 2);
                totalVal = Math.Round((totalvalAmt + FreightAmt + subtot5 + subtot3 ), 2);
                TcsAmt = mainTaxCalculation.GetTaxAmt(totalVal, TCS);
                totalVal = totalVal + TcsAmt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ItemRowWiseTaxCal(IMainTaxCalculation mainTaxCalculation, out decimal cgstval, out decimal sgstval, out decimal igstval, ref decimal totcgst, out decimal totval, ref decimal totsgst, ref decimal totigst, ref decimal totalvalAmt, DataRow row)
        {
            try
            {
                var itmcgstrate = row["ItemwiseCgst"].ToString().ValueConverter<decimal>();
                var itmsgstrate = row["ItemwiseSgst"].ToString().ValueConverter<decimal>();
                var itmigstrate = row["ItemwiseIgst"].ToString().ValueConverter<decimal>();
                var Totamt = row["TotalAmount"].ToString().ValueConverter<decimal>();
                var itemDiscAmt = row["ItemDiscAmt"].ToString().ValueConverter<decimal>();
                totval = Totamt - itemDiscAmt;

                cgstval = mainTaxCalculation.GetTaxAmt(totval, itmcgstrate);
                sgstval = mainTaxCalculation.GetTaxAmt(totval, itmsgstrate);
                igstval = mainTaxCalculation.GetTaxAmt(totval, itmigstrate);
                var totassval = mainTaxCalculation.GetTotalTaxAmt(totval, igstval, sgstval, cgstval);
                totalvalAmt = totalvalAmt + totassval;
                totcgst = totcgst + cgstval;
                totsgst = totsgst + sgstval;
                totigst = totigst + igstval;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ItemwiseTaxCal_For_SingleDocNo(IMainTaxCalculation mainTaxCalculation, DataSet dInvoiceDet, out decimal subtot3, ref decimal cgstval, ref decimal sgstval, ref decimal igstval, out decimal totcgstval, out decimal totsgstval, out decimal totigstval, out decimal totalVal, decimal FreightAmt, decimal PandF, decimal Insurance, decimal TCS, string PandAmtorPer, string InsAmtorPer, string TcsAmtorPer, out decimal TcsAmt)
        {
            try
            {
                decimal totcgst = 0,  totval = 0,  totsgst = 0,  totigst = 0,  totalvalAmt = 0;
                foreach (DataRow row in dInvoiceDet.Tables[1].Rows)
                {
                    ItemRowWiseTaxCal(mainTaxCalculation, out cgstval, out sgstval, out igstval, ref totcgst, out totval, ref totsgst, ref totigst, ref totalvalAmt, row);
                }
                ItemRowWiseSubTotCal(mainTaxCalculation, out subtot3, out totcgstval, out totsgstval, out totigstval, out totalVal, totcgst, totsgst, totigst, totalvalAmt, FreightAmt, PandF, Insurance, TCS, PandAmtorPer, InsAmtorPer, out TcsAmt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static decimal MainTaxCal_For_MultiDocNo(IMainTaxCalculation mainTaxCalculation, decimal Igst, decimal Cgst, decimal Sgst, decimal overallsubtot3, out decimal subtot3, out decimal totcgstval, out decimal totsgstval, out decimal totigstval, out decimal totalVal, decimal FreightAmt, decimal PandF, decimal Insurance, decimal TCS, decimal totalBasicAmt, string PandAmtorPer, string InsAmtorPer, string TcsAmtorPer, out decimal TcsAmt, string[] TotInvNos)
        {
            try
            {
                mainTaxCal(mainTaxCalculation, Igst, Cgst, Sgst, out subtot3, out totcgstval, out totsgstval, out totigstval, out totalVal, FreightAmt, PandF, Insurance, TCS, totalBasicAmt, PandAmtorPer, InsAmtorPer, out TcsAmt);
                if (TotInvNos.Length > 1)
                {
                    overallsubtot3 = Math.Round(overallsubtot3 + subtot3, 2);
                }
                return overallsubtot3;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void MainTaxCal_For_SingleDocNo(IMainTaxCalculation mainTaxCalculation, decimal Igst, decimal Cgst, decimal Sgst, out decimal subtot3, out decimal totcgstval, out decimal totsgstval, out decimal totigstval, out decimal totalVal, decimal FreightAmt, decimal PandF, decimal Insurance, decimal TCS, decimal totalBasicAmt, string PandAmtorPer, string InsAmtorPer, string TcsAmtorPer, out decimal TcsAmt, string[] TotInvNos)
        {
            try
            {
                mainTaxCal(mainTaxCalculation, Igst, Cgst, Sgst, out subtot3, out totcgstval, out totsgstval, out totigstval, out totalVal, FreightAmt, PandF, Insurance, TCS, totalBasicAmt, PandAmtorPer, InsAmtorPer, out TcsAmt);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void mainTaxCal(IMainTaxCalculation mainTaxCalculation, decimal Igst, decimal Cgst, decimal Sgst, out decimal subtot3, out decimal totcgstval, out decimal totsgstval, out decimal totigstval, out decimal totalVal, decimal FreightAmt, decimal PandF, decimal Insurance, decimal TCS, decimal totalBasicAmt, string PandAmtorPer, string InsAmtorPer, out decimal TcsAmt)
        {
            try
            {
                //commited on 14032026 vivek 

                //var subtot1 = FreightAmt > 0 ? totalBasicAmt + FreightAmt : totalBasicAmt;
                // var subtot2 = PandF > 0 ? (PandAmtorPer == "True" ? subtot1 + (subtot1 * PandF / 100) : subtot1 + PandF) : subtot1;
                //subtot3 = Math.Round(Insurance > 0 ? (InsAmtorPer == "True" ? subtot2 + (subtot2 * Insurance / 100) : subtot2 + Insurance) : subtot2, 2);

                //newly changed code
                var subtot2 = PandF > 0 ? (PandAmtorPer == "True" ? (totalBasicAmt * PandF / 100) : PandF) : 0;
                subtot3 = Math.Round(Insurance > 0 ? (InsAmtorPer == "True" ? (totalBasicAmt * Insurance / 100) :  Insurance) : 0, 2);
                subtot3 = (FreightAmt + subtot2 + subtot3 + totalBasicAmt);

                totcgstval = mainTaxCalculation.GetTaxAmt(subtot3, Cgst);
                totsgstval = mainTaxCalculation.GetTaxAmt(subtot3, Sgst);
                totigstval = mainTaxCalculation.GetTaxAmt(subtot3, Igst);
                totalVal = mainTaxCalculation.GetTotalTaxAmt(subtot3, totigstval, totsgstval, totcgstval);

                TcsAmt = mainTaxCalculation.GetTaxAmt(totalVal, TCS);
                totalVal = Math.Round(totalVal + TcsAmt, 2);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void ExtractingValues(DataSet dInvoiceDet, DataRow dr, out decimal Igst, out decimal Cgst, out decimal Sgst, DataRow row, out decimal FreightAmt, out decimal PandF, out decimal Insurance, out decimal TCS, out decimal totalBasicAmt, out string PandAmtorPer, out string InsAmtorPer, out string TcsAmtorPer, string[] TotInvNos)
        {
            try
            {
                //bool.TryParse(dr["RoffSales"].ToString(), out var RoffSales);
               // bool.TryParse(dr["RoffLabour"].ToString(), out var RoffLabour);
                Igst = row["Igst"].ToString().ValueConverter<decimal>();
                Cgst = row["Cgst"].ToString().ValueConverter<decimal>();
                Sgst = row["Sgst"].ToString().ValueConverter<decimal>();
                var ItemDiscAmt = row["ItemDiscAmt"].ToString().ValueConverter<decimal>();
                var MainDisc = row["MainDiscount"].ToString().ValueConverter<decimal>();
                var MainDiscAmtorPer = row["MaindiscAmtorPer"].ToString();

                int dcId = row.Field<int>("DcId");

                decimal totalAmt = dInvoiceDet.Tables[1]
                    .AsEnumerable()
                    .Where(rw => rw.Field<int>("DcId") == dcId)
                    .Sum(rw => Math.Round(rw.Field<decimal?>("TotalAmount") ?? 0m, 2));

                FreightAmt = row["FreightCharges"].ToString().ValueConverter<decimal>();
                PandF = row["PandF"].ToString().ValueConverter<decimal>();
                Insurance = row["Insurance"].ToString().ValueConverter<decimal>();
                TCS = row["TCS"].ToString().ValueConverter<decimal>();
                var discAmt = MainDiscAmtorPer == "True" ? (decimal)totalAmt * MainDisc / 100 : MainDisc;
                totalBasicAmt = (decimal)totalAmt - discAmt;
                PandAmtorPer = row["PandFAmtOrPer"].ToString();
                InsAmtorPer = row["InsuranceAmtOrPer"].ToString();
                TcsAmtorPer = row["TcsAmtorPer"].ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        private static void EwayCalculations_For_Single_DocNo(DataSet dInvoiceDet, IMainTaxCalculation mainTaxCalculation, DataRow dr, DataRow r, out decimal Igst, out decimal Cgst, out decimal Sgst, out decimal subtot3, out decimal cgstval, out decimal sgstval, out decimal igstval, out decimal totcgstval, out decimal totsgstval, out decimal totigstval, out decimal totalVal, string[] TotInvNos)
        {
            try
            {
                decimal FreightAmt, PandF, Insurance, TCS, totalBasicAmt;
                string PandAmtorPer, InsAmtorPer, TcsAmtorPer;
                ExtractingValues(dInvoiceDet, dr, out Igst, out Cgst, out Sgst, r, out FreightAmt, out PandF, out Insurance, out TCS, out totalBasicAmt, out PandAmtorPer, out InsAmtorPer, out TcsAmtorPer, TotInvNos);
                cgstval = 0; sgstval = 0; igstval = 0; decimal TcsAmt = 0; totalVal = 0;

                if (Cgst > 0 || Igst > 0)
                {
                    MainTaxCal_For_SingleDocNo(mainTaxCalculation, Igst, Cgst, Sgst, out subtot3, out totcgstval, out totsgstval, out totigstval, out totalVal, FreightAmt, PandF, Insurance, TCS, totalBasicAmt, PandAmtorPer, InsAmtorPer, TcsAmtorPer, out TcsAmt, TotInvNos);
                }
                else
                {
                    ItemwiseTaxCal_For_SingleDocNo(mainTaxCalculation, dInvoiceDet, out subtot3, ref cgstval, ref sgstval, ref igstval, out totcgstval, out totsgstval, out totigstval, out totalVal, FreightAmt, PandF, Insurance, TCS, PandAmtorPer, InsAmtorPer, TcsAmtorPer, out TcsAmt);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task GetEwayItemDtls(DataSet dInvoiceDet, EwayJsonSerializer ewayJsonSerializer, decimal Igst, decimal Cgst, decimal Sgst)
        {
            try
            {
                List<EwayItemList> itemList = new List<EwayItemList>();
                foreach (DataRow row in dInvoiceDet.Tables[1].Rows)
                {
                    decimal disamt = row["ItemDiscAmt"].ToString().ValueConverter<decimal>();
                    decimal totamt = row["TotalAmount"].ToString().ValueConverter<decimal>();
                    decimal itemcgstrate = row["ItemwiseCgst"].ToString().ValueConverter<decimal>();
                    decimal itemsgstrate = row["ItemwiseSgst"].ToString().ValueConverter<decimal>();
                    decimal itemigstrate = row["ItemwiseIgst"].ToString().ValueConverter<decimal>();
                    decimal cgstrate = Cgst > 0 ? Cgst : itemcgstrate > 0 ? itemcgstrate : 0;
                    decimal sgstrate = Sgst > 0 ? Sgst : itemsgstrate > 0 ? itemsgstrate : 0;
                    decimal igstrate = Igst > 0 ? Igst : itemigstrate > 0 ? itemigstrate : 0;

                    itemList.Add(new EwayItemList
                    {
                        productName = row["ItemCode"].ToString().Length <= 100 ? row["ItemCode"].ToString() : row["ItemCode"].ToString().Substring(0, 100),
                        productDesc = row["ItemName"].ToString().Length <= 100 ? row["ItemName"].ToString() : row["ItemName"].ToString().Substring(0, 100),
                        hsnCode = row["HSNCode"].ToString().Trim().ValueConverter<long>(),
                        quantity = row["Qty"].ToString().ValueConverter<decimal>(),
                        qtyUnit = row["MeasureUnit"].ToString().ToUpper(),
                        cgstRate = cgstrate.ToString().ValueConverter<decimal>(),
                        sgstRate = sgstrate.ToString().ValueConverter<decimal>(),
                        igstRate = igstrate.ToString().ValueConverter<decimal>(),
                        taxableAmount = Math.Round(totamt - disamt, 2)
                    });
                }
                ewayJsonSerializer.itemList = itemList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private  async Task  GetEwayDtls(EwayJsonSerializer ewayJsonSerializer, DataRow dr, DataRow r, decimal OtherCharges, decimal subtot3, decimal cgstval, decimal sgstval, decimal igstval, decimal totalVal, string Invnos, IUnitOfWork unitOfWork)
        {
            try
            {
                var subSupDesc = (r["SubSuppType"].ToString() == "8") ? r["SubSupplyDesc"].ToString() : "";
                //var DocDate = DateTime.Parse(r["docType"].ToString().Equals("BIL") || r["docType"].ToString().Equals("CHL") ? r["DcDate"].ToString() : r["InvDate"].ToString()).ToString("dd/MM/yyyy");
                
                var rawDate = r["docType"].ToString() == "BIL" ||
                r["docType"].ToString() == "CHL" ? r["DocDate"] : r["DocDate"];

                DateTime invDate;


                if (rawDate is DateTime dtValue)
                {
                    invDate = dtValue;
                }
                else
                {

                    string dateStr = rawDate?.ToString()?.Trim();

                    string[] formats =
                    {
                        "dd-MM-yyyy HH:mm:ss",
                        "MM-dd-yyyy HH:mm:ss",
                        "yyyy-MM-dd HH:mm:ss",

                        "dd-MM-yyyy",
                        "MM-dd-yyyy",
                        "yyyy-MM-dd",

                        "dd/MM/yyyy",
                        "MM/dd/yyyy",
                        "yyyy/MM/dd"
                    };

                    if (!DateTime.TryParseExact(
                            dateStr,   // ✅ string
                            formats,
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                            out invDate))
                    {
                        throw new Exception($"Invalid DocDate format: {dateStr}");
                    }
                }

                string dateString = invDate.ToString("dd/MM/yyyy");

                string DocDate = Regex.Replace(
                    dateString,
                    @"^(\d{2})-(\d{2})-(\d{4}).*",
                    "$1/$2/$3"
                );

                var TotInvNos = Invnos.Split(',');
                string DocNo1 = "";


                if (TotInvNos.Length == 1)
                {
                    DocNo1 = string.Concat(r["Prefix"].ToString().Trim(), Invnos);
                }
                else
                {
                    var firstInvNo = TotInvNos.First().Split('/')[0];
                    var lastInvNo = TotInvNos.Last().Split('/')[0];
                    DocNo1 = string.Concat(r["Prefix"].ToString().Trim(), $"{firstInvNo}to{lastInvNo}");
                }

                //Bill For Consignee
                var Custid = r["Custid"].ToString().ValueConverter<double>();

                var altCustid = await  unitOfWork.MfgDcs.GetQueryable()
                                 .Where(dc => DocNo1.Contains(dc.DcNo) && dc.Customer.CustId == Custid)
                                 .Select(dc => dc.ShippingId)
                                 .FirstOrDefaultAsync();

                var SupType = await unitOfWork.Customers
                            .GetQueryable()                  
                            .Where(c => c.CustId == Custid)  
                            .Select(c => c.SupTyp)           
                            .FirstOrDefaultAsync();          


                if (altCustid != null)
                {
                    var altCustomer =  await unitOfWork.CustomerIndirects
                                        .GetQueryable()
                                        .Where(c => c.CustId == Custid && c.AltCustId == altCustid)
                                        .FirstOrDefaultAsync();

                    if (altCustomer != null)
                    {
                        ewayJsonSerializer.toTrdName = altCustomer.AltCustName;
                        ewayJsonSerializer.toGstin = altCustomer.GSTNo;
                        ewayJsonSerializer.toAddr1 = altCustomer.AltCustAddr1?.Trim();
                        ewayJsonSerializer.toAddr2 = altCustomer.AltCustAddr2?.Trim();
                        ewayJsonSerializer.toPlace = altCustomer.City?.Trim();
                        ewayJsonSerializer.toPincode = Convert.ToInt32(altCustomer.Pincode); 
                    }

                    var statecode = string.IsNullOrEmpty(altCustomer?.State)
                                    ? 0
                                    : unitOfWork.States.GetQueryable()
                                        .Where(c => c.StateName == altCustomer.State)
                                        .Select(c => c.StateCode)
                                        .FirstOrDefault();

                    if (SupType == "SEZWOP" || SupType == "SEZWP")
                    {
                        ewayJsonSerializer.actToStateCode = statecode;
                        ewayJsonSerializer.toStateCode = 96;

                    }
                    else
                    {
                        ewayJsonSerializer.actToStateCode = r["StateCode"].ToString().ValueConverter<int>();
                        ewayJsonSerializer.toStateCode = r["StateCode"].ToString().ValueConverter<int>();
                    }
                }
                else
                {
                    ewayJsonSerializer.toTrdName = r["custName"].ToString();
                    var custaddr1 = r["CustAddress"].ToString().Trim();
                    var custaddr2 = "";
                    if (custaddr1.Length > 120)
                    {
                        custaddr2 = custaddr1.Substring(120).Trim();
                        custaddr1 = custaddr1.Substring(0, 120).Trim();
                    }
                    ewayJsonSerializer.toGstin = r["CustGstNo"].ToString();
                    ewayJsonSerializer.toAddr1 = custaddr1;
                    ewayJsonSerializer.toAddr2 = custaddr2;
                    ewayJsonSerializer.toPlace = r["Location"].ToString().Trim();
                    ewayJsonSerializer.toPincode = r["PinCode"].ToString().ValueConverter<int>();
                   
                    if (SupType == "SEZWOP" || SupType == "SEZWP")
                    {
                        ewayJsonSerializer.actToStateCode = r["StateCode"].ToString().ValueConverter<int>();
                        ewayJsonSerializer.toStateCode = 96;

                    }
                    else
                    {
                        ewayJsonSerializer.actToStateCode = r["StateCode"].ToString().ValueConverter<int>();
                        ewayJsonSerializer.toStateCode = r["StateCode"].ToString().ValueConverter<int>();
                    }
                }

                var ouraddr1 = dr["GSTAddress"].ToString().Trim();
                var ouraddr2 = "";
                if (ouraddr1.Length > 120)
                {
                    ouraddr2 = ouraddr1.Substring(120).Trim();
                    ouraddr1 = ouraddr1.Substring(0, 120).Trim();
                }

                ewayJsonSerializer.supplyType = "O";
                ewayJsonSerializer.subSupplyType = r["SubSuppType"].ToString();
                ewayJsonSerializer.subSupplyDesc = subSupDesc;
                ewayJsonSerializer.docType = r["docType"].ToString();
                ewayJsonSerializer.docNo = DocNo1;
                ewayJsonSerializer.docDate = DocDate;
                ewayJsonSerializer.fromGstin = dr["GSTIN"].ToString().Trim();
                ewayJsonSerializer.fromAddr1 = ouraddr1;
                ewayJsonSerializer.fromAddr2 = ouraddr2;
                ewayJsonSerializer.fromPlace = dr["City"].ToString().Trim();
                ewayJsonSerializer.fromPincode = dr["ZipCode"].ToString().ValueConverter<int>();
                ewayJsonSerializer.fromStateCode = dr["StateCode"].ToString().ValueConverter<int>();
                ewayJsonSerializer.actFromStateCode = dr["StateCode"].ToString().ValueConverter<int>();
                ewayJsonSerializer.totalValue = Math.Round(subtot3,2);
                ewayJsonSerializer.cgstValue = cgstval;
                ewayJsonSerializer.sgstValue = sgstval;
                ewayJsonSerializer.igstValue = igstval;
                ewayJsonSerializer.othersValue = OtherCharges;
                ewayJsonSerializer.totInvValue = Math.Round(OtherCharges + totalVal, 2);
                ewayJsonSerializer.transporterId = string.IsNullOrEmpty(r["EwayTransId"].ToString()) ? "" : r["EwayTransId"].ToString().Trim();
                ewayJsonSerializer.transporterName = string.IsNullOrEmpty(r["EwayTransName"].ToString()) ? "" : r["EwayTransName"].ToString().Trim();
                ewayJsonSerializer.transDocNo = string.IsNullOrEmpty(r["EwayTransDocNo"].ToString()) ? "" : r["EwayTransDocNo"].ToString().Trim();
                ewayJsonSerializer.transMode = r["ModeofTrasnport"].ToString();
                ewayJsonSerializer.transDistance = r["Distance"] != DBNull.Value ? Convert.ToInt32(Convert.ToDecimal(r["Distance"])) : 0;
                ewayJsonSerializer.transDocDate = DocDate;
                ewayJsonSerializer.vehicleNo = r["VehicleNo"].ToString();
                ewayJsonSerializer.vehicleType = "R";
                ewayJsonSerializer.transactionType = 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task <AuthEWay> GetUserNameandPassword(string username, string password)
        {
            try
            {
                AuthEWay auth = new AuthEWay();
                auth.UserName = username;//"API_Bhargavispl";
                auth.Password = password;//"$Winbspl789";
                return auth;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<string> GetGstIn(DataSet dinvoiceDet)
        {
            try
            {
                DataTable misc = dinvoiceDet.Tables["Companydetails"];
                string Gstin = misc.Rows[0]["GSTIN"].ToString().Trim();
                return Gstin;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<EWayDocument>> GetMfgDcByDcNos(string dcNos)
        {
            try
            {
                var dcNoList = dcNos.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim())
                                    .ToList();

                var result = await
                    (from d in _unitOfWork.MfgDcs.GetQueryable().AsNoTracking()
                     join s in _unitOfWork.MfgDcSubs.GetQueryable().AsNoTracking()
                         on d.DcId equals s.DcId
                     join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                         on s.ItemId equals i.ItemId
                     join c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                         on d.CustId equals c.CustId
                     where (string.IsNullOrEmpty(d.EwayBillNo) || d.EwayBillNo == "0")
                           && dcNoList.Contains(d.DcNo + d.Suffix)
                     select new EWayDocument
                     {
                         DcId = d.DcId,
                         Prefix = d.Prefix,
                         DocNo = EnsureSuffix(d.DcNo, d.Suffix),
                         Suffix = d.Suffix,

                         DocDate = d.DcDate,

                         ModeofTrasnport = d.ModeofTrasnport.ToString(),
                         SubSupplyDesc = d.SubSupplyDesc,
                         SubSuppType = d.SubSuppType.ToString(),
                         CustId = d.CustId,
                         CustName = c.CustName,
                         CustAddress = c.CustAddr,
                         CustGstNo = c.GSTNo,
                         Location = c.Location,
                         PinCode = c.PinCode,
                         StateCode = c.StateId,

                         DocType = "BIL",
                         DcSubId = s.DcSubId,
                         ItemId = s.ItemId,
                         Qty = s.Qty,
                         UnitPrice = s.UnitPrice,
                         TotalAmount = (s.Qty * s.UnitPrice),
                         ItemDiscAmt = 0,
                         EwayTransId = d.TransId,
                         EwayTransName = d.TransName,
                         EwayTransDocNo = d.TransDocNo,
                         VehicleNo = d.VehicleNo,
                         Distance = (decimal?)c.Distance,
                         // ✅ IMPORTANT FIX — Use i NOT s.Item
                         ItemCode = i.ItemCode,
                         ItemName = i.ItemName,
                         HSNCode = i.HSNCode,
                         MeasureUnit = i.MeasureUnit,
                         RoffSales = false,
                     }).ToListAsync();

                return result;
            }
            catch
            {
                return new List<EWayDocument>();
            }
        }

        //MfgInvoice E-way
        public async Task<List<EWayDocument>> GetMfgInvoiceByInvNos(string invNos)
        {
            try
            {
                var dcNoList = invNos.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim())
                                    .ToList();

                var result = await
                    (from d in _unitOfWork.MfgInvs.GetQueryable().AsNoTracking()
                     join s in _unitOfWork.MfgInvSubs.GetQueryable().AsNoTracking()
                         on d.InvId equals s.InvId
                     join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                         on s.ItemId equals i.ItemId
                     join c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                         on d.CustId equals c.CustId
                     where (string.IsNullOrEmpty(d.EWayNo) || d.EWayNo == "0")
                           && dcNoList.Contains(d.InvNo + d.Suffix)
                     select new EWayDocument
                     {
                         DcId = d.InvId,
                         Prefix = d.Prefix,
                         DocNo = EnsureSuffix(d.InvNo, d.Suffix),
                         Suffix = d.Suffix,

                         DocDate = d.InvDate,

                         ModeofTrasnport = d.TransportMode.ToString() ?? "",
                         SubSupplyDesc = d.SubSupplyDesc,
                         SubSuppType = d.SubSupplyType.ToString(),
                         CustId = d.CustId,
                         CustName = c.CustName,
                         CustAddress = c.CustAddr,
                         CustGstNo = c.GSTNo,
                         Location = c.Location,
                         PinCode = c.PinCode,
                         StateCode = c.StateId,



                         DocType = "INV",
                         DcSubId = s.InvSubId,
                         EwayTransId = d.TransId,
                         EwayTransName = d.TransName,
                         EwayTransDocNo = d.TransDocNo,
                         VehicleNo = d.VehicleNo,
                         Distance = (decimal?)c.Distance,

                         ItemId = s.ItemId,
                         Qty = s.Qty,
                         UnitPrice = s.UnitPrice,
                         TotalAmount = (s.Qty * s.UnitPrice),
                         ItemDiscAmt = d.DiscountAmount,
                         TotVal = d.GrandTotal,

                         MainDiscount = d.DiscountPercent,
                         MainDiscAmtOrPer = d.DiscAmtOrPer == true ? false : true,

                         FreightCharges = d.FreightCharges,
                         PandF = d.PackingPercent,
                         PandFAmtOrPer = d.PackingAmtOrPer == true ? false : true,

                         Insurance = d.InsurancePercent,
                         InsuranceAmtOrPer = d.InsuranceAmtOrPer == true ? false : true,

                         Cgst = d.CGstRate,
                         Sgst = d.SGstRate,
                         Igst = d.IGstRate,

                         ItemwiseCgst = 0,
                         ItemwiseSgst = 0,
                         ItemwiseIgst = 0,

                         TCS = d.TCSPercent,
                         TCSAmtOrPer = d.TCSAmtOrPer == true ? false : true,


                         // ✅ IMPORTANT FIX — Use first.i NOT first.s.Item
                         ItemCode = i.ItemCode,
                         ItemName = i.ItemName,
                         HSNCode = i.HSNCode,
                         MeasureUnit = i.MeasureUnit,
                         RoffSales = d.IsRoundOffEnabled,

                     }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return new List<EWayDocument>();
            }
        }

        //LabourDcOutgoing E-way
        public async Task<List<EWayDocument>> GetLabDcByDcNos(string dcNos)
        {
            try
            {

                var dcNoList = dcNos.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim())
                                    .ToList();
                var result = await
                (from d in _unitOfWork.LabourDcOutgoings.GetQueryable().AsNoTracking()
                 join s in _unitOfWork.LabourDcOutgoingSubs.GetQueryable().AsNoTracking()
                     on d.DcId equals s.DcId
                 join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                     on s.ItemId equals i.ItemId
                 join c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                     on d.CustId equals c.CustId
                 where (string.IsNullOrEmpty(d.EwayNo) || d.EwayNo == "0")
                       && dcNoList.Contains(d.DcNo + d.Suffix)
                 select new EWayDocument
                 {
                     DcId = d.DcId,
                     Prefix = d.Prefix,
                     DocNo = EnsureSuffix(d.DcNo, d.Suffix),
                     Suffix = d.Suffix,

                     DocDate = d.DcDate,

                     ModeofTrasnport = d.TransportMode.ToString() ?? "",
                     SubSupplyDesc = d.SubSupplyDesc,
                     SubSuppType = d.SubSupplyType.ToString() ?? "",
                     CustId = d.CustId,
                     CustName = c.CustName,
                     CustAddress = c.CustAddr,
                     CustGstNo = c.GSTNo,
                     Location = c.Location,
                     PinCode = c.PinCode,
                     StateCode = c.StateId,

                     DocType = d.SubSupplyType == 1 ? "BIL" : "CHL",
                     DcSubId = s.DcSubId,
                     ItemId = s.ItemId,
                     Qty = s.Qty,
                     UnitPrice = s.UnitPrice,
                     TotalAmount = (s.Qty * s.UnitPrice),
                     ItemDiscAmt = 0,
                     EwayTransId = d.TransId,
                     EwayTransName = d.TransName,
                     EwayTransDocNo = d.TransDocNo,
                     VehicleNo = d.VehicleNo,
                     Distance = (decimal?)c.Distance,
                     // ✅ IMPORTANT FIX — Use first.i NOT first.s.Item
                     ItemCode = i.ItemCode,
                     ItemName = i.ItemName,
                     HSNCode = i.HSNCode,
                     MeasureUnit = i.MeasureUnit,
                     RoffSales = false,

                 }).ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                return new List<EWayDocument>();
            }
        }

        // SubContractDc E-way
        public async Task<List<EWayDocument>> GetSubConDcByDcNos(string dcNos)
        {
            try
            {

                var dcNoList = dcNos.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(x => x.Trim())
                                    .ToList();

                var flatData = await
                    (from d in _unitOfWork.SubConDCOuts.GetQueryable().AsNoTracking()
                     join s in _unitOfWork.SubConDCOutSubs.GetQueryable()
                         on d.DcId equals s.DcId
                     join i in _unitOfWork.ItemRepositories.GetQueryable()
                         on s.ItemId equals i.ItemId
                     join c in _unitOfWork.Vendors.GetQueryable()
                         on d.VendorCode equals c.VendorCode
                     where
                          (string.IsNullOrEmpty(d.EwayBillNumber) || d.EwayBillNumber == "0")
                           && dcNoList.Contains(d.DcNo + d.Suffix) && s.TransType=="Out"
                     select new EWayDocument
                     {
                         DcId = d.DcId,
                         Prefix = d.Prefix,
                         DocNo = EnsureSuffix(d.DcNo, d.Suffix),
                         Suffix = d.Suffix,

                         DocDate = d.DcDate,

                         ModeofTrasnport = d.ModeOfTransportId.ToString() ?? "",
                         SubSupplyDesc = d.SubSupplyDesc,
                         SubSuppType = d.SubSupplyType.ToString() ?? "",
                         CustId = d.VendorCode,
                         CustName = c.VendorName,
                         CustAddress = c.VendorAddress,
                         CustGstNo = c.GSTNo,
                         Location = c.Location,
                         PinCode = c.PinCode,
                         StateCode = c.StateCode,

                         DocType = "CHL",
                         DcSubId = s.DcSubId,
                         ItemId = s.ItemId,
                         Qty = s.Qty,
                         UnitPrice = s.UnitPrice,
                         TotalAmount = (s.Qty * s.UnitPrice),
                         ItemDiscAmt = 0,
                         EwayTransId = d.EwayTransId,
                         EwayTransName = d.EwayTransName,
                         EwayTransDocNo = d.EwayTransDocNo,
                         VehicleNo = d.VehicleNo,
                         Distance = (decimal?)c.Distance,
                         // ✅ IMPORTANT FIX — Use i NOT s.Item
                         ItemCode = i.ItemCode,
                         ItemName = i.ItemName,
                         HSNCode = i.HSNCode,
                         MeasureUnit = i.MeasureUnit,
                         RoffSales = false,
                     }).ToListAsync();


                return flatData;
            }
            catch (Exception ex)
            {
                return new List<EWayDocument>();
            }
        }


        public static string EnsureSuffix(string docNo, string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return docNo;

            string expectedSuffix = "/";

            if (string.IsNullOrWhiteSpace(docNo))
                return expectedSuffix;

            if (docNo.EndsWith(expectedSuffix, StringComparison.OrdinalIgnoreCase))
                return docNo;

            return docNo + suffix;
        }

    }
}
