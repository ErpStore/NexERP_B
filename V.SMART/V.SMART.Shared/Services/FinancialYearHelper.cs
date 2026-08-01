using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public static class FinancialYearHelper
    {
        public static string GetFinancialYearSuffix(DateTime date)
        {
            int year = date.Month >= 4 ? date.Year : date.Year - 1;  // April = financial year start
            int nextYear = year + 1;

            return $"/{year}-{nextYear.ToString().Substring(2)}";
        }
    }
}
