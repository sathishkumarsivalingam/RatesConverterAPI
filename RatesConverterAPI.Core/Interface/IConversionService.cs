using RatesConverterAPI.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatesConverterAPI.Core.Interface
{
    public interface IConversionService
    {
        Task<string> GetLatestRates(ConversionRequest conversionRequest);
    }
}
