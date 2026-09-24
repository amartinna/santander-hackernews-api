using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Santander.HackerNews.Api.Models;
using Santander.HackerNews.Api.Services;

namespace Santander.HackerNews.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IMemoryCache _cache;

    public StoriesController(IMemoryCache cache)
    {
        _cache = cache;
    }

    [HttpGet("best")]
    public IActionResult GetBestStories([FromQuery] int n)
    {
        if (n <= 0)
        {
            return BadRequest("El parámetro de búsqueda 'n' debe ser un número entero mayor que cero.");
        }

        if (_cache.TryGetValue(HackerNewsCacheRefresher.CacheKey, out List<StoryDto>? stories) && stories != null)
        {
            var output = stories.Take(n);
            return Ok(output);
        }

        return StatusCode(503, "La infraestructura de datos se está inicializando. Por favor, reintente en unos instantes.");
    }
}