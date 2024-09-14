using RatesConverterAPI.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RatesConverterAPI.Application.Helper
{
    public class PaginationHelper
    {
        public static PaginatedResult<T> PaginateData<T>(IEnumerable<T> sourceData, int pageNumber, int pageSize)
        {
            // Ensure valid page number and page size
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }
            if (pageSize < 1)
            {
                pageSize = 10; // Default page size
            }

            // Get total record count
            int totalRecords = sourceData.Count();

            // Skip and take to get the data for the current page
            List<T> pagedData = sourceData
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Return paginated result
            return new PaginatedResult<T>(pagedData, totalRecords, pageNumber, pageSize);
        }
    }
}
