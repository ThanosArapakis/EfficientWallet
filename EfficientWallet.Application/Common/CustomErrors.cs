using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Common
{
    public class CustomErrors
    {
        public static Error ExchangeRateNotFound(string currency) => Error.NotFound(
            code: "EXR.NotFound",
            description: $"ExchangeRate for {currency} could not be retrieved");
    }
}
