using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService
{
    public interface IBankService
    {
        Task<IEnumerable<Banks>> GetAllBanksAsync();
        Task<Banks> GetBankAsync(int bankId);
        Task<bool> UpsertBankAsync(Banks bank);
        Task<bool> DeleteBankAsync(int bankId);
        Task<BankDetailsResponse> GetIfscDetailsAsync(string ifscCode);
        Task<bool> IsDuplicateAccountNoAsync(string accountNo, int? bankId = null);
    }
}
