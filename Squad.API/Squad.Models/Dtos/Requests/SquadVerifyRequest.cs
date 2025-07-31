using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// Request model for verifying Squad.co transactions
    /// </summary>
    public class SquadVerifyRequest
    {
        [JsonPropertyName("transaction_ref")]
        public string TransactionRef { get; set; } = string.Empty;
    }
} 