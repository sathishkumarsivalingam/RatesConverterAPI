using Microsoft.AspNetCore.Mvc;
using Polly;
using RatesConverterAPI.Core.Entity;
using RatesConverterAPI.Core.Interface;
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RatesConverterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricalRatesController : ControllerBase
    {
        private readonly IHistoricalRatesService _historicalRatesService;
        private readonly ILogger<HistoricalRatesController> _logger; // Updated logger type

        public HistoricalRatesController(IHistoricalRatesService historicalRatesService, ILogger<HistoricalRatesController> logger)
        {
            _historicalRatesService = historicalRatesService;
            _logger = logger;
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistoricalRates([FromQuery] HistoricalRatesRequest historicalRatesRequest)
        {
            // Validate the request model
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var response = await _historicalRatesService.GetHistoricalRates(historicalRatesRequest);
               
                return Ok(response);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching historical rates from external API.");
                return StatusCode(503, "External API error. Please try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal server error occurred while processing the request.");
                return StatusCode(500, "Internal server error.");
            }
        }
    }

}
