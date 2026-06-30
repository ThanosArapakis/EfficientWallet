using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Wallets.Results;
using ErrorOr;
using OpenMediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance
{
    public class RetrieveWalletBalanceQueryHandler(IWalletRepository _repo) : ICommandHandler<RetrieveWalletBalanceQuery, ErrorOr<RetrieveWalletBalanceResponse>>
    {
        public async Task<ErrorOr<RetrieveWalletBalanceResponse>> HandleAsync(RetrieveWalletBalanceQuery command, CancellationToken cancellationToken = default)
        => await _repo.RetrieveWalletBalanceAsync(command, cancellationToken);
    }
}
