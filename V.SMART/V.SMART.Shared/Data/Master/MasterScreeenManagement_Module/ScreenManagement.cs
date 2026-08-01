using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.MasterScreeenManagement
{
    public class ScreenManagement
    {
        public int Id { get; set; }

        [Required]
        public string ScreenName { get; set; }

        [Required]
        public string Display { get; set; }

        public string? Description { get; set; }

        public string? Topic { get; set; }

        public bool Required { get; set; } = false;
    }
}
