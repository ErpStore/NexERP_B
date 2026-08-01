using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.AccountsModule
{
    public class PaymentTypeModel
    {
        public int PaymentTypeId { get; set; }
        public string? PaymentTypeName { get; set; }
    }
    public static class TransTypes
    {
        public const string Payment = "PAYMENT";
        public const string Receipt = "RECEIPT";
        public const string PayAdvance = "PAY_ADVANCE";
        public const string BillAdjust = "BILL_ADJUST";
        public const string CustomerService = "CUST_SERVICE";
        public const string vendorService = "VEND_SERVICE";
        // 🔹 New types added
        public const string VendOpeningBalanc = "VEND_OPENING_BAL_RECEIPT";
        public const string CustomerOpeningBalance = "CUST_OPENING_BAL_RECEIPT";
    }


}
