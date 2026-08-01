using V.SMART.Shared.Data.Master.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Configuration.MasterConfiguration
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.HasKey(x => x.CustId);

            // =========================
            // Search Performance Indexes
            // =========================

            // For ordering (OrderByDescending CustId)
            builder.HasIndex(x => x.CustId);

            // Most common search fields
            builder.HasIndex(x => x.CustName);
            builder.HasIndex(x => x.GSTNo);
            builder.HasIndex(x => x.PANNo);

            // Foreign keys
            builder.HasIndex(x => x.StateId);
            builder.HasIndex(x => x.CurrId);

            // Boolean filters (very common in ERP)
            builder.HasIndex(x => x.Inactive);

            // Composite index (Very Important for ERP search screens)
            builder.HasIndex(x => new { x.Inactive, x.CustName });

            // =========================
            // Included Columns (SQL Server Optimization)
            // =========================

            builder.HasIndex(x => x.CustName)
                .IncludeProperties(x => new
                {
                    x.GSTNo,
                    x.ContactNo,
                    x.StateId,
                    x.CurrId,
                    x.Inactive
                });
        }
    }
}
