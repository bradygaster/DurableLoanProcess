using System;
using System.Threading.Tasks;
using DurableLoans.DomainModel;
using DurableLoans.ExchangeRateService;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DurableLoans.Web.Services
{
    public class CurrencyConversionService
    {
        private readonly IConfiguration _configuration;

        public ILogger<CurrencyConversionService> Logger { get; }

        public CurrencyConversionService(IConfiguration configuration,
            ILogger<CurrencyConversionService> logger)
        {
            _configuration = configuration;
            Logger = logger;
        }

        private async Task<double> GetExchangeRateAsync(Currency currencyTypeFrom, Currency currencyTypeTo)
        {
            var grpcUrl = _configuration["ExchangeRateService:BaseAddress"];
            using var channel = GrpcChannel.ForAddress(grpcUrl);
            Logger.LogInformation($"gRPC URL: {grpcUrl}");
            
            var client = new ExchangeRateManager.ExchangeRateManagerClient(channel);
            var request = new ExchangeRateRequest
            {
                CurrencyTypeFrom = currencyTypeFrom.ToAlias(),
                CurrencyTypeTo = currencyTypeTo.ToAlias(),
            };

            ExchangeRateReply exchangeRate = await client.GetExchangeRateAsync(request);

            return exchangeRate.ExchangeRate;
        }

        public async Task<decimal> GetConvertedAmountAsync(CurrencyConversion conversion)
        {
            decimal exchangeRate = Convert.ToDecimal(await GetExchangeRateAsync(
                conversion.CurrencyTypeFrom, conversion.CurrencyTypeTo));

            decimal convertedAmount = conversion.CurrencyTypeTo switch
            {
                Currency.BulgarianLev => decimal.Round(conversion.AmountToConvert / exchangeRate, 2),
                _ => decimal.Round(conversion.AmountToConvert * exchangeRate, 2),
            };

            return convertedAmount;
        }

        public Currency GetCurrencyEnumValueFromSymbol(string currencyType) =>
            currencyType switch
            {
                Constants.BulgarianLevSymbol    => Currency.BulgarianLev,
                Constants.UsDollarSymbol        => Currency.USDollar,
                _                               => throw new ArgumentException(message: "invalid currency type", paramName: nameof(currencyType)),
            };
    }
}
