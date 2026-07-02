using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Core.Api.BackgroundServices;
using EfficientWallet.Domain;
using EfficientWallet.ECB.Service;
using EfficientWallet.ECB.Service.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace EfficientWallet.Testing
{
    /// <summary>
    /// Tests for <see cref="ExchangeRateSyncWorker.RunAsync"/> — a single sync cycle:
    /// fetch rates from the ECB gateway, map them to <see cref="ExchangeRate"/> entities,
    /// upsert them and refresh the in-memory rate cache. The scoped services the worker
    /// resolves from <see cref="IServiceProvider"/> are supplied through a mocked scope chain.
    /// </summary>
    public class ExchangeRateSyncWorkerTests
    {
        private static readonly DateTime RatesDate = new(2026, 7, 1);

        /// <summary>
        /// Builds an <see cref="IServiceProvider"/> whose <c>CreateScope()</c> yields a scope that
        /// resolves the supplied <paramref name="ecb"/> and <paramref name="repo"/> instances.
        /// </summary>
        private static IServiceProvider BuildScopedProvider(IEcbService ecb, IExchangeRateRepository repo)
        {
            var scopedProvider = new Mock<IServiceProvider>();
            scopedProvider.Setup(p => p.GetService(typeof(IEcbService))).Returns(ecb);
            scopedProvider.Setup(p => p.GetService(typeof(IExchangeRateRepository))).Returns(repo);

            var scope = new Mock<IServiceScope>();
            scope.Setup(s => s.ServiceProvider).Returns(scopedProvider.Object);

            var scopeFactory = new Mock<IServiceScopeFactory>();
            scopeFactory.Setup(f => f.CreateScope()).Returns(scope.Object);

            var rootProvider = new Mock<IServiceProvider>();
            rootProvider.Setup(p => p.GetService(typeof(IServiceScopeFactory))).Returns(scopeFactory.Object);
            return rootProvider.Object;
        }

        private static Mock<IEcbService> EcbReturning(DateTime? ratesDate, params ExchangeRateItem[] items)
        {
            var ecb = new Mock<IEcbService>();
            ecb.Setup(s => s.GetExchangeRatesAsync())
               .ReturnsAsync(new GetEchangesResponse(ratesDate, items.ToList()));
            return ecb;
        }

        private static ExchangeRateSyncWorker CreateWorker(IServiceProvider provider, IRateCache cache)
            => new(Mock.Of<ILogger<ExchangeRateSyncWorker>>(), provider, cache);

        [Fact]
        public async Task RunAsync_FetchesRates_AndUpsertsMappedEntities()
        {
            var ecb = EcbReturning(RatesDate,
                new ExchangeRateItem("USD", 1.10m),
                new ExchangeRateItem("GBP", 0.85m));

            List<ExchangeRate>? upserted = null;
            var repo = new Mock<IExchangeRateRepository>();
            repo.Setup(r => r.UpsertAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ExchangeRate>, CancellationToken>((rates, _) => upserted = rates.ToList())
                .ReturnsAsync(2);

            var worker = CreateWorker(BuildScopedProvider(ecb.Object, repo.Object), Mock.Of<IRateCache>());

            await worker.RunAsync(CancellationToken.None);

            repo.Verify(r => r.UpsertAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.NotNull(upserted);
            Assert.Equal(2, upserted!.Count);

            var usd = Assert.Single(upserted, r => r.Currency == "USD");
            Assert.Equal(1.10m, usd.Rate);
            Assert.Equal(DateOnly.FromDateTime(RatesDate), usd.RatesDate);
            Assert.Equal("EUR", usd.BaseCurrency);

            var gbp = Assert.Single(upserted, r => r.Currency == "GBP");
            Assert.Equal(0.85m, gbp.Rate);
        }

        [Fact]
        public async Task RunAsync_StoresRateSnapshotInCache()
        {
            var ecb = EcbReturning(RatesDate,
                new ExchangeRateItem("USD", 1.10m),
                new ExchangeRateItem("GBP", 0.85m));

            var repo = new Mock<IExchangeRateRepository>();
            repo.Setup(r => r.UpsertAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            IReadOnlyDictionary<string, decimal>? snapshot = null;
            var cache = new Mock<IRateCache>();
            cache.Setup(c => c.SetRates(It.IsAny<IReadOnlyDictionary<string, decimal>>()))
                 .Callback<IReadOnlyDictionary<string, decimal>>(d => snapshot = d);

            var worker = CreateWorker(BuildScopedProvider(ecb.Object, repo.Object), cache.Object);

            await worker.RunAsync(CancellationToken.None);

            cache.Verify(c => c.SetRates(It.IsAny<IReadOnlyDictionary<string, decimal>>()), Times.Once);
            Assert.NotNull(snapshot);
            Assert.Equal(1.10m, snapshot!["USD"]);
            Assert.Equal(0.85m, snapshot["GBP"]);
        }

        [Fact]
        public async Task RunAsync_MapsMissingRateToZero()
        {
            var ecb = EcbReturning(RatesDate, new ExchangeRateItem("XYZ", null));

            List<ExchangeRate>? upserted = null;
            var repo = new Mock<IExchangeRateRepository>();
            repo.Setup(r => r.UpsertAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ExchangeRate>, CancellationToken>((rates, _) => upserted = rates.ToList())
                .ReturnsAsync(1);

            IReadOnlyDictionary<string, decimal>? snapshot = null;
            var cache = new Mock<IRateCache>();
            cache.Setup(c => c.SetRates(It.IsAny<IReadOnlyDictionary<string, decimal>>()))
                 .Callback<IReadOnlyDictionary<string, decimal>>(d => snapshot = d);

            var worker = CreateWorker(BuildScopedProvider(ecb.Object, repo.Object), cache.Object);

            await worker.RunAsync(CancellationToken.None);

            Assert.Equal(0m, Assert.Single(upserted!).Rate);
            Assert.Equal(0m, snapshot!["XYZ"]);
        }

        [Fact]
        public async Task RunAsync_ForwardsCancellationTokenToRepository()
        {
            using var cts = new CancellationTokenSource();
            var ecb = EcbReturning(RatesDate, new ExchangeRateItem("USD", 1.10m));

            CancellationToken received = default;
            var repo = new Mock<IExchangeRateRepository>();
            repo.Setup(r => r.UpsertAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ExchangeRate>, CancellationToken>((_, token) => received = token)
                .ReturnsAsync(1);

            var worker = CreateWorker(BuildScopedProvider(ecb.Object, repo.Object), Mock.Of<IRateCache>());

            await worker.RunAsync(cts.Token);

            Assert.Equal(cts.Token, received);
        }

        [Fact]
        public async Task RunAsync_WithNoRates_UpsertsEmptyList_AndSetsEmptySnapshot()
        {
            var ecb = EcbReturning(RatesDate);

            List<ExchangeRate>? upserted = null;
            var repo = new Mock<IExchangeRateRepository>();
            repo.Setup(r => r.UpsertAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()))
                .Callback<IEnumerable<ExchangeRate>, CancellationToken>((rates, _) => upserted = rates.ToList())
                .ReturnsAsync(0);

            IReadOnlyDictionary<string, decimal>? snapshot = null;
            var cache = new Mock<IRateCache>();
            cache.Setup(c => c.SetRates(It.IsAny<IReadOnlyDictionary<string, decimal>>()))
                 .Callback<IReadOnlyDictionary<string, decimal>>(d => snapshot = d);

            var worker = CreateWorker(BuildScopedProvider(ecb.Object, repo.Object), cache.Object);

            await worker.RunAsync(CancellationToken.None);

            Assert.NotNull(upserted);
            Assert.Empty(upserted!);
            Assert.NotNull(snapshot);
            Assert.Empty(snapshot!);
        }

        [Fact]
        public async Task RunAsync_WhenGatewayThrows_DoesNotUpsertOrUpdateCache()
        {
            var ecb = new Mock<IEcbService>();
            ecb.Setup(s => s.GetExchangeRatesAsync())
               .ThrowsAsync(new HttpRequestException("ECB unavailable"));

            var repo = new Mock<IExchangeRateRepository>();
            var cache = new Mock<IRateCache>();

            var worker = CreateWorker(BuildScopedProvider(ecb.Object, repo.Object), cache.Object);

            await Assert.ThrowsAsync<HttpRequestException>(() => worker.RunAsync(CancellationToken.None));

            repo.Verify(r => r.UpsertAsync(It.IsAny<IEnumerable<ExchangeRate>>(), It.IsAny<CancellationToken>()), Times.Never);
            cache.Verify(c => c.SetRates(It.IsAny<IReadOnlyDictionary<string, decimal>>()), Times.Never);
        }
    }
}
