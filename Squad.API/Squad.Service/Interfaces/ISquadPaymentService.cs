using Squad.Models.Dtos.Requests;
using Squad.Models.Dtos.Responses;

namespace Squad.Service.Interfaces
{
    /// <summary>
    /// Interface for Squad.co payment gateway service
    /// </summary>
    public interface ISquadPaymentService
    {
        /// <summary>
        /// Initiates a payment through Squad.co payment gateway
        /// </summary>
        /// <param name="request">Payment initiation request</param>
        /// <returns>Payment response with checkout URL</returns>
        Task<BaseResponse<SquadPaymentResponse>> InitiatePaymentAsync(SquadPaymentRequest request);

        /// <summary>
        /// Verifies a transaction status using Squad.co API
        /// </summary>
        /// <param name="transactionRef">Transaction reference to verify</param>
        /// <returns>Transaction verification response</returns>
        Task<BaseResponse<SquadVerifyResponse>> VerifyTransactionAsync(string transactionRef);

        /// <summary>
        /// Processes webhook notifications from Squad.co
        /// </summary>
        /// <param name="webhookRequest">Webhook payload from Squad.co</param>
        /// <returns>Webhook processing response</returns>
        Task<BaseResponse<object>> ProcessWebhookAsync(SquadWebhookRequest webhookRequest);

        /// <summary>
        /// Validates webhook signature for security
        /// </summary>
        /// <param name="payload">Raw webhook payload</param>
        /// <param name="signature">Signature from webhook header</param>
        /// <returns>True if signature is valid</returns>
        bool ValidateWebhookSignature(string payload, string signature);

        /// <summary>
        /// Processes payment redirect from Squad.co
        /// </summary>
        /// <param name="transactionRef">Transaction reference</param>
        /// <param name="status">Payment status</param>
        /// <returns>Redirect processing response</returns>
        Task<BaseResponse<object>> ProcessRedirectAsync(string transactionRef, string? status);

        /// <summary>
        /// Processes direct card payment
        /// </summary>
        /// <param name="request">Direct card payment request</param>
        /// <returns>Payment response</returns>
        Task<BaseResponse<object>> ProcessDirectCardPaymentAsync(DirectCardPaymentRequest request);

        /// <summary>
        /// Processes direct bank payment
        /// </summary>
        /// <param name="request">Direct bank payment request</param>
        /// <returns>Payment response</returns>
        Task<BaseResponse<object>> ProcessDirectBankPaymentAsync(DirectBankPaymentRequest request);

        /// <summary>
        /// Processes direct USSD payment
        /// </summary>
        /// <param name="request">Direct USSD payment request</param>
        /// <returns>Payment response</returns>
        Task<BaseResponse<object>> ProcessDirectUSSDPaymentAsync(DirectUSSDPaymentRequest request);
    }
} 