namespace EfficientWallet.ECB.Service.Contracts
{
    public record GetEchangesResponse
    (
        DateTime? RatesDate,
        List<ExchangeRateItem> ExchangeRates
    );

    public record ExchangeRateItem
    (
        string Currency,
        decimal? Rate
    );
}