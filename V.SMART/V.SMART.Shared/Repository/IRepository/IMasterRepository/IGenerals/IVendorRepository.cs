using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository
{
    public interface IVendorRepository : IRepository<Vendor>
    {
        Task<bool> ExistsByNameAsync(string VendorName, int? excludeId = null);
        Task<List<Vendor>> InstantSearchVendorsAsync(VendorVM search);
    }
}
