using bancoSol.DTOs;
using bancoSol.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace bancoSol.Services.Implementations
{
    public class ExchangeRateService : IExchangeRateService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ExchangeRateService> _logger;
        private const string CacheKey = "exchange_rate_USD_BOB";
        private const decimal FallbackRate = 6.94m;

        public ExchangeRateService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<ExchangeRateService> logger) 
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ExchangeRateResponse> GetRateAsync()
        {
            if (_cache.TryGetValue(CacheKey, out ExchangeRateResponse? cached) && cached != null)
            {
                _logger.LogInformation("Tipo de cambio obtenido desde caché | Rate: {Rate}", cached.Rate);
                return cached;
            }

            try
            {
                var response = await _httpClient.GetFromJsonAsync<HexaRateResponse>("/api/rates/USD/BOB/latest"); 

                var rate = new ExchangeRateResponse
                {
                    From = "USD",
                    To = "BOB",
                    Rate = response?.Data?.Mid ?? FallbackRate,
                    IsFallback = false, 
                    FetchedAt = DateTime.UtcNow
                };

                _cache.Set(CacheKey, rate, TimeSpan.FromMinutes(30));

                _logger.LogInformation(
                    "Tipo de cambio obtenido desde API | Rate: {Rate}",
                    rate.Rate);

                return rate;
            }
            catch (Exception ex)
            {
               
                _logger.LogWarning(ex,
                    "API de tipo de cambio no disponible, usando tasa de fallback: {Rate}",
                    FallbackRate);

           
                var fallback = new ExchangeRateResponse
                {
                    From = "USD",
                    To = "BOB",
                    Rate = FallbackRate,
                    IsFallback = true, 
                    FetchedAt = DateTime.UtcNow
                };

                _cache.Set(CacheKey, fallback, TimeSpan.FromMinutes(5)); 

                return fallback;
            }
        }
    }

    public class HexaRateResponse
    {
        public HexaRateData? Data { get; set; }
    }

    public class HexaRateData
    {
        public decimal Mid { get; set; }
    }
}