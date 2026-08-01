using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Rendering;

namespace V.SMART.Shared.Services
{
    public class BarcodeService
    {
        public byte[] GenerateBarcodeBytes(string text)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 120,                  // Increased for clarity
                    Width = 350,                   // Wider = less compression
                    Margin = 2,
                    PureBarcode = true
                }
            };

            var pixelData = writer.Write(text);
            return ConvertPixelDataToBytes(pixelData);
        }

        public byte[] GenerateQrBytes(string text)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = 250,
                    Width = 250,
                    Margin = 1,
                    ErrorCorrection = ErrorCorrectionLevel.Q
                }
            };

            var pixelData = writer.Write(text);
            return ConvertPixelDataToBytes(pixelData);
        }

        private byte[] ConvertPixelDataToBytes(PixelData pixelData)
        {
            using var bitmap = new Bitmap(pixelData.Width, pixelData.Height, PixelFormat.Format32bppArgb);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppArgb);

            System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, data.Scan0, pixelData.Pixels.Length);

            bitmap.UnlockBits(data);

            using var ms = new MemoryStream();
            bitmap.SetResolution(300, 300);  // 🔥 IMPORTANT: High DPI for scanning
            bitmap.Save(ms, ImageFormat.Png);

            return ms.ToArray();
        }
    }
}
