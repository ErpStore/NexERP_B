using V.SMART.Shared.Data.Master.General;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository
{
    public interface ICustomerIndirectRepository : IRepository<CustomerIndirect>
    {
        Task DeleteByCustIdAsync(int custId);
    }
}
