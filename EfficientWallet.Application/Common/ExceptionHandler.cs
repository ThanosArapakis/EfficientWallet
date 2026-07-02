using EfficientWallet.Application.Common.Contracts;
using ErrorOr;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Common;

public class ExceptionHandler<T>
{
    private readonly ILogger<T> _logger;

    private static readonly Action<ILogger, string, Exception?> _transactionFailed =
    LoggerMessage.Define<string>(
        LogLevel.Error,
        new EventId(0, "Request Failed"),
        "Errors:{Errors}"
    );

    private static readonly Action<ILogger, string, Exception?> _transactionCompleted =
    LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(1, "Request Completed"), "Request Completed Successfully:{Message}"
    );

    public ExceptionHandler(ILogger<T> logger)
    {
        _logger = logger;
    }

    public async Task<ResponseDto> HandleExceptionAsync<TResponse>(Func<Task<ErrorOr<TResponse>>> action)
    where TResponse : class
    {
        ErrorOr<TResponse> response = await action();

        //resolving the ErrorOr response of the services into the generic ResponseDto
        return response.Match(
            isSuccess =>
            {
                _transactionCompleted(_logger, JsonSerializer.Serialize(isSuccess.ToString()), null);
                return new ResponseDto(Result: isSuccess);
            },
            errors =>
            {
                _transactionFailed(_logger, JsonSerializer.Serialize(errors.Select(e => e.Description)), null);
                return new ResponseDto(
                        Result: null,
                        IsSuccess: false,
                        Message: string.Join("; ", errors.Select(e => e.Description))
                    );
            });
    }
}
