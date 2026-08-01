using AutoMapper.Configuration.Annotations;
using DocumentFormat.OpenXml.Vml.Office;



using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IAccountsService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;


namespace IQSMART.Shared.BusinessLayer.BusinessService.MasterService.AccountsService
{
    public class BankService : IBankService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IPaymentsService PaymentsService;
        private readonly HttpClient _httpClient;
        private object httpClientFactory;

        public BankService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService, IHttpClientFactory httpClientFactory, IPaymentsService PaymentsServices)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            PaymentsService = PaymentsServices;
            if (httpClientFactory == null)
                throw new ArgumentNullException(nameof(httpClientFactory));

            _httpClient = httpClientFactory.CreateClient("DefaultClient");
        }

        public async Task<IEnumerable<Banks>> GetAllBanksAsync()
        {
            try
            {
                return (await _unitOfWork.Banks.GetAllAsync()).OrderByDescending(b => b.CreatedDate);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to get all banks from service.");
                return Enumerable.Empty<Banks>();
            }
        }

        public async Task<Banks> GetBankAsync(int bankId)
        {
            try
            {
                return await _unitOfWork.Banks.GetAsync(bankId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get bank with ID {bankId}.");
                return null;
            }
        }

        public async Task<bool> IsDuplicateAccountNoAsync(string accountNo, int? bankId = null)
        {
            try
            {
                return await _unitOfWork.Banks.ExistsByNameAsync("AccountNo", accountNo?.Trim(), "BankId", bankId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error checking for duplicate account number.");
                return false;
            }
        }
        public async Task<Banks> GetAsNoTrackingAsync(int id)
        {
            return await _unitOfWork.Banks.GetQueryable().
                AsNoTracking().
                FirstOrDefaultAsync(b => b.BankId == id);
        }
        public async Task<bool> UpsertBankAsync(Banks bank)
        {
            try
            {
                // 1️⃣ Duplicate account check (already correct)
                if (await IsDuplicateAccountNoAsync(bank.AccountNo, bank.BankId == 0 ? null : bank.BankId))
                    throw new InvalidOperationException("Account Number already exists.");

                Banks? oldBank = null;

                if (bank.BankId != 0)
                    oldBank = await GetAsNoTrackingAsync(bank.BankId);

                if (bank.BankId == 0)
                {
                    // ➤ INSERT
                    bank.CreatedBy = await _currentUserService.GetUsernameAsync();
                    bank.CreatedDate = DateTime.Now;
                    bank.CurrentBalance = bank.BankOPBal;
                    await _unitOfWork.Banks.CreateAsync(bank);
                    await _unitOfWork.SaveAsync();

                    // Create opening fund trans
                    var ft = new FundTrans
                    {
                        TransNo = bank.BankId.ToString(),
                        TransDate = bank.CreatedDate ?? DateTime.Today,
                        TransType = "Opening Balance Adjustment",
                        TrasactionMode = "Bank",
                        BankId = bank.BankId,
                        LedgerType = "Bank Opening Balance",
                        BillAmount = bank.BankOPBal,
                        PaidOrReceived = bank.BankOPBal,
                        BalanceAfter = bank.BankOPBal,
                        RunningBalance = bank.BankOPBal,
                        PaymentRefId = null,
                        AdvanceRefId = null,
                        ReceiptRefId = null,
                        CreatedBy = bank.CreatedBy,
                        CreatedDate = DateTime.Now

                    };

                    await _unitOfWork.fundTranss.CreateAsync(ft);
                    await _unitOfWork.SaveAsync();
                }
                else
                {
                    // ➤ UPDATE
                    bank.ModifiedBy = await _currentUserService.GetUsernameAsync();
                    bank.ModifiedDate = DateTime.Now;
                    await _unitOfWork.Banks.UpdateAsync(bank);
                    await _unitOfWork.SaveAsync();

                    // 2️⃣ Find existing opening transaction
                    var openingTrans = await _unitOfWork.fundTranss
                        .GetQueryable()
                        .Where(x => x.BankId == bank.BankId && x.LedgerType == "Bank Opening Balance")
                        .FirstOrDefaultAsync();

                    if (openingTrans != null)
                    {
                        // 3️⃣ Check if any future transactions exist after opening
                        bool hasFutureTrans = await _unitOfWork.fundTranss
                            .GetQueryable()
                            .AnyAsync(x => x.BankId == bank.BankId && x.FundTransId > openingTrans.FundTransId);

                        decimal oldOP = oldBank?.BankOPBal ?? 0;
                        decimal newOP = bank.BankOPBal;
                        decimal diff = newOP - oldOP;

                        var lastTrans = await _unitOfWork.fundTranss
                                     .GetQueryable()
                                     .Where(x => x.BankId == bank.BankId)
                                     .OrderByDescending(x => x.FundTransId)
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync();

                        decimal previousRunningBalance = lastTrans?.RunningBalance ?? bank.BankOPBal;

                        if (!hasFutureTrans)
                        {
                            // ✔ No future transactions → update existing opening record
                            openingTrans.BillAmount = newOP;
                            openingTrans.PaidOrReceived = newOP;
                            openingTrans.BalanceAfter += diff;
                            openingTrans.RunningBalance += diff;
                            openingTrans.ModifiedBy = bank.ModifiedBy;
                            openingTrans.ModifiedDate = DateTime.Now;

                            await _unitOfWork.fundTranss.UpdateAsync(openingTrans);
                            await _unitOfWork.SaveAsync();

                            // ✔ Update bank current balance
                            bank.CurrentBalance = (oldBank?.CurrentBalance ?? 0) + diff;
                            await _unitOfWork.Banks.UpdateAsync(bank);
                            await _unitOfWork.SaveAsync();
                        }
                        else
                        {
                           
                            // ✔ Future transactions exist → do not edit old opening, create adjustment entry
                            var ftAdjust = new FundTrans
                            {
                                TransNo = bank.BankId.ToString(),
                                TransDate = DateTime.Today,
                                TransType = "Opening Balance Adjustment",
                                TrasactionMode = "Bank",
                                BankId = bank.BankId,
                                LedgerType = "Bank Opening Balance",
                                BillAmount = diff,
                                PaidOrReceived = diff,
                                BalanceAfter = (oldBank?.CurrentBalance ?? 0) + diff,
                                RunningBalance = previousRunningBalance+diff,
                                PaymentRefId = null,
                                AdvanceRefId=null,
                                ReceiptRefId=null,
                                CreatedBy = bank.ModifiedBy,
                                CreatedDate = DateTime.Now
                            };

                            await _unitOfWork.fundTranss.CreateAsync(ftAdjust);
                            await _unitOfWork.SaveAsync();

                            // Update bank balance
                            bank.CurrentBalance = (oldBank?.CurrentBalance ?? 0) + diff;
                            await _unitOfWork.Banks.UpdateAsync(bank);
                            await _unitOfWork.SaveAsync();
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error during bank upsert for AccountNo: {bank.AccountNo}");
                throw;
            }
        }


        //public async Task<bool> UpsertBankAsync(Banks bank)
        //{
        //    try
        //    {
        //        // -----------------------------------------------------
        //        // 1️⃣ DUPLICATE CHECK
        //        // -----------------------------------------------------
        //        if (await IsDuplicateAccountNoAsync(bank.AccountNo, bank.BankId == 0 ? null : bank.BankId))
        //            throw new InvalidOperationException("Account Number already exists.");


        //        decimal? oldOpeningBalance = null;
        //        decimal? oldCurrentBalance = null;

        //        // -----------------------------------------------------
        //        // 2️⃣ IF UPDATE → LOAD OLD BANK DATA
        //        // -----------------------------------------------------
        //        if (bank.BankId != 0)
        //        {
        //            var existing = await GetAsNoTrackingAsync(bank.BankId);
        //            oldOpeningBalance = existing.BankOPBal;
        //            oldCurrentBalance = existing.CurrentBalance ;
        //        }

        //        if (bank.BankId == 0)
        //        {
        //            bank.CreatedBy = await _currentUserService.GetUsernameAsync();
        //            bank.CreatedDate = DateTime.Now;

        //            bank.CurrentBalance = bank.BankOPBal;

        //            await _unitOfWork.Banks.CreateAsync(bank);
        //        }
        //        else
        //        {
        //            bank.ModifiedBy = await _currentUserService.GetUsernameAsync();
        //            bank.ModifiedDate = DateTime.Now;

        //            await _unitOfWork.Banks.UpdateAsync(bank);
        //        }

        //        await _unitOfWork.SaveAsync();

        //        if (bank.BankId != 0 && oldOpeningBalance == null)
        //        {
        //            var ft = new FundTrans
        //            {
        //                TransNo = bank.BankId.ToString(),
        //                TransDate = bank.CreatedDate ?? DateTime.Today,
        //                TransType = "Opening Balance",
        //                TrasactionMode = "Bank",
        //                BankId = bank.BankId,
        //                LedgerType="Bank Opening Balance",
        //                BillAmount = bank.BankOPBal,
        //                PaidOrReceived = bank.BankOPBal,
        //                BalanceAfter = bank.BankOPBal,
        //                RunningBalance = bank.BankOPBal,
        //                PaymentRefId = bank.BankId
        //            };

        //            await _unitOfWork.fundTranss.CreateAsync(ft);
        //            await _unitOfWork.SaveAsync();
        //        }
        //        var lastTrans = await _unitOfWork.fundTranss
        //         .GetQueryable()
        //         .Where(x => x.BankId == bank.BankId)
        //         .OrderByDescending(x => x.FundTransId)
        //         .AsNoTracking()
        //         .FirstOrDefaultAsync();

        //        decimal previousRunningBalance = lastTrans?.RunningBalance ?? bank.BankOPBal; // no entry = 0 or opening balance

        //        decimal oldOP = oldOpeningBalance ?? 0;
        //        decimal newOP = bank.BankOPBal;

        //        if (oldOpeningBalance!=null)
        //        {

        //            decimal diff = newOP - oldOP;
        //           // Update current balance
        //            decimal oldCB = oldCurrentBalance ?? oldOP;
        //            bank.CurrentBalance = oldCB + diff;
        //            bank.BankOPBal= oldCB + diff;
        //            await _unitOfWork.Banks.UpdateAsync(bank);
        //            await _unitOfWork.SaveAsync();

        //            // NEW running balance = last running balance + diff
        //            decimal newRunningBalance = previousRunningBalance + diff;
        //            var ftAdjust = new FundTrans
        //            {
        //                TransNo = bank.BankId.ToString(),
        //                TransDate = DateTime.Today,
        //                TransType = "Opening Balance",
        //                TrasactionMode = "Bank",
        //                BankId = bank.BankId,
        //                LedgerType = "Bank Opening Balance",
        //                BillAmount = newOP,
        //                PaidOrReceived = newOP,
        //                BalanceAfter = bank.CurrentBalance,
        //                RunningBalance = newRunningBalance,
        //                PaymentRefId = bank.BankId
        //            };
        //            await _unitOfWork.fundTranss.CreateAsync(ftAdjust);
        //            await _unitOfWork.SaveAsync();
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, $"Error during bank upsert for AccountNo: {bank.AccountNo}");
        //        throw;
        //    }
        //}

        //public async Task<bool> UpsertBankAsync(Banks bank)
        //{
        //    try
        //    {
        //        // Business rule: check for duplicates
        //        if (await IsDuplicateAccountNoAsync(bank.AccountNo, bank.BankId == 0 ? null : bank.BankId))
        //        {
        //            await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), _currentUserService.MachineName, _currentUserService.IpAddress, "Bank", $"Attempted to create/update duplicate bank account number: {bank.AccountNo}", $"BankId: {bank.BankId}");
        //            throw new InvalidOperationException("Account Number already exists.");
        //        }

        //        decimal? oldOpeningBalance = null;

        //        // 2️⃣ If updating → load old values
        //        if (bank.BankId != 0)
        //        {
        //            var existing = await _unitOfWork.Banks.GetAsync(bank.BankId);
        //            oldOpeningBalance = existing.BankOPBal;
        //        }
        //        if (bank.BankId == 0)
        //        {
        //            bank.CreatedBy = await _currentUserService.GetUsernameAsync();
        //            bank.CreatedDate = DateTime.Now;
        //            await _unitOfWork.Banks.CreateAsync(bank);
        //            await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), _currentUserService.MachineName, _currentUserService.IpAddress, "Bank", $"Bank Created Successfully {bank.BankName}", $"AccountNo.: {bank.AccountNo}");
        //        }
        //        else
        //        {
        //            bank.ModifiedBy = await _currentUserService.GetUsernameAsync();
        //            bank.ModifiedDate = DateTime.Now;
        //            await _unitOfWork.Banks.UpdateAsync(bank);
        //            await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), _currentUserService.MachineName, _currentUserService.IpAddress, "Bank", $"Bank Updated Successfully {bank.BankName}", $"AccountNo.: {bank.AccountNo}");
        //        }

        //        await _unitOfWork.SaveAsync();
        //        // 4️⃣ If NEW BANK: Create Opening Balance FundTrans
        //        if (bank.BankId != 0 && oldOpeningBalance == null)
        //        {
        //            var ft = new FundTrans
        //            {
        //                TransNo = bank.BankId.ToString(),
        //                TransDate = bank.CreatedDate ?? DateTime.Today,
        //                TransType = "Opening Balance",
        //                TrasactionMode = "Bank",
        //                BankId = bank.BankId,
        //                BillAmount = bank.BankOPBal,
        //                PaidOrReceived = bank.BankOPBal,
        //                BalanceAfter = bank.BankOPBal,
        //                RunningBalance = bank.BankOPBal,
        //                PaymentRefId = bank.BankId
        //            };

        //            await _unitOfWork.fundTranss.CreateAsync(ft);
        //            await _unitOfWork.SaveAsync();
        //        }


        //        // 5️⃣ If UPDATING BANK: Check if Opening Balance changed
        //        if (oldOpeningBalance != null && oldOpeningBalance != bank.BankOPBal)
        //        {
        //            decimal difference = bank.BankOPBal - oldOpeningBalance.Value;

        //            // Update current balance
        //            bank.CurrentBalance += difference;
        //            await _unitOfWork.Banks.UpdateAsync(bank);
        //            await _unitOfWork.SaveAsync();

        //            // Create adjustment entry
        //            var ftAdjust = new FundTrans
        //            {
        //                TransNo = bank.BankId.ToString(),
        //                TransDate = DateTime.Today,
        //                TransType = "Opening Balance Adjust",
        //                TrasactionMode = "Bank",
        //                BankId = bank.BankId,
        //                BillAmount = difference,
        //                PaidOrReceived = difference,
        //                BalanceAfter = bank.CurrentBalance,
        //                RunningBalance = bank.CurrentBalance,
        //                PaymentRefId = bank.BankId
        //            };

        //            await _unitOfWork.fundTranss.CreateAsync(ftAdjust);
        //            await _unitOfWork.SaveAsync();
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _loggingService.LogDeveloperError(ex, $"Error during bank upsert for AccountNo: {bank.AccountNo}");
        //        throw;
        //    }
        //}
        public async Task DeleteFundTransByBank(int bankId)
        {
            using var trx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var fundTrans = await _unitOfWork.fundTranss.GetQueryable()
                                    .Where(x => x.BankId == bankId)
                                    .ToListAsync();

                foreach (var row in fundTrans)
                {
                    await _unitOfWork.fundTranss.DeleteAsync(row.FundTransId); // ensure correct PK property name
                }

                await _unitOfWork.SaveAsync(); // ✔ important
                await trx.CommitAsync();
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }
        }


        public async Task<bool> DeleteBankAsync(int bankId)
        {
            try
            {
                var bank = await _unitOfWork.Banks.GetAsync(bankId);
                if (bank == null) return false;

                 await DeleteFundTransByBank(bankId);

                var deleted = await _unitOfWork.Banks.DeleteAsync(bankId);
                if (deleted)
                {
                    await _unitOfWork.SaveAsync();
                    await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), _currentUserService.MachineName, _currentUserService.IpAddress, "BankList", $"Bank Deleted successfully. {bank.BankName}", $"Bank AccountNo.:{bank.AccountNo}");
                }
                return deleted;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting bank with ID {bankId}.");
                return false;
            }
        }

        public async Task<BankDetailsResponse> GetIfscDetailsAsync(string ifscCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ifscCode) || !IsValidIfscFormat(ifscCode))
                {
                    return null;
                }

                var response = await _httpClient.GetAsync($"https://ifsc.razorpay.com/{ifscCode.Trim()}");

                if (!response.IsSuccessStatusCode)
                {
                    // IFSC not found or API error
                    return null;
                }

                var bankDetails = await response.Content.ReadFromJsonAsync<BankDetailsResponse>();
                return bankDetails;
               // return await _httpClient.GetFromJsonAsync<BankDetailsResponse>($"https://ifsc.razorpay.com/{ifscCode}");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching IFSC details for code: {ifscCode}");
                return null;
            }
        }

        private bool IsValidIfscFormat(string ifsc)
        {
            if (string.IsNullOrWhiteSpace(ifsc)) return false;
            var regex = new System.Text.RegularExpressions.Regex("^[A-Za-z]{4}0[A-Za-z0-9]{6}$");
            return regex.IsMatch(ifsc.Trim());
        }
    }
}
