using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Requests
{
    /// <summary>
    /// This class is used to represent the request payload for transferring funds to an account.
    /// </summary>
    /// The JsonPropertyName attributes are used to map the C# properties to the JSON keys expected by the HabariPay API.
    public class TransferRequest
    {
        [JsonPropertyName("remark")]
        public string Remark { get; set; }
        [JsonPropertyName("bank_code")]
        public string BankCode { get; set; }
        [JsonPropertyName("currency_id")]
        public string CurrencyId { get; set; }
        [JsonPropertyName("amount")]
        public string Amount { get; set; }
        [JsonPropertyName("account_number")]
        public string AccountNumber { get; set; }
        [JsonPropertyName("transaction_reference")]
        public string TransactionReference { get; set; }
        [JsonPropertyName("account_name")]
        public string AccountName { get; set; }


    }
}
