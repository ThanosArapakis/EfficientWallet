using EfficientWallet.ECB.Http.Contracts;
using EfficientWallet.ECB.Service.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.ECB.Service.Util
{
    internal static class EcbMapper
    {
        internal static GetEchangesResponse ToGetExchangesResponse(this EcbEnvelope ecbEnvelope)
        {
            return new GetEchangesResponse
            (
                DateTime.Parse(ecbEnvelope.Cube.Date, new CultureInfo("el-GR")),
                ecbEnvelope.Cube.Rates.Select(x => new ExchangeRate
                (
                    x.Currency,
                    x.Rate
                )).ToList()
            );
        }
    }

}
