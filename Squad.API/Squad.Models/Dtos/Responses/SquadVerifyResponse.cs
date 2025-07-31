using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Responses
{
    /// <summary>
    /// Wrapper for Squad.co API response
    /// </summary>
    public class SquadApiResponse<T>
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    /// <summary>
    /// Response model for Squad.co transaction verification
    /// </summary>
    public class SquadVerifyResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("transaction_amount")]
        public int TransactionAmount { get; set; }

        [JsonPropertyName("transaction_ref")]
        public string TransactionRef { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("merchant_id")]
        public string MerchantId { get; set; } = string.Empty;

        [JsonPropertyName("merchant_amount")]
        public decimal MerchantAmount { get; set; }

        [JsonPropertyName("merchant_name")]
        public string MerchantName { get; set; } = string.Empty;

        [JsonPropertyName("merchant_business_name")]
        public string MerchantBusinessName { get; set; } = string.Empty;

        [JsonPropertyName("merchant_email")]
        public string MerchantEmail { get; set; } = string.Empty;

        [JsonPropertyName("customer_email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [JsonPropertyName("customer_name")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("meta_data")]
        public string MetaData { get; set; } = string.Empty;

        [JsonPropertyName("transaction_status")]
        public string TransactionStatus { get; set; } = string.Empty;

        [JsonPropertyName("transaction_charges")]
        public decimal TransactionCharges { get; set; }

        [JsonPropertyName("transaction_currency_id")]
        public string TransactionCurrencyId { get; set; } = string.Empty;

        [JsonPropertyName("transaction_gateway_id")]
        public string TransactionGatewayId { get; set; } = string.Empty;

        [JsonPropertyName("transaction_type")]
        public string TransactionType { get; set; } = string.Empty;

        [JsonPropertyName("flat_charge")]
        public decimal FlatCharge { get; set; }

        [JsonPropertyName("is_suspicious")]
        public bool IsSuspicious { get; set; }

        [JsonPropertyName("is_refund")]
        public bool IsRefund { get; set; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
    }
} 