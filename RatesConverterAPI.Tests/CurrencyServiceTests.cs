using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RatesConverterAPI.Application.Services;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using RatesConverterAPI.Core.Configuration;
using System.Runtime;

namespace RatesConverterAPI.Tests
{
    public class CurrencyServiceTests
    {
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly Mock<ILogger<CurrencyService>> _mockLogger;
        private readonly Mock<IOptions<FrankFurterSettings>> _mockOptions;
        private readonly FrankFurterSettings _settings;
        private readonly CurrencyService _currencyService;
        private readonly HttpClient _httpClient;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public CurrencyServiceTests()
        {
            _mockMemoryCache = new Mock<IMemoryCache>();

            _mockLogger = new Mock<ILogger<CurrencyService>>();

            // Mock FrankFurter settings
            _settings = new FrankFurterSettings
            {
                BaseURL = "https://api.frankfurter.app/",
                LatestBaseUrl = "latest"
            };
            _mockOptions = new Mock<IOptions<FrankFurterSettings>>();
            _mockOptions.Setup(o => o.Value).Returns(_settings);
            // Create a mock HttpMessageHandler
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            // Use the mock HttpMessageHandler in HttpClient
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            // Instantiate the service with mocked dependencies
            _currencyService = new CurrencyService(_mockMemoryCache.Object, _httpClient, _mockLogger.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task GetLatestRates_ReturnsCachedRates_IfAvailable()
        {
            // Arrange
            string baseCurrency = "USD";
            string expectedCachedRates = "{\"amount\":1.0,\"base\":\"USD\"}";
            object cachedRates = expectedCachedRates;

            _mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out cachedRates)).Returns(true);

            // Act
            var result = await _currencyService.GetLatestRates(baseCurrency);

            // Assert
            Assert.Equal(expectedCachedRates, result);
        }

        ////[Fact]
        ////public async Task GetLatestRates_FetchesRates_IfNotCached()
        ////{
        ////    // Arrange
        ////    string baseCurrency = "EUR";
        ////    string apiResponse = "{\"amount\":1.0,\"base\":\"EUR\",\"date\":\"2024-09-12\",\"rates\":{\"USD\":1.1016}}";
        ////    object cachedRates = null;

        ////    // Setup cache to return no cached response
        ////    _mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out cachedRates)).Returns(false);

        ////    // Mock HttpMessageHandler's SendAsync method
        ////    _mockHttpMessageHandler
        ////        .Protected()
        ////        .Setup<Task<HttpResponseMessage>>(
        ////            "SendAsync",
        ////            //It.IsAny<HttpRequestMessage>(),        // Match any HttpRequestMessage
        ////            //It.IsAny<CancellationToken>()          // Match any CancellationToken
        ////            ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
        ////    ItExpr.IsAny<CancellationToken>()
        ////        )
        ////        .ReturnsAsync(new HttpResponseMessage
        ////        {
        ////            StatusCode = HttpStatusCode.OK,
        ////            Content = new StringContent(apiResponse),  // Simulate a valid response
        ////        });

        ////    // Setup cache to store the response after fetching
        ////    _mockMemoryCache.Setup(mc => mc.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()));

        ////    // Act
        ////    var result = await _currencyService.GetLatestRates(baseCurrency);

        ////    // Assert
        ////    Assert.Equal(apiResponse, result); // Ensure the result matches the mock response
        ////}
        [Fact]
        public async Task GetLatestRates_FetchesRates_IfNotCached()
        {
            // Arrange
            string baseCurrency = "EUR";
            string apiResponse = "{\"amount\":1.0,\"base\":\"EUR\",\"date\":\"2024-09-12\",\"rates\":{\"USD\":1.1016}}";
            object cachedRates = null;

            // Mock the cache to simulate no cached response
            _mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out cachedRates)).Returns(false);

            // Mock HttpMessageHandler's SendAsync method to simulate a successful API call
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),  // Matching GET request
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(apiResponse),  // Simulate a valid response
                });

            // Mock the creation of a cache entry
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            // Act
            var result = await _currencyService.GetLatestRates(baseCurrency);

            // Assert
            Assert.Equal(apiResponse, result); // Ensure the result matches the mock response
        }


        [Fact]
        public async Task GetLatestRates_ReturnsRateLimitExceededMessage_IfRateLimited()
        {
            // Arrange
            string baseCurrency = "USD";
            object cachedRates = null;

            _mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out cachedRates)).Returns(false);

            // Mock HttpMessageHandler to return "Too Many Requests"
            _mockHttpMessageHandler
                .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
               ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),  // Matching GET request
                    ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.TooManyRequests
            });

            // Act
            var result = await _currencyService.GetLatestRates(baseCurrency);

            // Assert
            Assert.Equal($"Rate limit exceeded for base currency: {baseCurrency}", result);
        }

        [Fact]
        public async Task GetLatestRates_LogsError_IfRequestFails()
        {
            // Arrange
            string baseCurrency = "USD";
            object cachedRates = null;

            _mockMemoryCache.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out cachedRates)).Returns(false);

            // Mock HttpMessageHandler to return an internal server error
            _mockHttpMessageHandler
                .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),  // Matching GET request
               ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

            // Act
            var result = await _currencyService.GetLatestRates(baseCurrency);

            // Assert
            Assert.Null(result);

        }
    }
}
