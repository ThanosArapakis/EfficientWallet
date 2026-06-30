using ErrorOr;
using OpenMediator;
using ReservationApp.core.api.Application.Common.Results;
using System.ComponentModel.DataAnnotations;

namespace EfficientWallet.Application.Wallets.Commands.CreateWallet
{
    public class CreateWalletCommand : ICommand<ErrorOr<PostResponse>>
    {
        [Required]
        public string Currency {  get; set; }

        [Required]
        public decimal Balance { get; set; } = 0;
    }
}
