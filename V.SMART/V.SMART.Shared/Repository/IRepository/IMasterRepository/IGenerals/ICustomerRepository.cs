using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository
{
    public interface ICustomerRepository : IRepository<Customer>
    {
        Task<Customer?> GetCustomerWithAllRelatedDataAsync(int id);
        //Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        //Task<List<Customer>> InstantSearchCustomersAsync(CustomerVM search);
    }
}
