using V.SMART.Shared.Data.Master.Accounts_Module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IAccounts
{
    public interface ICostCenterRepository : IRepository<CostCenter>
    {
        Task<string?> GetLastProjectNoByTypeAsync(int typeOfProjectId);
        Task<CostCenter?> GetCostCenterWithAllRelatedDataAsync(int id);

    }
}
