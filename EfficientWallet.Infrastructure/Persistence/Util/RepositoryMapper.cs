using EfficientWallet.Application.Wallets.Results;
using EfficientWallet.Domain;
using EfficientWallet.ECB.Service.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Infrastructure.Persistence.Util
{
    public static class RepositoryMapper
    {
        public static ExchangeRate ToExchangeRate(this ExchangeRateItem item, DateTime? ratesDate)
        => new ExchangeRate
        {
            RatesDate = DateOnly.FromDateTime(ratesDate!.Value),
            Currency = item.Currency,
            Rate = item.Rate ?? 0m,
            BaseCurrency = "EUR",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        public static RetrieveWalletBalanceResponse ToWalletBalanceResponse(this Wallet wallet) => new RetrieveWalletBalanceResponse(wallet.Balance);
    }
}
