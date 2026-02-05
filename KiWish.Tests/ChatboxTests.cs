using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KiWish.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace KiWish.Tests
{
    public class ChatboxTests
    {
        [Fact]
        public async Task ProductKnowledgeService_ReturnsContent_WhenFileExists()
        {
            // Arrange
            var service = new ProductKnowledgeService();

            // Act
            var content = await service.GetProductInfoAsync();

            // Assert
            Assert.NotNull(content);
            Assert.Contains("KiWish", content); // Basic check
        }

        [Fact]
        public async Task GeminiAIService_ReturnsError_WhenApiKeyMissing()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHandler.Object);
            
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Gemini:ApiKey"]).Returns("");
            
            var service = new GeminiAIService(httpClient, mockConfig.Object);

            // Act
            var response = await service.GetResponseAsync("Hello", "System");

            // Assert
            Assert.Contains("Error: API Key is missing", response);
        }

        [Fact]
        public async Task GeminiAIService_MakesCorrectCall_WhenConfigured()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>();
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"Hello User\"}]}}]}")
                });

            var httpClient = new HttpClient(mockHandler.Object);
            
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["Gemini:ApiKey"]).Returns("fake-key");
            mockConfig.Setup(c => c["Gemini:Model"]).Returns("gemini-1.5-flash");

            var service = new GeminiAIService(httpClient, mockConfig.Object);

            // Act
            var response = await service.GetResponseAsync("Hello", "System");

            // Assert
            Assert.Equal("Hello User", response);
        }
    }
}
