using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace WebhookBasedTimer.Tests
{

    using Service;

    [TestFixture]
    public class WebhookServiceTests
    {
        private Mock<HttpMessageHandler> httpMessageHandlerMock;
        private HttpClient httpClient;
        private WebhookService webhookService;

        [SetUp]
        public void SetUp()
        {
            // Set up the mock HttpMessageHandler
            httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpClient = new HttpClient(httpMessageHandlerMock.Object);
            webhookService = new WebhookService(httpClient);
        }

        [Test]
        public async Task SendWebhookAsync_ValidUrl_ShouldSendRequestSuccessfully()
        {
            // Arrange
            var webhookUrl = "https://example.com/webhook";
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            // Act
            await webhookService.SendWebhookAsync(webhookUrl);

            // Assert
            httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post && req.RequestUri == new Uri(webhookUrl)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task SendWebhookAsync_FailedRequest_ShouldHandleExceptionGracefully()
        {
            // Arrange
            var webhookUrl = "https://example.com/webhook";
            httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            Assert.DoesNotThrowAsync(async () => await webhookService.SendWebhookAsync(webhookUrl));

            // Assert
            httpMessageHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post && req.RequestUri == new Uri(webhookUrl)),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
