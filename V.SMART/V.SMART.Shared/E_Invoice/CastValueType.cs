using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public static class CastValueType
    {
        public static T ValueConverter<T>(this string str)
        {
            if (str == null || string.IsNullOrEmpty(str))
            {
                return default(T);
            }

            return (T)Convert.ChangeType(str, typeof(T));
        }
    }
}
