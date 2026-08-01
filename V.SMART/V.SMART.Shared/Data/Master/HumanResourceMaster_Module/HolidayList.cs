using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class HolidayList
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Holiday Name is required.")]
        [StringLength(100, ErrorMessage = "Holiday Name cannot exceed 100 characters.")]
        public string HolidayName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please Select Holiday Date")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateTime HolidayDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Holiday Day is required.")]
        [StringLength(20, ErrorMessage = "Holiday Day cannot exceed 20 characters.")]
        public string HolidayDay { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "CreatedBy cannot exceed 100 characters.")]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; } = null;
        public DateTime? ModifiedDate { get; set; } = null;

        // Local use only (not mapped to DB)
        [NotMapped]
        public bool IsEditable { get; set; } = false;
    }
}
