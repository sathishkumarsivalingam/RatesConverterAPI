
using FluentValidation;
using RatesConverterAPI.Core.Entity;

namespace RatesConverterAPI.Core.Validator
{

    public class HistoricalRatesRequestValidator : AbstractValidator<HistoricalRatesRequest>
    {
        public HistoricalRatesRequestValidator()
        {
            RuleFor(x => x.BaseCurrency)
                .NotEmpty()
                .Length(3, 3); // Assuming 3-letter currency codes

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("StartDate must be less than EndDate");

            RuleFor(x => x.Page)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100); // Example maximum page size
        }
    }

}
