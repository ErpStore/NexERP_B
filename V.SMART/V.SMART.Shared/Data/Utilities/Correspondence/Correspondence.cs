using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Utilities.Correspondence
{
    public class Correspondence
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "File Name is Required")]
        [StringLength(100, ErrorMessage = "File Name cannot exceed 100 characters.")]
        public string FileName { get; set; }

        public byte[]? Image { get; set; } = null;

        [Required(ErrorMessage = "Please Attach the File")]
        public string FilePath { get; set; }
        public string? FileSize { get; set; }
        public string? FileType { get; set; }
        public DateTime? UploadedOn { get; set; } = DateTime.Now;
        public string? UploadedBy { get; set; }
        public string? ReferenceType { get; set; }
        public string? ReferenceTypeName { get; set; }
        public int? ReferenceId { get; set; }

        [Required(ErrorMessage = "Document Type is Required")]
        public string DocumentType { get; set; }  //Correspondence /Drawing 

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? Modifiedby { get; set; }
    }
}
