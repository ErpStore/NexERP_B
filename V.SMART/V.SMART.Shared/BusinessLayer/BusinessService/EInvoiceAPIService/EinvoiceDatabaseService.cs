using DocumentFormat.OpenXml.InkML;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IEInvoiceAPI;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.E_Invoice;
using V.SMART.Shared.E_Invoice.E_InvoiceHelper;
using V.SMART.Shared.Pages.Master_Module_pages.Items_Pages;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.MultiCompanyService;
using V.SMART.Shared.ViewModels.EWayModel;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static V.SMART.Shared.E_Invoice.Enums;
using ItemList = V.SMART.Shared.E_Invoice.E_InvoiceHelper.ItemList;

namespace V.SMART.Shared.BusinessLayer.BusinessService.EInvoiceAPIService
{
    public class EinvoiceDatabaseService: IEinvoiceDatabaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _logs;
        private readonly IScreenManagementRepository _screenManage;

        private readonly ITenantProvider _tenantProvider;

        public bool IsPrefix = false, IsEway = false;

        public string ScreenName = "";
        public string? prefix = null;

        public static event EventHandler<APIEventArgs> ThrowExceptionEvent;

        public EinvoiceDatabaseService(IUnitOfWork unitOfWork, ILoggingService logs, IScreenManagementRepository screenManage, ITenantProvider tenant)
        {
            _unitOfWork = unitOfWork;
            _logs = logs;
            _screenManage = screenManage;
            _tenantProvider = tenant;
        }

