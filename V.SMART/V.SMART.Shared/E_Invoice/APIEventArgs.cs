using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.E_Invoice
{
    public class APIEventArgs:EventArgs
    {
        public string ErrorMesssage { get; set; }
    }
}
