using FastReport.Code.CodeDom.Compiler;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public static  class UpiQrService
    {
        public static byte[] GenerateUpiQr(string upiId, string name, decimal amount, string note)
        {
            string upiString = $"upi://pay?pa={upiId}&pn={name}&am={amount}&cu=INR&tn={note}";

            using QRCodeGenerator qrGenerator = new();
            using QRCodeData qrData = qrGenerator.CreateQrCode(upiString, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new(qrData);

            return qrCode.GetGraphic(20);
        }
    }
}
