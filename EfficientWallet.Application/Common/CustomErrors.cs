using ErrorOr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Common
{
    //Extension class for ErrorOr to define custom errors for the application
    public class CustomErrors
    {
        public static Error InvalidAmount=> Error.Validation(
            code: "Wallet.InvalidAmount",
            description: $"The given amount is invalid. Please provide a value > 0");

        public static Error InsufficientAmount(string currency) => Error.Validation(
            code: "Wallet.InsufficientAmount",
            description: $"Insufficient amount in {currency} wallet");
    }
}
