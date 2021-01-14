using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Data.Services
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _ctx;
        public WalletRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Wallet> GetWalletById(string id)
        {
            return await _ctx.Wallets.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Wallet> GetWalletByUserId(string id)
        {
            return await _ctx.Wallets.FirstOrDefaultAsync(x => x.UserId == id);
        }

        public async Task<IEnumerable<Wallet>> GetWalletsByUserId(string id)
        {
            return await _ctx.Wallets.Where(x => x.UserId == id).ToListAsync();
        }

        public async Task<Wallet> GetWalletOfUserByCurrency(string id, string currency)
        {
            var result = await GetWalletsByUserId(id);
            return result.FirstOrDefault(x => x.Currency == currency);
        }
    }
}
