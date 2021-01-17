using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using WalletAPI.Common.Utilities.Helpers;
using WalletAPI.Services.Core;
using WalletAPI.Services.Data.Services;
using WalletAPI.Services.DTOs;
using WalletAPI.Services.Models;

namespace Jumga.Services.API.Controllers
{
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUserCurrencyRepository _userCurrencyRepository;
        private readonly IGenericRepository<UserCurrency> _userCurrencyRepo;
        private readonly IGenericRepository<Wallet> _walletRepo;
        private readonly IWalletRepository _walletRepository;
        private readonly ICurrencyService _currencyService;

        public UserController(ILogger<UserController> logger, UserManager<User> userManager, 
            IUserCurrencyRepository userCurrencyRepository, IGenericRepository<UserCurrency> userCurrencyRepo,
            IGenericRepository<Wallet> walletRepo, IWalletRepository walletRepository, ICurrencyService currencyService)
        {
            _logger = logger;
            _userManager = userManager;
            _userCurrencyRepository = userCurrencyRepository;
            _userCurrencyRepo = userCurrencyRepo;
            _walletRepo = walletRepo;
            _walletRepository = walletRepository;
            _currencyService = currencyService;
        }

        // Get All Users
        [Authorize(Roles = "admin")]
        [HttpGet("{page}")]
        public async Task<IActionResult> GetAllUsers(int page)
        {
            if (page < 1)
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Page must be greater than 1"));

            var admin = await _userManager.GetUserAsync(User);
            if (admin == null)
                return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User does not have authorized access"));

            try
            {
                var users = _userManager.Users;
                if (users == null)
                    return NotFound(ResponseMessage.Message("Not found", errors: "Users not found"));
                return Ok(ResponseMessage.Message("Success", data: users));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
            }
        }

        // Promote User
        [Authorize(Roles = "admin")]
        [HttpPost("{id}/promote-user")]
        public async Task<IActionResult> PromoteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Not user id found"));

            var admin = await _userManager.GetUserAsync(User);

            if (admin == null)
                return BadRequest(ResponseMessage.Message("Not found", errors: "User does not exist"));
            if (admin.Id == id)
                return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User is unauthorized"));

            var user = await _userManager.FindByIdAsync(id);
            if(user == null)
                return BadRequest(ResponseMessage.Message("Not found", errors: "User does not exist"));

            try
            {
                if (!await _userManager.IsInRoleAsync(user, "noob"))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "User can not be promoted"));

                await _userManager.RemoveFromRoleAsync(user, "noob");
                await _userManager.AddToRoleAsync(user, "elite");

                return Ok(ResponseMessage.Message("Success", data: "Successfully promoted"));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
            }
        }

        // Demote User
        [Authorize(Roles = "admin")]
        [HttpPost("{id}/demote-user")]
        public async Task<IActionResult> DemoteUser(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Not user id found"));

                var admin = await _userManager.GetUserAsync(User);

                if (admin == null)
                    return BadRequest(ResponseMessage.Message("Not found", errors: "User does not exist"));
                if (admin.Id == id)
                    return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User is unauthorized"));

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return BadRequest(ResponseMessage.Message("Not found", errors: "User does not exist"));

                try
                {
                    if (!await _userManager.IsInRoleAsync(user, "elite"))
                        return BadRequest(ResponseMessage.Message("Bad request", "User can not be demoted"));

                    await _userManager.RemoveFromRoleAsync(user, "elite");
                    await _userManager.AddToRoleAsync(user, "noob");

                    return Ok(ResponseMessage.Message("Success", data: "Successfully demoted"));
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
            }
        }

        // Change User Currency
        [Authorize(Roles = "admin")]
        [HttpPost("{id}/change-user-currency")]
        public async Task<IActionResult> ChangeUserCurrency(string id, string newUserCurrency)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Not user id found"));

                newUserCurrency = newUserCurrency.ToUpper();
                if (!await _currencyService.VerifyCurrencyExist(newUserCurrency))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Enter a valid Currency Type"));

                var admin = await _userManager.GetUserAsync(User);

                if (admin == null)
                    return NotFound(ResponseMessage.Message("Not found", errors: "User does not exist"));
                if (admin.Id == id)
                    return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User is unauthorized"));

                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                    return BadRequest(ResponseMessage.Message("Not found", errors: "User does not exist"));

                if (await _userManager.IsInRoleAsync(user, "admin"))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "User is Admin, cannot change currency"));

                UserCurrency userCurrency;
                try
                {
                    userCurrency = await _userCurrencyRepository.GetUserCurrencyByUserId(user.Id);
                    if (userCurrency == null)
                        return NotFound(ResponseMessage.Message("Not found", "User currency does not exist"));
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                }

                try
                {
                    userCurrency.MainCurrency = newUserCurrency;
                    await _userCurrencyRepo.Update(userCurrency);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                }

                if (await _userManager.IsInRoleAsync(user, "noob"))
                {
                    try
                    {
                        var wallet = await _walletRepository.GetWalletByUserId(user.Id);
                        var value = await _currencyService.CurrencyConverter(wallet.Currency, newUserCurrency, wallet.Balance);
                        wallet.Balance = value;
                        wallet.Currency = newUserCurrency;
                        await _walletRepo.Update(wallet);
                    }
                    catch (Exception e)
                    {
                        await _userCurrencyRepo.Delete(userCurrency);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
                    }
                }                    

                return Ok(ResponseMessage.Message("Success", data: $"Successfully changed user with {user.Id} currency"));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
            }
        }
    }
}
