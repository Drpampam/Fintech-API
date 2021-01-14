using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using WalletAPI.Common.Utilities.Helpers;
using WalletAPI.Commons.Utilities.Helpers;
using WalletAPI.Services.Core;
using WalletAPI.Services.Data.Services;
using WalletAPI.Services.DTOs;
using WalletAPI.Services.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Jumga.Services.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IGenericRepository<UserCurrency> _userCurrencyRepository;
        private readonly IGenericRepository<Wallet> _walletRepository;
        private readonly IConfiguration _config;
        private readonly ICurrencyService _currencyService;

        public AuthController(ILogger<AuthController> logger, UserManager<User> userManager, 
            SignInManager<User> signInManager, IGenericRepository<UserCurrency> userCurrencyRepository,
            IGenericRepository<Wallet> walletRepository, IConfiguration configuration,
            ICurrencyService currencyService)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _userCurrencyRepository = userCurrencyRepository;
            _walletRepository = walletRepository;
            _config = configuration;
            _currencyService = currencyService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserToRegisterDTO model)
        {
            try
            {
                var checkUser = await _userManager.FindByEmailAsync(model.Email);
                if (checkUser != null)
                    return BadRequest(ResponseMessage.Message("Bad Request", errors: "User already exists"));

                model.Role = model.Role.ToLower();
                if (model.Role != "admin" && model.Role != "noob" && model.Role != "elite")
                    return BadRequest(ResponseMessage.Message("Bad Request", errors: "User role must be either Admin, Noob or Elite"));


                model.MainCurrency = model.MainCurrency.ToUpper();
                if (!await _currencyService.VerifyCurrencyExist(model.MainCurrency))
                    return BadRequest(ResponseMessage.Message("Bad request", errors: "Enter a valid Currency Type"));

                var userModel = new User
                {
                    UserName = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                // create user
                var result = await _userManager.CreateAsync(userModel, model.Password);

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                    return NotFound(ResponseMessage.Message("Not Found", errors: "User does not exit"));

                await _userManager.AddToRoleAsync(user, model.Role);

                if (!await _userManager.IsInRoleAsync(user, "admin"))
                {
                    UserCurrency mainCurrency;
                    try
                    {
                        mainCurrency = new UserCurrency
                        {
                            Id = Guid.NewGuid().ToString(),
                            UserId = user.Id,
                            MainCurrency = model.MainCurrency
                        };

                        await _userCurrencyRepository.Add(mainCurrency);
                    }
                    catch (Exception e)
                    {
                        await _userManager.DeleteAsync(user);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "User failed to create!!!"));
                    }

                    try
                    {
                        var wallet = new Wallet
                        {
                            Id = Guid.NewGuid().ToString(),
                            Currency = model.MainCurrency,
                            UserId = user.Id
                        };

                        await _walletRepository.Add(wallet);
                    }
                    catch (Exception e)
                    {
                        await _userManager.DeleteAsync(user);
                        await _userCurrencyRepository.Delete(mainCurrency);
                        _logger.LogError(e.Message);
                        return BadRequest(ResponseMessage.Message("Bad request", errors: "User failed to create!!!"));
                    }
                }
            }
            catch (DbException de)
            {
                _logger.LogError(de.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
            }

            return Ok(ResponseMessage.Message("Success", data: "Successfully registered!!!"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserToLoginDTO model)
        {
            var getToken = "";
            try
            {
                //get user by email
                var user = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);

                //Check if user exist
                if (user == null)
                {
                    return Unauthorized(ResponseMessage.Message("Unauthorized", errors: "Invalid credentials"));
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var role = await _userManager.GetRolesAsync(user);
                    getToken = JwtTokenConfig.GetToken(user, _config, role[0]);
                }
                else
                {
                    return BadRequest(ResponseMessage.Message("Bad Request", errors: "Failed to Login user"));
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest(ResponseMessage.Message("Bad request", errors: "Data processing error"));
            }
            return Ok(ResponseMessage.Message("Success", data: new { token = getToken }));
        }
    }
}
