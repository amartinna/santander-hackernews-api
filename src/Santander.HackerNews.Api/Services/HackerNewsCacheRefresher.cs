using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Santander.HackerNews.Api.Models;

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

                if (ids != null && ids.Count > 0)
                {
                    var targetIds = ids.Take(150);
                    var tasks = targetIds.Select(id => FetchStoryAsync(client, id, stoppingToken));
                    var results = await Task.WhenAll(tasks);

                    var processedStories = results
                        .Where(s => s != null)
                        .OrderByDescending(s => s!.Score)
                        .ToList();

                    _cache.Set(CacheKey, processedStories, TimeSpan.FromMinutes(10));
                    _logger.LogInformation("Métricas de Hacker News actualizadas correctamente en memoria caché.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico durante la sincronización asíncrona de la API externa.");
            }

            // Esperar 60s para refrescar
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
        }
        
    }

    private async Task<StoryDto?> FetchStoryAsync(HttpClient client, int id, CancellationToken token)
    {
        try
        {
            var response = await client.GetStringAsync($"https://firebaseio.com{id}.json", token);
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;

            return new StoryDto
            {
                Title = root.GetProperty("title").GetString() ?? string.Empty,
                Uri = root.TryGetProperty("url", out var urlProp) ? urlProp.GetString() ?? string.Empty : string.Empty,
                PostedBy = root.GetProperty("by").GetString() ?? string.Empty,
                Time = DateTimeOffset.FromUnixTimeSeconds(root.GetProperty("time").GetInt64()).UtcDateTime,
                Score = root.GetProperty("score").GetInt32(),
                CommentCount = root.TryGetProperty("descendants", out var descProp) ? descProp.GetInt32() : 0
            };
        }
        catch
        {
            return null; // Si falla un elemento de forma aislada, el flujo general continúa de forma elástica
        }
    }
}