using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Utility_Constants
{
    public interface ICalculationDocument
    {
        decimal TotalGrossAmount { get; set; }
        decimal DiscountAmount { get; set; }
        decimal DiscountPercent { get; set; }
        bool DiscAmtOrPer { get; set; }
        decimal TotalBasicAmount { get; set; }
        decimal FreightCharges { get; set; }
        decimal PackingCharges { get; set; }
        decimal PackingPercent { get; set; }
        bool PackingAmtOrPer { get; set; }
        decimal InsuranceCharges { get; set; }
        decimal InsurancePercent { get; set; }
        bool InsuranceAmtOrPer { get; set; }
        decimal TotalTaxable { get; set; }
        decimal CGstRate { get; set; }
        decimal SGstRate { get; set; }
        decimal IGstRate { get; set; }
        decimal TotalCGSTAmount { get; set; }
        decimal TotalSGSTAmount { get; set; }
        decimal TotalIGSTAmount { get; set; }
        decimal TCSAmount { get; set; }
        decimal TCSPercent { get; set; }
        bool TCSAmtOrPer { get; set; }
        decimal OtherCharges { get; set; }

        bool IsRoundOffEnabled { get; set; }
        decimal RoundOff { get; set; }
        decimal GrandTotal { get; set; }

        IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem { get; }
        bool HasItemWiseTax { get; }
    }

}
