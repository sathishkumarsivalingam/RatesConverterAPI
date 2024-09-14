using Microsoft.AspNetCore.Mvc;
using RatesConverterAPI.Core.Interface;
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RatesConverterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyService currencyService, ILogger<CurrencyController> logger)
        {
            _currencyService = currencyService;
            _logger = logger;
        }

        [HttpGet("latest/{baseCurrency}")]
        public async Task<IActionResult> GetLatestRates(string baseCurrency)
        {
            try
            {
                if(string.IsNullOrEmpty(baseCurrency))
                {
                    return BadRequest("Invalid input.");
                }

                var response = await _currencyService.GetLatestRates(baseCurrency);

                if (response == null)
                    return BadRequest("Exchange rates not found");

                if (response.Contains("Rate limit exceeded"))
                    return StatusCode(429, response);
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request for base currency: {BaseCurrency}", baseCurrency);
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
