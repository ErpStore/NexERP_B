using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    public class DefectInfo
    {
        [Key]
        public int DefectCode { get; set; } = 0;

        [StringLength(50,ErrorMessage = "Defect Name Cannot Exceed More Than 50 Characters")]
        [Required(ErrorMessage = "Defect Name is Required")]
        public string? DefectName { get; set; }

        public string? DefectDesc { get; set; }

        public int DefectNo { get; set; } = 0;

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
