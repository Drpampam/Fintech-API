using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Data.Services
{
    public interface IWalletRepository
    {
        Task<Wallet> GetWalletById(string id);
        Task<Wallet> GetWalletByUserId(string id);
        Task<IEnumerable<Wallet>> GetWalletsByUserId(string id);
        Task<Wallet> GetWalletOfUserByCurrency(string id, string currency);
    }
}
