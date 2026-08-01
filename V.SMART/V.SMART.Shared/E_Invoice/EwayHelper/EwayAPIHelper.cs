using V.SMART.Shared.E_Invoice.E_InvoiceHelper;
using V.SMART.Shared.Services.Extensions;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace V.SMART.Shared.E_Invoice.EwayHelper
{
    public class EwayAPIHelper: IAPIService
    {
        private string _JsonData;
        private byte[] _aeskey;
        private string appkey;
        private readonly IJSRuntime _js;

        public EwayAPIHelper(string jsonData)
        {
            _JsonData = jsonData;
           
        }

        public async Task<Jsonpayload> GetResponseByJsonData(AuthenticationDetails authentication)
        {
            try
            {
                AuthResponse authResponse = await PostAuthenticate(authentication).ConfigureAwait(true);
                _aeskey = Convert.FromBase64String(appkey);
                string enrytedsek = authResponse.Sek;
                var decryptedsek = DecryptBySymmerticKey(enrytedsek, _aeskey);
                byte[] decryptedsekbyte = Convert.FromBase64String(decryptedsek);
                var encrypteddata = EncryptBySymmetricKey(_JsonData, decryptedsek);
                var ewayResponse = await PostJsonData(authentication, authResponse, encrypteddata).ConfigureAwait(true);
                var ewaydet = DecryptBySymmerticKey(ewayResponse, decryptedsekbyte);
                var ewaydettt = Base64UrlDecode(ewaydet);
                var encd = Encoding.UTF8.GetString(ewaydettt);
                var response = JsonConvert.DeserializeObject<Jsonpayload>(encd);
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return null;
        }

        public async Task<AuthResponse> PostAuthenticate(AuthenticationDetails authenticationDetails)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //string uri = "https://developers.eraahi.com/api/ewaybillapi/v1.03/auth";
                    string uri = "https://newewaybill.alankitgst.com/ewaybillgateway/v1.03/auth";
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", authenticationDetails.SecretKey);
                    client.DefaultRequestHeaders.Add("Gstin", authenticationDetails.Gstin);
                    _aeskey = GetAppKey();
                    appkey = Convert.ToBase64String(_aeskey);
                    authenticationDetails.AuthDetailsEWay.app_key = appkey;
                    string authStr = JsonConvert.SerializeObject(authenticationDetails.AuthDetailsEWay);
                    byte[] authBytes = Encoding.UTF8.GetBytes(authStr);
                    return await GetResponse(authenticationDetails, client, uri, authBytes, new AuthResponse()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> PostJsonData(AuthenticationDetails authenticationDetails, AuthResponse authResponse, string encrypteddata)
        {
            try
            {
                APIResponse irnResponse = null;
                using (var client = new HttpClient())
                {
                    //string uri = "https://developers.eraahi.com/api/ewaybillapi/v1.03/ewayapi";
                    string uri = "https://newewaybill.alankitgst.com/ewaybillgateway/v1.03/ewayapi";
                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", authenticationDetails.SecretKey);
                    client.DefaultRequestHeaders.Add("Gstin", authenticationDetails.Gstin);
                    client.DefaultRequestHeaders.Add("AuthToken", authResponse.AuthToken);

                    RequestPayloadN requestPayloadN = new RequestPayloadN() { action = "GENEWAYBILL", Data = encrypteddata };
                    string jsonPayload = JsonConvert.SerializeObject(requestPayloadN);
                    StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                    //HttpResponseMessage response = await client.PostAsync(uri, content).ConfigureAwait(true);
                    HttpResponseMessage response = client.PostAsync(uri, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                        var respObj = JsonConvert.DeserializeObject<APIResponse>(responseContent);

                        if (respObj.Status == "0")
                        {
                            byte[] data = Convert.FromBase64String(respObj.error);
                            var Key = JObject.Parse(Encoding.UTF8.GetString(data)).GetValue("errorCodes")?.ToString();

                            //GetErrorValuesByKey(Key);
                            string[] code = Key.Split(',');
                            int.TryParse(code[0].ToString(), out int Code);

                            string errorMessage = EWayBillErrors.GetErrorMessage(Code);

                            throw new InvalidOperationException(errorMessage);
                            
                        }
                        else
                        {
                            irnResponse = JsonConvert.DeserializeObject<APIResponse>(responseContent);
                        }
                        
                    }
                    else
                    {
                        throw new Exception();
                    }
                    return irnResponse.Data;
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


        private async Task<AuthResponse> GetResponse(AuthenticationDetails authenticationDetails, HttpClient client, string uri, byte[] authBytes, AuthResponse auth)
        {
            HttpResponseMessage response = null;
            try
            {
                RequestPayloadN aRequestPayload = GetRequestPayloadN(authenticationDetails, authBytes);
                string jsonPayload = JsonConvert.SerializeObject(aRequestPayload);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                //response = await client.PostAsync(uri, content).ConfigureAwait(true);
                response = client.PostAsync(uri, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                    auth = JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                    
                }
                else
                {
                    throw new Exception();
                }

                return auth;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private RequestPayloadN GetRequestPayloadN(AuthenticationDetails authenticationDetails, byte[] authBytes)
        {
            return new RequestPayloadN()
            {
                Data = EncryptData(Convert.ToBase64String(authBytes), authenticationDetails.EwayPublicKey)
            };
        }

        private static byte[] GetAppKey()
        {
            Aes keys = Aes.Create();
            byte[] secretKey = keys.Key;
            return secretKey;
        }

        public string EncryptData(string data, string publicKey)
        {
            try
            {
                byte[] keyBytes = Convert.FromBase64String(publicKey);

                AsymmetricKeyParameter asymmetricKeyParameter = PublicKeyFactory.CreateKey(keyBytes);
                RsaKeyParameters rsaKeyParameters = (RsaKeyParameters)asymmetricKeyParameter;
                RSAParameters rsaParameters = new RSAParameters();
                rsaParameters.Modulus = rsaKeyParameters.Modulus.ToByteArrayUnsigned();
                rsaParameters.Exponent = rsaKeyParameters.Exponent.ToByteArrayUnsigned();
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(rsaParameters);
                byte[] plaintext = Encoding.UTF8.GetBytes(data);
                byte[] ciphertext = rsa.Encrypt(plaintext, false);
                string cipherresult = Convert.ToBase64String(ciphertext);
                return cipherresult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string DecryptBySymmerticKey(string encryptedText, byte[] key)
        {
            try
            {
                byte[] dataToDecrypt = Convert.FromBase64String(encryptedText);
                var keyBytes = key;
                AesManaged tdes = new AesManaged();
                tdes.KeySize = 256;
                tdes.BlockSize = 128;
                tdes.Key = keyBytes;
                tdes.Mode = CipherMode.ECB;
                tdes.Padding = PaddingMode.PKCS7;
                ICryptoTransform decrypt__1 = tdes.CreateDecryptor();
                byte[] deCipher = decrypt__1.TransformFinalBlock(dataToDecrypt, 0, dataToDecrypt.Length);
                tdes.Clear();
                string EK_result = Convert.ToBase64String(deCipher);
                return EK_result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static byte[] Base64UrlDecode(string base64UrlEncodedString)
        {
            // Convert Base64 URL-encoded string to Base64 string
            string base64String = base64UrlEncodedString.Replace('-', '+').Replace('_', '/');

            // Add padding characters if necessary
            switch (base64String.Length % 4)
            {
                case 2: base64String += "=="; break;
                case 3: base64String += "="; break;
            }

            // Decode Base64 string
            return Convert.FromBase64String(base64String);
        }

        public string EncryptBySymmetricKey(string jsondata, string sek)
        {
            //Encrypting SEK
            try
            {
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(jsondata);
                byte[] keyBytes = Convert.FromBase64String(sek);

                using (AesManaged aes = new AesManaged())
                {
                    aes.KeySize = 256;
                    aes.BlockSize = 128;
                    aes.Key = keyBytes;
                    aes.Mode = CipherMode.ECB;
                    aes.Padding = PaddingMode.PKCS7;
                    ICryptoTransform encryptor = aes.CreateEncryptor();
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                    aes.Clear();
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
