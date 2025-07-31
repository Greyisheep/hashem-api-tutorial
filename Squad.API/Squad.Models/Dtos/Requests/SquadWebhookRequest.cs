using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// Webhook payload model from Squad.co payment notifications
    /// </summary>
    public class SquadWebhookRequest
    {
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public WebhookData Data { get; set; } = new();

        [JsonPropertyName("signature")]
        public string Signature { get; set; } = string.Empty;
    }

    public class WebhookData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("transaction_ref")]
        public string TransactionRef { get; set; } = string.Empty;

        [JsonPropertyName("transaction_amount")]
        public int TransactionAmount { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("transaction_status")]
        public string TransactionStatus { get; set; } = string.Empty;

        [JsonPropertyName("transaction_type")]
        public string TransactionType { get; set; } = string.Empty;

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("gateway_ref")]
        public string GatewayRef { get; set; } = string.Empty;

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }
} 