        public async Task GetRequiredDetailsFromDb(string InvNo, string InvType, bool isPreView)
        {
            try
            {

                switch (InvType)
                {
                    case "Sales":
                        ScreenName = "Manufacturing Invoice";
                        prefix = await _unitOfWork.MfgInvs.GetQueryable()
                                         .Where(x => x.InvNo == InvNo)
                                         .Select(x => x.Prefix)
                                         .FirstOrDefaultAsync();
                        break;

                    case "Labour":
                        ScreenName = "Labour Invoice";
                        prefix = await _unitOfWork.LabInvs.GetQueryable()
                                         .Where(x => x.LabInvNo == InvNo)
                                         .Select(x => x.Prefix)
                                         .FirstOrDefaultAsync();
                        break;

                    case "Export":
                        ScreenName = "Export Invoice";
                        prefix = await _unitOfWork.ExpInvs.GetQueryable()
                                         .Where(x => x.ExpInvNo == InvNo)
                                         .Select(x => x.Prefix)
                                         .FirstOrDefaultAsync();
                        break;

                    case "CreditNote":
                        ScreenName = "Credit Note";
                        prefix = await _unitOfWork.CreditNotes.GetQueryable()
                                         .Where(x => x.CreditNo == InvNo)
                                         .Select(x => x.Prefix)
                                         .FirstOrDefaultAsync();
                        break;

                    case "DebitNote":
                        ScreenName = "Debit Note";
                        prefix = await _unitOfWork.DebitNotes.GetQueryable()
                                        .Where(x => x.DebitNo == InvNo)
                                        .Select(x => x.Prefix)
                                        .FirstOrDefaultAsync();
                        break;

                }

                IsEway = await _unitOfWork.ScreenManagements.GetQueryable()
                                .Where(x => x.ScreenName == ScreenName && x.Display == "E-Way Bill")
                                .Select(x => x.Required)
                                .FirstOrDefaultAsync();


                IsPrefix =await _unitOfWork.ScreenManagements.GetQueryable()
                                .Where(x => x.ScreenName == ScreenName && x.Display == "Prefix")
                                .Select(x => x.Required)
                                .FirstOrDefaultAsync();


                if (IsPrefix && string.IsNullOrEmpty(prefix))
                {
                    throw new InvalidOperationException($"Prefix is required for the {ScreenName}  number because 'Prefix Required' is enabled in Screen Management.");
                }
               


                var Auth =await GetUserNameandPassword(new LicenseProductKey(await GetProductKeyAsync()));

                if (Enum.TryParse(InvType, out InvType Type))
                {
                    if (!isPreView)
                    {
                        await GenerateAckNo(InvNo, Type, Auth.UserName, Auth.Password);
                    }
                    else
                    {
                        await PreView(InvNo, Type, Auth.UserName, Auth.Password);
                    }


                }
                else
                {
                    ThrowExceptionEvent.Invoke(this, new APIEventArgs() { ErrorMesssage = "Please Contact Vendor" });
                    ThrowExceptionEvent += ThrowExecption;
                    throw new Exception("Please Contact Vendor");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string?> GetProductKeyAsync()
        {
            try
            {
                var apiKey = await _unitOfWork.Companies
                                .GetQueryable()
                                .Select(x => x.APIEinvoiceLicenseKey)
                                .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new InvalidOperationException("Your API product key is empty. Please contact ERP Admin.");

                return apiKey;
            }
            catch
            {
                throw;
            }
        }

        public static void ThrowExecption(object sender, APIEventArgs args)
        {
            throw new Exception(args.ErrorMesssage);
        }

        public async Task<AuthEinvoice> GetUserNameandPassword(ILicenseProductKey licenseProductKey)
        {
            try
            {
                var auth = new AuthEinvoice();
                if (await licenseProductKey.IsValidProductKey())
                {
                    auth = await licenseProductKey.GetUserName();
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

        private async Task GenerateAckNo(string Invno, Enum InvType, string username, string password)
        {
            try
            {
                DataSet dinvoiceDet = await UpdateInvoiceDet(Invno, InvType);

                if (await IsValid(dinvoiceDet, InvType))
                {
                    IAPIService eInvoiceAPI = APIFactory.GetAPIService("EInvoice",await GetJsonData(dinvoiceDet, InvType));
                    var irnResponse = await eInvoiceAPI.GetResponseByJsonData(new AuthenticationDetails(await GetUserNameandPassword(username, password),await GetGstIn(dinvoiceDet))).ConfigureAwait(true);

                    switch (InvType)
                    {
                        case Enums.InvType.Sales:
                            var mfgRecord = await _unitOfWork.MfgInvs.GetQueryable().FirstOrDefaultAsync(x => x.InvNo == Invno);
                            if (mfgRecord!= null)
                            {
                                mfgRecord.ACKNO = irnResponse.AckNo;
                                DateTime ackDate;
                                if (DateTime.TryParse(irnResponse.AckDt, out ackDate))
                                {
                                    mfgRecord.ACKNODate = ackDate;
                                }
                                mfgRecord.IRNNo = irnResponse.Irn;
                                mfgRecord.SignedInvoiceQrCode = irnResponse.SignedQRCode;
                                mfgRecord.EWayNo = irnResponse.EwbNo;
                                DateTime ewayDate;
                                if (DateTime.TryParse(irnResponse.ewayBillDate, out ewayDate))
                                {
                                    mfgRecord.EWayDate = ewayDate;
                                }
                                await _unitOfWork.MfgInvs.UpdateAsync(mfgRecord);
                                await _unitOfWork.SaveAsync();
                            }
                            break;
                        case Enums.InvType.Labour:
                            var LabRecord = await _unitOfWork.LabInvs.GetQueryable().FirstOrDefaultAsync(x => x.LabInvNo == Invno);
                            if (LabRecord != null)
                            {
                                LabRecord.ACKNO = irnResponse.AckNo;
                                DateTime ackDate;
                                if (DateTime.TryParse(irnResponse.AckDt, out ackDate))
                                {
                                    LabRecord.ACKNODate = ackDate;
                                }
                                LabRecord.IRNNo = irnResponse.Irn;
                                LabRecord.SignedInvoiceQrCode = irnResponse.SignedQRCode;
                                //LabRecord.EWayNo = irnResponse.EwbNo;
                                //DateTime ewayDate;
                                //if (DateTime.TryParse(irnResponse.ewayBillDate, out ewayDate))
                                //{
                                //    LabRecord.EWayDate = ewayDate;
                                //}
                                await _unitOfWork.LabInvs.UpdateAsync(LabRecord);
                                await _unitOfWork.SaveAsync();
                            }
                            break;
                        case Enums.InvType.Export:
                            var ExpRecord = await _unitOfWork.ExpInvs.GetQueryable().FirstOrDefaultAsync(x => x.ExpInvNo == Invno);
                            if (ExpRecord != null)
                            {
                                ExpRecord.ACKNO = irnResponse.AckNo;
                                DateTime ackDate;
                                if (DateTime.TryParse(irnResponse.AckDt, out ackDate))
                                {
                                    ExpRecord.ACKNODate = ackDate;
                                }
                                ExpRecord.IRNNo = irnResponse.Irn;
                                ExpRecord.SignedInvoiceQrCode = irnResponse.SignedQRCode;
                                ExpRecord.EWayNo = irnResponse.EwbNo;
                                DateTime ewayDate;
                                if (DateTime.TryParse(irnResponse.ewayBillDate, out ewayDate))
                                {
                                    ExpRecord.EWayDate = ewayDate;
                                }
                                await _unitOfWork.ExpInvs.UpdateAsync(ExpRecord);
                                await _unitOfWork.SaveAsync();
                            }
                            break;
                        case Enums.InvType.CreditNote:
                            var CreditRecord = await _unitOfWork.CreditNotes.GetQueryable().FirstOrDefaultAsync(x => x.CreditNo == Invno);
                            if (CreditRecord != null)
                            {
                                CreditRecord.ACKNO = irnResponse.AckNo;
                                DateTime ackDate;
                                if (DateTime.TryParse(irnResponse.AckDt, out ackDate))
                                {
                                    CreditRecord.ACKNODate = ackDate;
                                }
                                CreditRecord.IRNNo = irnResponse.Irn;
                                CreditRecord.SignedInvoiceQrCode = irnResponse.SignedQRCode;
                               
                                await _unitOfWork.CreditNotes.UpdateAsync(CreditRecord);
                                await _unitOfWork.SaveAsync();
                            }
                            break;

                        case Enums.InvType.DebitNote:
                            var DebitRecord = await _unitOfWork.DebitNotes.GetQueryable().FirstOrDefaultAsync(x => x.DebitNo == Invno);
                            if (DebitRecord != null)
                            {
                                DebitRecord.ACKNO = irnResponse.AckNo;
                                DateTime ackDate;
                                if (DateTime.TryParse(irnResponse.AckDt, out ackDate))
                                {
                                    DebitRecord.ACKNODate = ackDate;
                                }
                                DebitRecord.IRNNo = irnResponse.Irn;
                                DebitRecord.SignedInvoiceQrCode = irnResponse.SignedQRCode;

                                await _unitOfWork.DebitNotes.UpdateAsync(DebitRecord);
                                await _unitOfWork.SaveAsync();
                            }
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
            
        }

        public async Task<bool> IsValid(DataSet dInvoiceDet, Enum InvTyp)
        {
            DataRow dr = dInvoiceDet.Tables[0].Rows[0];
            DataRow r = dInvoiceDet.Tables[1].Rows[0];
            try
            {
                var validations = new List<(Func<DataRow, bool>, string)>
                {
                    //My Company Details validations
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("LegalName")), "LegalName Should not be Empty in My Company Details...!"),
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("GSTAddress")) && dr.Field<string>("GSTAddress").Trim().Length <= 200, "The Length of Your Company Address Should not be Greater Than or Equal to 200 OR It Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("City")), "Location Should not be Empty in My Company Details...!"),
                    ((row) => !string.IsNullOrEmpty(dr.Field<string>("ZipCode")), "Pincode Should not be Empty in My Company Details...!"),
                    ((row) => (dr.Field<int?>("StateCode")!=null), "StateCode Should not be Empty in My Company Details...!"),

                    //Supplier Details validations
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("CustSupType")), "Customer Type Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("CustAddress")) && r.Field<string>("CustAddress").Trim().Length <= 200, "The Length of Supplier Address Should not be Greater Than or Equal to 200 OR It Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("InvType")), "Invoice Type Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("CustGstNo")), "Customer GSTIN Should Not be Empty OR The Length should be Equal to 15"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("Location")), "Customer Location Should Not be Empty"),
                    
                    //((row) => IsValidPincode(row), "Customer Pincode should not be empty."),                   
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("PinCode")), "Customer PinCode Should Not be Empty"),
                    ((row) => (r.Field<int?>("StateCode")!=null), "Customer or Vendor StateCode Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("MeasureUnit")), "Unit of Measurement Should Not be Empty"),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("HSNCode")), "HSNCode Should Not be Empty"),
                    ((row => ValidateHSNCode(r.Field<string>("HSNCode")), "HSNCode must be  6 or 8 digits")),
                    ((row) => !string.IsNullOrEmpty(r.Field<string>("IsService")), "Item Service Type Should Not be Empty")
                };

                //Direct Eway Along With Einvoice Details Validations
                if (IsEway && InvTyp.ToString() != "Labour")
                {
                    validations.Add(((row) => !string.IsNullOrEmpty(r.Field<string>("EwayTransId")), "Eway Transaction ID Should Not be Empty"));
                    validations.Add(((row) => !string.IsNullOrEmpty(r.Field<string>("EwayTransName")), "Eway Transaction Name Should Not be Empty"));
                    validations.Add(((row) => !string.IsNullOrEmpty(r.Field<string>("ModeofTrasnport")), "Eway Mode of Transportation Should Not be Empty"));
                    validations.Add(((row) => !string.IsNullOrEmpty(r.Field<string>("VehicleNo")), "Vehicle No. Should Not be Empty"));
                    validations.Add(((row) => IsValidVehicleNo(r.Field<string>("VehicleNo")), "Please enter valid Vehicle No."));
                }

                var Messages = validations.Select(validation => new { IsValid = validation.Item1(dr), ErrorMessage = validation.Item2 }).Where(validationResult =>
                !validationResult.IsValid).Select(validationResult => validationResult.ErrorMessage).ToList();

                if (Messages.Any())
                {
                    var message = string.Join(Environment.NewLine, Messages.Select((msg, index) => $"{index + 1}. {msg}"));
                    throw new InvalidOperationException(message);
                }

                return true;

            }
            catch (InvalidOperationException ex)
            {
                throw ;
            }
            catch (Exception ex)
            {
                throw ex;
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
                @"^[A-Z]{2}[0-9]{2}[A-Z]{1,2}[0-9]{4}$",   // KA01AB1234
                @"^[A-Z]{2}[0-9]{2}[A-Z]{1}[0-9]{4}$",     // KA01A1234
                @"^[0-9]{2}BH[0-9]{4}[A-Z]{2}$"            // BH series
            };

            return patterns.Any(p => Regex.IsMatch(vehicleNo, p));
        }
        private bool IsValidPincode(DataRow row)
        {
            // Try to cast Pincode to integer
            int? pincodeInt = null;
            string pincodeStr = null;

            try
            {
                pincodeInt = row.Field<int?>("PinCode");
            }
            catch (InvalidCastException)
            {
                // Ignore if cast fails
            }

            try
            {
                pincodeStr = row.Field<string>("PinCode");
            }
            catch (InvalidCastException)
            {
               
            }

            // Check if either the integer or string representation is valid
            return pincodeInt.HasValue || !string.IsNullOrEmpty(pincodeStr);
        }

        private async Task<string> GetJsonData(DataSet dInvoiceDet, Enum InvType)
        {
            IMainTaxCalculation mainTaxCalculation = new MainTaxCalculation();

            try
            {

                var IsEWay = IsEway; 

                DataRow dr = dInvoiceDet.Tables[0].Rows[0];
                DataRow r = dInvoiceDet.Tables[1].Rows[0];
                JsonSerializer JsonSerializer = new JsonSerializer();

                TranDtls tranDtls =await GetTransDtls(r);
                JsonSerializer.TranDtls = tranDtls;

                DocDtls DocDtls =await GetDocDtls(r);
                JsonSerializer.DocDtls = DocDtls;

                SellerDtls SellerDtls = await GetSellerDtls(dr);
                JsonSerializer.SellerDtls = SellerDtls;

                BuyerDtls BuyerDtls =await GetBuyerDtls(r);
                JsonSerializer.BuyerDtls = BuyerDtls;

                ShipDtls ShipDtls = await GetShipDtls(r, InvType.ToString());
                JsonSerializer.ShipDtls = ShipDtls;

                List<ItemList> itemLists = new List<ItemList>();

                int slno = 0;
                bool RoffSales=false, RoffLabour=false;

                bool.TryParse(r["RoffSales"].ToString(), out  RoffSales);
                RoffLabour = RoffSales;

                //bool.TryParse(dr["RoffLabour"].ToString(), out var RoffLabour);
                decimal Igst = r["Igst"].ToString().ValueConverter<decimal>();
                decimal Cgst = r["Cgst"].ToString().ValueConverter<decimal>();
                decimal Sgst = r["Sgst"].ToString().ValueConverter<decimal>();
                decimal ItemDiscAmt = r["ItemDiscAmt"].ToString().ValueConverter<decimal>();
                decimal MainDisc = r["MainDiscount"].ToString().ValueConverter<decimal>();
                var MainDiscAmtorPer = r["MaindiscAmtorPer"].ToString();
                decimal GST = Math.Round((Igst > 0 ? Igst : Cgst + Sgst).ToString().ValueConverter<decimal>(),2);
                decimal totalAmt = dInvoiceDet.Tables[1].AsEnumerable().Sum(rw => Math.Round(rw["TotalAmount"].ToString().ValueConverter<decimal>(), 2));

                decimal FreightAmt = r["FreightCharges"].ToString().ValueConverter<decimal>();

                decimal PandF = r["PandF"].ToString().ValueConverter<decimal>();
                decimal packAmt= r["PackingCharges"].ToString().ValueConverter<decimal>();
               

                decimal Insurance = r["Insurance"].ToString().ValueConverter<decimal>();
                decimal InsuAmt = r["Insurancecharges"].ToString().ValueConverter<decimal>();

                decimal TCS = r["TCS"].ToString().ValueConverter<decimal>();
                decimal TCSAmt = r["TCSCharges"].ToString().ValueConverter<decimal>();

                decimal OtherCharges = r["OtherCharges"].ToString().ValueConverter<decimal>();
                var mainTax = Sgst + Igst > 0 ? true : false;

                decimal todayval = 0;


                //Before Changing ANything Once get clarified nd move further because some calculations are changes here According to particular Customers
                bool IsExportCred = await IsCreditNoteExportedAsync(r["DocNo"].ToString().Trim()); //repositaryService.Retrieve_Value<bool>($"Select Export From CreditNote Where CrNo = '{r["InvNo"].ToString().Trim()}'").ToString().ValueConverter<bool>();
                if (InvType.Equals(Enums.InvType.Export) || InvType.Equals(Enums.InvType.CreditNote) && IsExportCred)//Export Invoice Calculations
                {
                    todayval = r["TodayVal"].ToString().ValueConverter<decimal>();
                    slno = await ExportInvCalculations(dInvoiceDet, mainTaxCalculation, JsonSerializer, itemLists, slno, MainDisc, FreightAmt, PandF, packAmt, Insurance, InsuAmt, TCS, TCSAmt, OtherCharges, todayval,Cgst,Sgst,Igst);
                }
                else if (mainTax || MainDisc > 0 || FreightAmt > 0 || PandF > 0 || Insurance > 0 || TCS > 0)//Main Tax Calculations
                {
                    decimal FrgtIgstAmt, FrgtCgstAmt, FrgtSgstAmt, FrgtTotVal, PandFAmt, PandFIgstAmt, PandFCgstAmt, PandFSgstAmt, PandFTotVal, InsAmt, InsIgstAmt, InsCgstAmt, InsSgstAmt, InsTotVal, TotAssVal, TaxIgstAmt, TaxCgstAmt, TaxSgstAmt, TaxTotVal, TCSOThers;
                    decimal totQty = 0, totItemAssAmt = 0;

                    foreach (DataRow row in dInvoiceDet.Tables[1].Rows)
                    {
                        slno++;
                        decimal Qty =Math.Round(row["Qty"].ToString().ValueConverter<decimal>(),2);
                        decimal Rate =Math.Round( row["UnitPrice"].ToString().ValueConverter<decimal>(),2);
                        decimal TotalAmount = Math.Round(row["TotalAmount"].ToString().ValueConverter<decimal>(), 2);
                        decimal itemDiscVal = Math.Round(row["ItemDiscAmt"].ToString().ValueConverter<decimal>(), 2);// if ItemDiscount
                        if (itemDiscVal == 0)
                        {
                            itemDiscVal = Math.Round(MainDiscAmtorPer == "True" ? TotalAmount * MainDisc / 100 : MainDisc, 2);
                        }
                        decimal AssAmt = Math.Round(TotalAmount - itemDiscVal, 2).ToString().ValueConverter<decimal>();
                        decimal ItemIgst = mainTaxCalculation.GetTaxAmt(AssAmt, Igst);
                        decimal ItemCgst = mainTaxCalculation.GetTaxAmt(AssAmt, Cgst);
                        decimal ItemSgst = mainTaxCalculation.GetTaxAmt(AssAmt, Sgst);
                        decimal TotItemVal = mainTaxCalculation.GetTotalTaxAmt(AssAmt, ItemIgst, ItemSgst, ItemCgst);
                        await AddNewItem(itemLists, slno, GST, row, Qty, Rate, TotalAmount, itemDiscVal, AssAmt, ItemIgst, ItemCgst, ItemSgst, TotItemVal);
                        totItemAssAmt = totItemAssAmt + AssAmt;

                    }

                    //calculation of discAmt
                    decimal totalBasicAmt = 0;
                    totalBasicAmt = totItemAssAmt;

                    Calculations(mainTaxCalculation, r, RoffSales, RoffLabour, Igst, Cgst, Sgst, FreightAmt, PandF, Insurance, TCS, OtherCharges, totalBasicAmt, out FrgtIgstAmt, out FrgtCgstAmt, out FrgtSgstAmt, out FrgtTotVal, out PandFAmt, out PandFIgstAmt, out PandFCgstAmt, out PandFSgstAmt, out PandFTotVal, out InsAmt, out InsIgstAmt, out InsCgstAmt, out InsSgstAmt, out InsTotVal, out TotAssVal, out TaxIgstAmt, out TaxCgstAmt, out TaxSgstAmt, out TaxTotVal, out TCSOThers, totQty, totItemAssAmt);


                    //CalCulation of Freight Charges
                    AddFreightCharges(itemLists, ref slno, GST, FreightAmt, FrgtIgstAmt, FrgtCgstAmt, FrgtSgstAmt, FrgtTotVal);

                    //CalCulation of PandF Charges
                    AddPackForwardingCharges(itemLists, ref slno, GST, PandF, PandFAmt, PandFIgstAmt, PandFCgstAmt, PandFSgstAmt, PandFTotVal);

                    //CalCulation of Insurance Charges. This also Calculation for Kamal bell Amartization charges
                    AddInsuranceCharges(itemLists, ref slno, GST, Insurance, InsAmt, InsIgstAmt, InsCgstAmt, InsSgstAmt, InsTotVal);

                    ValDtls ValDtls = GetValueDtls(TotAssVal, TaxIgstAmt, TaxCgstAmt, TaxSgstAmt, TaxTotVal, TCSOThers);
                    JsonSerializer.ValDtls = ValDtls;
                }
                else //ItemWise Tax Calculations
                {
                    await ItemWiseTaxCalculations(dInvoiceDet, mainTaxCalculation, JsonSerializer, r, itemLists,  slno,  GST, TCS, OtherCharges, RoffSales, RoffLabour);
                }

                JsonSerializer.ItemList = itemLists;

                //If Eway Bill 
                EwayDtls(InvType, JsonSerializer, IsEWay, r);

                string JSONString = Newtonsoft.Json.JsonConvert.SerializeObject(JsonSerializer, Newtonsoft.Json.Formatting.Indented);

                

                var path = await SaveJson(JSONString);
                return await File.ReadAllTextAsync(path);

                //return Newtonsoft.Json.JsonConvert.SerializeObject(JsonSerializer, Newtonsoft.Json.Formatting.Indented);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private  void EwayDtls(Enum invtype, JsonSerializer JsonSerializer, bool IsEWay, DataRow r)
        {
            try
            {
                if (IsEWay && (!invtype.Equals(InvType.CreditNote) || !invtype.Equals(InvType.DebitNote)))
                {
                    var indvdate = DateTime.Parse(r["DocDate"].ToString()).ToString("dd/MM/yyyy");

                    var Invno = IsPrefix ? string.Concat(r["Prefix"].ToString().Trim(), r["DocNo"].ToString().Trim()) : r["DocNo"].ToString().Trim();
                    double.TryParse(r["Distance"].ToString().Trim(), out double dist);

                    Ewaybill Ewaybill = new Ewaybill()
                    {
                        TransId = r["EwayTransId"].ToString().Trim(),
                        TransName = r["EwayTransName"].ToString().Trim(),
                        TransMode = r["TransportMode"].ToString().Trim(),
                        Distance = dist,
                        TransDocNo = r["EwayTransDocNo"].ToString().Trim(),
                        TransDocDt = indvdate,
                        VehNo = r["VehicleNo"].ToString().Trim(),
                        VehType = "R"
                    };
                    JsonSerializer.EwbDtls = Ewaybill;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void Calculations(IMainTaxCalculation mainTaxCalculation, DataRow r, bool RoffSales, bool RoffLabour, decimal Igst, decimal Cgst, decimal Sgst, decimal FreightAmt, decimal PandF, decimal Insurance, decimal TCS, decimal OtherCharges, decimal totalBasicAmt, out decimal FrgtIgstAmt, out decimal FrgtCgstAmt, out decimal FrgtSgstAmt, out decimal FrgtTotVal, out decimal PandFAmt, out decimal PandFIgstAmt, out decimal PandFCgstAmt, out decimal PandFSgstAmt, out decimal PandFTotVal, out decimal InsAmt, out decimal InsIgstAmt, out decimal InsCgstAmt, out decimal InsSgstAmt, out decimal InsTotVal, out decimal TotAssVal, out decimal TaxIgstAmt, out decimal TaxCgstAmt, out decimal TaxSgstAmt, out decimal TaxTotVal, out decimal TCSOThers, decimal totQty, decimal TotItemAmt)
        {
            try
            {
                FrgtIgstAmt = 0;
                FrgtCgstAmt = 0;
                FrgtSgstAmt = 0;
                FrgtTotVal = 0;

                PandFAmt = 0;
                PandFIgstAmt = 0;
                PandFCgstAmt = 0;
                PandFSgstAmt = 0;
                PandFTotVal = 0;

                InsAmt = 0;
                InsIgstAmt = 0;
                InsCgstAmt = 0;
                InsSgstAmt = 0;
                InsTotVal = 0;


                //For Others changes the calculations accordingly
                //calculation of freight
                if (FreightAmt > 0)
                {
                    FrgtIgstAmt = mainTaxCalculation.GetTaxAmt(FreightAmt, Igst);
                    FrgtCgstAmt = mainTaxCalculation.GetTaxAmt(FreightAmt, Cgst);
                    FrgtSgstAmt = mainTaxCalculation.GetTaxAmt(FreightAmt, Sgst);
                    FrgtTotVal = mainTaxCalculation.GetTotalTaxAmt(FreightAmt, FrgtIgstAmt, FrgtSgstAmt, FrgtCgstAmt);
                }

                //calculation of PackForward 
                if (PandF > 0)
                {
                    var PandAmtorPer = r["PandFAmtOrPer"].ToString();
                   // PandFAmt = Math.Round(PandAmtorPer == "True" ? (totalBasicAmt + FreightAmt) * PandF / 100 : totalBasicAmt + FreightAmt + PandF, 2).ToString().ValueConverter<decimal>();
                    PandFAmt = Math.Round(PandAmtorPer == "True" ? (totalBasicAmt ) * PandF / 100 : totalBasicAmt + PandF, 2).ToString().ValueConverter<decimal>();
                    PandFIgstAmt = mainTaxCalculation.GetTaxAmt(PandFAmt, Igst);
                    PandFCgstAmt = mainTaxCalculation.GetTaxAmt(PandFAmt, Cgst);
                    PandFSgstAmt = mainTaxCalculation.GetTaxAmt(PandFAmt, Sgst);
                    PandFTotVal = mainTaxCalculation.GetTotalTaxAmt(PandFAmt, PandFIgstAmt, PandFCgstAmt, PandFSgstAmt);
                }

                //calculation of Insurance
                if (Insurance > 0)
                {
                    var InsAmtorPer = r["InsuranceAmtOrPer"].ToString();
                   // InsAmt = Math.Round(InsAmtorPer == "True" ? (totalBasicAmt + FreightAmt + PandFAmt) * Insurance / 100 : InsAmtorPer == "False" ? totalBasicAmt + FreightAmt + PandFAmt + Insurance : 0, 2).ToString().ValueConverter<decimal>();

                    InsAmt = Math.Round(InsAmtorPer == "True" ? (totalBasicAmt) * Insurance / 100 : InsAmtorPer == "False" ? totalBasicAmt  + Insurance : 0, 2).ToString().ValueConverter<decimal>();
                    InsIgstAmt = mainTaxCalculation.GetTaxAmt(InsAmt, Igst);
                    InsCgstAmt = mainTaxCalculation.GetTaxAmt(InsAmt, Cgst);
                    InsSgstAmt = mainTaxCalculation.GetTaxAmt(InsAmt, Sgst);
                    InsTotVal = mainTaxCalculation.GetTotalTaxAmt(InsAmt, InsIgstAmt, InsCgstAmt, InsSgstAmt);
                }
                //For Others changes the calculations accordingly

                //calculation of TotalAssessable
                TotAssVal = mainTaxCalculation.GetTotalTaxAmt(totalBasicAmt, FreightAmt, PandFAmt, InsAmt);
                TaxIgstAmt = mainTaxCalculation.GetTaxAmt(TotAssVal, Igst);
                TaxCgstAmt = mainTaxCalculation.GetTaxAmt(TotAssVal, Cgst);
                TaxSgstAmt = mainTaxCalculation.GetTaxAmt(TotAssVal, Sgst);
                TaxTotVal = mainTaxCalculation.GetTotalTaxAmt(TotAssVal, TaxIgstAmt, TaxCgstAmt, TaxSgstAmt);

                //calculation of TCS & others
                var TCSAmt = Math.Round(TCS > 0 ? TaxTotVal * TCS / 100 : 0, 2).ToString().ValueConverter<decimal>();
                TCSOThers = (TCSAmt + OtherCharges).ToString().ValueConverter<decimal>();
                TaxTotVal = (RoffSales || RoffLabour ? (int)Math.Round(TaxTotVal + TCSOThers, MidpointRounding.AwayFromZero) : TaxTotVal + TCSOThers).ToString().ValueConverter<decimal>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<int> ExportInvCalculations(DataSet dInvoiceDet, IMainTaxCalculation mainTaxCalculation, JsonSerializer JsonSerializer, List<ItemList> itemLists, int slno, decimal disc, decimal freight, decimal PandF, decimal packfamt, decimal insurance, decimal insamt, decimal TCS, decimal tcsamt, decimal OtherChrg, decimal todayval, decimal Cgst, decimal Sgst, decimal Igst)
        {
            decimal TotAssVal = 0;
            decimal FrgtIgstAmt = 0;
            decimal FrgtCgstAmt = 0;
            decimal FrgtSgstAmt = 0;
            decimal FrgtTotVal = 0;

            decimal PandFAmt = 0;
            decimal PandFIgstAmt = 0;
            decimal PandFCgstAmt = 0;
            decimal PandFSgstAmt = 0;
            decimal PandFTotVal = 0;

            decimal InsAmt = 0;
            decimal InsIgstAmt = 0;
            decimal InsCgstAmt = 0;
            decimal InsSgstAmt = 0;
            decimal InsTotVal = 0;

            foreach (DataRow row in dInvoiceDet.Tables[1].Rows)
            {
                slno++;
                decimal Qty = row["Qty"].ToString().ValueConverter<decimal>();
                decimal.TryParse(row["UnitPrice"].ToString(), out decimal rate);
                decimal Rate = row["UnitPrice"].ToString().ValueConverter<decimal>();

                decimal TotalAmount = Math.Round(Qty * Rate, 5).ToString().ValueConverter<decimal>();
                decimal itemDiscVal = (TotalAmount * disc / 100).ToString().ValueConverter<decimal>();
                decimal AssAmt = (TotalAmount - itemDiscVal).ToString().ValueConverter<decimal>();

                decimal TotItemVal = Math.Round(AssAmt, 2);
                await AddNewItem(itemLists, slno, 0, row, Qty, Math.Round(rate, 2), TotalAmount, Math.Round(itemDiscVal, 2), Math.Round(AssAmt, 2), 0, 0, 0, TotItemVal);
                TotAssVal = Math.Round(TotAssVal + AssAmt, 5).ToString().ValueConverter<decimal>();

            }
            //18032026 vivek commited
            //decimal freightamt = Math.Round(TotAssVal * freight / 100, 2).ToString().ValueConverter<decimal>();
            //decimal pandfamt = Math.Round((TotAssVal + freightamt) * PandF / 100, 2).ToString().ValueConverter<decimal>();
            DataRow r = dInvoiceDet.Tables[1].Rows[0];

            if (freight > 0)
            {
                freight = (freight * todayval);
                FrgtIgstAmt = mainTaxCalculation.GetTaxAmt(freight, Igst);
                FrgtCgstAmt = mainTaxCalculation.GetTaxAmt(freight, Cgst);
                FrgtSgstAmt = mainTaxCalculation.GetTaxAmt(freight, Sgst);
                FrgtTotVal = mainTaxCalculation.GetTotalTaxAmt(freight, FrgtIgstAmt, FrgtSgstAmt, FrgtCgstAmt);
            }

            PandF = PandF > 0 ? PandF : packfamt;
            if (PandF > 0)
            {
                var PandAmtorPer = r["PandFAmtOrPer"].ToString();

                PandFAmt = Math.Round(PandAmtorPer == "True" ? (TotAssVal) * PandF / 100 : packfamt, 2).ToString().ValueConverter<decimal>();

                PandFIgstAmt = mainTaxCalculation.GetTaxAmt(PandFAmt, Igst);
                PandFCgstAmt = mainTaxCalculation.GetTaxAmt(PandFAmt, Cgst);
                PandFSgstAmt = mainTaxCalculation.GetTaxAmt(PandFAmt, Sgst);
                PandFTotVal = mainTaxCalculation.GetTotalTaxAmt(PandFAmt, PandFIgstAmt, PandFCgstAmt, PandFSgstAmt);
            }

            insurance = insurance > 0 ? insurance : insamt;
            if (insurance > 0)
            {
                var InsAmtorPer = r["InsuranceAmtOrPer"].ToString();

                InsAmt = Math.Round(InsAmtorPer == "True" ? (TotAssVal) * insurance / 100 : insamt, 2).ToString().ValueConverter<decimal>();

                InsIgstAmt = mainTaxCalculation.GetTaxAmt(InsAmt, Igst);
                InsCgstAmt = mainTaxCalculation.GetTaxAmt(InsAmt, Cgst);
                InsSgstAmt = mainTaxCalculation.GetTaxAmt(InsAmt, Sgst);
                InsTotVal = mainTaxCalculation.GetTotalTaxAmt(InsAmt, InsIgstAmt, InsCgstAmt, InsSgstAmt);
            }


            AddFreightChargesForExport(itemLists, ref slno, 0, freight, FrgtTotVal, FrgtIgstAmt, FrgtCgstAmt, FrgtSgstAmt, FrgtTotVal);

            AddPackForwardingChargesForExport(itemLists, ref slno, 0, PandF, PandFTotVal, PandFIgstAmt, PandFCgstAmt, PandFSgstAmt, PandFTotVal);

            AddInsuranceChargesForExport(itemLists, ref slno, 0, insurance, InsTotVal, InsIgstAmt, InsCgstAmt, InsSgstAmt, InsTotVal);

            TotAssVal = (TotAssVal + FrgtTotVal + PandFTotVal + InsTotVal).ToString().ValueConverter<decimal>();
            decimal TaxIgstAmt = mainTaxCalculation.GetTaxAmt(TotAssVal, Igst);
            decimal TaxCgstAmt = mainTaxCalculation.GetTaxAmt(TotAssVal, Cgst);
            decimal TaxSgstAmt = mainTaxCalculation.GetTaxAmt(TotAssVal, Sgst);

            decimal TaxTotVal = mainTaxCalculation.GetTotalTaxAmt(TotAssVal, TaxIgstAmt, TaxCgstAmt, TaxSgstAmt);

            var TCSAmt = Math.Round(TCS > 0 ? TaxTotVal * TCS / 100 : 0, 2).ToString().ValueConverter<decimal>();

            bool.TryParse(r["RoffSales"].ToString(), out bool isRound);

            decimal TCSOThers = (TCSAmt + OtherChrg).ToString().ValueConverter<decimal>();

            //TaxTotVal = (isRound ? (int)Math.Round(TaxTotVal + TCSOThers, MidpointRounding.AwayFromZero) : TaxTotVal + TCSOThers).ToString().ValueConverter<decimal>();

            var totval = (TaxTotVal + Math.Round(TCSOThers * todayval, 2)).ToString().ValueConverter<decimal>();

            ValDtls ValDtls = GetValueDtls(Math.Round(TotAssVal, 2), TaxIgstAmt, TaxCgstAmt, TaxSgstAmt, totval, Math.Round(TCSOThers * todayval, 2));
            JsonSerializer.ValDtls = ValDtls;
            return slno;
        }

        private async Task ItemWiseTaxCalculations(DataSet dInvoiceDet, IMainTaxCalculation mainTaxCalculation, JsonSerializer JsonSerializer, DataRow r, List<ItemList> itemLists,  int slno,  decimal GST, decimal TCS, decimal OtherCharges, bool roffsales, bool rofflab)
        {
            try
            {
                decimal TotAssVal = 0, TotIgst = 0, TotCgst = 0, TotSgst = 0;
                foreach (DataRow row in dInvoiceDet.Tables[1].Rows)
                {
                    slno++;
                    decimal Qty = row["Qty"].ToString().ValueConverter<decimal>();
                    decimal Rate = row["UnitPrice"].ToString().ValueConverter<decimal>();
                    decimal TotalAmount = row["TotalAmount"].ToString().ValueConverter<decimal>();

                    decimal ItemWiseIgst = row["ItemWiseIgst"].ToString().ValueConverter<decimal>();
                    decimal ItemWiseCgst = row["ItemWiseCgst"].ToString().ValueConverter<decimal>();
                    decimal ItemWiseSgst = row["ItemWiseSgst"].ToString().ValueConverter<decimal>();
                    GST = (ItemWiseIgst > 0 ? ItemWiseIgst : ItemWiseCgst + ItemWiseSgst).ToString().ValueConverter<decimal>();

                    decimal itemDiscVal = row["ItemDiscAmt"].ToString().ValueConverter<decimal>();// if ItemDiscount                    
                    decimal AssAmt = (TotalAmount - itemDiscVal).ToString().ValueConverter<decimal>();
                    decimal ItemIgst = mainTaxCalculation.GetTaxAmt(AssAmt, ItemWiseIgst);
                    decimal ItemCgst = mainTaxCalculation.GetTaxAmt(AssAmt, ItemWiseCgst);
                    decimal ItemSgst = mainTaxCalculation.GetTaxAmt(AssAmt, ItemWiseSgst);
                    decimal TotItemVal = mainTaxCalculation.GetTotalTaxAmt(AssAmt, ItemIgst, ItemSgst, ItemCgst);

                    AddNewItem(itemLists, slno,Math.Round(GST,2), row, Qty, Math.Round(Rate,2), Math.Round(TotalAmount,2),Math.Round(itemDiscVal,2), Math.Round(AssAmt,2), ItemIgst, ItemCgst, ItemSgst, Math.Round(TotItemVal,2));

                    TotAssVal = (TotAssVal + AssAmt).ToString().ValueConverter<decimal>();
                    TotIgst = (TotIgst + ItemIgst).ToString().ValueConverter<decimal>();
                    TotCgst = (TotCgst + ItemCgst).ToString().ValueConverter<decimal>();
                    TotSgst = (TotSgst + ItemSgst).ToString().ValueConverter<decimal>();
                }

                var TCSAmtorPer = r["TCSAmtorPer"].ToString();
                var TCSAmt = (TCSAmtorPer == "True" ? TotAssVal * TCS / 100 : TCS).ToString().ValueConverter<decimal>();

                var TCSOThers = (TCSAmt + OtherCharges).ToString().ValueConverter<decimal>();
                decimal TotAssVal1 = (roffsales || rofflab ? (int)Math.Round(TotAssVal + TotIgst + TotCgst + TotSgst + TCSOThers, MidpointRounding.AwayFromZero) : TotAssVal + TotIgst + TotCgst + TotSgst + TCSOThers).ToString().ValueConverter<decimal>();

                ValDtls ValDtls = GetValueDtls(Math.Round(TotAssVal,2),Math.Round(TotIgst,2), TotCgst, TotSgst, Math.Round(TotAssVal1,2), Math.Round(TCSOThers,2));
                JsonSerializer.ValDtls = ValDtls;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private static ValDtls GetValueDtls(decimal TotAssVal, decimal TaxIgstAmt, decimal TaxCgstAmt, decimal TaxSgstAmt, decimal TaxTotVal, decimal TcsOth)
        {
            try
            {
                ValDtls ValDtls = new ValDtls()
                {
                    AssVal = TotAssVal,
                    IgstVal = TaxIgstAmt,
                    CgstVal = TaxCgstAmt,
                    SgstVal = TaxSgstAmt,
                    CesVal = 0,
                    StCesVal = 0,
                    Discount = 0,
                    OthChrg = TcsOth,
                    RndOffAmt = 0,
                    TotInvVal = Math.Round(TaxTotVal,2),
                    //TotInvValFc = 0.0
                };
                return ValDtls;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task AddNewItem(List<ItemList> itemLists, int slno, decimal GST, DataRow row, decimal Qty, decimal Rate, decimal TotalAmount, decimal DiscountAmt, decimal AssAmt, decimal ItemIgst, decimal ItemCgst, decimal ItemSgst, decimal TotItemVal)
        {
            try
            {
                itemLists.Add(
                new ItemList
                {
                    SlNo = slno.ToString(),
                    PrdDesc = row["ItemCode"].ToString().Trim(),
                    IsServc = row["IsService"].ToString().Trim().ToUpper(),
                    HsnCd = row["HSNCode"].ToString().Trim(),
                    Qty = Qty,
                    FreeQty = 0,
                    Unit = row["MeasureUnit"].ToString().Trim().ToUpper(),
                    UnitPrice = Rate,
                    TotAmt = TotalAmount,
                    Discount = DiscountAmt,
                    PreTaxVal = 0,
                    AssAmt = AssAmt,
                    GstRt = GST,
                    IgstAmt = ItemIgst,
                    CgstAmt = ItemCgst,
                    SgstAmt = ItemSgst,
                    CesRt = 0,
                    CesAmt = 0,
                    CesNonAdvlAmt = 0,
                    StateCesRt = 0,
                    StateCesAmt = 0,
                    StateCesNonAdvlAmt = 0,
                    OthChrg = 0,
                    TotItemVal =Math.Round(TotItemVal,2)
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void AddInsuranceCharges(List<ItemList> itemLists, ref int slno, decimal GST, decimal Insurance, decimal InsAmt, decimal InsIgstAmt, decimal InsCgstAmt, decimal InsSgstAmt, decimal InsTotVal)
        {
            try
            {
                if (InsAmt > 0)
                {
                    slno++;
                    itemLists.Add(
                    new ItemList
                    {
                        SlNo = slno.ToString(),
                        PrdDesc = "Insurance Charges",//"Amartization Charges", //"Insurance Charges",  //for Kamal Bell Amartization and others Insurance
                        IsServc = "Y",
                        HsnCd = "996511",
                        Qty = 1,
                        FreeQty = 0,
                        Unit = "NOS",
                        UnitPrice = InsAmt,
                        TotAmt = InsAmt,
                        Discount = 0,
                        PreTaxVal = 0,
                        AssAmt = InsAmt,
                        GstRt = GST,
                        IgstAmt = InsIgstAmt,
                        CgstAmt = InsCgstAmt,
                        SgstAmt = InsSgstAmt,
                        CesRt = 0,
                        CesAmt = 0,
                        CesNonAdvlAmt = 0,
                        StateCesRt = 0,
                        StateCesAmt = 0,
                        StateCesNonAdvlAmt = 0,
                        OthChrg = 0,
                        TotItemVal = InsTotVal
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void AddPackForwardingCharges(List<ItemList> itemLists, ref int slno, decimal GST, decimal PandF, decimal PandFAmt, decimal PandFIgstAmt, decimal PandFCgstAmt, decimal PandFSgstAmt, decimal PandFTotVal)
        {
            try
            {
                if (PandF > 0)
                {
                    slno++;
                    itemLists.Add(
                    new ItemList
                    {
                        SlNo = slno.ToString(),
                        PrdDesc = "PackForward Charges",
                        IsServc = "Y",
                        HsnCd = "996511",
                        Qty = 1,
                        FreeQty = 0,
                        Unit = "NOS",
                        UnitPrice = PandFAmt,
                        TotAmt = PandFAmt,
                        Discount = 0,
                        PreTaxVal = 0,
                        AssAmt = PandFAmt,
                        GstRt = GST,
                        IgstAmt = PandFIgstAmt,
                        CgstAmt = PandFCgstAmt,
                        SgstAmt = PandFSgstAmt,
                        CesRt = 0,
                        CesAmt = 0,
                        CesNonAdvlAmt = 0,
                        StateCesRt = 0,
                        StateCesAmt = 0,
                        StateCesNonAdvlAmt = 0,
                        OthChrg = 0,
                        TotItemVal = PandFTotVal
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void AddFreightCharges(List<ItemList> itemLists, ref int slno, decimal GST, decimal FreightAmt, decimal FrgtIgstAmt, decimal FrgtCgstAmt, decimal FrgtSgstAmt, decimal FrgtTotVal)
        {
            try
            {
                if (FreightAmt > 0)
                {
                    slno++;
                    itemLists.Add(
                    new ItemList
                    {
                        SlNo = slno.ToString(),
                        PrdDesc = "Freight Charges",
                        IsServc = "Y",
                        HsnCd = "996511",
                        Qty = 1,
                        FreeQty = 0,
                        Unit = "NOS",
                        UnitPrice =Math.Round(FreightAmt,2),
                        TotAmt = Math.Round( FreightAmt,2),
                        Discount = 0,
                        PreTaxVal = 0,
                        AssAmt = Math.Round(FreightAmt,2),
                        GstRt = Math.Round(GST,2),
                        IgstAmt = Math.Round(FrgtIgstAmt,2),
                        CgstAmt = Math.Round( FrgtCgstAmt,2),
                        SgstAmt = Math.Round(FrgtSgstAmt,2),
                        CesRt = 0,
                        CesAmt = 0,
                        CesNonAdvlAmt = 0,
                        StateCesRt = 0,
                        StateCesAmt = 0,
                        StateCesNonAdvlAmt = 0,
                        OthChrg = 0,
                        TotItemVal = FrgtTotVal
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void AddFreightChargesForExport(List<ItemList> itemLists, ref int slno, decimal GST, decimal freight, decimal FreightAmt, decimal FrgtIgstAmt, decimal FrgtCgstAmt, decimal FrgtSgstAmt, decimal FrgtTotVal)
        {
            if (freight > 0)
            {
                slno++;
                itemLists.Add(
                new ItemList
                {
                    SlNo = slno.ToString(),
                    PrdDesc = "Freight Charges",
                    IsServc = "Y",
                    HsnCd = "996511",
                    Qty = 1,
                    FreeQty = 0,
                    Unit = "NOS",
                    UnitPrice = Math.Round(FreightAmt,2),
                    TotAmt = FreightAmt,
                    Discount = 0,
                    PreTaxVal = 0,
                    AssAmt = FreightAmt,
                    GstRt = Math.Round(GST,2),
                    IgstAmt = FrgtIgstAmt,
                    CgstAmt = FrgtCgstAmt,
                    SgstAmt = FrgtSgstAmt,
                    CesRt = 0,
                    CesAmt = 0,
                    CesNonAdvlAmt = 0,
                    StateCesRt = 0,
                    StateCesAmt = 0,
                    StateCesNonAdvlAmt = 0,
                    OthChrg = 0,
                    TotItemVal = FrgtTotVal
                });
            }
        }

        private static void AddPackForwardingChargesForExport(List<ItemList> itemLists, ref int slno, decimal GST, decimal PandF, decimal PandFAmt, decimal PandFIgstAmt, decimal PandFCgstAmt, decimal PandFSgstAmt, decimal PandFTotVal)
        {
            if (PandF > 0)
            {
                slno++;
                itemLists.Add(
                new ItemList
                {
                    SlNo = slno.ToString(),
                    PrdDesc = "PackForward Charges",
                    IsServc = "Y",
                    HsnCd = "996511",
                    Qty = 1,
                    FreeQty = 0,
                    Unit = "NOS",
                    UnitPrice = Math.Round(PandFAmt,2),
                    TotAmt = PandFAmt,
                    Discount = 0,
                    PreTaxVal = 0,
                    AssAmt = PandFAmt,
                    GstRt = Math.Round(GST,2),
                    IgstAmt = PandFIgstAmt,
                    CgstAmt = PandFCgstAmt,
                    SgstAmt = PandFSgstAmt,
                    CesRt = 0,
                    CesAmt = 0,
                    CesNonAdvlAmt = 0,
                    StateCesRt = 0,
                    StateCesAmt = 0,
                    StateCesNonAdvlAmt = 0,
                    OthChrg = 0,
                    TotItemVal = PandFTotVal
                });
            }
        }

        private static void AddInsuranceChargesForExport(List<ItemList> itemLists, ref int slno, decimal GST, decimal Insur, decimal InsuranceAmt, decimal PandFIgstAmt, decimal PandFCgstAmt, decimal PandFSgstAmt, decimal totInsuranceAmt)
        {
            if (Insur > 0)
            {
                slno++;
                itemLists.Add(
                new ItemList
                {
                    SlNo = slno.ToString(),
                    PrdDesc = "Insurance Charges",
                    IsServc = "Y",
                    HsnCd = "996511",
                    Qty = 1,
                    FreeQty = 0,
                    Unit = "NOS",
                    UnitPrice = Math.Round(InsuranceAmt, 2),
                    TotAmt = InsuranceAmt,
                    Discount = 0,
                    PreTaxVal = 0,
                    AssAmt = InsuranceAmt,
                    GstRt = Math.Round(GST, 2),
                    IgstAmt = PandFIgstAmt,
                    CgstAmt = PandFCgstAmt,
                    SgstAmt = PandFSgstAmt,
                    CesRt = 0,
                    CesAmt = 0,
                    CesNonAdvlAmt = 0,
                    StateCesRt = 0,
                    StateCesAmt = 0,
                    StateCesNonAdvlAmt = 0,
                    OthChrg = 0,
                    TotItemVal = totInsuranceAmt
                });
            }
        }


        private async Task<DataSet> UpdateInvoiceDet(string Invno, Enum InvType)
        {
            try
            {
                DataSet dinvoiceDet = new DataSet();
                
                var company = await _unitOfWork.Companies
                            .GetQueryable()
                            .Select(x => new Companydetails
                            {
                                LegalName = x.LegalName,
                                GSTAddress = x.GSTAddress,
                                City = x.City,
                                ZipCode = x.ZipCode,
                                StateCode = x.StateCode,
                                GSTIN = x.GSTIN
                            }).FirstOrDefaultAsync();

                if (company != null)
                {
                    var dt = EinvoiceDatabaseService.ToDataTable(
                        new List<Companydetails> { company });

                    dinvoiceDet.Tables.Add(dt);
                }

                switch (InvType)
                {
                    case Enums.InvType.Sales:
                        var mfgResult = await GetMfgInvoiceByInvNo(Invno);
                        var mfgdt = EinvoiceDatabaseService.ToDataTable(mfgResult);
                        dinvoiceDet.Tables.Add(mfgdt);
                        break;
                    case Enums.InvType.Labour:
                        var labResult = await GetLabourInvoiceByInvNo(Invno);
                        var labdt = EinvoiceDatabaseService.ToDataTable(labResult);
                        dinvoiceDet.Tables.Add(labdt);
                        break;
                    case Enums.InvType.Export:
                        var expResult = await GetExportInvoiceByInvNo(Invno);
                        var expdt = EinvoiceDatabaseService.ToDataTable(expResult);
                        dinvoiceDet.Tables.Add(expdt);

                        break;
                    case Enums.InvType.CreditNote:
                        var crResult = await GetCreditNoteInvoiceByInvNo(Invno);
                        var crdt = EinvoiceDatabaseService.ToDataTable(crResult);
                        dinvoiceDet.Tables.Add(crdt);
                        break;
                    case Enums.InvType.DebitNote:
                        var drResult = await GetDebitNoteInvoiceByInvNo(Invno);
                        var drdt = EinvoiceDatabaseService.ToDataTable(drResult);
                        dinvoiceDet.Tables.Add(drdt);
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

        private async Task<TranDtls> GetTransDtls(DataRow r)
        {
            try
            {
                return new TranDtls()
                {
                    TaxSch = "GST",
                    SupTyp = r["CustSupType"].ToString().Trim().ToUpper(),
                    IgstOnIntra = "N",
                    RegRev = "N"               
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<BuyerDtls> GetBuyerDtls(DataRow r)
        {
            try
            {
                int.TryParse(r["PinCode"].ToString(), out var Bpincode);
                var addr1 = r["CustAddress"].ToString().Trim();
                var addr2 = "";
                if (addr1.Length > 100)
                {
                    addr2 = addr1.Substring(100).Trim();
                    addr1 = addr1.Substring(0, 100).Trim();
                }
                else
                {
                    addr2 = "   ";
                    addr1 = addr1.Substring(0, addr1.Length).Trim();
                }

                BuyerDtls BuyerDtls = new BuyerDtls()
                {
                    Gstin = r["CustGstNo"].ToString().Trim().ToUpper(),
                    LglNm = r["CustName"].ToString().Trim(),
                    Addr1 = addr1,
                    Addr2 = addr2,
                    Loc = r["Location"].ToString().Trim(),
                    Pin = Bpincode,
                    Stcd = r["StateCode"].ToString().Trim(),
                    Pos = r["StateCode"].ToString().Trim(),
                };
                return BuyerDtls;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<SellerDtls> GetSellerDtls(DataRow dr)
        {
            try
            {

                int.TryParse(dr["ZipCode"].ToString(), out var Spincode);
                var addr1 = dr["GSTAddress"].ToString().Trim();
                var addr2 = "";

                if (addr1.Length > 100)
                {
                    addr2 = addr1.Substring(100).Trim();
                    addr1 = addr1.Substring(0, 100).Trim();
                }
                else
                {
                    addr2 = "   ";
                    addr1 = addr1.Substring(0, addr1.Length).Trim();
                }

                SellerDtls SellerDtls = new SellerDtls()
                {
                    Gstin = dr["GSTIN"].ToString().Trim().ToUpper(),
                    LglNm = dr["LegalName"].ToString().Trim(),
                    Addr1 = addr1,
                    Addr2 = addr2,
                    Loc = dr["City"].ToString().Trim(),
                    Pin = Spincode,
                    Stcd = dr["StateCode"].ToString().Trim(),
                };
                return SellerDtls;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<DocDtls> GetDocDtls(DataRow r)
        {
            try
            {
                var invDate = Convert.ToDateTime(r["DocDate"]);
                string docDate = invDate.ToString("dd/MM/yyyy");

                var invNo = IsPrefix
                    ? string.Concat(r["Prefix"]?.ToString()?.Trim(), r["DocNo"]?.ToString()?.Trim())
                    : r["DocNo"]?.ToString()?.Trim();

                return new DocDtls()
                {
                    Typ = r["InvType"]?.ToString()?.Trim(),
                    No = invNo,
                    Dt = docDate
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error in GetDocDtls: " + ex.Message);
            }
        }

        private async Task<ShipDtls> GetShipDtls(DataRow r, string invType)
        {
            try
            {
                if (invType != "CreditNote")
                {
                    if (r.Table.Columns.Contains("AltPincode") && !string.IsNullOrWhiteSpace(r["AltPincode"].ToString()))
                    {
                        int.TryParse(r["AltPincode"].ToString(), out var Bpincode);
                        var addr1 = r["AltCustAddr1"].ToString().Trim();
                        var addr2 = r["AltCustAddr2"].ToString().Trim();

                        if (addr1.Length > 100)
                        {
                            addr2 = addr1.Substring(100).Trim();
                            addr1 = addr1.Substring(0, 100).Trim();
                        }
                        else
                        {
                            if (addr2.Length > 0)
                            {
                                addr2 = addr2;
                                addr1 = addr1.Substring(0, addr1.Length).Trim();
                            }
                            else
                            {
                                addr2 = "   ";
                                addr1 = addr1.Substring(0, addr1.Length).Trim();
                            }

                        }

                        ShipDtls ShipDtls = new ShipDtls()
                        {
                            Gstin = r["AltGSTNo"].ToString().Trim().ToUpper(),
                            LglNm = r["AltCustName"].ToString().Trim(),
                            Addr1 = addr1,
                            Addr2 = addr2,
                            Loc = r["AltLocation"].ToString().Trim(),
                            Pin = Bpincode,
                            Stcd = r["AltStateCode"].ToString().Trim(),
                        };
                        return ShipDtls;
                    }


                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task <AuthEinvoice> GetUserNameandPassword(string username, string password)
        {
            try
            {
                AuthEinvoice auth = new AuthEinvoice();
                auth.UserName = username;
                auth.Password = password;
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
                string Gstin = misc.Rows[0]["GSTIN"].ToString();
                return Gstin;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

   

        public string GetUploadsRootPath()
        {
            try
            {
                var baseDir = AppContext.BaseDirectory;
                DirectoryInfo dir = new DirectoryInfo(baseDir);

                while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "wwwroot")))
                {
                    dir = dir.Parent;
                }

                if (dir != null)
                {
                    return Path.Combine(dir.FullName, "wwwroot");
                }
                return Path.Combine(baseDir, "wwwroot");

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<string> SaveJson(string jsonString, string refType = "ManualUpload")
        {
            try
            {
                var tenant = _tenantProvider.GetCurrentTenant();
                string companyName = tenant?.Hostname;

                companyName = string.IsNullOrWhiteSpace(companyName) ? "DefaultCompany" : companyName.Trim();
                refType = string.IsNullOrWhiteSpace(refType) ? "ManualUpload" : refType.Trim();

                var safeCompany = string.Concat(companyName.Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();
                var safeRefType = string.Concat(refType.Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();

                var basePath = GetUploadsRootPath();

                string folderPath = Path.Combine(basePath, "uploads", safeCompany, "json");

                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                string fileName = $"EInv.json";
                string fullPath = Path.Combine(folderPath, fileName);

                await File.WriteAllTextAsync(fullPath, jsonString);

                return fullPath;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private async Task PreView(string Invno, Enum InvType, string username, string password)
        {
            try
            {

                DataSet dinvoiceDet = await UpdateInvoiceDet(Invno, InvType);

                if (await IsValid(dinvoiceDet, InvType))
                {
                    IAPIService eInvoiceAPI = APIFactory.GetAPIService("EInvoice", await GetJsonData(dinvoiceDet, InvType));
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in PreView()");

            }
        }

        private async Task<List<EInvoiceDocument>> GetMfgInvoiceByInvNo(string invNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invNo))
                    return null;

                var data = await
                    (from d in _unitOfWork.MfgInvs.GetQueryable().AsNoTracking()
                     join s in _unitOfWork.MfgInvSubs.GetQueryable().AsNoTracking()
                         on d.InvId equals s.InvId
                     join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                         on s.ItemId equals i.ItemId
                     join c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                         on d.CustId equals c.CustId

                     // ✅ LEFT JOIN CustomerIndirects
                     join ci in _unitOfWork.CustomerIndirects.GetQueryable().AsNoTracking()
                         on d.ShipAddrsId equals ci.AltCustId into ciGrp
                     from ci in ciGrp.DefaultIfEmpty()

                         // ✅ LEFT JOIN States
                     join st in _unitOfWork.States.GetQueryable().AsNoTracking()
                         on ci.State equals st.StateName into stGrp
                     from st in stGrp.DefaultIfEmpty()

                     where !d.IsCancel
                           && !d.PoShortClose
                           && (string.IsNullOrEmpty(d.ACKNO) || d.ACKNO == "0")
                           && d.InvNo == invNo
                     select new EInvoiceDocument
                     {
                         // ================= HEADER =================
                         InvType = d.InvType,
                         Prefix = d.Prefix,
                         InvId = d.InvId,
                         DocNo = EnsureSuffix(d.InvNo, d.Suffix),
                         DocDate = d.InvDate,
                         TodayVal = d.TodayVal,

                         CustId = c.CustId,
                         CustName = c.CustName,
                         CustAddress = c.CustAddr,
                         CustGstNo = c.GSTNo,
                         Location = c.Location,
                         PinCode = c.PinCode,
                         StateCode = c.StateId,
                         Email = c.Email,
                         CustSupType = c.SupTyp,
                         Distance = (decimal?)c.Distance,

                         TotVal = Math.Round(d.GrandTotal, 2),
                         MainDiscount = d.DiscountPercent,
                         MainDiscAmtOrPer = d.DiscAmtOrPer == true ? false : true,

                         FreightCharges = d.FreightCharges,
                         PandF = d.PackingPercent,
                         PandFAmtOrPer = d.PackingAmtOrPer == true ? false : true,
                         PackingCharges = d.PackingCharges,

                         Insurance = d.InsurancePercent,
                         InsuranceAmtOrPer = d.InsuranceAmtOrPer == true ? false : true,
                         Insurancecharges = d.InsuranceCharges,

                         OtherCharges = Math.Round(Convert.ToDecimal(d.OtherCharges), 2),

                         Cgst = d.CGstRate,
                         Sgst = d.SGstRate,
                         Igst = d.IGstRate,

                         TCS = d.TCSPercent,
                         TCSAmtOrPer = d.TCSAmtOrPer == true ? false : true,
                         TCSCharges = d.TCSAmount,

                         TransportMode = d.TransportMode != null ? d.TransportMode.ToString() : "0",
                         VehicleNo = d.VehicleNo,
                         EwayTransId = d.TransId,
                         EwayTransName = d.TransName,
                         EwayTransDocNo = d.TransDocNo,

                         RoffSales = d.IsRoundOffEnabled,

                         // ================= ITEM =================
                         InvSubId = s.InvSubId,
                         Qty = Math.Round(s.Qty, 2),
                         UnitPrice = Math.Round(s.UnitPrice, 2),
                         TotalAmount = Math.Round(s.Qty * s.UnitPrice, 2),
                         ItemDiscAmt = Math.Round(d.DiscountAmount, 2),
                         ItemWiseCgst = s.LineCGSTRate,
                         ItemWiseSgst = s.LineSGSTRate,
                         ItemWiseIgst = s.LineIGSTRate,

                         ItemCode = i.ItemCode,
                         ItemName = i.ItemName,
                         HSNCode = i.HSNCode,
                         MeasureUnit = i.MeasureUnit,
                         IsService = i.IsServcS,


                         // ================= ALT ADDRESS SAFE =================
                         AltPincode = ci != null ? ci.Pincode : null,
                         AltCustAddr1 = ci != null ? ci.AltCustAddr1 : null,
                         AltCustAddr2 = ci != null ? ci.AltCustAddr2 : null,
                         AltGSTNo = ci != null ? ci.GSTNo : null,
                         AltCustName = ci != null ? ci.AltCustName : null,
                         AltLocation = ci != null ? ci.City : null,

                         AltStateCode = st != null ? st.StateCode : 0

                     }).ToListAsync();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<EInvoiceDocument>> GetLabourInvoiceByInvNo(string invNo)
        {
            try
            {
                int stateCode = 0; 

                if (string.IsNullOrWhiteSpace(invNo))
                    return null;

                var data = await (
                    from d in _unitOfWork.LabInvs.GetQueryable().AsNoTracking()
                    join s in _unitOfWork.LabInvSubs.GetQueryable().AsNoTracking()
                        on d.LabInvId equals s.LabInvId
                    join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                        on s.ItemId equals i.ItemId
                    join c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                        on d.CustId equals c.CustId

                    // ✅ LEFT JOIN CustomerIndirects
                    join ci in _unitOfWork.CustomerIndirects.GetQueryable().AsNoTracking()
                        on d.ShipAddrsId equals ci.AltCustId into ciGrp
                    from ci in ciGrp.DefaultIfEmpty()

                        // ✅ LEFT JOIN States
                    join st in _unitOfWork.States.GetQueryable().AsNoTracking()
                        on (ci != null ? Convert.ToInt32(ci.State) : 0)
                        equals st.StateCode into stGrp
                    from st in stGrp.DefaultIfEmpty()

                    where !d.InvCancel
                          && !d.ShortClose
                          && (string.IsNullOrEmpty(d.ACKNO) || d.ACKNO == "0")
                          && d.LabInvNo == invNo
                    select new EInvoiceDocument
                    {
                        // ================= HEADER =================
                        InvType = d.InvType,
                        Prefix = d.Prefix,
                        InvId = d.LabInvId,
                        DocNo = EnsureSuffix(d.LabInvNo, d.Suffix),
                        DocDate = d.LabInvDate,
                        TodayVal = d.TodayVal,

                        CustId = c.CustId,
                        CustName = c.CustName,
                        CustAddress = c.CustAddr,
                        CustGstNo = c.GSTNo,
                        Location = c.Location,
                        PinCode = c.PinCode,
                        StateCode = c.StateId,
                        Email = c.Email,
                        CustSupType = c.SupTyp,
                        Distance = (decimal?)c.Distance,

                        TotVal = Math.Round(d.GrandTotal, 2),
                        MainDiscount = d.DiscountPercent,
                        MainDiscAmtOrPer = d.DiscAmtOrPer == true ? false : true,

                        FreightCharges = d.FreightCharges,
                        PandF = d.PackingPercent,
                        PandFAmtOrPer = d.PackingAmtOrPer == true ? false : true,
                        PackingCharges = d.PackingCharges,

                        Insurance = d.InsurancePercent,
                        InsuranceAmtOrPer = d.InsuranceAmtOrPer == true ? false : true,
                        Insurancecharges = d.InsuranceCharges,

                        OtherCharges = Math.Round(Convert.ToDecimal(d.OtherCharges), 2),

                        Cgst = d.CGstRate,
                        Sgst = d.SGstRate,
                        Igst = d.IGstRate,

                        TCS = d.TCSPercent,
                        TCSAmtOrPer = d.TCSAmtOrPer == true ? false : true,
                        TCSCharges = d.TCSAmount,

                        RoffSales = d.IsRoundOffEnabled,

                        // ================= ITEM =================
                        InvSubId = s.LabInvSubId,
                        Qty = Math.Round(s.Qty,2),
                        UnitPrice = Math.Round(s.UnitPrice,2),
                        TotalAmount = Math.Round(s.Qty * s.UnitPrice, 2),

                        ItemDiscAmt = Math.Round(d.DiscountAmount, 2),

                        ItemWiseCgst = s.LineCGSTRate,
                        ItemWiseSgst=s.LineSGSTRate,
                        ItemWiseIgst=s.LineIGSTRate,

                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        HSNCode = i.SACCode,
                        MeasureUnit = i.MeasureUnit,
                        IsService = i.IsServcL,


                        // ================= ALT ADDRESS SAFE =================
                        AltPincode = ci != null ? ci.Pincode : null,
                        AltCustAddr1 = ci != null ? ci.AltCustAddr1 : null,
                        AltCustAddr2 = ci != null ? ci.AltCustAddr2 : null,
                        AltGSTNo = ci != null ? ci.GSTNo : null,
                        AltCustName = ci != null ? ci.AltCustName : null,
                        AltLocation = ci != null ? ci.City : null,

                        AltStateCode = st != null  ? st.StateCode : 0


                    }).ToListAsync();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }


        //Export Invoice
        private async Task<List<EInvoiceDocument>> GetExportInvoiceByInvNo(string InvNo)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(InvNo))
                    return null;

                var data = await
                    (from d in _unitOfWork.ExpInvs.GetQueryable().AsNoTracking()
                     join s in _unitOfWork.ExpInvSubs.GetQueryable().AsNoTracking()
                         on d.ExpInvId equals s.ExpInvId
                     join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                         on s.ItemId equals i.ItemId
                     join c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                         on d.CustId equals c.CustId

                     // ✅ LEFT JOIN CustomerIndirects
                     join ci in _unitOfWork.CustomerIndirects.GetQueryable().AsNoTracking()
                         on d.ShipAddrsId equals ci.AltCustId into ciGrp
                     from ci in ciGrp.DefaultIfEmpty()

                         // ✅ LEFT JOIN States
                     join st in _unitOfWork.States.GetQueryable().AsNoTracking()
                         on ci.State equals st.StateName into stGrp
                     from st in stGrp.DefaultIfEmpty()

                     where !d.IsCancel
                           && !d.InvShortClose
                           && (string.IsNullOrEmpty(d.ACKNO) || d.ACKNO == "0")
                           && d.ExpInvNo == InvNo
                     select new EInvoiceDocument
                     {
                         // ================= HEADER =================
                         InvType = d.InvType,
                         Prefix = d.Prefix,
                         InvId = d.ExpInvId,
                         DocNo = EnsureSuffix(d.ExpInvNo, d.Suffix),
                         DocDate = d.ExpInvDate,
                         TodayVal = d.TodayVal,

                         CustId = c.CustId,
                         CustName = c.CustName,
                         CustAddress = c.CustAddr,
                         CustGstNo = c.GSTNo,
                         Location = c.Location,
                         PinCode = c.PinCode,
                         StateCode = c.StateId,
                         Email = c.Email,
                         CustSupType = c.SupTyp,
                         Distance = (decimal?)c.Distance,

                         TotVal = Math.Round(d.GrandTotal, 2),
                         MainDiscount = d.DiscountPercent,
                         MainDiscAmtOrPer = d.DiscAmtOrPer == true ? false : true,

                         FreightCharges = d.FreightCharges,
                         PandF = d.PackingPercent,
                         PandFAmtOrPer = d.PackingAmtOrPer == true ? false : true,
                         PackingCharges = d.PackingCharges,

                         Insurance = d.InsurancePercent,
                         InsuranceAmtOrPer = d.InsuranceAmtOrPer == true ? false : true,
                         Insurancecharges = d.InsuranceCharges,

                         OtherCharges = Math.Round(Convert.ToDecimal(d.OtherCharges), 2),

                         Cgst = d.CGstRate,
                         Sgst = d.SGstRate,
                         Igst = d.IGstRate,

                         TCS = d.TCSPercent,
                         TCSAmtOrPer = d.TCSAmtOrPer == true ? false : true,
                         TCSCharges = d.TCSAmount,

                         TransportMode = d.TransportMode != null ? d.TransportMode.ToString() : "0",
                         VehicleNo = d.VehicleNo,
                         EwayTransId = d.TransId,
                         EwayTransName = d.TransName,
                         EwayTransDocNo = d.TransDocNo,

                         RoffSales = d.IsRoundOffEnabled,

                         // ================= ITEM =================
                         InvSubId = s.ExpInvSubId,
                         Qty = Math.Round(s.Qty, 2),
                         UnitPrice = Math.Round(s.UnitPrice, 2),
                         TotalAmount = Math.Round(s.Qty * s.UnitPrice, 2),
                         ItemDiscAmt = Math.Round(d.DiscountAmount, 2),
                         ItemWiseCgst = s.LineCGSTRate,
                         ItemWiseSgst = s.LineSGSTRate,
                         ItemWiseIgst = s.LineIGSTRate,

                         ItemCode = i.ItemCode,
                         ItemName = i.ItemName,
                         HSNCode = i.HSNCode,
                         MeasureUnit = i.MeasureUnit,
                         IsService = i.IsServcS,


                         // ================= ALT ADDRESS SAFE =================
                         AltPincode = ci != null ? ci.Pincode : null,
                         AltCustAddr1 = ci != null ? ci.AltCustAddr1 : null,
                         AltCustAddr2 = ci != null ? ci.AltCustAddr2 : null,
                         AltGSTNo = ci != null ? ci.GSTNo : null,
                         AltCustName = ci != null ? ci.AltCustName : null,
                         AltLocation = ci != null ? ci.City : null,

                         AltStateCode = st != null ? st.StateCode : 0

                     }).ToListAsync();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Credit Note 
        private async Task<List<EInvoiceDocument>> GetCreditNoteInvoiceByInvNo(string InvNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InvNo))
                    return null;

                var data = await
                    (from d in _unitOfWork.CreditNotes.GetQueryable().AsNoTracking()
                     join s in _unitOfWork.CreditNoteSubs.GetQueryable().AsNoTracking()
                         on d.CrId equals s.CrId
                     join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                         on s.ItemId equals i.ItemId
                     join c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                         on d.CustId equals c.CustId

                     // ✅ LEFT JOIN CustomerIndirects
                     join ci in _unitOfWork.CustomerIndirects.GetQueryable().AsNoTracking()
                         on d.ShipAddrsId equals ci.AltCustId into ciGrp
                     from ci in ciGrp.DefaultIfEmpty()

                         // ✅ LEFT JOIN States
                     join st in _unitOfWork.States.GetQueryable().AsNoTracking()
                         on ci.State equals st.StateName into stGrp
                     from st in stGrp.DefaultIfEmpty()
                     where
                          (string.IsNullOrEmpty(d.ACKNO) || d.ACKNO == "0")
                           && d.CreditNo == InvNo
                     select new EInvoiceDocument
                     {
                         // ================= HEADER =================
                         InvType = d.InvType,
                         Prefix = d.Prefix,
                         InvId = d.CrId,
                         DocNo = EnsureSuffix(d.CreditNo, d.Suffix),
                         DocDate = d.CreditDate,
                         TodayVal = d.TodayVal,

                         CustId = c.CustId,
                         CustName = c.CustName,
                         CustAddress = c.CustAddr,
                         CustGstNo = c.GSTNo,
                         Location = c.Location,
                         PinCode = c.PinCode,
                         StateCode = c.StateId,
                         Email = c.Email,
                         CustSupType = c.SupTyp,
                         Distance = (decimal?)c.Distance,

                         TotVal = Math.Round(d.GrandTotal, 2),
                         MainDiscount = d.DiscountPercent,
                         MainDiscAmtOrPer = d.DiscAmtOrPer == true ? false : true,

                         FreightCharges = d.FreightCharges,
                         PandF = d.PackingPercent,
                         PandFAmtOrPer = d.PackingAmtOrPer == true ? false : true,
                         PackingCharges = d.PackingCharges,

                         Insurance = d.InsurancePercent,
                         InsuranceAmtOrPer = d.InsuranceAmtOrPer == true ? false : true,
                         Insurancecharges = d.InsuranceCharges,

                         OtherCharges = Math.Round(Convert.ToDecimal(d.OtherCharges), 2),

                         Cgst = d.CGstRate,
                         Sgst = d.SGstRate,
                         Igst = d.IGstRate,

                         TCS = d.TCSPercent,
                         TCSAmtOrPer = d.TCSAmtOrPer == true ? false : true,
                         TCSCharges = d.TCSAmount,

                         //TransportMode = d.TransportMode != null ? d.TransportMode.ToString() : "0",
                         //VehicleNo = d.VehicleNo,
                         //EwayTransId = d.TransId,
                         //EwayTransName = d.TransName,
                         //EwayTransDocNo = d.TransDocNo,

                         RoffSales = d.IsRoundOffEnabled,

                         // ================= ITEM =================
                         InvSubId = s.ExpInvSubId,
                         Qty = Math.Round(s.Qty, 2),
                         UnitPrice = Math.Round(s.UnitPrice, 2),
                         TotalAmount = Math.Round(s.Qty * s.UnitPrice, 2),
                         ItemDiscAmt = Math.Round(d.DiscountAmount, 2),
                         ItemWiseCgst = s.LineCGSTRate,
                         ItemWiseSgst = s.LineSGSTRate,
                         ItemWiseIgst = s.LineIGSTRate,

                         ItemCode = i.ItemCode,
                         ItemName = i.ItemName,
                         HSNCode = i.HSNCode,
                         MeasureUnit = i.MeasureUnit,
                         IsService = i.IsServcS,


                         // ================= ALT ADDRESS SAFE =================
                         AltPincode = ci != null ? ci.Pincode : null,
                         AltCustAddr1 = ci != null ? ci.AltCustAddr1 : null,
                         AltCustAddr2 = ci != null ? ci.AltCustAddr2 : null,
                         AltGSTNo = ci != null ? ci.GSTNo : null,
                         AltCustName = ci != null ? ci.AltCustName : null,
                         AltLocation = ci != null ? ci.City : null,

                         AltStateCode = st != null ? st.StateCode : 0

                     }).ToListAsync();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<EInvoiceDocument>> GetDebitNoteInvoiceByInvNo(string InvNo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(InvNo))
                    return null;

                var data = await
                    (from d in _unitOfWork.DebitNotes.GetQueryable().AsNoTracking()
                     join s in _unitOfWork.DebitNoteSubs.GetQueryable().AsNoTracking()
                         on d.DbId equals s.DbId
                     join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                         on s.ItemId equals i.ItemId
                     join c in _unitOfWork.Vendors.GetQueryable().AsNoTracking()
                         on d.VendorCode equals c.VendorCode

                     // ✅ LEFT JOIN CustomerIndirects
                     join ci in _unitOfWork.VendorInDirects.GetQueryable().AsNoTracking()
                         on d.ShipAddrsId equals ci.AltVendorCode into ciGrp
                     from ci in ciGrp.DefaultIfEmpty()

                         // ✅ LEFT JOIN States
                     join st in _unitOfWork.States.GetQueryable().AsNoTracking()
                         on ci.StateName equals st.StateName into stGrp
                     from st in stGrp.DefaultIfEmpty()
                     where
                          (string.IsNullOrEmpty(d.ACKNO) || d.ACKNO == "0")
                           && d.DebitNo == InvNo
                     select new EInvoiceDocument
                     {
                         // ================= HEADER =================
                         InvType = d.InvType,
                         Prefix = d.Prefix,
                         InvId = d.DbId,
                         DocNo = EnsureSuffix(d.DebitNo, d.Suffix),
                         DocDate = d.DebitDate,
                         TodayVal = d.TodayVal,

                         CustId = c.VendorCode,
                         CustName = c.VendorName,
                         CustAddress = c.VendorAddress,
                         CustGstNo = c.GSTNo,
                         Location = c.Location,
                         PinCode = c.PinCode,
                         StateCode = c.StateCode,
                         Email = c.Email,
                         CustSupType = "B2B",
                         Distance = (decimal?)c.Distance,

                         TotVal = Math.Round(d.GrandTotal, 2),
                         MainDiscount = d.DiscountPercent,
                         MainDiscAmtOrPer = d.DiscAmtOrPer == true ? false : true,

                         FreightCharges = d.FreightCharges,
                         PandF = d.PackingPercent,
                         PandFAmtOrPer = d.PackingAmtOrPer == true ? false : true,
                         PackingCharges = d.PackingCharges,

                         Insurance = d.InsurancePercent,
                         InsuranceAmtOrPer = d.InsuranceAmtOrPer == true ? false : true,
                         Insurancecharges = d.InsuranceCharges,

                         OtherCharges = Math.Round(Convert.ToDecimal(d.OtherCharges), 2),

                         Cgst = d.CGstRate,
                         Sgst = d.SGstRate,
                         Igst = d.IGstRate,

                         TCS = d.TCSPercent,
                         TCSAmtOrPer = d.TCSAmtOrPer == true ? false : true,
                         TCSCharges = d.TCSAmount,

                         //TransportMode = d.TransportMode != null ? d.TransportMode.ToString() : "0",
                         //VehicleNo = d.VehicleNo,
                         //EwayTransId = d.TransId,
                         //EwayTransName = d.TransName,
                         //EwayTransDocNo = d.TransDocNo,

                         RoffSales = d.IsRoundOffEnabled,

                         // ================= ITEM =================
                         InvSubId = s.DbSubId,
                         Qty = Math.Round(s.Qty, 2),
                         UnitPrice = Math.Round(s.UnitPrice, 2),
                         TotalAmount = Math.Round(s.Qty * s.UnitPrice, 2),
                         ItemDiscAmt = Math.Round(d.DiscountAmount, 2),
                         ItemWiseCgst = s.LineCGSTRate,
                         ItemWiseSgst = s.LineSGSTRate,
                         ItemWiseIgst = s.LineIGSTRate,

                         ItemCode = i.ItemCode,
                         ItemName = i.ItemName,
                         HSNCode = i.HSNCode,
                         MeasureUnit = i.MeasureUnit,
                         IsService = i.IsServcS,


                         // ================= ALT ADDRESS SAFE =================
                         AltPincode = ci != null ? ci.PinCode : null,
                         AltCustAddr1 = ci != null ? ci.AltVendorAddr1 : null,
                         AltCustAddr2 = ci != null ? ci.AltVendorAddr2 : null,
                         AltGSTNo = ci != null ? ci.GSTNo : null,
                         AltCustName = ci != null ? ci.AltVendorName : null,
                         AltLocation = ci != null ? ci.City : null,

                         AltStateCode = st != null ? st.StateCode : 0

                     }).ToListAsync();

                return data;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string EnsureSuffix(string InvNo, string suffix)
        {
            if (string.IsNullOrWhiteSpace(suffix))
                return InvNo;

            string expectedSuffix = "/";

            if (string.IsNullOrWhiteSpace(InvNo))
                return expectedSuffix;

            if (InvNo.EndsWith(expectedSuffix, StringComparison.OrdinalIgnoreCase))
                return InvNo;

            return InvNo + suffix;
        }

        public async Task<bool> IsCreditNoteExportedAsync(string creditNo)
        {
            return await _unitOfWork.CreditNotes
                .GetQueryable()
                .AnyAsync(x => x.CreditNo == creditNo && x.Export == true);
        }
    }
}
