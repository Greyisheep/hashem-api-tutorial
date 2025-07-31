using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Responses
{
    /// <summary>
    /// Response model for Squad.co payment initiation
    /// </summary>
    public class SquadPaymentResponse
    {
        [JsonPropertyName("auth_url")]
        public string? AuthUrl { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("merchant_info")]
        public MerchantInfo? MerchantInfo { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = string.Empty;

        [JsonPropertyName("recurring")]
        public RecurringInfo? Recurring { get; set; }

        [JsonPropertyName("is_recurring")]
        public bool IsRecurring { get; set; }

        [JsonPropertyName("plan_code")]
        public string? PlanCode { get; set; }

        [JsonPropertyName("callback_url")]
        public string? CallbackUrl { get; set; }

        [JsonPropertyName("transaction_ref")]
        public string TransactionRef { get; set; } = string.Empty;

        [JsonPropertyName("transaction_memo")]
        public string? TransactionMemo { get; set; }

        [JsonPropertyName("transaction_amount")]
        public int TransactionAmount { get; set; }

        [JsonPropertyName("authorized_channels")]
        public string[]? AuthorizedChannels { get; set; }

        [JsonPropertyName("checkout_url")]
        public string CheckoutUrl { get; set; } = string.Empty;
    }

    public class MerchantInfo
    {
        [JsonPropertyName("merchant_response")]
        public string? MerchantResponse { get; set; }

        [JsonPropertyName("merchant_name")]
        public string? MerchantName { get; set; }

        [JsonPropertyName("merchant_logo")]
        public string? MerchantLogo { get; set; }

        [JsonPropertyName("merchant_id")]
        public string MerchantId { get; set; } = string.Empty;
    }

    public class RecurringInfo
    {
        [JsonPropertyName("frequency")]
        public string? Frequency { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("plan_code")]
        public string? PlanCode { get; set; }

        [JsonPropertyName("customer_name")]
        public string? CustomerName { get; set; }
    }
} 