using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Utility_Constants
{
    public interface ICalculationDocumentSubItem
    {
        // Inputs (always safe decimal)
        decimal Qty { get; }
        decimal UnitPrice { get; }
        decimal LineDiscountPercent { get; }
        decimal LineDiscountAmount { get; }
        decimal LineCGSTRate { get; }
        decimal LineSGSTRate { get; }
        decimal LineIGSTRate { get; }

        // Computed
        decimal LineGross { get; }
        decimal LineBasicAmount { get; }
        decimal LineCGSTAmount { get; }
        decimal LineSGSTAmount { get; }
        decimal LineIGSTAmount { get; }
    }

}
