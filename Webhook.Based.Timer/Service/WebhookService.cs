namespace WebhookBasedTimer.Service
{
    using System.Net.Http;
    using System.Threading.Tasks;


    public interface IWebhookService
    {
        Task SendWebhookAsync(string webhookUrl);
    }

    public class WebhookService : IWebhookService
    {
        private readonly HttpClient httpClient;

        public WebhookService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task SendWebhookAsync(string webhookUrl)
        {
            try
            {
                var response = await httpClient.PostAsync(webhookUrl, null);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Handle failure (e.g., logging)
            }
        }
    }

}
