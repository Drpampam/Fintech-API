using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Data.Services
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly AppDbContext _ctx;
        public TransactionRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<Transaction> GetTransactionById(string id)
        {
            return await _ctx.Transactions.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionByStatus(string status)
        {
            return await _ctx.Transactions.Where(x => x.Status == status).ToListAsync();
        }
    }
}
