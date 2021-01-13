using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Data.Services
{
    public class UserCurrencyRepository : IUserCurrencyRepository
    {
        private readonly AppDbContext _ctx;
        public UserCurrencyRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<UserCurrency> GetUserCurrencyByUserId(string id)
        {
            return await _ctx.UserCurrencies.FirstOrDefaultAsync(x => x.UserId == id);
        }
    }
}
