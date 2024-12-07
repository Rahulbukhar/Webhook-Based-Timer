
using Newtonsoft.Json.Linq;

namespace WebhookBasedTimer.Repository
{
    using WebhookBasedTimer.Model;

    /// <summary>
    /// Interface for interacting with timer data storage.
    /// Provides methods for creating, updating, and retrieving timers.
    /// </summary>
    public interface ITimerRepository
    {
        /// <summary>
        /// Creates a new timer in the repository.
        /// </summary>
        /// <param name="timer">The timer instance to be created.</param>
        /// <returns>A task representing the asynchronous operation. The result is the created timer as a JObject.</returns>
        Task<JObject> CreateTimerAsync(TimerInstance timer);

        /// <summary>
        /// Updates an existing timer in the repository.
        /// </summary>
        /// <param name="timer">The timer instance with updated values.</param>
        /// <returns>A task representing the asynchronous operation. The result is the updated timer as a JObject.</returns>
        Task<JObject> UpdateTimerAsync(TimerInstance timer);

        /// <summary>
        /// Retrieves a specific timer by its ID.
        /// </summary>
        /// <param name="id">The ID of the timer to retrieve.</param>
        /// <returns>A task representing the asynchronous operation. The result is the timer instance with the given ID.</returns>
        Task<TimerInstance> GetTimerAsync(string id);

        /// <summary>
        /// Retrieves all timers in the repository.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The result is a list of all timer instances.</returns>
        Task<IEnumerable<TimerInstance>> GetAllTimersAsync();

        /// <summary>
        /// Retrieves all timers that are currently active, with their remaining time.
        /// </summary>
        /// <param name="currentTime">The current date and time, used to calculate the remaining time for each timer.</param>
        /// <returns>A task representing the asynchronous operation. The result is a list of timers with their time left.</returns>
        Task<List<JObject>> GetTimersWithTimeLeftAsync(DateTime currentTime);
    }

}
