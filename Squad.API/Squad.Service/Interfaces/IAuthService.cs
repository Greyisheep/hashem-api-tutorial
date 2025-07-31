using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;

namespace Squad.Service.Interfaces
{
    public interface IAuthService
    {
        BaseResponse<LogInResponse> LogIn(LogInRequest request);
    }
}
