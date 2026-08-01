using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Utility_Constants
{
    public static class CommonConstants
    {
        public static readonly List<decimal> IGSTRates = new()
        {
            0.000m, 0.100m, 0.250m, 1.000m, 1.500m,
            3.000m, 5.000m, 6.000m, 7.500m, 12.000m,
            18.000m, 28.000m
        };

        public static readonly List<decimal> GSTRates = new()
        {
            0.000m, 0.050m, 0.125m, 0.500m, 0.750m,
            1.500m, 2.500m, 3.000m, 3.750m, 6.000m,
            9.000m, 14.000m
        };

        public static decimal GetIGST(decimal rate) => IGSTRates.FirstOrDefault(r => r == rate);
        public static decimal GetGST(decimal rate) => GSTRates.FirstOrDefault(r => r == rate);

    }
}
