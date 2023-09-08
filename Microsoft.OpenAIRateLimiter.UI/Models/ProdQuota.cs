using System.Text.Json.Serialization;

namespace Microsoft.OpenAIRateLimiter.UI.Models
{
    public class ProdQuota
    {
        [JsonPropertyName("subscriptionKey")]
        public string SubscriptionKey { get; set; } = default!;

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = default!;

        [JsonPropertyName("amount")]
        public string Amount { get; set; } = default!;

        [JsonPropertyName("tokenAmount")]
        public int TokenAmount { get; set; } = default!;

        [JsonPropertyName("monthlyAmount")]
        public int MonthlyAmount { get; set; } = default!;

        [JsonPropertyName("createdTime")]
        public string CreatedTime { get; set; } = DateTime.Now.ToString();

    }
}
