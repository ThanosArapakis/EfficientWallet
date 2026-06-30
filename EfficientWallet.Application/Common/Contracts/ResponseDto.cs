using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EfficientWallet.Application.Common.Contracts
{
    public record ResponseDto(object? Result, bool IsSuccess = true, string? Message = null);

}
