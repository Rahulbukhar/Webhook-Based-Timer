using Microsoft.Azure.Cosmos;

namespace WebhookBasedTimer.Service
{

    using Model;
    using Repository;

    /// <summary>
    /// Service class for managing timers, including creating, retrieving, and listing timers.
    /// </summary>
    public class TimerService : ITimerService
    {
        private readonly ITimerRepository timerRepository;
        private readonly IWebhookService webhookService;

        /// <summary>
        /// Construct TimerService
        /// </summary>
        /// <param name="webhookService">An instance of <see cref="IWebhookService"/></param>
        /// <param name="timerRepository">An instance of <see cref="ITimerRepository"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        public TimerService(IWebhookService webhookService, ITimerRepository timerRepository)
        {
            this.webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
            this.timerRepository = timerRepository ?? throw new ArgumentNullException(nameof(timerRepository));
        }

        /// <summary>
        /// Creates a new timer with the specified request parameters.
        /// </summary>
        /// <param name="request">The request containing the timer creation details.</param>
        /// <returns>A task representing the asynchronous operation. The result is the created <see cref="TimerInstance"/>.</returns>
        public async Task<TimerInstance> CreateTimerAsync(CreateTaskRequest request)
        {
            // Calculate the expiry time based on the request parameters.
            DateTime expiryTime = DateTime.UtcNow
                .AddHours(request.Hours)
                .AddMinutes(request.Minutes)
                .AddSeconds(request.Seconds);

            // Create a new timer instance.
            var timer = new TimerInstance
            {
                Id = Guid.NewGuid().ToString(),
                DateCreated = DateTime.UtcNow,
                ExpiryTime = expiryTime,
                WebhookUrl = request.WebhookUrl,
                Status = TimerStatusHelpers.Started,
                TimeLeft = (int)(expiryTime - DateTime.UtcNow).TotalSeconds,
                Hours = request.Hours,
                Minutes = request.Minutes,
                Seconds = request.Seconds,
                IsActive = true,
                IsTriggered = false
            };

            if(timer.TimeLeft <= 0)
            {
                timer.TimeLeft = 0;
                timer.IsTriggered = true;
                timer.IsActive = false;
                
                _ = webhookService.SendWebhookAsync(timer.WebhookUrl);
            }

            try
            {
                // Attempt to create the timer in the repository.
                var response = await timerRepository.CreateTimerAsync(timer);
                return response?.ToObject<TimerInstance>();
            }
            catch (CosmosException ex)
            {
                // Log the error and throw a more detailed exception.
                // Add logging here as needed.
                throw new Exception($"Error creating timer with ID {timer.Id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves a timer by its unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the timer.</param>
        /// <returns>A task representing the asynchronous operation. The result is the <see cref="TimerInstance"/> or null if not found.</returns>
        public async Task<TimerInstance> GetTimerByIdAsync(string id)
        {
            try
            {
                // Retrieve the timer from the repository.
                var timer = await timerRepository.GetTimerAsync(id);
                return timer;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Log the 'not found' scenario and return null.
                // Add logging here as needed.
                return null;
            }
            catch (CosmosException ex)
            {
                // Log and rethrow any other Cosmos DB-related exceptions.
                throw new Exception($"Error retrieving timer with ID {id}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Retrieves all timers in the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The result is an enumerable list of <see cref="TimerInstance"/>s.</returns>
        public async Task<IEnumerable<TimerInstance>> GetAllTimersAsync()
        {
            try
            {
                // Retrieve all timers from the repository.
                return await timerRepository.GetAllTimersAsync();
            }
            catch (CosmosException ex)
            {
                // Log the error and rethrow.
                throw new Exception($"Error retrieving all timers: {ex.Message}", ex);
            }
        }
    }
}
