using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice.E_InvoiceHelper
{
    public class JsonSerializer
    {
        public string Version = "1.1";
        public TranDtls TranDtls;
        public DocDtls DocDtls;
        public SellerDtls SellerDtls;
        public BuyerDtls BuyerDtls;
        public ShipDtls ShipDtls;
        public List<ItemList> ItemList;
        public ValDtls ValDtls;
        public Ewaybill EwbDtls;
    }

    public class TranDtls
    {
        public string TaxSch { set; get; }
        public string SupTyp { set; get; }
        public string IgstOnIntra { set; get; }
        public string RegRev { set; get; }
        public string EcmGstin { set; get; }
    }

    public class DocDtls
    {
        public string Typ { set; get; }
        public string No { set; get; }
        public string Dt { set; get; }

    }

    public class SellerDtls
    {
        public string Gstin { set; get; }
        public string LglNm { set; get; }
        public string Addr1 { set; get; }
        public string Addr2 { set; get; }
        public string Loc { set; get; }
        public int Pin { set; get; }
        public string Stcd { set; get; }
        public string Ph { set; get; }
        public string Em { set; get; }
    }

    public class BuyerDtls
    {
        public string Gstin { set; get; }
        public string LglNm { set; get; }
        public string Addr1 { set; get; }
        public string Addr2 { set; get; }
        public string Loc { set; get; }
        public int Pin { set; get; }
        public string Stcd { set; get; }
        public string Pos { set; get; }
        public string Ph { set; get; }
        public string Em { set; get; }
    }
    public class ShipDtls
    {
        public string Gstin { set; get; }
        public string LglNm { set; get; }
        public string Addr1 { set; get; }
        public string Addr2 { set; get; }
        public string Loc { set; get; }
        public int Pin { set; get; }
        public string Stcd { set; get; }
    }
    public class ItemList
    {
        public string SlNo { set; get; }
        public string PrdDesc { set; get; }
        public string IsServc { set; get; }
        public string HsnCd { set; get; }
        public decimal Qty { set; get; }
        public decimal FreeQty { set; get; }
        public string Unit { set; get; }
        public decimal UnitPrice { set; get; }
        public decimal TotAmt { set; get; }
        public decimal Discount { set; get; }
        public decimal PreTaxVal { set; get; }
        public decimal AssAmt { set; get; }
        public decimal GstRt { set; get; }
        public decimal IgstAmt { set; get; }
        public decimal CgstAmt { set; get; }
        public decimal SgstAmt { set; get; }
        public decimal CesRt { set; get; }
        public decimal CesAmt { set; get; }
        public decimal CesNonAdvlAmt { set; get; }
        public decimal StateCesRt { set; get; }
        public decimal StateCesAmt { set; get; }
        public decimal StateCesNonAdvlAmt { set; get; }
        public decimal OthChrg { set; get; }
        public decimal TotItemVal { set; get; }
    }

    public class ValDtls
    {
        public decimal AssVal { set; get; }
        public decimal IgstVal { set; get; }
        public decimal CgstVal { set; get; }
        public decimal SgstVal { set; get; }
        public decimal CesVal { set; get; }
        public decimal StCesVal { set; get; }
        public decimal Discount { set; get; }
        public decimal OthChrg { set; get; }
        public decimal RndOffAmt { set; get; }
        public decimal TotInvVal { set; get; }
        public decimal TotInvValFc { set; get; }
    }

    public class Ewaybill
    {
        public string TransId { set; get; }
        public string TransName { set; get; }
        public string TransMode { set; get; }
        public double Distance { set; get; }
        public string TransDocNo { set; get; }
        public string TransDocDt { set; get; }
        public string VehNo { set; get; }
        public string VehType { set; get; }
    }
}
