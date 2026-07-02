using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Wallets.Commands.AdjustWalletBalance;
using EfficientWallet.Application.Wallets.Commands.CreateWallet;
using EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance;
using EfficientWallet.Domain;
using EfficientWallet.Domain.Enums;
using EfficientWallet.Infrastructure.Persistence;
using EfficientWallet.Infrastructure.Persistence.Repository;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EfficientWallet.Testing
{
    /// <summary>
    /// Behavioural tests for <see cref="WalletRepository"/>. These run against the EF Core
    /// in-memory provider (a real <see cref="AppDbContext"/> over an isolated store) so the
    /// repository's data-access plumbing is exercised end to end, while the currency
    /// conversion is mocked so each test controls the exchange arithmetic explicitly.
    /// </summary>
    public class WalletTests
    {
        /// <summary>Creates a fresh context backed by a uniquely-named in-memory store.</summary>
        private static AppDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        /// <summary>A converter that returns the amount unchanged, regardless of currency.</summary>
        private static IBalanceConverter IdentityConverter()
        {
            var converter = new Mock<IBalanceConverter>();
            converter
                .Setup(c => c.Convert(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .Returns((string _, string _, decimal amount) => amount);
            return converter.Object;
        }

        private static WalletRepository CreateRepository(AppDbContext context, IBalanceConverter converter)
            => new WalletRepository(context, Mock.Of<IRateCache>(), converter);

        private static async Task<Wallet> SeedWalletAsync(AppDbContext context, decimal balance, string currency)
        {
            var wallet = new Wallet { Balance = balance, Currency = currency };
            context.Wallets.Add(wallet);
            await context.SaveChangesAsync();
            return wallet;
        }

        [Fact]
        public async Task CreateWallet_PersistsWallet_AndReturnsGeneratedId()
        {
            using var context = CreateContext();
            var repository = CreateRepository(context, IdentityConverter());
            var command = new CreateWalletCommand { Currency = "EUR", Balance = 150m };

            var result = await repository.CreateWallet(command);

            Assert.False(result.IsError);
            Assert.True(result.Value.WalletId > 0);

            var stored = await context.Wallets.FindAsync(result.Value.WalletId);
            Assert.NotNull(stored);
            Assert.Equal(150m, stored!.Balance);
            Assert.Equal("EUR", stored.Currency);
        }

        [Fact]
        public async Task RetrieveWalletBalance_ReturnsBalanceConvertedToRequestedCurrency()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 100m, currency: "EUR");

            var converter = new Mock<IBalanceConverter>();
            converter.Setup(c => c.Convert("EUR", "USD", 100m)).Returns(110m);
            var repository = CreateRepository(context, converter.Object);

            var result = await repository.RetrieveWalletBalanceAsync(
                new RetrieveWalletBalanceQuery { WalletId = wallet.Id, Currency = "USD" });

            Assert.False(result.IsError);
            Assert.Equal(110m, result.Value.Balance);
        }

        [Fact]
        public async Task RetrieveWalletBalance_WhenWalletMissing_ReturnsNotFound()
        {
            using var context = CreateContext();
            var repository = CreateRepository(context, IdentityConverter());

            var result = await repository.RetrieveWalletBalanceAsync(
                new RetrieveWalletBalanceQuery { WalletId = 999, Currency = "USD" });

            Assert.True(result.IsError);
            Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        }

        [Fact]
        public async Task AdjustWalletBalance_AddFunds_IncreasesBalance()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 100m, currency: "EUR");
            var repository = CreateRepository(context, IdentityConverter());

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = wallet.Id,
                Amount = 50m,
                Currency = "EUR",
                Strategy = Strategy.AddFundsStrategy
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.False(result.IsError);
            var updated = await context.Wallets.FindAsync(wallet.Id);
            Assert.Equal(150m, updated!.Balance);
        }

        [Fact]
        public async Task AdjustWalletBalance_AddFunds_ConvertsFromCommandCurrencyIntoWalletCurrency()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 100m, currency: "EUR");

            // 50 USD is worth 45 EUR; conversion is command currency -> wallet currency.
            var converter = new Mock<IBalanceConverter>();
            converter.Setup(c => c.Convert("USD", "EUR", 50m)).Returns(45m);
            var repository = CreateRepository(context, converter.Object);

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = wallet.Id,
                Amount = 50m,
                Currency = "USD",
                Strategy = Strategy.AddFundsStrategy
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.False(result.IsError);
            var updated = await context.Wallets.FindAsync(wallet.Id);
            Assert.Equal(145m, updated!.Balance);
            converter.Verify(c => c.Convert("USD", "EUR", 50m), Times.Once);
        }

        [Fact]
        public async Task AdjustWalletBalance_SubtractFunds_WhenSufficient_DecreasesBalance()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 100m, currency: "EUR");
            var repository = CreateRepository(context, IdentityConverter());

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = wallet.Id,
                Amount = 40m,
                Currency = "EUR",
                Strategy = Strategy.SubtractFundsStrategy
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.False(result.IsError);
            var updated = await context.Wallets.FindAsync(wallet.Id);
            Assert.Equal(60m, updated!.Balance);
        }

        [Fact]
        public async Task AdjustWalletBalance_SubtractFunds_WhenInsufficient_ReturnsErrorAndLeavesBalanceUnchanged()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 30m, currency: "EUR");
            var repository = CreateRepository(context, IdentityConverter());

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = wallet.Id,
                Amount = 50m,
                Currency = "EUR",
                Strategy = Strategy.SubtractFundsStrategy
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Validation, result.FirstError.Type);
            Assert.Equal("Wallet.InsufficientAmount", result.FirstError.Code);

            var updated = await context.Wallets.FindAsync(wallet.Id);
            Assert.Equal(30m, updated!.Balance);
        }

        [Fact]
        public async Task AdjustWalletBalance_ForceSubtract_AllowsNegativeBalance()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 30m, currency: "EUR");
            var repository = CreateRepository(context, IdentityConverter());

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = wallet.Id,
                Amount = 50m,
                Currency = "EUR",
                Strategy = Strategy.ForceSubtractFundsStrategy
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.False(result.IsError);
            var updated = await context.Wallets.FindAsync(wallet.Id);
            Assert.Equal(-20m, updated!.Balance);
        }

        [Fact]
        public async Task AdjustWalletBalance_WhenWalletMissing_ReturnsNotFound()
        {
            using var context = CreateContext();
            var repository = CreateRepository(context, IdentityConverter());

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = 999,
                Amount = 10m,
                Currency = "EUR",
                Strategy = Strategy.AddFundsStrategy
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.True(result.IsError);
            Assert.Equal(ErrorType.NotFound, result.FirstError.Type);
        }

        [Fact]
        public async Task AdjustWalletBalance_WithInvalidAmount_ReturnsValidationErrorWithoutTouchingWallet()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 100m, currency: "EUR");
            var repository = CreateRepository(context, IdentityConverter());

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = wallet.Id,
                Amount = 0m,
                Currency = "EUR",
                Strategy = Strategy.AddFundsStrategy
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.True(result.IsError);
            Assert.Equal("Wallet.InvalidAmount", result.FirstError.Code);

            var updated = await context.Wallets.FindAsync(wallet.Id);
            Assert.Equal(100m, updated!.Balance);
        }

        [Fact]
        public async Task AdjustWalletBalance_WithInvalidStrategy_ReturnsForbidden()
        {
            using var context = CreateContext();
            var wallet = await SeedWalletAsync(context, balance: 100m, currency: "EUR");
            var repository = CreateRepository(context, IdentityConverter());

            var command = new AdjustWalletBalanceCommand
            {
                WalletId = wallet.Id,
                Amount = 10m,
                Currency = "EUR",
                Strategy = (Strategy)99
            };

            var result = await repository.AdjustWalletBalance(command);

            Assert.True(result.IsError);
            Assert.Equal(ErrorType.Forbidden, result.FirstError.Type);
        }
    }
}
