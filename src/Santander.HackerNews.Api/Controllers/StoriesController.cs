using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Santander.HackerNews.Api.Controllers;

public class StoriesController : ControllerBase
{
    private readonly IMemoryCache _cache;

    public StoriesController(IMemoryCache cache)
    {
        _cache = cache;
    }
}