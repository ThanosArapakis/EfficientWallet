using EfficientWallet.Application.Wallets.Commands.CreateWallet;
using EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance;
using EfficientWallet.Application.Wallets.Results;
using EfficientWallet.Domain;
using ErrorOr;
using ReservationApp.core.api.Application.Common.Results;
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
        //public Task<Wallet> GetWalletAsync(Expression<Func<Wallet, bool>> predicate, CancellationToken cancellationToken = default);
        public Task<ErrorOr<RetrieveWalletBalanceResponse>> RetrieveWalletBalanceAsync(RetrieveWalletBalanceQuery query, CancellationToken cancellationToken = default);
        public Task<ErrorOr<PostResponse>> CreateWallet(CreateWalletCommand command, CancellationToken cancellationToken = default);
    }
}
