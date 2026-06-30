using EfficientWallet.Application.Common.Contracts;
using EfficientWallet.Application.Common.Interfaces;
using EfficientWallet.Application.Wallets.Commands.CreateWallet;
using EfficientWallet.Application.Wallets.Queries.RetrieveWalletBalance;
using EfficientWallet.Application.Wallets.Results;
using EfficientWallet.Domain;
using EfficientWallet.ECB.Service;
using EfficientWallet.Infrastructure.Persistence.Util;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using OpenMediator.Buses;
using ReservationApp.core.api.Application.Common.Results;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EfficientWallet.Core.Api.Controllers
{
    [ApiController]
    [Route("/api/wallets")]
    public class WalletController(IMediatorBus _mediator, ILogger<WalletController> logger) : BaseController<WalletController>(logger)
    {
        [HttpGet("retrieveWalletBalance/{walletId}")]
        public async Task<ResponseDto> RetrieveWalletBalance([FromQuery] string currency, long walletId)
        {
            var query = new RetrieveWalletBalanceQuery { WalletId = walletId, Currency = currency };
            return await HandleExceptionAsync(async () => await _mediator.SendAsync<RetrieveWalletBalanceQuery, ErrorOr<RetrieveWalletBalanceResponse>>(query));
        }

        [HttpPost]
        public async Task<ResponseDto> CreateWallet([FromBody] CreateWalletCommand command) 
            => await HandleExceptionAsync(async () => await _mediator.SendAsync<CreateWalletCommand, ErrorOr<PostResponse>>(command));
    }
}
