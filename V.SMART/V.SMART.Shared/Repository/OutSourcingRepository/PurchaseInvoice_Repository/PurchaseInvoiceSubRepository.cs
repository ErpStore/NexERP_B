using V.SMART.Shared.Data;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseGRN_Repository;
using V.SMART.Shared.Repository.IRepository.IOutSourcingRepository.IPurchaseInvoice_Repository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.OutSourcingRepository.PurchaseInvoice_Repository
{
    public class PurchaseInvoiceSubRepository: Repository<PurchaseInvoiceSub>,IPurchaseInvoiceSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public PurchaseInvoiceSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }


        public async Task<List<PurchaseInvoiceSub>> GetPurchaseINVSubDataByInvId(int InvId)
        {
            return await _db.PurchaseInvoiceSub
                .Where(s => s.InvId == InvId)
                .ToListAsync();
        }
    }
}
