using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice.EwayHelper
{
    public class EwayJsonSerializer
    {
        public string supplyType;
        public string subSupplyType;
        public string subSupplyDesc;
        public string docType;
        public string docNo;
        public string docDate;
        public string fromTrdName;
        public string fromGstin;
        public string fromAddr1;
        public string fromAddr2;
        public string fromPlace;
        public int fromPincode;
        public int fromStateCode;
        public int actFromStateCode;
        public string toGstin;
        public string toTrdName;
        public string toAddr1;
        public string toAddr2;
        public string toPlace;
        public int toPincode;
        public int toStateCode;
        public int actToStateCode;
        public decimal totalValue;
        public decimal cgstValue;
        public decimal sgstValue;
        public decimal igstValue;
        public decimal othersValue;
        public decimal totInvValue;
        public string transporterId;
        public string transporterName;
        public string transDocNo;
        public string transMode;
        public int transDistance;
        public string transDocDate;
        public string vehicleNo;
        public string vehicleType;
        public int transactionType;
        public List<EwayItemList> itemList;
    }
    public class EwayItemList
    {
        public string productName { get; set; }
        public string productDesc { get; set; }
        public long hsnCode { get; set; }
        public decimal quantity { get; set; }
        public string qtyUnit { get; set; }
        public decimal cgstRate { get; set; }
        public decimal sgstRate { get; set; }
        public decimal igstRate { get; set; }
        public decimal taxableAmount { get; set; }
    }
}
