using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Data.Services
{
    public class WalletRepository
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
    }
}
