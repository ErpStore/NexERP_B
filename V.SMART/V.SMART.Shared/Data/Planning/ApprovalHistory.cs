using DocumentFormat.OpenXml.Drawing.Charts;
using V.SMART.Shared.Data.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Planning
{
    public class ApprovalHistory
    {
        public int Id { get; set; }
        public int RecordId { get; set; }
        public string ApprovalType { get; set; }
        public  int Level { get; set; }
        public string Status { get; set; }
        public string? ActionBy { get; set; }
        public DateTime ActionDate { get; set; } = DateTime.Now;
        public string? Reason { get; set; }
    }
}
