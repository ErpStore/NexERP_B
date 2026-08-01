using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.AccountsViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService
{
    public interface ISummaryAndGraphsService
    {
        Task<DataTable> GetPendingBills(string Doctype, DateTime? fromDate, DateTime? toDate);
    }
}
