namespace WebhookBasedTimer.Model
{
    public class TaskStatusResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; }
        public DateTime ExpirationTime { get; set; }
        public int TimeRemaining { get; set; }
    }
}
