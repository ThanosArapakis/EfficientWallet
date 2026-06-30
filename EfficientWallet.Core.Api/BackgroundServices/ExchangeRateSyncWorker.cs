using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Domain;
using EfficientWallet.ECB.Service;
using EfficientWallet.Infrastructure.Persistence.Util;

namespace EfficientWallet.Core.Api.BackgroundServices
{
    public class ExchangeRateSyncWorker : BackgroundService
    {
        private readonly ILogger<ExchangeRateSyncWorker> _logger;
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);
        private readonly IServiceProvider _services;
        private IRateCache _cache;

        public ExchangeRateSyncWorker(ILogger<ExchangeRateSyncWorker> logger, IServiceProvider serviceProvider, IRateCache cache)
        {
            _logger = logger;
            _services = serviceProvider;
            _cache = cache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Exchange Rate Sync Worker started at: {time}", DateTimeOffset.Now);
            using var timer = new PeriodicTimer(Interval);
            do
            {
                try
                {
                    await RunAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "ECB rate update cycle failed.");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            //Resolve scoped services
            using var scope = _services.CreateScope();
            var gateway = scope.ServiceProvider.GetRequiredService<IEcbService>();
            var repo = scope.ServiceProvider.GetRequiredService<IExchangeRateRepository>();

            //fetching the new exchange rate values and map
            var getRatesResponse = await gateway.GetExchangeRatesAsync();
            List<ExchangeRate> exchangeRates = getRatesResponse.ExchangeRates.ConvertAll(x => x.ToExchangeRate(getRatesResponse.RatesDate));

            //Updating DB
            await repo.UpsertAsync(exchangeRates, cancellationToken);

            //Hold the new values in cache, overriding the previous ones
            var snapshot = exchangeRates.ToDictionary(r => r.Currency, r => r.Rate);
            _cache.SetRates(snapshot);
            _logger.LogInformation("Exchange rates synchronized successfully at: {time}", DateTimeOffset.Now);
        }
    }
}
