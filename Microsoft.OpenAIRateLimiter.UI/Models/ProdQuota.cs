using System.Text.Json.Serialization;

namespace Microsoft.OpenAIRateLimiter.UI.Models
{
    public class ProdQuota
    {
        [JsonPropertyName("subscriptionKey")]
        public string SubscriptionKey { get; set; } = default!;

        public string RowKey { get; set; } = default!;

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = default!;

        [JsonPropertyName("timestamp")]
        public DateTimeOffset? Timestamp { get; set; } = default!;

        [JsonPropertyName("model")]
        public string Model { get; set; } = "";

        [JsonPropertyName("tokenAmount")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("promptTokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("operation")]
        public string Operation { get; set; } = default!;

        [JsonPropertyName("amount")]
        public double Amount { get; set; }

        [JsonPropertyName("transCost")]
        public string TransCost { get; set; } = default!;

        [JsonPropertyName("balance")]
        public string Balance { get; set; } = default!;

    }
}
