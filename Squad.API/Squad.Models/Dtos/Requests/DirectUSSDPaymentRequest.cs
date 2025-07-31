using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// Request model for direct USSD payments through Squad.co
    /// </summary>
    public class DirectUSSDPaymentRequest
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

        [JsonPropertyName("ussd")]
        public USSDBankDetails Ussd { get; set; } = new();

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = "ussd";

        [JsonPropertyName("customer")]
        public CustomerDetails Customer { get; set; } = new();
    }

    /// <summary>
    /// USSD bank details for direct payment
    /// </summary>
    public class USSDBankDetails
    {
        [JsonPropertyName("bank_code")]
        public string BankCode { get; set; } = string.Empty;
    }
} 