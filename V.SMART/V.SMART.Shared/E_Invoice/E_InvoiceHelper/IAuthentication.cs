using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice.E_InvoiceHelper
{
    public interface IAuthentication
    {
        Task<AuthResponse> PostAuthenticate(AuthenticationDetails authenticationDetails);
    }

    public class AuthenticationDetails
    {
        private AuthEinvoice _authEinvoice;
        private AuthEWay _authEway;

        private string _gstin;
        public string SecretKey => "AL5z0E3V6I8Z7q4l2X"; //Provided from Alankit
        public string Gstin => _gstin;
        public string EwayPublicKey => "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjo1FvyiKcQ9hDR2+vH0+O2XazuLbo2bPfRiiUnpaPhE3ly+Pwh05gvEuzo2UhUIDg98cX4E0vbfWOF1po2wWTBxb8jMY1nAJ8fz1xyHc1Wa7KZ0CeTvAGeifkMux7c22pMu6pBGJN8f3q7MnIW/uSJloJF6+x4DZcgvnDUlgZD3Pcoi3GJF1THbWQi5pDQ8U9hZsSJfpsuGKnz41QRsKs7Dz7qmcKT2WwN3ULWikgCzywfuuREWb4TVE2p3e9WuoDNPUziLZFeUfMP0NqYsiGVYHs1tVI25G42AwIVJoIxOWys8Zym9AMaIBV6EMVOtQUBbNIZufix/TwqTlxNPQVwIDAQAB";//EinvoiceResources.EwayPublicKey;
        public string EinvoicePublicKey => "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAjo1FvyiKcQ9hDR2+vH0+O2XazuLbo2bPfRiiUnpaPhE3ly+Pwh05gvEuzo2UhUIDg98cX4E0vbfWOF1po2wWTBxb8jMY1nAJ8fz1xyHc1Wa7KZ0CeTvAGeifkMux7c22pMu6pBGJN8f3q7MnIW/uSJloJF6+x4DZcgvnDUlgZD3Pcoi3GJF1THbWQi5pDQ8U9hZsSJfpsuGKnz41QRsKs7Dz7qmcKT2WwN3ULWikgCzywfuuREWb4TVE2p3e9WuoDNPUziLZFeUfMP0NqYsiGVYHs1tVI25G42AwIVJoIxOWys8Zym9AMaIBV6EMVOtQUBbNIZufix/TwqTlxNPQVwIDAQAB";//EinvoiceResources.PublicKey;

        //public string EinvoicePublicKey => EinvoiceResources.EinvoicePublicKey;
        public AuthEinvoice AuthDetailsEInvoice => _authEinvoice;
        public AuthEWay AuthDetailsEWay => _authEway;

        public AuthenticationDetails(AuthEinvoice auth, string gst)
        {
            _authEinvoice = auth;
            _gstin = gst;
        }

        public AuthenticationDetails(AuthEWay auth, string gst)
        {
            _authEway = auth;
            _gstin = gst;
        }
    }
    public class AuthEinvoice
    {
        public string Password { get; set; }
        public string UserName { get; set; }
        public string AppKey { get; set; }
        public bool ForceRefreshAccessToken => false;
    }

    public class AuthEWay
    {
        public string action => "ACCESSTOKEN";
        public string Password { get; set; }
        public string app_key { get; set; }
        public string UserName { get; set; }
    }
}
