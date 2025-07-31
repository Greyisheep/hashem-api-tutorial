using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// Request model for direct bank payments through Squad.co
    /// </summary>
    public class DirectBankPaymentRequest
    {
        [JsonPropertyName("transaction_reference")]
        public string? TransactionReference { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("pass_charge")]
        public bool PassCharge { get; set; } = false;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "NGN";

        [JsonPropertyName("webhook_url")]
        public string? WebhookUrl { get; set; }

        [JsonPropertyName("bank")]
        public BankDetails Bank { get; set; } = new();

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = "bank";

        [JsonPropertyName("customer")]
        public CustomerDetails Customer { get; set; } = new();
    }

    /// <summary>
    /// Bank details for direct payment
    /// </summary>
    public class BankDetails
    {
        [JsonPropertyName("bank_code")]
        public string BankCode { get; set; } = string.Empty;

        [JsonPropertyName("account_or_phoneno")]
        public string AccountOrPhoneNo { get; set; } = string.Empty;
    }
} 