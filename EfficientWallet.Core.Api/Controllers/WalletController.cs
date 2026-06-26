using Microsoft.AspNetCore.Mvc;

namespace EfficientWallet.Core.Api.Controllers
{
    [ApiController]
    [Route("wallet")]
    public class WalletController : ControllerBase
    {

        private readonly ILogger<WalletController> _logger;

        public WalletController(ILogger<WalletController> logger)
        {
            _logger = logger;
        }
    }
}
