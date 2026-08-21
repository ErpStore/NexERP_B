using System.Diagnostics.CodeAnalysis;
using Serilog.Core;
using Serilog.Events;
using V.SMART.Shared.Data;

namespace V.SMART.Api.Logging
{
    /// <summary>
    /// M2-B11 — stops <see cref="TenantInfo"/> ever being serialised into a log sink with its
    /// <see cref="TenantInfo.ConnectionString"/> attached.
    /// <para>
    /// <b>Why this is not optional.</b> <see cref="TenantInfo"/> is
    /// <c>{ Id, Name, Hostname, ConnectionString }</c>
    /// (<c>V.SMART/V.SMART.Shared/Data/TenantInfo.cs:5-8</c>) — four fields, one of which is a
    /// credential-bearing connection string, and
    /// <c>TenantDbContextFactory.cs:18-19</c> passes it straight to <c>UseSqlServer</c>.
    /// Structured logging serialises objects: a single <c>Log.Information("{@Tenant}", t)</c>
    /// anywhere in this codebase, now or in five years, would publish database credentials
    /// into a searchable, retained, possibly third-party sink. That is strictly worse than the
    /// flat files R-23 exists to replace.
    /// </para>
    /// <para>
    /// Per R-23's action item the replacement carries <c>TenantId</c> and <c>Hostname</c> only.
    /// <see cref="TenantInfo.Name"/> is dropped as well — it is a customer name, of no
    /// diagnostic value beyond the id, and this is the one place it is cheap to not emit.
    /// </para>
    /// </summary>
    public sealed class TenantInfoDestructuringPolicy : IDestructuringPolicy
    {
        // [NotNullWhen(true)] mirrors Serilog's own IDestructuringPolicy declaration. Omitting it
        // is a nullability-contract mismatch (CS8767), which the committed CI analyzer ratchet
        // (ci/warning-baseline.json, .github/workflows/ci.yml) fails on. Behaviour is unchanged:
        // this method already never returns true with a null result.
        public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, [NotNullWhen(true)] out LogEventPropertyValue? result)
        {
            if (value is not TenantInfo tenant)
            {
                result = null;
                return false;
            }

            result = new StructureValue(
                new[]
                {
                    new LogEventProperty("TenantId", new ScalarValue(tenant.Id)),
                    new LogEventProperty("Hostname", new ScalarValue(tenant.Hostname))
                },
                "TenantInfo");

            return true;
        }
    }
}
