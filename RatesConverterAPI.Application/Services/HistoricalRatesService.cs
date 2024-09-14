using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using RatesConverterAPI.Application.DTOs;
using RatesConverterAPI.Application.Helper;
using RatesConverterAPI.Core.Configuration;
using RatesConverterAPI.Core.Entity;
using RatesConverterAPI.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RatesConverterAPI.Application.Services
{
    public class HistoricalRatesService : IHistoricalRatesService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly ILogger<HistoricalRatesService> _logger;
        private readonly FrankFurterSettings _settings;

        public HistoricalRatesService(IMemoryCache cache, HttpClient httpClient, ILogger<HistoricalRatesService> logger, IOptions<FrankFurterSettings> settings)
        {
            _cache = cache;
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;

        }

        public async Task<object> GetHistoricalRates(HistoricalRatesRequest historicalRatesRequest)
        {
            // Generate a cache key based on the request parameters
            string cacheKey = $"HistoricalRates-{historicalRatesRequest.BaseCurrency}-{historicalRatesRequest.StartDate:yyyy-MM-dd}-{historicalRatesRequest.EndDate:yyyy-MM-dd}";

            // Try to get data from cache
            if (_cache.TryGetValue(cacheKey, out CurrencyRateResponse cachedRates))
            {
                _logger.LogInformation("Returning cached historical rates for base currency: {BaseCurrency}", historicalRatesRequest.BaseCurrency);

                // Implement pagination logic on cached data
                var pagedData = PaginationHelper.PaginateData(cachedRates.Rates, historicalRatesRequest.Page, historicalRatesRequest.PageSize);
                return pagedData;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseURL}{historicalRatesRequest.StartDate:yyyy-MM-dd}..{historicalRatesRequest.EndDate:yyyy-MM-dd}?base={historicalRatesRequest.BaseCurrency}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning($"Rate limit exceeded for historic rates: {historicalRatesRequest.BaseCurrency}");
                    return $"Rate limit exceeded for historic rates: {historicalRatesRequest.BaseCurrency}";
                    // Handle rate limit exceeded case (e.g., return a specific message or retry later)
                }

                if (response.IsSuccessStatusCode)
                {
                    string stringResponse = await response.Content.ReadAsStringAsync();
                    // Deserialize the JSON response
                    var result = JsonConvert.DeserializeObject<CurrencyRateResponse>(stringResponse);
                    // Manually assign StartDate and EndDate if they are not present in the JSON response
                    result.StartDate = historicalRatesRequest.StartDate;
                    result.EndDate = historicalRatesRequest.EndDate;
                    // Cache the full data for future requests
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30)); // Cache for 30 minutes
                    _logger.LogInformation("Cached historical rates for base currency: {BaseCurrency}", historicalRatesRequest.BaseCurrency);

                    // Implement pagination logic on fresh data
                    var pagedData = PaginationHelper.PaginateData(result.Rates, historicalRatesRequest.Page, historicalRatesRequest.PageSize);
                    return pagedData;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch historical rates for base currency: {BaseCurrency}, Status Code: {StatusCode}", historicalRatesRequest.BaseCurrency, response.StatusCode);
                    throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error retrieving historical rates for base currency: {BaseCurrency}", historicalRatesRequest.BaseCurrency);
                throw; // Rethrow the exception to be handled by the controller
            }
        }
    }


}
