# RatesConverterAPI

## Overview

`RatesConverterAPI` is a backend service that provides currency conversion rates. It allows users to fetch historical currency rates and manage caching for improved performance. This application uses .NET 7, integrates with the Frankfurter API, and employs Onion Architecture for clean separation of concerns.

## Features

- **Fetch Historical Rates**: Retrieve historical currency rates for a given date range.
- **Caching**: Supports caching of responses to improve performance and reduce API calls.
- **Error Handling**: Logs and manages errors gracefully.

## Getting Started

### Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/)
- [Postman](https://www.postman.com/) (optional, for testing endpoints)

### Installation

1. **Clone the repository:**

   ```bash
   git clone https://github.com/sathishkumarsivalingam/ratesconverterapi.git
2. Navigate to the project directory:
   cd ratesconverterapi
3. Restore dependencies:
   dotnet restore
4. Run the application:
   dotnet run
The application will start

Configuration
The application uses settings defined in appsettings.json. Make sure to configure the following:

FrankFurterSettings:
   BaseURL: Base URL for the Frankfurter API.
API Endpoints

1.) Conversion
  EndPoint: api/Conversion/convert
  Description: Converts the amount From Country to To Country
  Parameters:
    amount (number): amount user intend to convert.
    fromCurrency (string): The base currency to convert from.
    toCurrency (string): The currency to convert to.
  
  Example Request:
    {
      "amount": 10,
      "fromCurrency": "EUR",
      "toCurrency": "INR"
    }
  Example Response:
    {"amount":10.0,"base":"EUR","date":"2024-09-13","rates":{"INR":929.88}}
    
2. Get Latest Currency Rates
   Endpoint: GET /api/Currency/latest/{currencyCode}
   Description: Retrieves the latest currency conversion rates for the specified base currency code.
   Path Parameter:
    currencyCode (string): The base currency code for which the latest conversion rates are retrieved. Example: INR.

   Example Request:
    GET /api/Currency/latest/INR
   Example Response:
    {
      "amount": 1.0,
      "base": "INR",
      "date": "2024-09-13",
      "rates": {
        "AUD": 0.01779,
        "BGN": 0.02103,
        "BRL": 0.06683,
        "CAD": 0.0162,
        "CHF": 0.01009,
        "CNY": 0.08456,
        "CZK": 0.27043,
        "DKK": 0.08025,
        "EUR": 0.01075,
        "GBP": 0.00908,
        "HKD": 0.09293,
        "HUF": 4.2513,
        "IDR": 183.59,
        "ILS": 0.0442,
        "ISK": 1.6378,
        "JPY": 1.6794,
        "KRW": 15.8534,
        "MXN": 0.2319,
        "MYR": 0.05126,
        "NOK": 0.12743,
        "NZD": 0.01933,
        "PHP": 0.66718,
        "PLN": 0.04613,
        "RON": 0.05349,
        "SEK": 0.1222,
        "SGD": 0.01549,
        "THB": 0.39776,
        "TRY": 0.40451,
        "USD": 0.01192,
        "ZAR": 0.21205
      }
    }

3. Get Historical Rates
   Endpoint: GET api/HistoricalRates/history
   Description: Fetches historical currency rates for a given date range.
   Parameters:
    baseCurrency (string): The base currency to convert from.
    startDate (string, format: yyyy-MM-dd): The start date for historical rates.
    endDate (string, format: yyyy-MM-dd): The end date for historical rates.
    page (int, optional): Page number for pagination.
    pageSize (int, optional): Number of items per page.

   Sample Request: api/HistoricalRates/history?BaseCurrency=EUR&StartDate=2024-09-01&EndDate=2024-09-15&Page=1&PageSize=5
   Sample Response: {
  "pageNumber": 1,
  "pageSize": 5,
  "totalRecords": 10,
  "totalPages": 2,
  "data": [
            {
              "key": "2024-09-02",
              "value": {
                "AUD": 1.6322,
                "BGN": 1.9558,
                "BRL": 6.2185,
                "CAD": 1.4932,
                "CHF": 0.9415,
                "CNY": 7.8677,
                "CZK": 25.045,
                "DKK": 7.4587,
                "GBP": 0.84218,
                "HKD": 8.6239,
                "HUF": 392.55,
                "IDR": 17190,
                "ILS": 4.0415,
                "INR": 92.81,
                "ISK": 153.1,
                "JPY": 162.56,
                "KRW": 1481.32,
                "MXN": 21.762,
                "MYR": 4.8171,
                "NOK": 11.73,
                "NZD": 1.7767,
                "PHP": 62.513,
                "PLN": 4.275,
                "RON": 4.9753,
                "SEK": 11.351,
                "SGD": 1.4464,
                "THB": 37.834,
                "TRY": 37.581,
                "USD": 1.1061,
                "ZAR": 19.8166
              }
            },
            {
              "key": "2024-09-03",
              "value": {
                "AUD": 1.6394,
                "BGN": 1.9558,
                "BRL": 6.2056,
                "CAD": 1.4951,
                "CHF": 0.9409,
                "CNY": 7.8614,
                "CZK": 25.071,
                "DKK": 7.4595,
                "GBP": 0.84085,
                "HKD": 8.605,
                "HUF": 393.38,
                "IDR": 17171,
                "ILS": 4.0623,
                "INR": 92.67,
                "ISK": 153.5,
                "JPY": 161.26,
                "KRW": 1481.23,
                "MXN": 21.878,
                "MYR": 4.8195,
                "NOK": 11.766,
                "NZD": 1.7839,
                "PHP": 62.427,
                "PLN": 4.2783,
                "RON": 4.9735,
                "SEK": 11.372,
                "SGD": 1.4453,
                "THB": 37.833,
                "TRY": 37.495,
                "USD": 1.1035,
                "ZAR": 19.8345
              }
            },
            {
              "key": "2024-09-04",
              "value": {
                "AUD": 1.645,
                "BGN": 1.9558,
                "BRL": 6.2523,
                "CAD": 1.498,
                "CHF": 0.9396,
                "CNY": 7.8648,
                "CZK": 25.086,
                "DKK": 7.4605,
                "GBP": 0.84248,
                "HKD": 8.6174,
                "HUF": 393.23,
                "IDR": 17126,
                "ILS": 4.1074,
                "INR": 92.8,
                "ISK": 153.5,
                "JPY": 160.26,
                "KRW": 1481.5,
                "MXN": 21.964,
                "MYR": 4.8101,
                "NOK": 11.805,
                "NZD": 1.7861,
                "PHP": 62.502,
                "PLN": 4.2788,
                "RON": 4.9726,
                "SEK": 11.411,
                "SGD": 1.444,
                "THB": 37.785,
                "TRY": 37.623,
                "USD": 1.105,
                "ZAR": 19.7997
              }
            },
            {
              "key": "2024-09-05",
              "value": {
                "AUD": 1.6498,
                "BGN": 1.9558,
                "BRL": 6.2481,
                "CAD": 1.4996,
                "CHF": 0.939,
                "CNY": 7.8739,
                "CZK": 25.027,
                "DKK": 7.4611,
                "GBP": 0.84318,
                "HKD": 8.6493,
                "HUF": 392.3,
                "IDR": 17110,
                "ILS": 4.1011,
                "INR": 93.2,
                "ISK": 153.5,
                "JPY": 159.2,
                "KRW": 1481.54,
                "MXN": 22.279,
                "MYR": 4.8136,
                "NOK": 11.7895,
                "NZD": 1.7874,
                "PHP": 62.231,
                "PLN": 4.2683,
                "RON": 4.9708,
                "SEK": 11.3995,
                "SGD": 1.4444,
                "THB": 37.303,
                "TRY": 37.719,
                "USD": 1.1097,
                "ZAR": 19.736
              }
            },
            {
              "key": "2024-09-06",
              "value": {
                "AUD": 1.6503,
                "BGN": 1.9558,
                "BRL": 6.1855,
                "CAD": 1.4984,
                "CHF": 0.9365,
                "CNY": 7.865,
                "CZK": 25.034,
                "DKK": 7.462,
                "GBP": 0.84293,
                "HKD": 8.6526,
                "HUF": 394.75,
                "IDR": 17114,
                "ILS": 4.116,
                "INR": 93.21,
                "ISK": 153.3,
                "JPY": 158.93,
                "KRW": 1477.3,
                "MXN": 22.151,
                "MYR": 4.8082,
                "NOK": 11.8015,
                "NZD": 1.7858,
                "PHP": 62.118,
                "PLN": 4.28,
                "RON": 4.9735,
                "SEK": 11.3698,
                "SGD": 1.4428,
                "THB": 37.228,
                "TRY": 37.739,
                "USD": 1.1103,
                "ZAR": 19.6875
              }
            }
          ]
        }
