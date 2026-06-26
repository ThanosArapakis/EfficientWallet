namespace EfficientWallet.ECB.Service.Contracts
{
    public record GetEchangesResponse
    (
        DateTime? RatesDate,
        List<ExchangeRate> ExchangeRates
    );

    public record ExchangeRate
    (
        string Currency,
        decimal? Rate
    );
}