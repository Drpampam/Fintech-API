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
using WalletAPI.Services.Data.Services;
using WalletAPI.Services.DTOs;
using WalletAPI.Services.Models;

namespace Jumga.Services.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IUserCurrencyRepository _userCurrencyRepository;
        private readonly IGenericRepository<UserCurrency> _userCurrencyRepo;
        public UserController(ILogger<UserController> logger, UserManager<User> userManager, 
            IUserCurrencyRepository userCurrencyRepository, IGenericRepository<UserCurrency> userCurrencyRepo)
        {
            _logger = logger;
            _userManager = userManager;
            _userCurrencyRepository = userCurrencyRepository;
            _userCurrencyRepo = userCurrencyRepo;
        }

        // Promote User
        [HttpPost("{id}/promote-user")]
        public async Task<IActionResult> PromoteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Not user id found"));

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return BadRequest(ResponseMessage.Message("Not found", errors: "User does not exist"));
            if (user.Id != id)
                return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User is unauthorized"));

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
                return BadRequest(ResponseMessage.Message("Bad request", errors: new { message = "Data processing error" }));
            }
        }

        // Demote User
        [HttpPost("{id}/demote-user")]
        public async Task<IActionResult> DemoteUser(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Not user id found"));

                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return BadRequest(ResponseMessage.Message("Not found", errors: "User does not exist"));
                if (user.Id != id)
                    return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "User is unauthorized"));

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
                    return BadRequest(ResponseMessage.Message("Bad request", errors: new { message = "Data processing error" }));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: new { message = "Data processing error" }));
            }
        }

        [HttpPost("{id}/change-user-currency")]
        public async Task<IActionResult> ChangeUserCurrency(string id, string newUserCurrency)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Not user id found"));

                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                    return NotFound(ResponseMessage.Message("Not found", errors: "User does not exist"));

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
                    return BadRequest(ResponseMessage.Message("Bad request", errors: new { message = "Data processing error" }));
                }

                try
                {
                    userCurrency.MainCurrency = newUserCurrency;
                    await _userCurrencyRepo.Update(userCurrency);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                    return BadRequest(ResponseMessage.Message("Bad request", errors: new { message = "Data processing error" }));
                }

                // if(user)

                return Ok(ResponseMessage.Message("Success", data: "Successfully changed user with id currency"));
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: new { message = "Data processing error" }));
            }
        }
    }
}
