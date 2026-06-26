using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.ECB.Http.Settings
{
    internal class ECBClientSettings
    {
        internal const string Section = "HttpClients:ECB";

        [Required]
        public required string BaseAddress { get; set; }

        [Required]
        public required int TimeoutInSeconds { get; set; }
    }
}
