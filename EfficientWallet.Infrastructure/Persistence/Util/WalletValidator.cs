using EfficientWallet.Application.Common;
using EfficientWallet.Application.Wallets.Commands.AdjustWalletBalance;
using EfficientWallet.Domain.Enums;
using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Infrastructure.Persistence.Util
{
    public static class WalletValidator
    {
        public static ErrorOr<bool> Validate(this AdjustWalletBalanceCommand command)
        {
            if (command.Amount <= 0)
            {
                return CustomErrors.InvalidAmount;
            }
            if (string.IsNullOrWhiteSpace(command.Currency))
            {
                return Error.Forbidden(description: "Currency is required");
            }
            if (!Enum.IsDefined(typeof(Strategy), command.Strategy))
            {
                return Error.Forbidden(description: "Invalid strategy");
            }
            return true;
        }
    }
}
