using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Services
{

    public class DesktopPathProvider : IPathProvider
    {
        public string GetReportTemplatePath()
        {
            // ✅ 1. Runtime (Published/Desktop)
            var runtimePath = Path.Combine(AppContext.BaseDirectory, "templates");
            if (Directory.Exists(runtimePath))
                return runtimePath;

            // ✅ 2. DEV (Find solution root dynamically)
            var current = AppContext.BaseDirectory;
            DirectoryInfo dir = new DirectoryInfo(current);

            while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "V.SMART.Shared")))
            {
                dir = dir.Parent;
            }

            if (dir == null)
                throw new DirectoryNotFoundException("Solution root not found.");

            var devPath = Path.Combine(dir.FullName, "V.SMART.Shared", "wwwroot", "templates");

            if (Directory.Exists(devPath))
                return devPath;

            throw new DirectoryNotFoundException($"Template folder not found.\nChecked:\n{runtimePath}\n{devPath}");
        }
    }

}
