using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Accounts_Module
{
    public class ProjectTypeMaster
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter type of project")]
        [StringLength(50, ErrorMessage = "Type of project must be less than 50 characters.")]
        public string TypeOfProject { get; set; }
        [StringLength(10, ErrorMessage = "The type of code must be less than 10 characters.")]
        public string? TypeOfCode { get; set; }
        [Required(ErrorMessage = "Please enter a valid start series number")]
        [StringLength(20, ErrorMessage = "Start Series must be less than 20 characters.")]
        public string StartSeries { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } = null;

        [NotMapped]
        public bool IsEditable { get; set; } = false;
    }
}
