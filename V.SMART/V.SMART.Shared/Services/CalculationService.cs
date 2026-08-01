using V.SMART.Shared.Utility_Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public class CalculationService : ICalculationService
    {
        public async Task UpdateTotalsAsync(ICalculationDocument document)
        {
            if (document == null || document.CalculationDocumentSubItem == null || !document.CalculationDocumentSubItem.Any())
                return;

            var items = document.CalculationDocumentSubItem;

            // 1. Gross
            document.TotalGrossAmount = items.Sum(x => x.LineGross);

            // 2. Discount
            if (items.Any(x => x.LineDiscountAmount > 0))
            {
                document.DiscountAmount = items.Sum(x => x.LineDiscountAmount);
            }
            else
            {
                document.DiscountAmount = document.DiscAmtOrPer
                    ? document.DiscountAmount   // fixed amount (already entered)
                    : (document.TotalGrossAmount * document.DiscountPercent / 100m);
            }

            // 3. Subtotal
            document.TotalBasicAmount = document.TotalGrossAmount - document.DiscountAmount;

            // 4. Freight, Packing, Insurance
            if (!document.PackingAmtOrPer)
                document.PackingCharges = (document.TotalBasicAmount * document.PackingPercent / 100m);

            if (!document.InsuranceAmtOrPer)
                document.InsuranceCharges = (document.TotalBasicAmount * document.InsurancePercent / 100m);

            // 5. Taxable Amount
            document.TotalTaxable = document.TotalBasicAmount
                                    + document.FreightCharges
                                    + document.PackingCharges
                                    + document.InsuranceCharges;

            // 6. Taxes
            if (document.HasItemWiseTax)
            {
                document.TotalCGSTAmount = 0m;
                document.TotalSGSTAmount = 0m;
                document.TotalIGSTAmount = 0m;

                decimal totalLineGross = items.Sum(x => x.LineGross);

                foreach (var item in items)
                {
                    decimal proportion = totalLineGross > 0 ? item.LineGross / totalLineGross : 0m;

                    decimal proportionalDiscount = (!document.DiscAmtOrPer && !items.Any(x => x.LineDiscountAmount > 0))
                        ? document.DiscountAmount * proportion
                        : 0m;

                    decimal adjustedBasic = item.LineGross - item.LineDiscountAmount - proportionalDiscount;

                    decimal taxableBase = adjustedBasic
                                         + (document.FreightCharges * proportion)
                                         + (document.PackingCharges * proportion)
                                         + (document.InsuranceCharges * proportion);

                    document.TotalCGSTAmount += taxableBase * item.LineCGSTRate / 100m;
                    document.TotalSGSTAmount += taxableBase * item.LineSGSTRate / 100m;
                    document.TotalIGSTAmount += taxableBase * item.LineIGSTRate / 100m;
                }
            }
            else
            {
                document.TotalCGSTAmount = document.TotalTaxable * document.CGstRate / 100m;
                document.TotalSGSTAmount = document.TotalTaxable * document.SGstRate / 100m;
                document.TotalIGSTAmount = document.TotalTaxable * document.IGstRate / 100m;
            }

            // 7. Pre-TCS Total
            var grandBeforeTCS = document.TotalTaxable
                               + document.TotalCGSTAmount
                               + document.TotalSGSTAmount
                               + document.TotalIGSTAmount;

            // 8. TCS
            document.TCSAmount = document.TCSAmtOrPer
                ? document.TCSAmount   // fixed amount
                : (grandBeforeTCS * document.TCSPercent / 100m);


            // 9.Grand Total
            var preRoundGrandTotal = grandBeforeTCS + document.TCSAmount + document.OtherCharges;

            if (document.IsRoundOffEnabled)
            {
                var rounded = Math.Round(preRoundGrandTotal, 0, MidpointRounding.AwayFromZero);
                document.RoundOff = rounded - preRoundGrandTotal;
                document.GrandTotal = rounded;
            }
            else
            {
                document.RoundOff = 0;
                document.GrandTotal = preRoundGrandTotal;
            }

            await Task.CompletedTask;
        }
    }

}
