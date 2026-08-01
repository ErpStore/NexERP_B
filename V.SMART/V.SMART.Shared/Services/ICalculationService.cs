using V.SMART.Shared.Utility_Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public interface ICalculationService
    {
        Task UpdateTotalsAsync(ICalculationDocument document);
    }
}
