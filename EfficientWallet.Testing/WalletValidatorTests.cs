using EfficientWallet.Application.Wallets.Commands.AdjustWalletBalance;
using EfficientWallet.Domain.Enums;
using EfficientWallet.Infrastructure.Persistence.Util;
using ErrorOr;

namespace EfficientWallet.Testing
{
    /// <summary>
    /// Unit tests for the <see cref="WalletValidator.Validate"/> guard clauses applied to
    /// <see cref="AdjustWalletBalanceCommand"/>.
    /// </summary>
    public class WalletValidatorTests
    {
        private static AdjustWalletBalanceCommand ValidCommand() => new()
        {
            WalletId = 1,
            Amount = 10m,
            Currency = "EUR",
            Strategy = Strategy.AddFundsStrategy
        };

        [Fact]
        public void Validate_ValidCommand_ReturnsTrue()
        {
            var result = ValidCommand().Validate();

            Assert.False(result.IsError);
            Assert.True(result.Value);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void Validate_NonPositiveAmount_ReturnsInvalidAmount(int amount)
        {
            var command = ValidCommand();
            command.Amount = amount;

            var result = command.Validate();

            Assert.True(result.IsError);
            Assert.Equal("Wallet.InvalidAmount", result.FirstError.Code);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void Validate_MissingCurrency_ReturnsForbidden(string? currency)
        {
            var command = ValidCommand();
            command.Currency = currency!;

            var result = command.Validate();

            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Forbidden, result.FirstError.Type);
        }

        [Fact]
        public void Validate_InvalidStrategy_ReturnsForbidden()
        {
            var command = ValidCommand();
            command.Strategy = (Strategy)99;

            var result = command.Validate();

            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Forbidden, result.FirstError.Type);
        }
    }
}
