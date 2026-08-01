using V.SMART.Shared.Data.Master.Inventory_module;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Configuration.MasterConfiguration
{
    public class ItemCustomerAssignConfiguration : IEntityTypeConfiguration<ItemCustomerAssign>
    {
        public void Configure(EntityTypeBuilder<ItemCustomerAssign> entity)
        {
            entity.HasIndex(e => new { e.CustomerId, e.ItemId })
                  .HasDatabaseName("IX_ItemCustomerAssign_Customer_Item");

            entity.HasIndex(e => e.ItemId)
                  .HasDatabaseName("IX_ItemCustomerAssign_ItemId");
        }
    }
}
