using EfficientWallet.Application.Wallets.Results;
using ErrorOr;
using OpenMediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance
{
    public class RetrieveWalletBalanceQuery : ICommand<ErrorOr<RetrieveWalletBalanceResponse>>
    {
        public long WalletId { get; set; }

        public required string Currency { get; set; }
    }
}
