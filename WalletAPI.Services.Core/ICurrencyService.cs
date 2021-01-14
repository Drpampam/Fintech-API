using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WalletAPI.Services.Core
{
    public interface ICurrencyService
    {
        Task<double> CurrencyConverter(string mainCurrency, string transactionCurrency, double amount);
        Task<bool> VerifyCurrencyExist(string curr);
    }
}
