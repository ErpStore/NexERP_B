using V.SMART.Shared.Data;

namespace V.SMART.Shared.Services.MultiCompanyService
{
    public interface ITenantProvider
    {
        TenantInfo GetCurrentTenant();
        void SetTenant(string tenantNameOrHost); // for MAUI
        string GetTenantLogoPath();
        string GetTenantCorrespondenceUploadPath();
    }
}
