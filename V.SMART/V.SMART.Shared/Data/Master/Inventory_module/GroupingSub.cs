namespace V.SMART.Shared.Data.Master.Inventory
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class GroupingSub
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int GroupingId { get; set; }

        [ForeignKey(nameof(GroupingId))]
        public Grouping Grouping { get; set; }

        public int? SlNo { get; set; }

        // Factor
        public int? Factors { get; set; }

        [ForeignKey(nameof(Factors))]
        public Factor? Factor { get; set; }

        // Process
        public int? Processid { get; set; }

        [ForeignKey(nameof(Processid))]
        public Process? Process { get; set; }
    }

}
