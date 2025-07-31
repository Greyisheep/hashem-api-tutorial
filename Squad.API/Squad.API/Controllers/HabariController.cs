using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;
using Squad.Service.Interfaces;

namespace Squad.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HabariController : ControllerBase
    {
        private readonly IHabariService _services;

        public HabariController(IHabariService services)
        {
            _services = services;
        }

        [HttpPost("LookUp")]
        [ProducesResponseType(typeof(BaseResponse<LookUpResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<LookUpResponse>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(BaseResponse<LookUpResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LookUp([FromBody] EncryptedPayload encryptedPayload)
        {
            var response = await _services.LookUp(encryptedPayload);
            return StatusCode((int)response.StatusCode, response);
        }

        [HttpPost("Transfer")]
        [ProducesResponseType(typeof(BaseResponse<TransferResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<TransferResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<TransferResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<TransferResponse>), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(BaseResponse<TransferResponse>), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(BaseResponse<TransferResponse>), StatusCodes.Status424FailedDependency)]
        public async Task<IActionResult> Transfer([FromBody] EncryptedPayload encryptedPayload)
        {
            var response = await _services.Transfer(encryptedPayload);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("ReQuery")]
        [ProducesResponseType(typeof(BaseResponse<RequeryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<RequeryResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<RequeryResponse>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ReQuery([FromBody] EncryptedPayload encryptedPayload)
        {
            var response = await _services.ReQuery(encryptedPayload);
            return StatusCode((int)response.StatusCode, response);
        }
        [HttpPost("Get_All_Transfers")]
        [ProducesResponseType(typeof(BaseResponse<GetAllTransfersResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<GetAllTransfersResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<GetAllTransfersResponse>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllTransfers()
        {
            var response = await _services.GetAllTransfers();
            return StatusCode((int)response.StatusCode, response);
        }

    }
}
