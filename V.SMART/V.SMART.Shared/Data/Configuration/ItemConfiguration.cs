using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Configuration
{
    public class ItemConfiguration : IEntityTypeConfiguration<Item>
    {
        public void Configure(EntityTypeBuilder<Item> entity)
        {
            // ---------- SEARCH / GRID ----------
            entity.HasIndex(e => e.ItemCode)
                  .HasDatabaseName("IX_Item_ItemCode");

            entity.HasIndex(e => e.ItemName)
                  .HasDatabaseName("IX_Item_ItemName");

            entity.HasIndex(e => e.Specification)
                  .HasDatabaseName("IX_Item_Specification");

            entity.HasIndex(e => e.CreatedDate)
                  .HasDatabaseName("IX_Item_CreatedDate");

            entity.HasIndex(e => e.Hide)
                  .HasDatabaseName("IX_Item_Hide");

            // ---------- FOREIGN KEYS ----------
            entity.HasIndex(e => e.CategoryCode)
                  .HasDatabaseName("IX_Item_CategoryCode");

            entity.HasIndex(e => e.RmId)
                  .HasDatabaseName("IX_Item_RmId");

            // ---------- COMPOSITE (GRID OPTIMIZED) ----------
            entity.HasIndex(e => new { e.Hide, e.CreatedDate })
                  .HasDatabaseName("IX_Item_Hide_CreatedDate");

            entity.HasIndex(e => new { e.Hide, e.ItemCode })
                .HasDatabaseName("IX_Item_Hide_ItemCode");

        }
    }
}
