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
                _logger.LogInformation("Refreshing Hacker News cache.");
            }
            catch (Exception ex) when (stoppingToken.IsCancellationRequested == false)
            {
                _logger.LogError(ex, "Error refreshing Hacker News cache.");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}