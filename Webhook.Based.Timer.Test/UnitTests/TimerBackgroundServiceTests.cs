using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace WebhookBasedTimer.Tests
{
    using Model;
    using Newtonsoft.Json.Linq;
    using Service;
    using WebhookBasedTimer.Repository;

        [TestFixture]
        public class TimerBackgroundServiceTests
        {
            private Mock<ITimerRepository> mockTimerRepository;
            private Mock<IWebhookService> mockWebhookService;
            private TimerBackgroundService timerBackgroundService;
            private CancellationTokenSource cancellationTokenSource;

            [SetUp]
            public void SetUp()
            {
                mockTimerRepository = new Mock<ITimerRepository>();
                mockWebhookService = new Mock<IWebhookService>();
                cancellationTokenSource = new CancellationTokenSource();
                timerBackgroundService = new TimerBackgroundService(mockTimerRepository.Object, mockWebhookService.Object);
            }

            [TearDown]
            public void TearDown()
            {
                cancellationTokenSource.Cancel();
            }

            [Test]
            public async Task ExecuteAsync_ExpiredTimers_SendsWebhookAndUpdatesTimer()
            {
                
                var expiredTimers = new List<JObject>
                {
                    new JObject
                    {
                        { "Id", Guid.NewGuid().ToString() },
                        { "ExpiryTime", DateTime.UtcNow.AddSeconds(-5) },
                        { "WebhookUrl", "https://example.com/webhook" },
                        { "IsActive", true },
                        { "IsTriggered", false },
                        { "Status", "Started" }
                    }
                };

                mockTimerRepository
                    .Setup(repo => repo.GetTimersWithTimeLeftAsync(It.IsAny<DateTime>()))
                    .ReturnsAsync(expiredTimers);

                mockTimerRepository
                    .Setup(repo => repo.UpdateTimerAsync(It.IsAny<TimerInstance>()))
                    .ReturnsAsync(new JObject());

                mockWebhookService
                    .Setup(service => service.SendWebhookAsync(It.IsAny<string>()))
                    .Returns(Task.CompletedTask);

                
                var executeTask = timerBackgroundService.StartAsync(cancellationTokenSource.Token);
                await Task.Delay(2000); // Allow the service to process
                cancellationTokenSource.Cancel();
                await executeTask;

                
                mockTimerRepository.Verify(repo => repo.GetTimersWithTimeLeftAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
                mockWebhookService.Verify(service => service.SendWebhookAsync(It.IsAny<string>()), Times.AtLeastOnce);
                mockTimerRepository.Verify(repo => repo.UpdateTimerAsync(It.IsAny<TimerInstance>()), Times.AtLeastOnce);
            }

            //[Test]
            //public async Task ExecuteAsync_NoExpiredTimers_NoActionsTaken()
            //{
                
            //    mockTimerRepository
            //        .Setup(repo => repo.GetTimersWithTimeLeftAsync(It.IsAny<DateTime>()))
            //        .ReturnsAsync(new List<JObject>()); // No expired timers

                
            //    var executeTask = timerBackgroundService.StartAsync(cancellationTokenSource.Token);
            //    await Task.Delay(2); // Allow the service to process
            //    cancellationTokenSource.Cancel();
            //    await executeTask;

                
            //    mockTimerRepository.Verify(repo => repo.GetTimersWithTimeLeftAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
            //    mockWebhookService.Verify(service => service.SendWebhookAsync(It.IsAny<string>()), Times.Never);
            //    mockTimerRepository.Verify(repo => repo.UpdateTimerAsync(It.IsAny<TimerInstance>()), Times.Never);
            //}

            //[Test]
            //public async Task ExecuteAsync_ExceptionInTimerProcessing_ContinuesExecution()
            //{
                
            //    var expiredTimers = new List<JObject>
            //    {
            //        new JObject
            //        {
            //            { "Id", Guid.NewGuid().ToString() },
            //            { "ExpiryTime", DateTime.UtcNow.AddSeconds(-5) },
            //            { "WebhookUrl", "https://example.com/webhook" },
            //            { "IsActive", true },
            //            { "IsTriggered", false },
            //            { "Status", "Started" }
            //        }
            //    };

            //    mockTimerRepository
            //        .Setup(repo => repo.GetTimersWithTimeLeftAsync(It.IsAny<DateTime>()))
            //        .ReturnsAsync(expiredTimers);

            //    mockTimerRepository
            //        .Setup(repo => repo.UpdateTimerAsync(It.IsAny<TimerInstance>()))
            //        .ThrowsAsync(new Exception("Simulated exception"));

                
            //    var executeTask = timerBackgroundService.StartAsync(cancellationTokenSource.Token);
            //    await Task.Delay(2000); // Allow the service to process
            //    cancellationTokenSource.Cancel();
            //    await executeTask;

                
            //    mockTimerRepository.Verify(repo => repo.GetTimersWithTimeLeftAsync(It.IsAny<DateTime>()), Times.AtLeastOnce);
            //    mockWebhookService.Verify(service => service.SendWebhookAsync(It.IsAny<string>()), Times.Never); // Webhook not called due to exception
            //    mockTimerRepository.Verify(repo => repo.UpdateTimerAsync(It.IsAny<TimerInstance>()), Times.Once);
            //}
        }
}
