using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NoSqlDataAccess.Common.Model;
using NoSqlDataAccess.Common;
using WebhookBasedTimer.Model;
using WebhookBasedTimer.Repository;
using NoSqlDataAccess.Common.Query.Model;

namespace WebhookBasedTimer.Tests
{
    [TestFixture]
    public class TimerRepositoryTests
    {
        private Mock<INoSqlDbService> mockDbService;
        private TimerRepository repository;

        [SetUp]
        public void Setup()
        {
            mockDbService = new Mock<INoSqlDbService>();
            repository = new TimerRepository(mockDbService.Object);
        }

        [Test]
        public async Task CreateTimerAsync_ShouldCallCreateObjectsAsyncAndReturnFirstResult()
        {
            
            var timerInstance = new TimerInstance { Id = Guid.NewGuid().ToString() };
            var expectedResponse = JObject.FromObject(new { id = timerInstance.Id });
            mockDbService
                .Setup(s => s.CreateObjectsAsync(It.IsAny<string>(), It.IsAny<List<JObject>>()))
                .ReturnsAsync(new List<JObject> { expectedResponse });

            
            var result = await repository.CreateTimerAsync(timerInstance);

            
            Assert.That(result, Is.EqualTo(expectedResponse));
            mockDbService.Verify(s => s.CreateObjectsAsync(It.IsAny<string>(), It.IsAny<List<JObject>>()), Times.Once);
        }

        [Test]
        public async Task UpdateTimerAsync_ShouldCallUpdateByIdAsyncAndReturnUpdatedState()
        {
            
            var timerInstance = new TimerInstance
            {
                Id = Guid.NewGuid().ToString(),
                TimeLeft = 0,
                Status = "Finished",
                IsActive = false
            };
            var expectedResponse = JObject.FromObject(new { id = timerInstance.Id });
            mockDbService
                .Setup(s => s.UpdateByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    ReturnValueOption.ALL_NEW))
                .ReturnsAsync((null, expectedResponse));

            
            var result = await repository.UpdateTimerAsync(timerInstance);

            
            Assert.That(result, Is.EqualTo(expectedResponse));
            mockDbService.Verify(s => s.UpdateByIdAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, object>>(),
                    ReturnValueOption.ALL_NEW), Times.Once);
        }

        [Test]
        public async Task GetTimerAsync_ValidId_ShouldReturnTimerInstance()
        {
            
            var timerId = Guid.NewGuid().ToString();
            var expectedTimer = new TimerInstance { Id = timerId };
            var response = JObject.FromObject(expectedTimer);
            mockDbService
                .Setup(s => s.FindByIdsAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), null))
                .ReturnsAsync(new List<JObject> { response });

            
            var result = await repository.GetTimerAsync(timerId);

            
            Assert.IsNotNull(result);
            Assert.That(result.Id, Is.EqualTo(timerId));
            mockDbService.Verify(s => s.FindByIdsAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public async Task GetTimerAsync_InvalidId_ShouldReturnNull()
        {
            
            mockDbService
                .Setup(s => s.FindByIdsAsync(It.IsAny<string>(), It.IsAny<List<string>>(), It.IsAny<string>(), null))
                .ReturnsAsync(new List<JObject>());

            
            var result = await repository.GetTimerAsync("invalid-id");

            
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetAllTimersAsync_ShouldReturnListOfTimers()
        {
            
            var timerList = new List<TimerInstance>
            {
                new TimerInstance { Id = Guid.NewGuid().ToString() },
                new TimerInstance { Id = Guid.NewGuid().ToString() }
            };
            var response = timerList.Select(t => JObject.FromObject(t)).ToList();
            mockDbService
                .Setup(s => s.FindBySearchCriteriaAsync(It.IsAny<string>(), null, It.IsAny<string>(), null, null, "ServiceUser"))
                .ReturnsAsync(response);

            
            var result = await repository.GetAllTimersAsync();

            
            Assert.That(result.Count(), Is.EqualTo(timerList.Count));
            mockDbService.Verify(s => s.FindBySearchCriteriaAsync(It.IsAny<string>(), null, It.IsAny<string>(), null, null, "ServiceUser"), Times.Once);
        }

        [Test]
        public async Task GetTimersWithTimeLeftAsync_ShouldReturnFilteredTimers()
        {
            
            var currentTime = DateTime.UtcNow;
            var response = new List<JObject>
            {
                JObject.FromObject(new TimerInstance { Id = Guid.NewGuid().ToString(), IsActive = true }),
                JObject.FromObject(new TimerInstance { Id = Guid.NewGuid().ToString(), IsActive = true })
            };
            mockDbService
                .Setup(s => s.FindBySearchCriteriaAsync(It.IsAny<string>(), null, It.IsAny<string>(), It.IsAny<SearchExpression>(), null, It.IsAny<string>()))
                .ReturnsAsync(response);

            
            var result = await repository.GetTimersWithTimeLeftAsync(currentTime);

            
            Assert.That(result.Count, Is.EqualTo(response.Count));
            mockDbService.Verify(s => s.FindBySearchCriteriaAsync(
                It.IsAny<string>(), null, It.IsAny<string>(), It.IsAny<SearchExpression>(), null, It.IsAny<string>()), Times.Once);
        }
    }
}
