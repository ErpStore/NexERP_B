using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesPo
{
    public class PoType
    {
        [Key]
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string SeriesNo { get; set; }

        [NotMapped]
        public bool IsEditable { get; set; }= false;
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }=DateTime.Now;

    }
}
