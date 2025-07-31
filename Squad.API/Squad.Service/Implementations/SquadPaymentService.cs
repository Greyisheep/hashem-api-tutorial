using Microsoft.Extensions.Logging;
using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;
using Squad.Service.Interfaces;
using Squad.Service.Utilities;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Squad.Service.Implementations
{
    /// <summary>
    /// Implementation of Squad.co payment gateway service
    /// </summary>
    public class SquadPaymentService : ISquadPaymentService
    {
        private readonly ILogger<SquadPaymentService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _squadApiKey;
        private readonly string _squadBaseUrl;
        private readonly string _webhookSecret;
        private readonly string _notificationEmail;

        public SquadPaymentService(ILogger<SquadPaymentService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _squadApiKey = Environment.GetEnvironmentVariable("SQUAD_API_KEY") ?? throw new InvalidOperationException("SQUAD_API_KEY not configured");
            _squadBaseUrl = Environment.GetEnvironmentVariable("SQUAD_BASE_URL") ?? "https://sandbox-api-d.squadco.com";
            _webhookSecret = Environment.GetEnvironmentVariable("SQUAD_WEBHOOK_SECRET") ?? throw new InvalidOperationException("SQUAD_WEBHOOK_SECRET not configured");
            _notificationEmail = Environment.GetEnvironmentVariable("NOTIFICATION_EMAIL") ?? "greyisheep@gmail.com";
            
            // Configure HTTP client for Squad.co API
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_squadApiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Initiates a payment through Squad.co payment gateway
        /// </summary>
        public async Task<BaseResponse<SquadPaymentResponse>> InitiatePaymentAsync(SquadPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Initiating payment for email: {Email}, amount: {Amount}", request.Email, request.Amount);

                // Generate transaction reference if not provided
                if (string.IsNullOrEmpty(request.TransactionRef))
                {
                    request.TransactionRef = GenerateTransactionReference();
                }

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_squadBaseUrl}/transaction/initiate", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Squad.co API response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var squadResponse = JsonSerializer.Deserialize<SquadPaymentResponse>(responseContent);
                    return new BaseResponse<SquadPaymentResponse>
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "Payment initiated successfully",
                        Data = squadResponse
                    };
                }
                else
                {
                    _logger.LogError("Squad.co API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new BaseResponse<SquadPaymentResponse>
                    {
                        StatusCode = response.StatusCode,
                        Message = $"Payment initiation failed: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment");
                return new BaseResponse<SquadPaymentResponse>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while initiating payment"
                };
            }
        }

        /// <summary>
        /// Verifies a transaction status using Squad.co API
        /// </summary>
        public async Task<BaseResponse<SquadVerifyResponse>> VerifyTransactionAsync(string transactionRef)
        {
            try
            {
                _logger.LogInformation("Verifying transaction: {TransactionRef}", transactionRef);

                var response = await _httpClient.GetAsync($"{_squadBaseUrl}/transaction/verify/{transactionRef}");
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Squad.co verification response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var squadResponse = JsonSerializer.Deserialize<SquadApiResponse<SquadVerifyResponse>>(responseContent);
                    return new BaseResponse<SquadVerifyResponse>
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "Transaction verified successfully",
                        Data = squadResponse?.Data
                    };
                }
                else
                {
                    _logger.LogError("Squad.co verification error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new BaseResponse<SquadVerifyResponse>
                    {
                        StatusCode = response.StatusCode,
                        Message = $"Transaction verification failed: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying transaction");
                return new BaseResponse<SquadVerifyResponse>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while verifying transaction"
                };
            }
        }

        /// <summary>
        /// Processes webhook notifications from Squad.co
        /// </summary>
        public async Task<BaseResponse<object>> ProcessWebhookAsync(SquadWebhookRequest webhookRequest)
        {
            try
            {
                _logger.LogInformation("Processing webhook for transaction: {TransactionRef}", webhookRequest.Data.TransactionRef);

                // Log webhook event details
                _logger.LogInformation("Webhook event: {Event}, Status: {Status}, Amount: {Amount}", 
                    webhookRequest.Event, webhookRequest.Data.TransactionStatus, webhookRequest.Data.TransactionAmount);

                // Process based on event type
                switch (webhookRequest.Event.ToLower())
                {
                    case "charge_successful":
                        await ProcessSuccessfulPayment(webhookRequest.Data);
                        break;
                    case "charge.failed":
                        await ProcessFailedPayment(webhookRequest.Data);
                        break;
                    case "transfer.success":
                        await ProcessSuccessfulTransfer(webhookRequest.Data);
                        break;
                    default:
                        _logger.LogWarning("Unknown webhook event: {Event}", webhookRequest.Event);
                        break;
                }

                return new BaseResponse<object>
                {
                    StatusCode = HttpStatusCode.OK,
                    Message = "Webhook processed successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return new BaseResponse<object>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while processing webhook"
                };
            }
        }

        /// <summary>
        /// Validates webhook signature for security using HMAC-SHA512
        /// </summary>
        public bool ValidateWebhookSignature(string payload, string signature)
        {
            try
            {
                using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_webhookSecret));
                var computedSignature = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLower();
                
                return computedSignature.Equals(signature.ToLower(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }

        /// <summary>
        /// Processes payment redirect from Squad.co
        /// </summary>
        public async Task<BaseResponse<object>> ProcessRedirectAsync(string transactionRef, string? status)
        {
            try
            {
                _logger.LogInformation("Processing redirect for transaction: {TransactionRef}, Status: {Status}", transactionRef, status);

                // Verify the transaction
                var verificationResponse = await VerifyTransactionAsync(transactionRef);
                
                if (verificationResponse.StatusCode == HttpStatusCode.OK && verificationResponse.Data != null)
                {
                    // Send email notification for successful payments
                    if (verificationResponse.Data.TransactionStatus?.ToLower() == "success")
                    {
                        // Create WebhookData from verification response for email notification
                        var webhookData = new WebhookData
                        {
                            TransactionRef = verificationResponse.Data.TransactionRef,
                            TransactionStatus = verificationResponse.Data.TransactionStatus,
                            TransactionAmount = verificationResponse.Data.TransactionAmount,
                            Email = verificationResponse.Data.Email
                        };
                        await SendPaymentSuccessEmail(webhookData);
                    }

                    return new BaseResponse<object>
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = $"Payment redirect processed. Status: {verificationResponse.Data?.TransactionStatus}"
                    };
                }

                return new BaseResponse<object>
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Message = "Invalid transaction reference"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing redirect");
                return new BaseResponse<object>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while processing redirect"
                };
            }
        }

        /// <summary>
        /// Processes direct card payment
        /// </summary>
        public async Task<BaseResponse<object>> ProcessDirectCardPaymentAsync(DirectCardPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing direct card payment for email: {Email}", request.Customer.Email);

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_squadBaseUrl}/transaction/initiate/process-payment", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Direct card payment response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var paymentResponse = JsonSerializer.Deserialize<object>(responseContent);
                    return new BaseResponse<object>
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "Direct card payment processed successfully",
                        Data = paymentResponse
                    };
                }
                else
                {
                    _logger.LogError("Direct card payment error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new BaseResponse<object>
                    {
                        StatusCode = response.StatusCode,
                        Message = $"Direct card payment failed: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing direct card payment");
                return new BaseResponse<object>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while processing direct card payment"
                };
            }
        }

        /// <summary>
        /// Processes direct bank payment
        /// </summary>
        public async Task<BaseResponse<object>> ProcessDirectBankPaymentAsync(DirectBankPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing direct bank payment for email: {Email}", request.Customer.Email);

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_squadBaseUrl}/transaction/initiate/process-payment", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Direct bank payment response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var paymentResponse = JsonSerializer.Deserialize<object>(responseContent);
                    return new BaseResponse<object>
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "Direct bank payment processed successfully",
                        Data = paymentResponse
                    };
                }
                else
                {
                    _logger.LogError("Direct bank payment error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new BaseResponse<object>
                    {
                        StatusCode = response.StatusCode,
                        Message = $"Direct bank payment failed: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing direct bank payment");
                return new BaseResponse<object>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while processing direct bank payment"
                };
            }
        }

        /// <summary>
        /// Processes direct USSD payment
        /// </summary>
        public async Task<BaseResponse<object>> ProcessDirectUSSDPaymentAsync(DirectUSSDPaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Processing direct USSD payment for email: {Email}", request.Customer.Email);

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_squadBaseUrl}/transaction/initiate/process-payment", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Direct USSD payment response: {Response}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    var paymentResponse = JsonSerializer.Deserialize<object>(responseContent);
                    return new BaseResponse<object>
                    {
                        StatusCode = HttpStatusCode.OK,
                        Message = "Direct USSD payment processed successfully",
                        Data = paymentResponse
                    };
                }
                else
                {
                    _logger.LogError("Direct USSD payment error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new BaseResponse<object>
                    {
                        StatusCode = response.StatusCode,
                        Message = $"Direct USSD payment failed: {responseContent}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing direct USSD payment");
                return new BaseResponse<object>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = "An error occurred while processing direct USSD payment"
                };
            }
        }

        /// <summary>
        /// Generates a unique transaction reference
        /// </summary>
        private string GenerateTransactionReference()
        {
            return $"SQ_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
        }

        /// <summary>
        /// Processes successful payment webhook
        /// </summary>
        private async Task ProcessSuccessfulPayment(WebhookData data)
        {
            _logger.LogInformation("Processing successful payment for transaction: {TransactionRef}", data.TransactionRef);
            
            // Send email notification
            await SendPaymentSuccessEmail(data);
            
            // TODO: Implement your business logic here
            // - Update order status
            // - Update inventory
            // - etc.
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Processes failed payment webhook
        /// </summary>
        private async Task ProcessFailedPayment(WebhookData data)
        {
            _logger.LogWarning("Processing failed payment for transaction: {TransactionRef}", data.TransactionRef);
            
            // TODO: Implement your business logic here
            // - Update order status
            // - Send failure notification
            // - Restore inventory
            // - etc.
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Processes successful transfer webhook
        /// </summary>
        private async Task ProcessSuccessfulTransfer(WebhookData data)
        {
            _logger.LogInformation("Processing successful transfer for transaction: {TransactionRef}", data.TransactionRef);
            
            // TODO: Implement your business logic here
            // - Update transfer status
            // - Send confirmation
            // - etc.
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sends payment success email notification
        /// </summary>
        private async Task SendPaymentSuccessEmail(WebhookData data)
        {
            try
            {
                var emailSubject = $"Payment Success - Transaction {data.TransactionRef}";
                var emailBody = $@"
                    <h2>Payment Successful!</h2>
                    <p><strong>Transaction Reference:</strong> {data.TransactionRef}</p>
                    <p><strong>Amount:</strong> {data.TransactionAmount:N2} {data.Currency}</p>
                    <p><strong>Customer Email:</strong> {data.Email}</p>
                    <p><strong>Payment Method:</strong> {data.TransactionType}</p>
                    <p><strong>Date:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                    <br>
                    <p>This is an automated notification from your Squad.co payment integration.</p>";

                // TODO: Implement actual email sending logic
                // For now, we'll just log the email details
                _logger.LogInformation("Payment success email would be sent to {Email} for transaction {TransactionRef}", 
                    _notificationEmail, data.TransactionRef);
                
                // In a real implementation, you would use a service like SendGrid, MailKit, etc.
                // await _emailService.SendEmailAsync(_notificationEmail, emailSubject, emailBody);
                
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending payment success email");
            }
        }
    }
} 