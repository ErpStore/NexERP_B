using V.SMART.Shared.Data;
using V.SMART.Shared.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace V.SMART.Shared.Services.MultiCompanyService
{

    public class TenantProvider : ITenantProvider
    {
        private readonly MasterDbContext _masterDb;
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private TenantInfo _cached;
        private string _manualTenant = null;
        private TenantConfig _cachedTenantConfig;

        private UserSession session;
        public TenantProvider(MasterDbContext masterDb, IHttpContextAccessor? httpContextAccessor = null, UserSession session = null)
        {
            _masterDb = masterDb;
            _httpContextAccessor = httpContextAccessor;
            this.session = session;
        }

        public TenantInfo GetCurrentTenant()
        {
            if (_cached != null)
                return _cached;

            try
            {
                // ✅ 0. Explicit binding (M2-A05). Set by AuthController's Login/Refresh/Logout
                // actions, via SetTenant(), before this method is first resolved from the
                // request's DI scope — see AuthController.cs's own comment on why that ordering
                // matters. Resolved by Name OR Hostname, the same two columns step 3 already
                // matches. This step is what SetTenant()/_manualTenant were for; before M2-A05
                // this method never read _manualTenant at all, so the setter was a no-op beyond
                // invalidating the cache (the finding INV-005/KB-014 already recorded).
                if (!string.IsNullOrWhiteSpace(_manualTenant))
                {
                    _cached = _masterDb.Tenants.FirstOrDefault(t =>
                        t.Name == _manualTenant || t.Hostname == _manualTenant);
                    if (_cached != null)
                    {
                        if (session != null)
                            session.HostName = _cached.Hostname;
                        Console.WriteLine($"[TenantProvider] Resolved tenant from explicit binding.");
                        return _cached;
                    }
                }

                // ✅ 1. API mode (from JWT TenantId claim)
                var tenantIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst("TenantId")?.Value;
                if (!string.IsNullOrEmpty(tenantIdClaim) && int.TryParse(tenantIdClaim, out var tenantId))
                {
                    _cached = _masterDb.Tenants.FirstOrDefault(t => t.Id == tenantId);
                    if (_cached != null)
                    {
                        if (session != null)
                            session.HostName = _cached.Hostname;
                        Console.WriteLine($"[TenantProvider] Resolved tenant from JWT TenantId: {tenantId}");
                        return _cached;
                    }
                }

                // ✅ 2. Web mode (by Host)
                var host = _httpContextAccessor?.HttpContext?.Request?.Host.Host;
                if (!string.IsNullOrEmpty(host))
                {
                    if (session != null)
                        session.HostName = host;
                    _cached = _masterDb.Tenants.FirstOrDefault(t => t.Hostname == host);
                    if (_cached != null)
                    {
                        Console.WriteLine($"[TenantProvider] Resolved tenant from host: {host}");
                        return _cached;
                    }
                }

                // ✅ 3. Desktop / API fallback (tenant.json)
                var config = LoadTenantConfigFromJson();
                if (config != null)
                {
                    _cached = _masterDb.Tenants.FirstOrDefault(t =>
                        (!string.IsNullOrWhiteSpace(config.Tenant) && t.Name == config.Tenant) ||
                        (!string.IsNullOrWhiteSpace(config.HostName) && t.Hostname == config.HostName));

                    if (_cached != null)
                    {
                        Console.WriteLine($"[TenantProvider] Resolved tenant from tenant.json: Name = {config.Tenant}, Host = {config.HostName}");
                        return _cached;
                    }
                }

                Console.WriteLine("[TenantProvider] ❌ Unable to determine tenant.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TenantProvider] Error in GetCurrentTenant: {ex.Message}");
            }

            return null;
        }

        // M2-A05 — made functional. Before this task, _manualTenant was assigned here but never
        // read by GetCurrentTenant(), so this setter only ever invalidated the cache. Step 0
        // above now consults it, resolving by Tenants.Name or Tenants.Hostname on the next call.
        public void SetTenant(string tenantNameOrHost)
        {
            _manualTenant = tenantNameOrHost;
            _cached = null;
        }

        //Only For Desktop
        public string GetTenantLogoPath()
        {
            try
            {
                var config = LoadTenantConfigFromJson();

                if (config == null || string.IsNullOrEmpty(config.ServerPath) || string.IsNullOrEmpty(config.CompanyName))
                    return string.Empty;

                var fullPath = Path.Combine(config.ServerPath, config.CompanyName, "CompanyLogos");

                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TenantProvider] Error in GetTenantLogoPath: {ex.Message}");
                return string.Empty;
            }
        }

        public string GetTenantCorrespondenceUploadPath()
        {
            try
            {
                var config = LoadTenantConfigFromJson();

                if (config == null || string.IsNullOrEmpty(config.ServerPath) || string.IsNullOrEmpty(config.CompanyName))
                    return string.Empty;

                var fullPath = Path.Combine(config.ServerPath, config.CompanyName, "Correspondences");

                if (!Directory.Exists(fullPath))
                    Directory.CreateDirectory(fullPath);

                return fullPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TenantProvider] Error in GetTenantLogoPath: {ex.Message}");
                return string.Empty;
            }
        }



        private TenantConfig LoadTenantConfigFromJson()
        {
            if (_cachedTenantConfig != null)
                return _cachedTenantConfig;

            try
            {
                var basePath = AppContext.BaseDirectory;
                var filePath = Path.Combine(basePath, "wwwroot", "config", "tenant.json");

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"[TenantProvider] tenant.json not found at: {filePath}");
                    return null;
                }

                var json = File.ReadAllText(filePath);
                _cachedTenantConfig = JsonSerializer.Deserialize<TenantConfig>(json);
                return _cachedTenantConfig;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TenantProvider] Error reading tenant.json: {ex.Message}");
                return null;
            }
        }
    }
}
