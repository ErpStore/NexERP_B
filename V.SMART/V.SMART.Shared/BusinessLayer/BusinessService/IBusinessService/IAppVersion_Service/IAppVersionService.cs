using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Services
{
    public interface IAppVersionService
    {
        Task<string?> GetLatestVersionAsync();
    }
}
