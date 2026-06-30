using EfficientWallet.Application.Common;
using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Domain;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Services
{
    public class BalanceConverter(IRateCache _cache) : IBalanceConverter
    {
        public decimal Convert(string fromCurrency, string toCurrency, decimal amount)
        {
            // No conversion needed if the currencies match.
            if (CompareCurrency(fromCurrency, toCurrency))
                return amount;

            decimal amountInEur = ToEur(fromCurrency, amount);
            return FromEur(toCurrency, amountInEur);
        }

        // X -> EUR.  The feed quotes 1 EUR = rate[X] units of X, so units / rate = EUR.
        private decimal ToEur(string currency, decimal amount)
        {
            if (CompareCurrency(currency, Const.EcbBaseCurrency))
                return amount;

            if (!_cache.TryGetRate(currency, out var rate))
                throw new InvalidOperationException($"No exchange rate available for '{currency}'.");

            return amount / rate;
        }

        private decimal FromEur(string currency, decimal amountInEur)
        {
            if (CompareCurrency(currency, Const.EcbBaseCurrency))
                return amountInEur;

            if (!_cache.TryGetRate(currency, out var rate))
                throw new InvalidOperationException($"No exchange rate available for '{currency}'.");

            return amountInEur * rate;
        }

        private bool CompareCurrency(string currency1, string currency2)
            => currency1.Equals(currency2, StringComparison.CurrentCultureIgnoreCase);
    }
}
