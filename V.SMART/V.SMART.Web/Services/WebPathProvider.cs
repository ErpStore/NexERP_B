using V.SMART.Shared.Services;

namespace V.SMART.Web.Services
{
    public class WebPathProvider : IPathProvider
    {
        private readonly IWebHostEnvironment _env;
        public WebPathProvider(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string GetReportTemplatePath()
        {
            // ✅ 1. Try published/static assets path (Plesk)
            var prodPath = Path.Combine(_env.WebRootPath,"_content","V.SMART.Shared","templates");

            if (Directory.Exists(prodPath))
                return prodPath;

            // ✅ 2. Fallback to local project structure (DEV)
            var devPath = Path.GetFullPath(Path.Combine(_env.ContentRootPath, "..", "V.SMART.Shared", "wwwroot", "templates"));

            if (Directory.Exists(devPath))
                return devPath;

            throw new DirectoryNotFoundException("Report template folder not found in both PROD and DEV paths.");
        }
    }
}
