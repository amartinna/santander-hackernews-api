using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace Santander.HackerNews.Api.Services;

public class HackerNewsCacheRefresher : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HackerNewsCacheRefresher> _logger;
    public const string CacheKey = "HackerNews_BestStories";

    public HackerNewsCacheRefresher(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<HackerNewsCacheRefresher> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                
                var idsJson = await client.GetStringAsync("https://firebaseio.com", stoppingToken);
                var ids = JsonSerializer.Deserialize<List<int>>(idsJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la sincronización asíncrona de la API externa.");
            }

            // Esperar 60s para refrescar
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
    }
}