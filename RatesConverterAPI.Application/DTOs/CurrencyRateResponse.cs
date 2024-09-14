using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatesConverterAPI.Application.DTOs
{
    public class CurrencyRateResponse : BaseRatesResponse
    {       // Maps to "base"
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
