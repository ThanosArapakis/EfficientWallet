using EfficientWallet.ECB.Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.ECB.Service
{
    public interface IEcbService
    {
        public Task<GetEchangesResponse> GetExchangeRatesAsync();
    }
}
