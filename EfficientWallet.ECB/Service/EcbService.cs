using EfficientWallet.ECB.Http;
using EfficientWallet.ECB.Service.Contracts;
using EfficientWallet.ECB.Service.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.ECB.Service
{
    public class EcbService(IEcbClient _client) : IEcbService
    {
        public async Task<GetEchangesResponse> GetExchangeRatesAsync()
        {
            var ecbReponse = await _client.GetExchangeRatesAsync();

            return ecbReponse.ToGetExchangesResponse();
        }
    }
}
