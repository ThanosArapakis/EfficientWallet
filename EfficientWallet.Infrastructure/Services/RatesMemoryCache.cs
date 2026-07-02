using EfficientWallet.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Infrastructure.Services
{
    public class RatesMemoryCache(IMemoryCache _cache) : IRateCache
    {
        private const string Key = "ecb-rates";

        public bool TryGetRate(string currency, out decimal rate)
        {
            rate = default;
            return _cache.TryGetValue(Key, out IReadOnlyDictionary<string, decimal> rates)
                && rates.TryGetValue(currency, out rate);
        }

        public void SetRates(IReadOnlyDictionary<string, decimal> rates)
            =>  _cache.Set(Key, rates);
    }
}
