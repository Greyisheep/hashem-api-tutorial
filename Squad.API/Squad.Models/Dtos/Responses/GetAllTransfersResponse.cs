using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Responses
{
    /// <summary>
    /// This class is used to represent the response payload for retrieving all transfers.
    /// </summary>
    /// The JsonPropertyName attributes are used to map the C# properties to the JSON keys expected by the HabariPay API.
    public class GetAllTransfersResponse
    {
        [JsonPropertyName("account_number_credited")]
        public string AccountNumberCredited { get; set; }
        [JsonPropertyName("amount_debited")]
        public string AmountDebited { get; set; }
        [JsonPropertyName("total_amount_debited")]
        public string TotalAmountDebited { get; set; }
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("recipient")]
        public string Recipient { get; set; }
        [JsonPropertyName("bank_code")]
        public string BankCode { get; set; }
        [JsonPropertyName("transaction_reference")]
        public string TransactionReference { get; set; }
        [JsonPropertyName("transaction_status")]
        public string TransactionStatus { get; set; }
        [JsonPropertyName("switch_transaction")]
        public object SwitchTransaction { get; set; }

    }
}
