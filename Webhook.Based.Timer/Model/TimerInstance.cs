using Newtonsoft.Json;

namespace WebhookBasedTimer.Model
{
    public class TimerInstance
    {
        /// <summary>
        /// Unique identifier for the task instance
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Timestamp when the task was created
        /// </summary>
        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Exact time the task is set to expire
        /// </summary>
        [JsonProperty("expiryTime")]
        public DateTime ExpiryTime { get; set; }

        /// <summary>
        /// Webhook URL to trigger when the timer expires
        /// </summary>
        [JsonProperty("webhookUrl")]
        public string WebhookUrl { get; set; }

        /// <summary>
        /// "Started" or "Finished"
        /// </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        /// Remaining time in seconds until expiration
        /// </summary>
        [JsonProperty("timeLeft")]
        public int TimeLeft { get; set; }

        /// <summary>
        /// Hours component of the timer duration
        /// </summary>
        [JsonProperty("hours")]
        public int Hours { get; set; }

        /// <summary>
        /// Minutes component of the timer duration
        /// </summary>
        [JsonProperty("minutes")]
        public int Minutes { get; set; }

        /// <summary>
        /// Seconds component of the timer duration
        /// </summary>
        [JsonProperty("seconds")]
        public int Seconds { get; set; }

        /// <summary>
        /// The userId
        /// </summary>
        [JsonProperty("userId")]
        public string UserId { get; set; } = "TenantUser";

        /// <summary>
        /// IsActive
        /// </summary>
        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        /// <summary>
        /// IsTriggered
        /// </summary>
        [JsonProperty("isTriggered")]
        public bool IsTriggered { get; internal set; }
    }

}
