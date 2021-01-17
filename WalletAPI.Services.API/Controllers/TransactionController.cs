using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WalletAPI.Common.Utilities.Helpers;
using WalletAPI.Services.Core;
using WalletAPI.Services.Data.Services;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<TransactionController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGenericRepository<Transaction> _transactionRepo;
        private readonly IWalletRepository _walletRepository;
        private readonly IUserCurrencyRepository _userCurrencyRepository;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericRepository<Wallet> _walletRepo;
        public TransactionController(ILogger<TransactionController> logger, UserManager<User> userManager,
            ITransactionRepository transactionRepository, IGenericRepository<Transaction> transactionRepo,
            IWalletRepository walletRepository, IUserCurrencyRepository userCurrencyRepository,
            ICurrencyService currencyService, IGenericRepository<Wallet> walletRepo)
        {
            _logger = logger;
            _userManager = userManager;
            _transactionRepository = transactionRepository;
            _transactionRepo = transactionRepo;
            _walletRepository = walletRepository;
            _userCurrencyRepository = userCurrencyRepository;
            _currencyService = currencyService;
            _walletRepo = walletRepo;
        }

        // Endpoint to approve a transaction by the admin
        [Authorize(Roles = "admin")]
        [HttpPatch("approve/user/{id}")]
        public async Task<IActionResult> ApproveTransaction(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Id can not be empty"));

            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
                return NotFound(ResponseMessage.Message("Not found", errors: "User does not exist"));
            if (admin.Id == id)
                return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User does not have authorized access"));

            Transaction transaction;
            try
            {
                transaction = await _transactionRepository.GetTransactionById(id);
                if (transaction == null)
                    return NotFound(ResponseMessage.Message("Not found", "Transaction does not exist"));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Data access error", errors: "Could not access record from data source"));
            }

            if (transaction.Status == "approved")
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Transaction is already approved"));

            Wallet wallet;
            try
            {
                wallet = await _walletRepository.GetWalletById(transaction.WalletId);
                if (wallet == null)
                    return NotFound(ResponseMessage.Message("Not found", errors: "Wallet does not exist"));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Data access error", errors: "Could not access record from data source"));
            }

            UserCurrency userCurrency;
            try
            {
                userCurrency = await _userCurrencyRepository.GetUserCurrencyByUserId(wallet.UserId);
                if (userCurrency == null)
                    return NotFound(ResponseMessage.Message("Not found", errors: "User currency does not exist"));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Data access error", errors: "Could not access record from data source"));
            }

            try
            {
                var currencyConversion = await _currencyService.CurrencyConverter(transaction.Currency, userCurrency.MainCurrency, transaction.Amount);

                if (transaction.Type == "deposit")
                {
                    wallet.Balance += currencyConversion;
                }
                else
                {
                    if (wallet.Balance >= currencyConversion)
                        wallet.Balance -= currencyConversion;
                    else
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Insufficient balance available"));
                }

                await _walletRepo.Update(wallet);                
            }
            catch (Exception e)
            {
                await _walletRepo.Delete(wallet);
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Transaction could not be completed"));
            }

            try
            {
                transaction.Status = "approved";
                await _transactionRepo.Update(transaction);
            }
            catch (Exception e)
            {
                await _walletRepo.Delete(wallet);
                await _transactionRepo.Delete(transaction);
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Transaction Status could not be updated"));
            }

            return Ok(ResponseMessage.Message("Success", data: $"Transaction with {transaction.Id} has been approved"));
        }

        // Endpoint to get all transactions by status by the admin
        [Authorize(Roles = "admin")]
        [HttpGet("{status}")]
        public async Task<IActionResult> GetTransaction(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest(ResponseMessage.Message("Invalid input", errors:"Status cannot be empty"));

            status = status.ToLower();
            if (status == "pending" && status == "approved")
                return BadRequest(ResponseMessage.Message("Invalid input", errors: "Status should be either pending or approved"));

            try
            {
                var transactions = await _transactionRepository.GetTransactionByStatus(status);

                List<Transaction> result = new List<Transaction>();
                foreach (var item in transactions)
                {
                    var transactionToReturn = new Transaction
                    {
                        Id = item.Id,
                        UserId = item.UserId,
                        WalletId = item.WalletId,
                        Type = item.Type,
                        Currency = item.Currency,
                        Amount = item.Amount,
                        Status = item.Status,
                        CreatedAt = item.CreatedAt,
                        UpdatedAt = item.UpdatedAt
                    };

                    result.Add(transactionToReturn);
                }

                return Ok(ResponseMessage.Message("Success", data: result));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Data access error", errors: "Could not access record from data source"));
            }
        }
    }
}
