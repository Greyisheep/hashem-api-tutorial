using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;
using Squad.Service.Interfaces;

namespace Squad.API.Controllers
{
    /// <summary>
    /// Controller for Squad.co payment gateway operations
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SquadPaymentController : ControllerBase
    {
        private readonly ISquadPaymentService _squadPaymentService;
        private readonly ILogger<SquadPaymentController> _logger;

        public SquadPaymentController(ISquadPaymentService squadPaymentService, ILogger<SquadPaymentController> logger)
        {
            _squadPaymentService = squadPaymentService;
            _logger = logger;
        }

        /// <summary>
        /// Initiates a payment through Squad.co payment gateway
        /// </summary>
        /// <param name="request">Payment initiation request</param>
        /// <returns>Payment response with checkout URL</returns>
        [HttpPost("initiate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResponse<SquadPaymentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<SquadPaymentResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<SquadPaymentResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<SquadPaymentResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> InitiatePayment([FromBody] SquadPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Payment initiation requested for email: {Email}", request.Email);
                
                var response = await _squadPaymentService.InitiatePaymentAsync(request);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment initiation endpoint");
                return StatusCode(500, new BaseResponse<SquadPaymentResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Verifies a transaction status using Squad.co API
        /// </summary>
        /// <param name="transactionRef">Transaction reference to verify</param>
        /// <returns>Transaction verification response</returns>
        [HttpGet("verify/{transactionRef}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResponse<SquadVerifyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<SquadVerifyResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<SquadVerifyResponse>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<SquadVerifyResponse>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BaseResponse<SquadVerifyResponse>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> VerifyTransaction(string transactionRef)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(transactionRef))
                {
                    return BadRequest(new BaseResponse<SquadVerifyResponse>
                    {
                        StatusCode = System.Net.HttpStatusCode.BadRequest,
                        Message = "Transaction reference is required"
                    });
                }

                _logger.LogInformation("Transaction verification requested for: {TransactionRef}", transactionRef);
                
                var response = await _squadPaymentService.VerifyTransactionAsync(transactionRef);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in transaction verification endpoint");
                return StatusCode(500, new BaseResponse<SquadVerifyResponse>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Redirect endpoint for payment completion
        /// </summary>
        /// <param name="transactionRef">Transaction reference from Squad.co</param>
        /// <param name="reference">Alternative transaction reference parameter</param>
        /// <param name="status">Payment status</param>
        /// <returns>Browser redirect to frontend with payment result</returns>
        [HttpGet("redirect")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymentRedirect([FromQuery] string? transactionRef, [FromQuery] string? reference, [FromQuery] string? status)
        {
            try
            {
                // Squad.co sends 'reference' parameter, so we use that if transactionRef is null
                var actualTransactionRef = transactionRef ?? reference;
                
                _logger.LogInformation("Payment redirect received - TransactionRef: {TransactionRef}, Status: {Status}", actualTransactionRef, status);

                if (string.IsNullOrWhiteSpace(actualTransactionRef))
                {
                    // Redirect to frontend with error
                    var errorUrl = $"http://localhost:8081?error=missing_transaction_ref";
                    return Redirect(errorUrl);
                }

                // Process the redirect in the background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _squadPaymentService.ProcessRedirectAsync(actualTransactionRef, status);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing redirect in background");
                    }
                });

                // Redirect to frontend with payment result
                var frontendUrl = $"http://localhost:8081?transactionRef={actualTransactionRef}&status={status ?? "unknown"}";
                return Redirect(frontendUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in payment redirect endpoint");
                // Redirect to frontend with error
                var errorUrl = $"http://localhost:8081?error=redirect_error";
                return Redirect(errorUrl);
            }
        }

        /// <summary>
        /// Webhook endpoint for Squad.co payment notifications
        /// </summary>
        /// <param name="webhookRequest">Webhook payload from Squad.co</param>
        /// <returns>Webhook processing response</returns>
        [HttpPost("webhook")]
        [AllowAnonymous] // Webhooks don't require authentication
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProcessWebhook([FromBody] SquadWebhookRequest webhookRequest)
        {
            try
            {
                // Validate webhook signature for security
                var rawBody = await GetRawRequestBodyAsync();
                var signature = Request.Headers["x-squad-encrypted-body"].FirstOrDefault();

                if (!_squadPaymentService.ValidateWebhookSignature(rawBody, signature ?? string.Empty))
                {
                    _logger.LogWarning("Invalid webhook signature received");
                    return Unauthorized(new BaseResponse<object>
                    {
                        StatusCode = System.Net.HttpStatusCode.Unauthorized,
                        Message = "Invalid webhook signature"
                    });
                }

                _logger.LogInformation("Webhook received for transaction: {TransactionRef}", webhookRequest.Data.TransactionRef);
                
                var response = await _squadPaymentService.ProcessWebhookAsync(webhookRequest);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in webhook processing endpoint");
                return StatusCode(500, new BaseResponse<object>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Direct card payment endpoint
        /// </summary>
        /// <param name="request">Direct card payment request</param>
        /// <returns>Payment response</returns>
        [HttpPost("direct/card")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DirectCardPayment([FromBody] DirectCardPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Direct card payment requested for email: {Email}", request.Customer.Email);
                
                var response = await _squadPaymentService.ProcessDirectCardPaymentAsync(request);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct card payment endpoint");
                return StatusCode(500, new BaseResponse<object>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Direct bank payment endpoint
        /// </summary>
        /// <param name="request">Direct bank payment request</param>
        /// <returns>Payment response</returns>
        [HttpPost("direct/bank")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DirectBankPayment([FromBody] DirectBankPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Direct bank payment requested for email: {Email}", request.Customer.Email);
                
                var response = await _squadPaymentService.ProcessDirectBankPaymentAsync(request);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct bank payment endpoint");
                return StatusCode(500, new BaseResponse<object>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Direct USSD payment endpoint
        /// </summary>
        /// <param name="request">Direct USSD payment request</param>
        /// <returns>Payment response</returns>
        [HttpPost("direct/ussd")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DirectUSSDPayment([FromBody] DirectUSSDPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Direct USSD payment requested for email: {Email}", request.Customer.Email);
                
                var response = await _squadPaymentService.ProcessDirectUSSDPaymentAsync(request);
                return StatusCode((int)response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in direct USSD payment endpoint");
                return StatusCode(500, new BaseResponse<object>
                {
                    StatusCode = System.Net.HttpStatusCode.InternalServerError,
                    Message = "An unexpected error occurred"
                });
            }
        }

        /// <summary>
        /// Gets the raw request body for signature validation
        /// </summary>
        private async Task<string> GetRawRequestBodyAsync()
        {
            Request.Body.Position = 0;
            using var reader = new StreamReader(Request.Body);
            return await reader.ReadToEndAsync();
        }
    }
} 