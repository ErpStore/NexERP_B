using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels
{
    public class RejectionMasterVM
    {
        public int RejectionId { get; set; }

        [Required(ErrorMessage = "RejectionCode is required.")]
        [StringLength(250, ErrorMessage = "Item Code cannot exceed 250 characters.")]
        public string? RejectionCode { get; set; }

     
        [StringLength(250)]
        public string? RejectionDescription { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsEditing { get; set; }
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class RejectionMasterStatistics
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public enum ActivityType
    {
        Create,
        Update,
        Delete,
        BulkOperation,
        Export
    }
}
