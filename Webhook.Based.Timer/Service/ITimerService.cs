using WebhookBasedTimer.Model;

namespace WebhookBasedTimer.Service
{
    /// <summary>
    /// Interface for managing timer operations. 
    /// Provides methods to create, retrieve, and manage timers.
    /// </summary>
    public interface ITimerService
    {
        /// <summary>
        /// Creates a new timer based on the provided request.
        /// </summary>
        /// <param name="request">The request containing the timer creation details.</param>
        /// <returns>A task representing the asynchronous operation. The result is the created <see cref="TimerInstance"/>.</returns>
        Task<TimerInstance> CreateTimerAsync(CreateTaskRequest request);

        /// <summary>
        /// Retrieves a timer by its unique ID.
        /// </summary>
        /// <param name="id">The unique identifier of the timer.</param>
        /// <returns>A task representing the asynchronous operation. The result is the <see cref="TimerInstance"/> associated with the provided ID.</returns>
        Task<TimerInstance> GetTimerByIdAsync(string id);

        /// <summary>
        /// Retrieves all timers in the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. The result is an enumerable list of all <see cref="TimerInstance"/>s.</returns>
        Task<IEnumerable<TimerInstance>> GetAllTimersAsync();
    }
}
