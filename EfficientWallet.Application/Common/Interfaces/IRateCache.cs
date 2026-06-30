using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Common.Interfaces
{
    public interface IRateCache
    {
        public bool TryGetRate(string currency, out decimal rate);

        public void SetRates(IReadOnlyDictionary<string, decimal> rates);

    }
}
