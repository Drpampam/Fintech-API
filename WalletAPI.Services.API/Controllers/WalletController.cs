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
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IGenericRepository<Wallet> _walletRepo;
        private readonly IWalletRepository _walletRepository;
        private readonly IGenericRepository<Transaction> _transactionRepo;
        private readonly ICurrencyService _currencyService;
        private readonly IUserCurrencyRepository _userCurrencyRepository;
        public WalletController(ILogger<WalletController> logger, UserManager<User> userManager,
            IGenericRepository<Wallet> walletRepo, IWalletRepository walletRepository,
            IGenericRepository<Transaction> transactionRepo, ICurrencyService currencyService,
            IUserCurrencyRepository userCurrencyRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _walletRepo = walletRepo;
            _walletRepository = walletRepository;
            _transactionRepo = transactionRepo;
            _currencyService = currencyService;
            _userCurrencyRepository = userCurrencyRepository;
        }

        [Authorize(Roles = "elite")]
        [HttpPost("add-wallet")]
        public async Task<IActionResult> AddWallet(string currency)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound(ResponseMessage.Message("Not found", "User does not exist"));

            currency = currency.ToUpper();
            if (!await _currencyService.VerifyCurrencyExist(currency))
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Enter a valid Currency Type"));

            try
            {
                var wallet = new Wallet
                {
                    Id = Guid.NewGuid().ToString(),
                    Currency = currency,
                    UserId = user.Id
                };

                await _walletRepo.Add(wallet);

                return Ok(ResponseMessage.Message("Success", data: "Wallet was successfully added"));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
            }            
        }

        [Authorize(Roles = "admin")]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserWallet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Id cannot be empty"));

            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
                return NotFound(ResponseMessage.Message("Not found", errors: "User does not exist"));
            if (admin.Id == id)
                return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User does not have authorized access"));

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(ResponseMessage.Message("Not found", errors: "User does not exist"));

            try
            {
                var result = await _walletRepository.GetWalletsByUserId(id);
                List<Wallet> wallet = new List<Wallet>();

                foreach (var item in result)
                {
                    var walletToReturn = new Wallet
                    {
                        Id = item.Id,
                        UserId = item.UserId,
                        Currency = item.Currency,
                        Balance = item.Balance
                    };

                    wallet.Add(walletToReturn);
                }

                return Ok(ResponseMessage.Message("Success", data: wallet));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Data access error", errors: "Could not access record from data source" ));
            }
        }

        [Authorize(Roles = "noob, elite")]
        [HttpPost("fund-wallet")]
        public async Task<IActionResult> FundUserWallet(string currency, double amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound(ResponseMessage.Message("Not found", errors: "User does not exit"));
            
            if (amount > 0)
            {
                currency = currency.ToUpper();
                if (!await _currencyService.VerifyCurrencyExist(currency))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Enter a valid Currency Type"));

                Wallet wallet;
                try
                {
                    wallet = await _walletRepository.GetWalletByUserId(user.Id);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                }

                if (await _userManager.IsInRoleAsync(user, "noob"))
                {
                    Transaction transaction;
                    try
                    {
                        transaction = new Transaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            Type = "deposit",
                            Currency = currency,
                            Amount = amount,
                            Status = "pending",
                            WalletId = wallet.Id
                        };

                        await _transactionRepo.Add(transaction);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                    }

                    try
                    {
                        var result = await _walletRepository.GetWalletByUserId(user.Id);

                        if (result == null)
                            return NotFound(ResponseMessage.Message("Not found", errors: "Wallet does not exist"));

                        var curr = result.Currency;

                        var convertedAmount = await _currencyService.CurrencyConverter(currency, curr, amount);

                        wallet.Balance += convertedAmount;
                        await _walletRepo.Update(wallet);
                        return Ok(ResponseMessage.Message("Success", data: $"Succesffuly funded wallet {result.Id} with {currency}{amount}"));
                    }
                    catch (Exception e)
                    {
                        await _transactionRepo.Delete(transaction);
                        await _walletRepo.Delete(wallet);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Failed to fund wallet"));
                    }
                }
                else
                {
                    var result = await _walletRepository.GetWalletOfUserByCurrency(user.Id, currency);
                    if (result == null)
                    {
                        try
                        {
                            Wallet newWallet = new Wallet
                            {
                                Id = Guid.NewGuid().ToString(),
                                UserId = user.Id,
                                Currency = currency
                            };

                            await _walletRepo.Add(newWallet);
                            result = await _walletRepository.GetWalletOfUserByCurrency(user.Id, currency);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message);
                            return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                        }
                    }

                    Transaction transaction;
                    try
                    {
                        transaction = new Transaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            Type = "deposit",
                            WalletId = result.Id,
                            Amount = amount,
                            Currency = currency,
                            Status = "approved"
                        };

                        await _transactionRepo.Add(transaction);
                    }
                    catch (Exception e)
                    {
                        await _walletRepo.Delete(wallet);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                    }

                    try
                    {
                        wallet.Balance += amount;
                        await _walletRepo.Update(wallet);
                        return Ok(ResponseMessage.Message("Success", data: $"Succesffuly funded wallet {result.Id} with {currency}{amount}"));
                    }
                    catch (Exception e)
                    {
                        await _transactionRepo.Delete(transaction);
                        await _walletRepo.Delete(wallet);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Failed to fund wallet"));
                    }
                }
            }
            return BadRequest(ResponseMessage.Message("Bad request", errors: "Amount must be greater than zero"));            
        }

        [Authorize(Roles = "admin")]
        [HttpPost("fund-wallet/{id}")]
        public async Task<IActionResult> FundWalletByAdmin(string currency, double amount, string id)
        {
            var admin = await _userManager.GetUserAsync(User);
            if (admin == null) return NotFound(ResponseMessage.Message("Not found", errors: "User does not exit"));
            if (amount > 0)
            {
                currency = currency.ToUpper();
                if (!await _currencyService.VerifyCurrencyExist(currency))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Enter a valid Currency Type"));
                Wallet wallet;
                try
                {
                    wallet = await _walletRepository.GetWalletById(id);
                    if (wallet == null)
                        return NotFound(ResponseMessage.Message("Not found", errors: "Wallet does not exist"));
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                }

                Transaction transaction;
                try
                {
                    transaction = new Transaction
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = wallet.UserId,
                        Type = "deposit",
                        WalletId = wallet.Id,
                        Amount = amount,
                        Currency = currency,
                        Status = "approved"
                    };

                    await _transactionRepo.Add(transaction);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                }

                try
                {
                    var user = await _userManager.FindByIdAsync(wallet.UserId);
                    if (user == null)
                        return NotFound(ResponseMessage.Message("Not found", errors: $"User with wallet {id} does not exist"));

                    UserCurrency userCurrency;
                    try
                    {
                        userCurrency = await _userCurrencyRepository.GetUserCurrencyByUserId(user.Id);
                    }
                    catch (Exception e)
                    {
                        await _transactionRepo.Delete(transaction);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Data access error", errors: "Could not access record from data source"));
                    }

                    var curr = userCurrency.MainCurrency;

                    var convertedAmount = await _currencyService.CurrencyConverter(currency, curr, amount);

                    wallet.Balance += convertedAmount;
                    await _walletRepo.Update(wallet);
                    return Ok(ResponseMessage.Message("Success", data: $"Wallet {id} has been funded with {currency}{amount} by {admin.Id} successfully!!!"));
                }
                catch (Exception e)
                {
                    await _transactionRepo.Delete(transaction);
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Failed to fund wallet"));
                }
            }
            return BadRequest(ResponseMessage.Message("Bad request", errors: "Amount must be greater than zero"));            
        }

        [Authorize(Roles = "noob, elite")]
        [HttpPost("wallet-withdraw")]
        public async Task<IActionResult> WithdrawFromWallet(string currency, double amount)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return NotFound(ResponseMessage.Message("Not found", "User does not exist"));

            if(amount > 0)
            {
                currency = currency.ToUpper();
                if (!await _currencyService.VerifyCurrencyExist(currency))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Enter a valid Currency Type"));

                Transaction transaction;

                if (await _userManager.IsInRoleAsync(user, "noob"))
                {
                    var wallet = await _walletRepository.GetWalletByUserId(user.Id);

                    try
                    {
                        transaction = new Transaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            Type = "withdraw",
                            WalletId = wallet.Id,
                            Amount = amount,
                            Currency = currency,
                            Status = "pending"
                        };

                        await _transactionRepo.Add(transaction);
                        return Ok(ResponseMessage.Message($"Success", data: $"Withdrawal transaction {transaction.Id} initiated. Awaiting approval from an Admin"));
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                    }
                }
                else
                {
                    var wallet = await _walletRepository.GetWalletOfUserByCurrency(user.Id, currency);

                    string transactionCurr = currency;
                    if (wallet == null)
                    {
                        try
                        {
                            var curr = await _userCurrencyRepository.GetUserCurrencyByUserId(user.Id);
                            if (curr == null)
                                return NotFound(ResponseMessage.Message("Not found", errors: "User currency not found"));
                            wallet = await _walletRepository.GetWalletOfUserByCurrency(user.Id, curr.MainCurrency);
                            if (wallet == null)
                                return NotFound(ResponseMessage.Message("Not found", errors: "Wallet currency not found"));
                            transactionCurr = curr.MainCurrency;
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message);
                            return BadRequest(ResponseMessage.Message("Data access error", errors: "Could not access record from data source"));
                        }
                    }

                    try
                    {
                        transaction = new Transaction
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            Type = "withdraw",
                            WalletId = wallet.Id,
                            Amount = amount,
                            Currency = currency,
                            Status = "approved"
                        };

                        await _transactionRepo.Add(transaction);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                    }

                    try
                    {
                        var conversion = await _currencyService.CurrencyConverter(currency, transactionCurr, amount);
                        if (wallet.Balance >= conversion)
                        {
                            wallet.Balance -= conversion;

                            await _walletRepo.Update(wallet);
                            return Ok(ResponseMessage.Message("Success", data: "Withdrawal successful"));
                        }
                        else
                        {
                            return BadRequest(ResponseMessage.Message("Bad request", errors: "Insufficient funds available for withdrawal"));
                        }
                    }
                    catch (Exception e)
                    {
                        await _transactionRepo.Delete(transaction);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                    }
                }
            }
            return BadRequest(ResponseMessage.Message("Bad request", errors: "Amount must be greater than zero"));
        }
    }
}
