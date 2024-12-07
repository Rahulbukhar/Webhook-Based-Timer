using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;

namespace WebhookBasedTimer.Tests
{

    using Controllers;
    using Model;
    using Service;

    [TestFixture]
    public class TimerControllerTests
    {
        private Mock<ITimerService> mockTimerService;

        private TimerController timerController;

        [SetUp]
        public void SetUp()
        {
            mockTimerService = new Mock<ITimerService> ();
            timerController = new TimerController(mockTimerService.Object);
        }

        [Test]
        public async Task SetTimer_ReturnsOk_WhenRequestIsValid()
        {
            var request = new CreateTaskRequest
            {
                Hours = 1,
                Minutes = 30,
                Seconds = 45,
                WebhookUrl = "https://example.com/webhook"
            };

            var timerInstance = new TimerInstance
            {
                Id = Guid.NewGuid().ToString(),
                TimeLeft = 5400, // 1 hour 30 minutes
            };

            mockTimerService.Setup(service => service.CreateTimerAsync(request)).ReturnsAsync(timerInstance);

            
            var result = await timerController.SetTimer(request);

            
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var response = okResult.Value as TimerResponse;
            Assert.That(response.Id, Is.EqualTo(timerInstance.Id));
            Assert.That(response.TimeLeft, Is.EqualTo(timerInstance.TimeLeft));
        }

        [Test]
        public async Task SetTimer_ReturnsBadRequest_WhenRequestIsNull()
        {
            
            var result = await timerController.SetTimer(null);

            
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult.Value, Is.EqualTo("Request cannot be null."));
        }

        [Test]
        public async Task GetTimerStatus_ReturnsOk_WhenTimerExists()
        {
            
            var taskId = Guid.NewGuid().ToString();
            var timerInstance = new TimerInstance
            {
                Id = taskId,
                TimeLeft = 3000
            };

            mockTimerService.Setup(service => service.GetTimerByIdAsync(taskId)).ReturnsAsync(timerInstance);

            
            var result = await timerController.GetTimerStatus(taskId);

            
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var response = okResult.Value as TimerResponse;
            Assert.That(response.Id, Is.EqualTo(timerInstance.Id));
            Assert.That(response.TimeLeft, Is.EqualTo(timerInstance.TimeLeft));
        }

        [Test]
        public async Task GetTimerStatus_ReturnsNotFound_WhenTimerDoesNotExist()
        {
            
            var taskId = Guid.NewGuid().ToString();

            mockTimerService.Setup(service => service.GetTimerByIdAsync(taskId)).ReturnsAsync((TimerInstance)null);

            
            var result = await timerController.GetTimerStatus(taskId);

            
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo($"Task with ID {taskId} not found."));
        }

        [Test]
        public async Task ListTimers_ReturnsOk_WhenTimersExist()
        {
            
            var timers = new List<TimerInstance>
            {
                new TimerInstance { Id = Guid.NewGuid().ToString(), TimeLeft = 1000 },
                new TimerInstance { Id =  Guid.NewGuid().ToString(), TimeLeft = 2000 }
            };

            mockTimerService.Setup(service => service.GetAllTimersAsync()).ReturnsAsync(timers);

            
            var result = await timerController.ListTimers();

            
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var response = okResult.Value as List<TimerInstance>;
            Assert.That(response.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task ListTimers_ReturnsNotFound_WhenNoTimersExist()
        {
            
            mockTimerService.Setup(service => service.GetAllTimersAsync()).ReturnsAsync(new List<TimerInstance>());

            
            var result = await timerController.ListTimers();

            
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult.Value, Is.EqualTo("No active tasks found."));
        }
    }
}
