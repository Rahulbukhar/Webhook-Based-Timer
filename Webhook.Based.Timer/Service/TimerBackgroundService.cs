
namespace WebhookBasedTimer.Service
{
    using Model;
    using Repository;

    public class TimerBackgroundService : BackgroundService
    {
        private readonly ITimerRepository timerRepository;
        private readonly IWebhookService webhookService;
        private readonly TimeSpan checkInterval = TimeSpan.FromSeconds(1);

        public TimerBackgroundService(ITimerRepository timerRepository, IWebhookService webhookService)
        {
            this.timerRepository = timerRepository;
            this.webhookService = webhookService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                
                    var expiredTimers = await timerRepository.GetTimersWithTimeLeftAsync(DateTime.Now);

                    List<TimerInstance> timerInstances = new List<TimerInstance>();
                    TimerInstance timerInstance = null;
                    foreach (var jObject in expiredTimers)
                    {
                        try
                        {
                            timerInstance = jObject.ToObject<TimerInstance>();

                            var timeleft = (int)(timerInstance.ExpiryTime - DateTime.UtcNow).TotalSeconds;

                            timerInstance.TimeLeft = timeleft <= 0 ? 0 : timeleft;

                            if (timeleft <= 0)
                            {
                                var updateValues = new Dictionary<string, object> { { nameof(TimerInstance.TimeLeft), timerInstance.TimeLeft } };

                                await webhookService.SendWebhookAsync(timerInstance.WebhookUrl);

                                timerInstance.IsTriggered = true;
                                timerInstance.Status = TimerStatusHelpers.Finished;
                                timerInstance.IsActive = false;
                            }

                            await timerRepository.UpdateTimerAsync(timerInstance);
                            await Task.Delay(checkInterval, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to process timer {timerInstance.Id}: {ex.Message}");
                        }
                    }
             }
        }

    }
}
