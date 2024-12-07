namespace WebhookBasedTimer.Model
{
    public class CreateTaskRequest
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }
        public string WebhookUrl { get; set; }
    }

    public class TimerResponse
    {
        public string Id { get; set; }
        public int TimeLeft { get; set; }
    }
}
