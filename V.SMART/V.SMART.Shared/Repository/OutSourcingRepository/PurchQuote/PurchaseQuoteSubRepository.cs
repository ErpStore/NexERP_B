using V.SMART.Shared.Data;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository.IPurchaseAndSubConRepository.IPurchQuote;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.PurchaseAndSubConRepository.PurchaseQuotation
{
    public class PurchaseQuoteSubRepository :Repository<PurchaseQuoteSub>, IPurchaseQuoteSubRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        public PurchaseQuoteSubRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

       
    }
}
