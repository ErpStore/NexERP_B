

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Repository.IRepository;

namespace V.SMART.Shared.Repository.IRepository.IAccountRepository.IPaymentsRepository
{
    public interface IPaymentsSubRepository : IRepository<PaymentsSub>
    {

    }
    public interface IAdvaceadjustmentSubRepository : IRepository<AdvaceadjustmentSub>
    {

    }
    public interface IReceiptsSubRepository : IRepository<ReceiptsSub>
    {

    }

}
