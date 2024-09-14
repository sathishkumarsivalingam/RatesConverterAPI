using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatesConverterAPI.Application.DTOs
{
    public class BaseRatesResponse
    {
        public decimal Amount { get; set; }
        public string Base { get; set; }
        //public Dictionary<string, decimal> Rates { get; set; }
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }

    }
}
