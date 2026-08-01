
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;

namespace V.SMART.Shared.Data.Planning.Estimaton
{
    public class Estimate
    {
        [Key]
        public int EstiamateId { get; set; }//Id

        [Required(ErrorMessage = "Customer is required.")]
        public int CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public Customer Customer { get; set; }

        //public ContactPerson? ContactPerson { get; set; }
        public string? ContactPerson { get; set; }

        public int? CostCenterId { get; set; }

        [ForeignKey(nameof(CostCenterId))]
        public CostCenter? CostCenter { get; set; }


        [Required(ErrorMessage = "Please select an Item. Item code should not be empty.")]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        [Required(ErrorMessage = "Estimate No. is required.")]
        [StringLength(30, ErrorMessage = "Estimate No. cannot exceed 30 characters.")]
        public string EstiamateNo { get; set; }

        [Required(ErrorMessage = "Suffix is required.")]
        public string? Suffix { get; set; }

        [Required]
        public DateTime EstimateDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        [Precision(18, 3)]
        public decimal? Qty { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public string? TypeofWork { get; set; }
        public string? EnquiryNo { get; set; }

        


        //Other Head
        public decimal? Overp { get; set; }//OverHeads %
        public decimal? Marp { get; set; } //Margin %
        public decimal? Riskp { get; set; } //RriskFactor %
        public decimal? Packingper { get; set; } //Packingper %
        public decimal? Logesticper { get; set; } //Logesticper %

        public decimal? Over { get; set; } //Overheads
        public decimal? Mar { get; set; } //Margin
        public decimal? Risk { get; set; } //Risk Factor
        public decimal? Packing { get; set; } //Packing
        public decimal? Logestic { get; set; } //Logestic

        //15-07-2026
        public string? Shape { get; set; }//Shape
        public decimal? Density { get; set; } //Density
        public decimal? Length { get; set; } //Length
        public decimal? Width { get; set; } //Width
        public decimal? Height { get; set; } //Height
        public decimal? Thickness { get; set; } //Thickness
        public decimal? WallThickness { get; set; } //WallThickness
        public decimal? OuterDia { get; set; } //OuterDia
        public decimal? InnerDia { get; set; } //InnerDia
        public decimal? Weight { get; set; } //Weight
        public decimal? RmPrice { get; set; } //RmPrice
        public decimal? RmTotAmount { get; set; } //RmTotal
        //




        //Raw Matericost Columns  FirstLine
        public string? MtlSQR { get; set; }//Material Sqr/Finish Len.
        public string? FnsizeSQR { get; set; }//Finish Size/Finish Width
        public string? finsqrlen { get; set; }//Material Sqr/Finish Thk
        public decimal? RmthSQR { get; set; }//Rawmaterial Th. 
        public decimal? RmwdSQR { get; set; }//Rawmaterial Width 
        public decimal? RmlenSQR { get; set; }//Rawmaterial Len.
        public decimal? DenSQR { get; set; }//Density
        public decimal? RmkgSQR { get; set; }//Rawmaterial Kg.
        public decimal? RmrateSQR { get; set; }//Rawmaterial Rate
        public decimal? RmcostSQR { get; set; }//Rawmaterial Cost

        //SecondLine
        public string? MtlRND { get; set; }//Material Round/Finish Len.
        public string? FnsizeRND { get; set; }//Finish Size/Finish Dia
        public decimal? RmdiaRND { get; set; }//Rawmaterial Dia
        public decimal? RmlenRND { get; set; }//Rawmaterial Len.
        public decimal? DensityRND { get; set; }//Density
        public decimal? RmkgRND { get; set; }//Rawmaterial  Kg.
        public decimal? RmrateRND { get; set; }//Rawmaterial Rate
        public decimal? RmcostRND { get; set; }//RawMaterial Cost

        //
        public decimal? Castwt { get; set; }//Costing wt.
        public decimal? CastAmt { get; set; }//Costing Amount.

        //
        public string? CutSize { get; set; }//Cut Size in MM.


        //Amount Calc
        //Cust.    txtCust
        public decimal? Conv { get; set; }//Conversion Cost
        public decimal? totalRmcost { get; set; }//RawMaterial
        public decimal? Totalcost { get; set; }//TotalCost
        public decimal? Grand { get; set; }//Grand


        //
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsManualEstimate { get; set; }

        public int? RefEnqSubId { get; set; }

        [ForeignKey(nameof(RefEnqSubId))]
        public EnquirySalesSub? EnquirySalesSub { get; set; }

        public virtual ICollection<EstimateSub> EstimateSub { get; set; } = new List<EstimateSub>();

    }
}
