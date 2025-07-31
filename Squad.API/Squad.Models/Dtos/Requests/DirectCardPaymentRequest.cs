using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// Request model for direct card payments through Squad.co
    /// </summary>
    public class DirectCardPaymentRequest
    {
        [JsonPropertyName("transaction_reference")]
        public string? TransactionReference { get; set; }

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "NGN";

        [JsonPropertyName("pass_charge")]
        public bool PassCharge { get; set; } = false;

        [JsonPropertyName("webhook_url")]
        public string? WebhookUrl { get; set; }

        [JsonPropertyName("card")]
        public CardDetails Card { get; set; } = new();

        [JsonPropertyName("payment_method")]
        public string PaymentMethod { get; set; } = "card";

        [JsonPropertyName("customer")]
        public CustomerDetails Customer { get; set; } = new();

        [JsonPropertyName("redirect_url")]
        public string? RedirectUrl { get; set; }
    }

    /// <summary>
    /// Card details for direct payment
    /// </summary>
    public class CardDetails
    {
        [JsonPropertyName("number")]
        public string Number { get; set; } = string.Empty;

        [JsonPropertyName("cvv")]
        public string Cvv { get; set; } = string.Empty;

        [JsonPropertyName("expiry_month")]
        public string ExpiryMonth { get; set; } = string.Empty;

        [JsonPropertyName("expiry_year")]
        public string ExpiryYear { get; set; } = string.Empty;
    }

    /// <summary>
    /// Customer details for direct payment
    /// </summary>
    public class CustomerDetails
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }
} 