using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.ISubContractGRNRepository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.SubContractGRN
{
    public class SubConGRN
    {
        [Key]
        public int GRNId { get; set; }

        public bool IsWithoutPoDc { get; set; }
        public bool IsNoInspection { get; set; } = false;
        public bool IsManual { get; set; } = false;
        public bool Rejection { get; set; } = false;
        public bool Return { get; set; } = false;

        [Required, MaxLength(50)]
        public string GRNNo { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime GRNDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime GRNDateNow { get; set; } = DateTime.Now;

        [Required]
        public int? VendorCode { get; set; }

        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? vendor { get; set; }

        public int NoOfItems { get; set; }

        [MaxLength(250)]
        public string? MainRemark { get; set; }


        [Required]
        public string? PartyDC { get; set; }

        [Required]
        public DateTime? PartyDCDate { get; set; }



        public int? AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? AddStore { get; set; }

        public bool GRNTally { get; set; } = false;


        public bool Cancel { get; set; } = false;

        [StringLength(250)]
        public string? CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }

        [StringLength(500)]
        public string? CancelReason { get; set; }

        public bool ShortClose { get; set; } = false;


        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<SubConGRNSub>? SubConGRNSubs { get; set; }
        public virtual ICollection<SubConGRNTrack>? SubConGRNTracks { get; set; }
    }
}
