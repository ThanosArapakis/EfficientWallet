using EfficientWallet.Application.Common.Contracts;
using EfficientWallet.Application.Wallets.Commands.AdjustWalletBalance;
using EfficientWallet.Application.Wallets.Commands.CreateWallet;
using EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance;
using EfficientWallet.Application.Wallets.Results;
using EfficientWallet.Domain;
using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Common.Interfaces
{
    public interface IWalletRepository : IRepository<Wallet>
    {
        public Task<ErrorOr<RetrieveWalletBalanceResponse>> RetrieveWalletBalanceAsync(RetrieveWalletBalanceQuery query, CancellationToken cancellationToken = default);
        public Task<ErrorOr<PostResponse>> CreateWallet(CreateWalletCommand command, CancellationToken cancellationToken = default);
        public Task<ErrorOr<PostResponse>> AdjustWalletBalance(AdjustWalletBalanceCommand command, CancellationToken cancellationToken = default);
    }
}
