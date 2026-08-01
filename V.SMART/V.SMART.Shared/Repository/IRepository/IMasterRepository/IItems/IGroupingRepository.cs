using V.SMART.Shared.Data.Master.Inventory;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository
{
    public interface IGroupingRepository : IRepository<Grouping>
    {
        Task<(string Prefix, string FRCNo)> GetInitialPrefixAndFRCNoAsync();

    }
}
