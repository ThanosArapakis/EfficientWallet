using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Domain.Enums
{
    public enum Strategy
    {
        AddFundsStrategy = 0,
        SubtractFundsStrategy = 1,
        ForceSubtractFundsStrategy = 2
    }
}
