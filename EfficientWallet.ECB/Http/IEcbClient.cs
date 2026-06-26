using EfficientWallet.ECB.Http.Contracts;
using Refit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.ECB.Http
{
    public interface IEcbClient
    {
        [Get("")]
        Task<EcbEnvelope> GetExchangeRatesAsync();
    }
}
