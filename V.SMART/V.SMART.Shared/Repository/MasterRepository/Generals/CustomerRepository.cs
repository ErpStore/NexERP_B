using DocumentFormat.OpenXml.InkML;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IGeneralRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.GeneralRepository
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;

        public CustomerRepository(ApplicationDbContext db, CurrentUserService currentUserService, ILoggingService loggingService)
            : base(db, loggingService)
        {
            _db = db;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }

        //public async Task<bool> ExistsByNameAsync(string CustName, int? excludeId = null)
        //{
        //    return await _db.Customer.AnyAsync(c => c.CustName == CustName && (!excludeId.HasValue || c.CustId != excludeId.Value));
        //}

        public async Task<Customer?> GetCustomerWithAllRelatedDataAsync(int id)
        {
            return await _db.Customer
                .Include(c => c.State)
                .Include(c => c.Currency)
                .Include(c => c.CustomerIndirects)
                .Include(c => c.ContactPersons)
                .FirstOrDefaultAsync(c => c.CustId == id);
        }

        //public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        //{
        //    return await _db.Customer
        //        .Where(c => c.CustName.Contains(searchText)) // Perform the search in the database
        //        .Select(c => new CustomerVM
        //        {
        //            CustId = c.CustId,
        //            CustName = c.CustName,
        //            CustAddress = c.CustAddr,
        //            GSTNo = c.GSTNo,
        //            ContactNo = c.ContactNo,
        //            CurrId = c.CurrId,
        //            BusiType = c.BusiType,
        //            TaxValidate = c.TaxValidate
        //        })
        //        .Take(25) // Return a limited number of results
        //        .ToListAsync();
        //}



        //public override async Task<Customer> UpdateAsync(Customer obj)
        //{
        //    try
        //    {
        //        var existing = await _db.Customer
        //            .Include(c => c.Currency)
        //            .AsNoTracking()
        //            .FirstOrDefaultAsync(c => c.CustId == obj.CustId);

        //        if (existing == null)
        //            return new Customer();

        //        var changes = new StringBuilder();

        //        void LogIfChanged<T>(string propName, T? oldVal, T? newVal)
        //        {
        //            if (!EqualityComparer<T>.Default.Equals(oldVal, newVal))
        //            {
        //                changes.AppendLine($"{propName}: '{oldVal}' → '{newVal}'");
        //            }
        //        }

        //        LogIfChanged(nameof(existing.CustName), existing.CustName, obj.CustName);
        //        LogIfChanged(nameof(existing.CustAddr), existing.CustAddr, obj.CustAddr);
        //        LogIfChanged(nameof(existing.GSTNo), existing.GSTNo, obj.GSTNo);
        //        LogIfChanged(nameof(existing.PANNo), existing.PANNo, obj.PANNo);
        //        LogIfChanged(nameof(existing.VendorCode), existing.VendorCode, obj.VendorCode);
        //        LogIfChanged(nameof(existing.ContactNo), existing.ContactNo, obj.ContactNo);
        //        LogIfChanged(nameof(existing.StateId), existing.StateId, obj.StateId);
        //        LogIfChanged(nameof(existing.StateName), existing.StateName, obj.StateName);
        //        LogIfChanged(nameof(existing.State), existing.State, obj.State);
        //        LogIfChanged(nameof(existing.OpenBal), existing.OpenBal, obj.OpenBal);
        //        LogIfChanged(nameof(existing.OpenBalPndg), existing.OpenBalPndg, obj.OpenBalPndg);
        //        LogIfChanged(nameof(existing.OpenBalDate), existing.OpenBalDate, obj.OpenBalDate);
        //        LogIfChanged(nameof(existing.Email), existing.Email, obj.Email);
        //        LogIfChanged(nameof(existing.Inactive), existing.Inactive, obj.Inactive);
        //        LogIfChanged(nameof(existing.Remark), existing.Remark, obj.Remark);
        //        LogIfChanged(nameof(existing.PinCode), existing.PinCode, obj.PinCode);
        //        LogIfChanged(nameof(existing.Scope), existing.Scope, obj.Scope);
        //        LogIfChanged(nameof(existing.BusiType), existing.BusiType, obj.BusiType);
        //        LogIfChanged(nameof(existing.TanNo), existing.TanNo, obj.TanNo);
        //        LogIfChanged(nameof(existing.CINNo), existing.CINNo, obj.CINNo);
        //        LogIfChanged(nameof(existing.RejectRetrac), existing.RejectRetrac, obj.RejectRetrac);
        //        LogIfChanged("Currency", existing.Currency?.CurrName, obj.Currency?.CurrName);
        //        LogIfChanged(nameof(existing.BankDetails), existing.BankDetails, obj.BankDetails);
        //        LogIfChanged(nameof(existing.Location), existing.Location, obj.Location);
        //        LogIfChanged(nameof(existing.Distance), existing.Distance, obj.Distance);
        //        LogIfChanged(nameof(existing.TDS), existing.TDS, obj.TDS);
        //        LogIfChanged(nameof(existing.TDSExmptdAmt), existing.TDSExmptdAmt, obj.TDSExmptdAmt);
        //        LogIfChanged(nameof(existing.TDSBillval), existing.TDSBillval, obj.TDSBillval);
        //        LogIfChanged(nameof(existing.TDSDeductper), existing.TDSDeductper, obj.TDSDeductper);
        //        LogIfChanged(nameof(existing.TdsDeduct), existing.TdsDeduct, obj.TdsDeduct);
        //        LogIfChanged(nameof(existing.SupTyp), existing.SupTyp, obj.SupTyp);
        //        LogIfChanged(nameof(existing.TdsDeductInSales), existing.TdsDeductInSales, obj.TdsDeductInSales);
        //        LogIfChanged(nameof(existing.LineId), existing.LineId, obj.LineId);
        //        LogIfChanged(nameof(existing.TaxValidate), existing.TaxValidate, obj.TaxValidate);

        //        if (changes.Length == 0)
        //            return existing;

        //        var tracked = await _db.Customer.FirstOrDefaultAsync(c => c.CustId == obj.CustId);
        //        if (tracked != null)
        //        {
        //            tracked.CustName = obj.CustName;
        //            tracked.CustAddr = obj.CustAddr;
        //            tracked.GSTNo = obj.GSTNo;
        //            tracked.PANNo = obj.PANNo;
        //            tracked.VendorCode = obj.VendorCode;
        //            tracked.ContactNo = obj.ContactNo;
        //            tracked.StateId = obj.StateId;
        //            tracked.StateName = obj.StateName;
        //            tracked.State = obj.State;
        //            tracked.OpenBal = obj.OpenBal;
        //            tracked.OpenBalPndg = obj.OpenBalPndg;
        //            tracked.OpenBalDate = obj.OpenBalDate;
        //            tracked.Email = obj.Email;
        //            tracked.Inactive = obj.Inactive;
        //            tracked.CreatedBy = obj.CreatedBy;
        //            tracked.CreatedDate = obj.CreatedDate;
        //            tracked.ModifiedBy = obj.ModifiedBy;
        //            tracked.ModifiedDate = DateTime.Now;
        //            tracked.Remark = obj.Remark;
        //            tracked.PinCode = obj.PinCode;
        //            tracked.Scope = obj.Scope;
        //            tracked.BusiType = obj.BusiType;
        //            tracked.TanNo = obj.TanNo;
        //            tracked.CINNo = obj.CINNo;
        //            tracked.RejectRetrac = obj.RejectRetrac;
        //            tracked.CurrId = obj.CurrId;
        //            tracked.BankDetails = obj.BankDetails;
        //            tracked.Location = obj.Location;
        //            tracked.Distance = obj.Distance;
        //            tracked.TDS = obj.TDS;
        //            tracked.TDSExmptdAmt = obj.TDSExmptdAmt;
        //            tracked.TDSBillval = obj.TDSBillval;
        //            tracked.TDSDeductper = obj.TDSDeductper;
        //            tracked.TdsDeduct = obj.TdsDeduct;
        //            tracked.SupTyp = obj.SupTyp;
        //            tracked.TdsDeductInSales = obj.TdsDeductInSales;
        //            tracked.LineId = obj.LineId;
        //            tracked.TaxValidate = obj.TaxValidate;

        //            await _db.SaveChangesAsync();

        //            await _loggingService.LogUserAction(
        //                UserName: await _currentUserService.GetUsernameAsync(),
        //                Machine: _currentUserService.MachineName,
        //                IP_Address: _currentUserService.IpAddress,
        //                screen: "Customer Master",
        //                action: "Customer data modified",
        //                additionalInfo: changes.ToString()
        //            );

        //            return tracked;
        //        }

        //        return existing;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, "Error in CustomerRepository.UpdateAsync");
        //        return new Customer();
        //    }
        //}

        //public async Task<List<Customer>> InstantSearchCustomersAsync(CustomerVM search)
        //{
        //    try
        //    {
        //        _db.Database.SetCommandTimeout(60);

        //        IQueryable<Customer> query = _db.Customer
        //            .AsNoTracking()
        //            .Include(i => i.State);

        //        if (!string.IsNullOrWhiteSpace(search.CustName))
        //        {
        //            var words = search.CustName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //            foreach (var w in words)
        //                query = query.Where(i => EF.Functions.Like(i.CustName, $"%{w}%"));
        //        }

        //        if (!string.IsNullOrWhiteSpace(search.CustAddress))
        //        {
        //            var words = search.CustAddress.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //            foreach (var w in words)
        //                query = query.Where(i => EF.Functions.Like(i.CustAddr, $"%{w}%"));
        //        }

        //        if (!string.IsNullOrWhiteSpace(search.GSTNo))
        //            query = query.Where(i => EF.Functions.Like(i.GSTNo, $"%{search.GSTNo}%"));

        //        if (!string.IsNullOrWhiteSpace(search.PANNo))
        //            query = query.Where(i => EF.Functions.Like(i.PANNo, $"%{search.PANNo}%"));

        //        if (!string.IsNullOrWhiteSpace(search.PinCode))
        //            query = query.Where(i => EF.Functions.Like(i.PinCode, $"%{search.PinCode}%"));

        //        if (!string.IsNullOrWhiteSpace(search.BusiType))
        //            query = query.Where(i => EF.Functions.Like(i.BusiType, $"%{search.BusiType}%"));

        //        if (!string.IsNullOrWhiteSpace(search.StateName))
        //            query = query.Where(i => EF.Functions.Like(i.StateName, $"%{search.StateName}%"));

        //        if (search.StateId.HasValue)
        //            query = query.Where(i => i.StateId == search.StateId.Value);

        //        if (!string.IsNullOrWhiteSpace(search.Email))
        //            query = query.Where(i => EF.Functions.Like(i.Email, $"%{search.Email}%"));

        //        if (!string.IsNullOrWhiteSpace(search.ContactNo))
        //            query = query.Where(i => EF.Functions.Like(i.ContactNo, $"%{search.ContactNo}%"));

        //        if (search.Inactive)
        //        {
        //            query = query.Where(i => i.Inactive == search.Inactive);
        //        }

        //        if (!string.IsNullOrWhiteSpace(search.CreatedBy))
        //            query = query.Where(i => EF.Functions.Like(i.CreatedBy, $"%{search.CreatedBy}%"));

        //        if (!string.IsNullOrWhiteSpace(search.ModifiedBy))
        //            query = query.Where(i => EF.Functions.Like(i.ModifiedBy, $"%{search.ModifiedBy}%"));

        //        if (search.FromDate.HasValue)
        //            query = query.Where(i => i.CreatedDate >= search.FromDate.Value);

        //        if (search.ToDate.HasValue)
        //            query = query.Where(i => i.CreatedDate <= search.ToDate.Value);

        //        return await query
        //            .OrderBy(i => i.CustName)
        //            .Take(100)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, "Error in CustomerRepository.SearchCustomersAsync");
        //        return new List<Customer>();
        //    }
        //}


    }
}
