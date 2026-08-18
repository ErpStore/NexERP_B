using System;
using Microsoft.Extensions.Configuration;

namespace V.SMART.Shared.Data.MigrationData
{
    /// <summary>
    /// Resolves the connection strings used by the <c>IDesignTimeDbContextFactory</c>
    /// implementations in this folder. These factories run only under the <c>dotnet ef</c>
    /// tooling; they never run as part of an application host.
    /// </summary>
    /// <remarks>
    /// There is deliberately no default connection string. A silent fallback to a working
    /// value is exactly how the credentials removed by task M0-03-02 reached source control
    /// (see docs/kb/risks/technical-debt-register.md R-01).
    /// </remarks>
    internal static class DesignTimeConnectionString
    {
        /// <summary>
        /// Reads <paramref name="configurationKey"/> from user-secrets and the environment,
        /// or throws an <see cref="InvalidOperationException"/> naming both ways to supply it.
        /// </summary>
        internal static string Resolve(string configurationKey, string purpose)
        {
            var environmentVariableName = configurationKey.Replace(":", "__");

            // 1. Environment variable — the form servers, containers and CI use.
            var connectionString = Environment.GetEnvironmentVariable(environmentVariableName);

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            // 2. User-secrets of this project — the form developer machines use.
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(typeof(DesignTimeConnectionString).Assembly, optional: true)
                .Build();

            connectionString = configuration[configurationKey];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return connectionString;
            }

            throw new InvalidOperationException(
                "Design-time connection string '" + configurationKey + "' is not configured (" + purpose + ")." +
                Environment.NewLine +
                "Supply it in one of these two ways; there is deliberately no default value." +
                Environment.NewLine +
                "  Environment variable: " + environmentVariableName +
                Environment.NewLine +
                "    PowerShell:  $env:" + environmentVariableName + " = \"<connection string>\"" +
                Environment.NewLine +
                "    bash:        export " + environmentVariableName + "=\"<connection string>\"" +
                Environment.NewLine +
                "  User-secrets:" +
                Environment.NewLine +
                "    dotnet user-secrets set \"" + configurationKey + "\" \"<connection string>\" --project V.SMART/V.SMART.Shared/V.SMART.Shared.csproj" +
                Environment.NewLine +
                "  See docs/CONFIGURATION.md.");
        }
    }
}
