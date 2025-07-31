using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;
using Squad.Service.Interfaces;
using Squad.Service.Utilities;

namespace Squad.Service.Implementations
{
    public class AuthService : IAuthService
    {
        public BaseResponse<LogInResponse> LogIn(LogInRequest request)
        {
            try
            {
                if (request == null)
                {
                    return new BaseResponse<LogInResponse>
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Invalid username or password"
                    };
                }

                var userName = Environment.GetEnvironmentVariable("USERNAME");
                var password = Environment.GetEnvironmentVariable("PASSWORD");

                if (userName == request.Username && password == request.Password)
                {
                    return new BaseResponse<LogInResponse>
                    {
                        StatusCode = System.Net.HttpStatusCode.OK,
                        Message = "LogIn Successful",
                        Data = new LogInResponse
                        {
                            Token = Helper.GenerateJwtToken(request.Username, "User"),
                        }
                    };
                }

                return new BaseResponse<LogInResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    Message = "Invalid username or password"
                };

            }
            catch (Exception ex)
            {
                return new BaseResponse<LogInResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
