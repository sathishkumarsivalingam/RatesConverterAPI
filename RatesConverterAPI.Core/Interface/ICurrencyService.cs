using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatesConverterAPI.Core.Interface
{
    public interface ICurrencyService
    {
        Task<string> GetLatestRates(string baseCurrency);
    }
}
