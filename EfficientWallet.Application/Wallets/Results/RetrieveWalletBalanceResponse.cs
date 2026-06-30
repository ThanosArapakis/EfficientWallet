using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Wallets.Results
{
    public record RetrieveWalletBalanceResponse
    (
        decimal Balance
    );
}
