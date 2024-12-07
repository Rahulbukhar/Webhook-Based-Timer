using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Moq;
using NUnit.Framework;

namespace WebhookBasedTimer.Tests
{

    using Model;
    using Newtonsoft.Json.Linq;
    using WebhookBasedTimer.Repository;
    using Service;

    [TestFixture]
    public class TimerServiceTests
    {
        private Mock<ITimerRepository> mockTimerRepository;
        private Mock<IWebhookService> mockWebhookService;
        private TimerService timerService;

        [SetUp]
        public void SetUp()
        {
            mockTimerRepository = new Mock<ITimerRepository>();
            mockWebhookService = new Mock<IWebhookService>();
            timerService = new TimerService(mockWebhookService.Object, mockTimerRepository.Object);
        }

        [Test]
        public async Task CreateTimerAsync_ValidRequest_ReturnsCreatedTimer()
        {
            
            var request = new CreateTaskRequest
            {
                Hours = 1,
                Minutes = 30,
                Seconds = 0,
                WebhookUrl = "https://example.com/webhook"
            };

            var timer = new TimerInstance
            {
                Id = Guid.NewGuid().ToString(),
                ExpiryTime = DateTime.UtcNow.AddHours(1).AddMinutes(30),
                Status = TimerStatusHelpers.Started,
                TimeLeft = 5400,
                WebhookUrl = request.WebhookUrl,
                IsActive = true
            };

            mockTimerRepository
                .Setup(repo => repo.CreateTimerAsync(It.IsAny<TimerInstance>()))
                .ReturnsAsync(JObject.FromObject(timer));
                

            
            var result = await timerService.CreateTimerAsync(request);

            
            Assert.IsNotNull(result);
            Assert.That(result.WebhookUrl, Is.EqualTo(timer.WebhookUrl));
            mockTimerRepository.Verify(repo => repo.CreateTimerAsync(It.IsAny<TimerInstance>()), Times.Once);
        }

        [Test]
        public async Task CreateTimerAsync_InvalidRequest_TriggersWebhook()
        {
            
            var request = new CreateTaskRequest
            {
                Hours = 0,
                Minutes = 0,
                Seconds = 0,
                WebhookUrl = "https://example.com/webhook"
            };
            
            
            await timerService.CreateTimerAsync(request);

            
            mockWebhookService.Verify(webhook => webhook.SendWebhookAsync(request.WebhookUrl), Times.Once);
        }

        [Test]
        public async Task GetTimerByIdAsync_ValidId_ReturnsTimer()
        {
            
            var timerId = Guid.NewGuid().ToString();
            var timer = new TimerInstance
            {
                Id = timerId,
                ExpiryTime = DateTime.UtcNow.AddMinutes(30),
                Status = TimerStatusHelpers.Started,
                IsActive = true
            };

            mockTimerRepository
                .Setup(repo => repo.GetTimerAsync(timerId))
                .ReturnsAsync(timer);

            
            var result = await timerService.GetTimerByIdAsync(timerId);

            
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(timerId));
            mockTimerRepository.Verify(repo => repo.GetTimerAsync(timerId), Times.Once);
        }

        [Test]
        public async Task GetTimerByIdAsync_InvalidId_ReturnsNull()
        {
            
            var timerId = Guid.NewGuid().ToString();

            mockTimerRepository
                .Setup(repo => repo.GetTimerAsync(timerId))
                .ThrowsAsync(new CosmosException("Not found", System.Net.HttpStatusCode.NotFound, 0, "", 0));

            
            var result = await timerService.GetTimerByIdAsync(timerId);

            
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllTimersAsync_ValidCall_ReturnsTimers()
        {
            
            var timers = new List<TimerInstance>
            {
                new TimerInstance { Id = Guid.NewGuid().ToString(), IsActive = true },
                new TimerInstance { Id = Guid.NewGuid().ToString(), IsActive = false }
            };

            mockTimerRepository
                .Setup(repo => repo.GetAllTimersAsync())
                .ReturnsAsync(timers);

            
            var result = await timerService.GetAllTimersAsync();

            
            Assert.IsNotNull(result);
            Assert.That(result.Count(), Is.EqualTo(timers.Count));
        }
    }
}
