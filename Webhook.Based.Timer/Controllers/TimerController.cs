using Microsoft.AspNetCore.Mvc;

namespace WebhookBasedTimer.Controllers
{
    using Service;
    using Model;

    /// <summary>
    /// Controller class for managing timer tasks.
    /// </summary>
    [ApiController]
    [Route("api/timers")]
    public class TimerController : ControllerBase
    {
        private readonly ITimerService timerService;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="timerService">The timerService.</param>
        public TimerController(ITimerService timerService)
        {
            this.timerService = timerService;
        }

        /// <summary>
        /// Create a new task with a timer.
        /// </summary>
        /// <param name="request">Task creation request.</param>
        /// <returns>Task ID.</returns>
        /// <response code="200">Task created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="500">Internal Server Error.</response>
        [HttpPost]
        [Route("tasks", Name = "SetTimer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(HttpRequestError), 400)]
        [ProducesResponseType(typeof(HttpRequestError), 500)]
        [Produces("application/json")]
        public async Task<IActionResult> SetTimer([FromBody] CreateTaskRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Request cannot be null.");
                }

                TimerInstance timer =  await timerService.CreateTimerAsync(request);
                
                var response = new TimerResponse
                {
                    Id = timer.Id,
                    TimeLeft = timer.TimeLeft
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                return LogAndTransformException(e);
            }
        }

        /// <summary>
        /// Get the status of an existing task.
        /// </summary>
        /// <param name="taskId">Task ID.</param>
        /// <returns>Task status and time left.</returns>
        /// <response code="200">Task status retrieved successfully.</response>
        /// <response code="404">Task not found.</response>
        /// <response code="500">Internal Server Error.</response>
        [HttpGet]
        [Route("tasks/{taskId}/status", Name = "GetTimerStatus")]
        [ProducesResponseType(typeof(TaskStatusResponse), 200)]
        [ProducesResponseType(typeof(HttpRequestError), 404)]
        [ProducesResponseType(typeof(HttpRequestError), 500)]
        [Produces("application/json")]
        public async Task<IActionResult> GetTimerStatus([FromRoute] string taskId)
        {
            try
            {
                var status = await timerService.GetTimerByIdAsync(taskId);
                if (status == null)
                {
                    return NotFound($"Task with ID {taskId} not found.");
                }
                
                var response = new TimerResponse
                {
                    Id = status.Id,
                    TimeLeft = status.TimeLeft
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                return LogAndTransformException(e);
            }
        }

        /// <summary>
        /// Get a list of all active tasks with their status.
        /// </summary>
        /// <returns>A list of active tasks with status and time left.</returns>
        [HttpGet]
        [Route("listTasks", Name = "ListTimers")]
        [ProducesResponseType(typeof(List<TaskStatusResponse>), 200)]
        [ProducesResponseType(typeof(HttpRequestError), 500)]
        [Produces("application/json")]
        public async Task<IActionResult> ListTimers()
        {
            try
            {
                var tasks = await timerService.GetAllTimersAsync();

                if (tasks == null || tasks.Count() == 0)
                {
                    return NotFound("No active tasks found.");
                }

                return Ok(tasks);
            }
            catch (Exception e)
            {
                return LogAndTransformException(e);
            }
        }

        /// <summary>
        /// Utility method for exception handling
        /// </summary>
        private IActionResult LogAndTransformException(Exception e)
        {
            Console.Error.WriteLine(e.ToString());

            return StatusCode(500, new HttpRequestError
            {
                Message = "An unexpected error occurred.",
                Details = e.Message
            });
        }
    }

    public class HttpRequestError
    {
        /// <summary>
        /// The Error Message
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// The Error Details
        /// </summary>
        public string Details { get; set; }
    }
}
