using EfficientWallet.Application.Common.Interfaces;
using ErrorOr;
using OpenMediator;
using ReservationApp.core.api.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Wallets.Commands.AdjustWalletBalance
{
    public class AdjustWalletBalanceCommandHandler(IWalletRepository _repo) : ICommandHandler<AdjustWalletBalanceCommand, ErrorOr<PostResponse>>
    {
        public async Task<ErrorOr<PostResponse>> HandleAsync(AdjustWalletBalanceCommand command, CancellationToken cancellationToken = default)
        => await _repo.AdjustWalletBalance(command, cancellationToken);
    }
}
