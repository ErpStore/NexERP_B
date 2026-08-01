using V.SMART.Shared.Data.Utilities.Correspondence;

namespace V.SMART.Shared.Repository.IRepository.IUtilitiesRepository
{
    public interface ICorrespondanceRepository : IRepository<Correspondence>
    {
        Task<(int uploaded, int total)> GetCorrespondenceStatusAsync(string referenceType, int referenceId, string documentType);

    }
}
