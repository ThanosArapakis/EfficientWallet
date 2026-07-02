using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Services;
using Moq;

namespace EfficientWallet.Testing
{
    /// <summary>
    /// Unit tests for <see cref="BalanceConverter"/>. Rates are expressed the ECB way:
    /// 1 EUR = <c>rate</c> units of the foreign currency. The cache is mocked so each
    /// test declares exactly which rates are available.
    /// </summary>
    public class BalanceConverterTests
    {
        private static BalanceConverter CreateConverter(params (string currency, decimal rate)[] rates)
        {
            var cache = new Mock<IRateCache>();
            foreach (var (currency, rate) in rates)
            {
                var value = rate;
                cache.Setup(c => c.TryGetRate(currency, out value)).Returns(true);
            }
            return new BalanceConverter(cache.Object);
        }

        [Fact]
        public void Convert_SameCurrency_ReturnsAmountUnchanged()
        {
            var converter = CreateConverter();

            var result = converter.Convert("USD", "USD", 100m);

            Assert.Equal(100m, result);
        }

        [Fact]
        public void Convert_SameCurrency_IsCaseInsensitive()
        {
            var converter = CreateConverter();

            var result = converter.Convert("eur", "EUR", 50m);

            Assert.Equal(50m, result);
        }

        [Fact]
        public void Convert_FromEur_MultipliesByRate()
        {
            var converter = CreateConverter(("USD", 1.10m));

            var result = converter.Convert("EUR", "USD", 100m);

            Assert.Equal(110m, result);
        }

        [Fact]
        public void Convert_ToEur_DividesByRate()
        {
            var converter = CreateConverter(("USD", 1.25m));

            var result = converter.Convert("USD", "EUR", 125m);

            Assert.Equal(100m, result);
        }

        [Fact]
        public void Convert_BetweenTwoForeignCurrencies_RoutesThroughEur()
        {
            // 100 USD -> 80 EUR (÷1.25) -> 64 GBP (×0.80)
            var converter = CreateConverter(("USD", 1.25m), ("GBP", 0.80m));

            var result = converter.Convert("USD", "GBP", 100m);

            Assert.Equal(64m, result);
        }

        [Fact]
        public void Convert_WhenRateMissing_ThrowsInvalidOperationException()
        {
            var converter = CreateConverter(); // no rates registered

            Assert.Throws<InvalidOperationException>(() => converter.Convert("EUR", "XYZ", 100m));
        }
    }
}
