
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.ViewModels.AccountsViewModel;

namespace V.SMART.Shared.Repository.IRepository.IAccountRepository.IPaymentsRepository
{
    public interface IFundTransRepository : IRepository<FundTrans>
    {
        Task<FundTransResult> GetWithFiltersAsync(FundTransFilter filter);
        Task<int> GetTotalCountAsync(FundTransFilter filter);
        Task<List<string>> LoadDistinctTransactionTypes();
    }
}
