using EfficientWallet.Application.Common.Interfaces;
using ErrorOr;
using OpenMediator;
using ReservationApp.core.api.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Wallets.Commands.CreateWallet
{
    public class CreateWalletCommandHandler(IWalletRepository _repo) : ICommandHandler<CreateWalletCommand, ErrorOr<PostResponse>>
    {
        public async Task<ErrorOr<PostResponse>> HandleAsync(CreateWalletCommand command, CancellationToken cancellationToken = default)
        => await _repo.CreateWallet(command, cancellationToken);
    }
}
