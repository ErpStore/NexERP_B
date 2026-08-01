using V.SMART.Shared.E_Invoice.E_InvoiceHelper;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.Extensions;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public class LicenseProductKey: ILicenseProductKey
    {
        IDictionary<string, string> decryptedDictionary;


        public LicenseProductKey(string productKey)
        {
            decryptedDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Decrypt(productKey));

        }

        public static string Decrypt(string base64EncryptedData)
        {
            string encryptKey = "VxJj2VVb/c5z6wIEuKEpC9dEw6BU4NUngl0ajF27kTM=";
            string encryptiv = "DXv+We6MtMadzJ4H6Qylqw==";
            byte[] encryptedData = Convert.FromBase64String(base64EncryptedData);
            byte[] key = Convert.FromBase64String(encryptKey);
            byte[] iv = Convert.FromBase64String(encryptiv);
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream(encryptedData))
                {
                    using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        private bool CheckForNoOfDays(string endDate, out int noOfDays)
        {
            noOfDays = 0;

            if (string.IsNullOrWhiteSpace(endDate))
                return false;

            string[] formats =
            {
                "dd/MM/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss",
                "yyyy-MM-dd HH:mm:ss",

                "dd-MM-yyyy HH:mm:ss",
                "MM-dd-yyyy HH:mm:ss",

                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "yyyy/MM/dd",

                "dd-MM-yyyy",
                "MM-dd-yyyy",
                "yyyy-MM-dd"
            };

            if (!DateTime.TryParseExact(
                    endDate.Trim(),
                    formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime parsedDate))
            {
                throw new Exception($"Invalid EndDate format: {endDate}");
            }

            noOfDays = (parsedDate.Date - DateTime.Today).Days;

            return noOfDays > 0;
        }

        public async Task<bool> IsValidProductKey()
        {
            if (decryptedDictionary == null)
                return true;

            if (!decryptedDictionary.TryGetValue("EndDate", out var endDate))
                return true;

            if (!CheckForNoOfDays(endDate, out var noOfDays))
            {
                throw new InvalidOperationException(
                    "Sorry!!! Evaluation of E-Invoice has been Expired! Please contact Bhargavi Soft-Tech Pvt Ltd: +91-9844046791, 8660821782, BHARGAVI SOFT-TECH PVT LTD");
            }

            if (noOfDays < 7)
            {
                throw new InvalidOperationException(
                    $"E-Invoice License is going to expire in {noOfDays} days, Please contact: +91-9844046791, 8660821782, BHARGAVI SOFT-TECH PVT LTD");
            }

            return true;
        }

        public async Task<AuthEinvoice> GetUserName()
        {
            if (decryptedDictionary != null)
            {
                if (decryptedDictionary.TryGetValue("username", out var username) && decryptedDictionary.TryGetValue("password", out var password))
                {
                    return new AuthEinvoice { UserName = username, Password = password };
                }
            }
            //await _js.ToastrWarning($"User Name or Password not correct in the license key Please contact: +91-9844046791, 8660821782, BHARGAVI SOFT-TECH PVT LTD");
            Environment.Exit(1);
            return null;
        }

        public async Task<AuthEWay> GetUserNameEway()
        {
            if (decryptedDictionary != null)
            {
                if (decryptedDictionary.TryGetValue("username", out var username) && decryptedDictionary.TryGetValue("password", out var password))
                {
                    //return new AuthEWay { };
                    return new AuthEWay { UserName = username, Password = password };
                }
            }
            //await _js.ToastrWarning($"User Name or Password not correct in the license key Please contact: +91-9844046791, 8660821782, BHARGAVI SOFT-TECH PVT LTD");
            Environment.Exit(1);
            return null;
        }

    }
}
