using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// Request model for initiating payments through Squad.co payment gateway
    /// </summary>
    public class SquadPaymentRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "NGN";

        [JsonPropertyName("customer_name")]
        public string? CustomerName { get; set; }

        [JsonPropertyName("initiate_type")]
        public string InitiateType { get; set; } = "inline";

        [JsonPropertyName("transaction_ref")]
        public string? TransactionRef { get; set; }

        [JsonPropertyName("callback_url")]
        public string? CallbackUrl { get; set; }

        [JsonPropertyName("payment_channels")]
        public string[]? PaymentChannels { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }

        [JsonPropertyName("pass_charge")]
        public bool PassCharge { get; set; } = false;

        [JsonPropertyName("is_recurring")]
        public bool IsRecurring { get; set; } = false;
    }
} 