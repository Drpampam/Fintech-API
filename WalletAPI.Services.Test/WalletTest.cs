using NUnit.Framework;
using System;
using WalletAPI.Services.Core;

namespace WalletAPI.Services.Test
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void CheckIfCurrencyExistSucceedsIfInputExists()
        {
            string currency = "NGN";
            var curr = new CurrencyService();
            var result = curr.VerifyCurrencyExist(currency).Result;
            var expected = true; 

            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        public void CheckIfCurrencyExistFailedWhenInputDoesNotExist()
        {
            string currency = "AAA";
            var curr = new CurrencyService();
            var result = curr.VerifyCurrencyExist(currency).Result;
            var expected = false;

            Assert.That(expected, Is.EqualTo(result));
        }

        [Test]
        public void CheckIfCurrencyConversionSucceeds()
        {
            string mainCurrency = "NGN";
            string transactionCurrency = "USD";
            double amount = 1234;

            var curr = new CurrencyService();
            var result = curr.CurrencyConverter(mainCurrency, transactionCurrency, amount).Result;
            Assert.Pass();
        }

        [Test]
        public void CheckIfCurrencyConversionFailsForInvalidCurrencyInput()
        {
            string mainCurrency = "NGN";
            string transactionCurrency = "AAA";
            double amount = 1234;

            var curr = new CurrencyService();

            Assert.Throws<AggregateException>(() => curr.CurrencyConverter(mainCurrency, transactionCurrency, amount).Wait());
        }
    }
}