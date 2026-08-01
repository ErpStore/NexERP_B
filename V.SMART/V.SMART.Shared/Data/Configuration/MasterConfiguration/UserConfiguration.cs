using V.SMART.Shared.Data.Master.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Configuration.MasterConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // =========================
            // PRIMARY KEY
            // =========================
            builder.HasKey(x => x.UserId);

            // =========================
            // UNIQUE INDEXES
            // =========================

            // Username must be unique (login performance)
            builder.HasIndex(x => x.UserName)
                   .IsUnique()
                   .HasDatabaseName("IX_User_UserName");

            // Email unique (optional but recommended)
            builder.HasIndex(x => x.EmailId)
                   .IsUnique()
                   .HasFilter("[EmailId] IS NOT NULL")
                   .HasDatabaseName("IX_User_EmailId");

            // =========================
            // SEARCH FILTER INDEXES
            // =========================

            // Role + Active (Most used in filter screen)
            builder.HasIndex(x => new { x.Role, x.IsActive })
                   .HasDatabaseName("IX_User_Role_IsActive");

            // Created Date (Date range filter)
            builder.HasIndex(x => x.CreatedDate)
                   .HasDatabaseName("IX_User_CreatedDate");

            // Staff relation
            builder.HasIndex(x => x.StaffId)
                   .HasDatabaseName("IX_User_StaffId");

            // Phone search
            builder.HasIndex(x => x.PhoneNumber)
                   .HasDatabaseName("IX_User_PhoneNumber");

            // =========================
            // GRID + PAGINATION INDEX
            // =========================

            builder.HasIndex(x => new { x.CreatedDate, x.UserId })
                   .HasDatabaseName("IX_User_Paging");

            // =========================
            // RELATIONSHIP
            // =========================
            builder.HasOne(x => x.Staff)
                   .WithMany()
                   .HasForeignKey(x => x.StaffId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
