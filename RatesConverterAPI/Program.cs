using AspNetCoreRateLimit;
using RatesConverterAPI.Application.Services;
using RatesConverterAPI.Core.Interface;
using Polly;
using FluentValidation.AspNetCore;
using RatesConverterAPI.Core.Validator;
using RatesConverterAPI.Core.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);

builder.Services.Configure<FrankFurterSettings>(builder.Configuration.GetSection("FrankFurter"));

// Add services to the container.
//builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});
//builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<HistoricalRatesRequestValidator>());
// Add FluentValidation to the Controllers
builder.Services.AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<HistoricalRatesRequestValidator>());

// Load IP rate limiting configurations from appsettings.json
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Register your services
builder.Services.AddSingleton<ICurrencyService, CurrencyService>();
builder.Services.AddSingleton<IConversionService, ConversionService>();
builder.Services.AddSingleton<IHistoricalRatesService, HistoricalRatesService>();

// Adding Polly for retry policies
builder.Services.AddHttpClient<ICurrencyService, CurrencyService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt)));
builder.Services.AddHttpClient<IConversionService, ConversionService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt)));
builder.Services.AddHttpClient<IHistoricalRatesService, HistoricalRatesService>()
    .AddTransientHttpErrorPolicy(policyBuilder =>
        policyBuilder.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt)));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIpRateLimiting(); // Enables IP rate limiting middleware

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
