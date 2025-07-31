using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// This class is used to represent the request payload for looking up an account.
    /// </summary>
    /// The JsonPropertyName attributes are used to map the C# properties to the JSON keys expected by the HabariPay API.
    public class LookUpRequest
    {
        [JsonPropertyName("bank_code")]
        public string BankCode { get; set; }
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; }


    }
}
