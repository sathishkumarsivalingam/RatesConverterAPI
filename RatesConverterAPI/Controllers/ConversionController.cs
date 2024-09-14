using Microsoft.AspNetCore.Mvc;
using RatesConverterAPI.Core.Entity;
using RatesConverterAPI.Core.Interface;
using System.Net.Http;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RatesConverterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConversionController : ControllerBase
    {
        private readonly IConversionService conversionService;

        public ConversionController(IConversionService conversionService)
        {
            this.conversionService = conversionService;
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertCurrency([FromBody] ConversionRequest request)
        {
            string[] restrictedCurrencies = { "TRY", "PLN", "THB", "MXN" };
            if (restrictedCurrencies.Contains(request.ToCurrency) || restrictedCurrencies.Contains(request.FromCurrency))
            {
                return BadRequest("Conversion for this currency is not allowed.");
            }

            try
            {
                var response = await conversionService.GetLatestRates(request);
                if (response == null)
                    return BadRequest("Conversion not available");

                if (response.Contains("Rate limit exceeded"))
                    return StatusCode(429, response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
