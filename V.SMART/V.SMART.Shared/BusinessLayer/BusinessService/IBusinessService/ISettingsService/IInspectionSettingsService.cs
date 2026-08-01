using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface IInspectionSettingsService
    {
        Task SaveInspectionSettingsAsync(IEnumerable<InspectionSettings> settings);

        Task<List<InspectionSettings>> GetAllInspectionSettings();
    }
}
