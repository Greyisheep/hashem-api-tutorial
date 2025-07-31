using System.Net;
using System.Text.Json.Serialization;

namespace Squad.Models.Dtos.Responses
{
    public class BaseResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success => (int)StatusCode >= 200 && (int)StatusCode < 300;
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("status")]
        public HttpStatusCode StatusCode { get; set; }
        [JsonPropertyName("data")]
        public T Data { get; set; }

    }
}
