using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Data.Services
{
    public interface IUserCurrencyRepository
    {
        Task<UserCurrency> GetUserCurrencyByUserId(string id);
    }
}
