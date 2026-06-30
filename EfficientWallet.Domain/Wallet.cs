using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Domain
{
    public class Wallet
    {
        public long Id { get; set; }
        public decimal Balance { get; set; } = 0;
        public string Currency {  get; set; }

    }
}
