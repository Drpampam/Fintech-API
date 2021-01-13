using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WalletAPI.Services.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly ILogger<WalletController> _logger;
        public WalletController(ILogger<WalletController> logger)
        {
            _logger = logger;
        }

        [HttpPost("add-wallet")]
        public async Task<IActionResult> AddWallet()
        {
            return Ok();
        }
    }
}
