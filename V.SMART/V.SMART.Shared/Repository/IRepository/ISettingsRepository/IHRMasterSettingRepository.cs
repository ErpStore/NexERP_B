using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.GeneralSettingsMaster;

namespace V.SMART.Shared.Repository.IRepository.ISettingsRepository
{
    public interface IHRMasterSettingRepository : IRepository<HRMasterSetting>
    {
        Task<HRMasterSetting> GetFirstAsync();
    }
}
