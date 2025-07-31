using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Responses
{
    /// <summary>
    /// This class is used to represent the response payload for looking up an account.
    /// </summary>
    /// The JsonPropertyName attributes are used to map the C# properties to the JSON keys expected by the HabariPay API.
    public class LookUpResponse
    {
        [JsonPropertyName("account_name")]
        public string AccountName { get; set; }
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; }
    }
}
