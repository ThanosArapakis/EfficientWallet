using EfficientWallet.Application.Common;
using EfficientWallet.Domain.Enums;
using ErrorOr;
using OpenMediator;
using ReservationApp.core.api.Application.Common.Results;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Wallets.Commands.AdjustWalletBalance
{
    public class AdjustWalletBalanceCommand : ICommand<ErrorOr<PostResponse>>
    {
        [Required]
        public long WalletId { get; set; }
        [Required]       
        public decimal Amount { get; set; }
        public required string Currency {  get; set; }
        [Required]
        public Strategy Strategy { get; set; }
    }
}
