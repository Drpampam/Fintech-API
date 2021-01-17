using Microsoft.Extensions.Options;
using NUnit.Framework;
using WalletAPI.Services.Core;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Test
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CheckIfCurrencyExist()
        {
            string currency = "NGN";
            var curr = new CurrencyService();
            var result = curr.VerifyCurrencyExist(currency).Result;
            var expected = true; 

            Assert.That(expected, Is.EqualTo(result));
        }
    }
}