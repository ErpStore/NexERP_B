using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public interface IEncrypt
    {
        string EncryptData(string data, string publicKey);

        string EncryptBySymmetricKey(string jsondata, string sek);
    }

    public interface IDecrypt
    {
        string DecryptBySymmerticKey(string encryptedText, byte[] key);

    }
}
