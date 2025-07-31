using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    public class ReQueryRequest
    {
        /// <summary>
        /// This class is used to represent the request payload for re-querying a transaction.
        /// </summary>
        /// The JsonPropertyName attributes are used to map the C# properties to the JSON keys expected by the HabariPay API.
        [JsonPropertyName("transaction_reference")]
        public string TransactionReference { get; set; }


    }
}
