using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using WalletAPI.Commons.Utilities.Helpers;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Core
{
    public class CurrencyService : ICurrencyService
    {
        private readonly Fixer _fixerSettings;
        public CurrencyService(IOptions<Fixer> fixerSettings)
        {
            _fixerSettings = fixerSettings.Value;
        }

        public async Task<double> CurrencyConverter(string mainCurrency, string transactionCurrency, double amount)
        {
            // JSON call
            var response = await Util.APICall(_fixerSettings.API);

            // Currency Converter Logic
            var currencyToBase = Convert.ToDecimal(amount) / response.Rates[mainCurrency];
            var convertedCurrency = response.Rates[transactionCurrency] * currencyToBase;

            return Convert.ToDouble(convertedCurrency);
        }

        public async Task<bool> VerifyCurrencyExist(string curr)
        {
            var response = await Util.APICall(_fixerSettings.API);
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
