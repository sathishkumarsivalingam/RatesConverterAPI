using Moq;
using Xunit;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using RatesConverterAPI.Core.Configuration;
using RatesConverterAPI.Application.Services;
using RatesConverterAPI.Core.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using RatesConverterAPI.Application.DTOs;
using Moq.Protected;

namespace RatesConverterAPI.Tests
{
    public class HistoricalRatesServiceTests
    {
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<ILogger<HistoricalRatesService>> _mockLogger;
        private readonly Mock<IOptions<FrankFurterSettings>> _mockOptions;
        private readonly HttpClient _mockHttpClient;
        private readonly HistoricalRatesService _service;

        public HistoricalRatesServiceTests()
        {
            _mockCache = new Mock<IMemoryCache>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockLogger = new Mock<ILogger<HistoricalRatesService>>();
            _mockOptions = new Mock<IOptions<FrankFurterSettings>>();

            var settings = new FrankFurterSettings
            {
                BaseURL = "https://api.frankfurter.app/"
            };
            _mockOptions.Setup(s => s.Value).Returns(settings);

            // Create HttpClient using mocked handler
            _mockHttpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app/")
            };

            _service = new HistoricalRatesService(_mockCache.Object, _mockHttpClient, _mockLogger.Object, _mockOptions.Object);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsCachedData_WhenAvailable()
        {
            // Arrange
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "USD",
                StartDate = DateTime.Parse("2024-09-02"),
                EndDate = DateTime.Parse("2024-09-13"),
                Page = 1,
                PageSize = 10
            };

            var cachedData = new CurrencyRateResponse
            {
                Base = "USD",
                StartDate = DateTime.Parse("2024-09-02"),
                EndDate = DateTime.Parse("2024-09-13"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
    {
        {
            "2024-09-02", new Dictionary<string, decimal>
            {
                { "EUR", 0.85M },
                { "GBP", 0.75M }
            }
        },
        {
            "2024-09-03", new Dictionary<string, decimal>
            {
                { "EUR", 0.86M },
                { "GBP", 0.76M }
            }
        }
    }
            };


            // Setup cache to return cached data
            object cacheEntry = cachedData;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(true);

            // Act
            var result = await _service.GetHistoricalRates(request);

            // Assert
            Assert.NotNull(result);
            _mockLogger.Verify(
              m => m.Log(
                  LogLevel.Information,
                  It.IsAny<EventId>(),
                  It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Returning cached historical rates")),
                  null,
                  (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
              ),
              Times.Once
          );
            _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out cacheEntry), Times.Once);
        }

        [Fact]
        public async Task GetHistoricalRates_ReturnsFreshData_AndCachesIt_WhenCacheMiss()
        {
            // Arrange
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "USD",
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today,
                Page = 1,
                PageSize = 10
            };

            var freshData = new CurrencyRateResponse
            {
                Base = "USD",
                StartDate = DateTime.Parse("2024-09-02"),
                EndDate = DateTime.Parse("2024-09-13"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
    {
        {
            "2024-09-02", new Dictionary<string, decimal>
            {
                { "EUR", 0.85M },
                { "GBP", 0.75M }
            }
        },
        {
            "2024-09-03", new Dictionary<string, decimal>
            {
                { "EUR", 0.86M },
                { "GBP", 0.76M }
            }
        }
    }
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK, // This should be OK (200)
                Content = new StringContent(JsonConvert.SerializeObject(freshData))
            };

            // Mocking cache miss
            object cacheEntry = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(false);

            // Mocking CreateEntry (which is used internally by Set)
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            // Setup the mock HTTP handler to return a successful response
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(httpResponse); // Return the successful response

            // Act
            var result = await _service.GetHistoricalRates(request);

            // Assert
            Assert.NotNull(result); // Ensure the result is not null
            _mockLogger.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Cached historical rates")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );

            //_mockLogger.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2)); // Verify logging
            _mockCache.Verify(x => x.CreateEntry(It.IsAny<object>()), Times.Once); // Verify cache entry created
        }

        [Fact]
        public async Task GetHistoricalRates_LogsWarning_WhenRequestFails()
        {
            // Arrange
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "USD",
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today
            };

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            // Setup cache miss and HTTP failure
            object cacheEntry = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(false);
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
                    Content = httpResponse.Content
                });

            //_mockHttpMessageHandler
            //    .Setup(m => m.Send(It.IsAny<HttpRequestMessage>()))
            //    .Returns(httpResponse);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetHistoricalRates(request));

            _mockLogger.Verify(
                m => m.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to fetch historical rates")),
                    null,
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task GetHistoricalRates_LogsError_AndThrowsException_WhenHttpRequestExceptionOccurs()
        {
            // Arrange
            var request = new HistoricalRatesRequest
            {
                BaseCurrency = "USD",
                StartDate = DateTime.Today.AddDays(-10),
                EndDate = DateTime.Today
            };

            // Setup cache miss and HTTP exception
            object cacheEntry = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cacheEntry)).Returns(false);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m => m.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Throws(new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetHistoricalRates(request));

            // Assert that the exception is correct
            Assert.Equal("Network error", exception.Message);

        }

    }
}
