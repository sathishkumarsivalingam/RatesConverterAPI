using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    public class ConversionService : IConversionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ConversionService> _logger;
        private readonly FrankFurterSettings _settings;

        public ConversionService(HttpClient httpClient, ILogger<ConversionService> logger, IOptions<FrankFurterSettings> settings)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;
        }
        public async Task<string> GetLatestRates(ConversionRequest conversionRequest)
        {

            try
            {
                var response = await _httpClient.GetAsync($"{_settings.BaseURL}{_settings.LatestBaseUrl}?amount={conversionRequest.Amount}&from={conversionRequest.FromCurrency}&to={conversionRequest.ToCurrency}");

                if (response.IsSuccessStatusCode)
                {
                    var rates = await response.Content.ReadAsStringAsync();
                    return rates;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    _logger.LogWarning($"Rate limit exceeded for conversion api with Amount, From Currency and To currency: {conversionRequest.Amount},{conversionRequest.FromCurrency},{conversionRequest.ToCurrency}");
                    return $"Rate limit exceeded for conversion api with Amount, From Currency and To currency: {conversionRequest.Amount},{conversionRequest.FromCurrency},{conversionRequest.ToCurrency}";
                    // Handle rate limit exceeded case (e.g., return a specific message or retry later)
                }
                // Log the error if response is not successful
                _logger.LogError($"Failed to fetch latest rates for base currency: {conversionRequest.FromCurrency}, Status Code: {response.StatusCode}");
                return null;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching the latest rates.");
                return null;
            }
        }
    }
}
