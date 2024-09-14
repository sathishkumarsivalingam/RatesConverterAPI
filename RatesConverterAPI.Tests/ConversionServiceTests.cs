using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using RatesConverterAPI.Application.Services;
using RatesConverterAPI.Core.Configuration;
using RatesConverterAPI.Core.Entity;
using RatesConverterAPI.Core.Interface;
using System.Net;

namespace RatesConverterAPI.Tests
{
    public class ConversionServiceTests
    {
        private readonly Mock<ILogger<ConversionService>> _mockLogger;
        private readonly Mock<IOptions<FrankFurterSettings>> _mockOptions;
        private readonly FrankFurterSettings _settings;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly ConversionService _conversionService;

        public ConversionServiceTests()
        {
            // Mock logger
            _mockLogger = new Mock<ILogger<ConversionService>>();

            // Mock FrankFurter settings
            _settings = new FrankFurterSettings
            {
                BaseURL = "https://api.frankfurter.app/",
                LatestBaseUrl = "latest"
            };
            _mockOptions = new Mock<IOptions<FrankFurterSettings>>();
            _mockOptions.Setup(o => o.Value).Returns(_settings);

            // Create a mock HttpMessageHandler to simulate the HttpClient
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Use the mocked handler in the HttpClient
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            // Create the service instance with mocked dependencies
            _conversionService = new ConversionService(_httpClient, _mockLogger.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task GetLatestRates_ReturnsRates_OnSuccess()
        {
            // Arrange
            var conversionRequest = new ConversionRequest
            {
                Amount = 100,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            string apiResponse = "{ 'rates': { 'EUR': 0.85 }, 'base': 'USD', 'date': '2024-09-12' }";

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(apiResponse),
                });

            // Act
            var result = await _conversionService.GetLatestRates(conversionRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("EUR", result);
            _mockLogger.Verify(
                m => m.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    (Func<object, Exception, string>)It.IsAny<object>()),
                Times.Never
            );
        }

        [Fact]
        public async Task GetLatestRates_LogsWarning_IfRateLimitExceeded()
        {
            // Arrange
            var conversionRequest = new ConversionRequest
            {
                Amount = 100,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.TooManyRequests,
                });

            // Act
            var result = await _conversionService.GetLatestRates(conversionRequest);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Rate limit exceeded", result);
            _mockLogger.Verify(
                m => m.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Rate limit exceeded")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetLatestRates_ReturnsNull_IfRequestFails()
        {
            // Arrange
            var conversionRequest = new ConversionRequest
            {
                Amount = 100,
                FromCurrency = "USD",
                ToCurrency = "EUR"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                });

            // Act
            var result = await _conversionService.GetLatestRates(conversionRequest);

            // Assert
            Assert.Null(result);
            _mockLogger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to fetch latest rates")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }
    }

}