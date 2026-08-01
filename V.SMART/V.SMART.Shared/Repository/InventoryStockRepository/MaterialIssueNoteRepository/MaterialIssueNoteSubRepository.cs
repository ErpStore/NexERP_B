using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote;
using V.SMART.Shared.Repository.IRepository.IInventoryStockRepository.IMaterialIssueNoteRepository;
using V.SMART.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Repository.InventoryStockRepository.MaterialIssueNoteRepository
{
    public class MaterialIssueNoteSubRepository : Repository<MaterialIssNoteSub>, IMaterialIssueNoteSubRepository
    {

        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _logs;

        public MaterialIssueNoteSubRepository(ApplicationDbContext db, ILoggingService logs) : base(db, logs)
        {
            _db = db;
            _logs = logs;
        }


    }
}
