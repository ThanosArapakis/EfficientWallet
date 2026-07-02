using EfficientWallet.Application.Common.Contracts;
using ErrorOr;
using OpenMediator;
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
