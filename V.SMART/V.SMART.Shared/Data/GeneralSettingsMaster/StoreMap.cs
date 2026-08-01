using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.GeneralSettingsMaster
{
    public class StoreMap
    {
        [Key]
        public int Id { get; set; }
        public int SlNo { get; set; }
        public string ScreenName { get; set; }
        public string FormName { get; set; }
        public int? StoreId { get; set; }

        [ForeignKey(nameof(StoreId))]
        public virtual Store? Store { get; set; }
    }

}
