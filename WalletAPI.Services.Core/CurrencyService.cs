using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using WalletAPI.Commons.Utilities.Helpers;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Core
{
    public class CurrencyService : ICurrencyService
    {
        private readonly string API = "http://data.fixer.io/api/latest?access_key=a2a696ed2a992fef7ae665b3afd06243";

        public async Task<double> CurrencyConverter(string mainCurrency, string transactionCurrency, double amount)
        {
            // JSON call
            var response = await Util.APICall(API);

            // Currency Converter Logic
            var currencyToBase = Convert.ToDecimal(amount) / response.Rates[mainCurrency];
            var convertedCurrency = response.Rates[transactionCurrency] * currencyToBase;

            return Convert.ToDouble(convertedCurrency);
        }

        public async Task<bool> VerifyCurrencyExist(string curr)
        {
            var response = await Util.APICall(API);
            var verify = response.Rates.Keys;
            foreach (var item in verify)
            {
                if (item.Contains(curr))
                    return true;
            }

            return false;
        }
    }
}
