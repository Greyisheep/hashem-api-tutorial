using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;
using Squad.Service.Interfaces;

namespace Squad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Log_In")]
        [ProducesResponseType(typeof(BaseResponse<LogInResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<LogInResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<LogInResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LogIn([FromBody] LogInRequest request)
        {
            var response = _authService.LogIn(request);
            return StatusCode((int)response.StatusCode, response);
        }
    }
}
