using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RatesConverterAPI.Core.Configuration;
using RatesConverterAPI.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatesConverterAPI.Application.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CurrencyService> _logger;
        private readonly FrankFurterSettings _settings;

        public CurrencyService(IMemoryCache cache, HttpClient httpClient, ILogger<CurrencyService> logger, IOptions<FrankFurterSettings> settings)
        {
            _cache = cache;
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task<string> GetLatestRates(string baseCurrency)
        {
            string cacheKey = $"LatestRates-{baseCurrency}";
            if (_cache.TryGetValue(cacheKey.ToLower(), out string cachedRates))
            {
                return cachedRates;
            }

            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseURL}{_settings.LatestBaseUrl}?base={baseCurrency}");

                if (response.IsSuccessStatusCode)
                {
                    var rates = await response.Content.ReadAsStringAsync();
                    _cache.Set(cacheKey.ToLower(), rates, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
                    return rates;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning("Rate limit exceeded for base currency: {BaseCurrency}", baseCurrency);
                    return $"Rate limit exceeded for base currency: {baseCurrency}";
                    // Handle rate limit exceeded case (e.g., return a specific message or retry later)
                }

                _logger.LogError("Failed to fetch latest rates for base currency: {BaseCurrency}, Status Code: {StatusCode}", baseCurrency, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while fetching latest rates for base currency: {BaseCurrency}", baseCurrency);
                throw; // Consider whether to rethrow or return a specific error response
            }
        }
    }


}
