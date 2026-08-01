using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.General
{
    public class State
    {
        [Key]
        public int StateCode { get; set; }
        public string StateName { get; set; }
        public bool IsSystemDefined { get; set; } = false;
    }
}
