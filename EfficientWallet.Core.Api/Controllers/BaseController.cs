using EfficientWallet.Application.Common;
using EfficientWallet.Application.Common.Contracts;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using OpenMediator.Buses;

namespace EfficientWallet.Core.Api.Controllers
{
    public class BaseController<T> : ControllerBase
    {
        private IMediatorBus? _mediator;

        protected IMediatorBus Mediator => _mediator ??= HttpContext.RequestServices.GetService<IMediatorBus>() ??
                                                         throw new InvalidOperationException("Mediator not registered");

        private protected readonly ExceptionHandler<T> _exceptionHandler;

        protected BaseController(ILogger<T> logger)
        {
            _exceptionHandler = new ExceptionHandler<T>(logger);
        }

        private protected async Task<ResponseDto> HandleExceptionAsync<TResponse>(Func<Task<ErrorOr<TResponse>>> action)
        where TResponse : class
        {
            return await _exceptionHandler.HandleExceptionAsync(action).ConfigureAwait(false);
        }
    }
}
