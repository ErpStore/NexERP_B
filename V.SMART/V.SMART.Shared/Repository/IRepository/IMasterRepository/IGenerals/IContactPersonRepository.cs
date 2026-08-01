using V.SMART.Shared.Data.Master.General;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository
{
    public interface IContactPersonRepository : IRepository<ContactPerson>
    {
        Task DeleteByCustIdAsync(int custId);
    }
}
