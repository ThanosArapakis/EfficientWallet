using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Common.Interfaces
{
    public interface IBalanceConverter
    {
        public decimal Convert(string fromCurrency, string toCurrency, decimal amount);
    }
}
