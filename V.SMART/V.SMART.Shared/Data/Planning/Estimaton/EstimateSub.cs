using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;

namespace V.SMART.Shared.Data.Planning.Estimaton
{
    public class EstimateSub
    {
        [Key]
        public int EstiamateSubId { get; set; }
        public int EstiamateId { get; set; }//Id

        [ForeignKey(nameof(EstiamateId))]
        public virtual Estimate Estimate { get; set; }

        public int ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public Process Process { get; set; }

        public decimal? HrlyRate { get; set; } //Hourly Rate
        public decimal? Hrs { get; set; } //Hours

        public decimal? Amount { get; set; }   //Amount
        public int? RefEnqSubId { get; set; }

        [ForeignKey(nameof(RefEnqSubId))]
        public EnquirySalesSub? EnquirySalesSub { get; set; }

    }
}
