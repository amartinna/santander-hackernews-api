using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Santander.HackerNews.Api.Controllers;
using Santander.HackerNews.Api.Services;

namespace Santander.HackerNews.UnitTests;

public class StoriesControllerTests
{
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly StoriesController _controller;

    public StoriesControllerTests()
    {
        _cacheMock = new Mock<IMemoryCache>();
        _controller = new StoriesController(_cacheMock.Object);
    }

    [Fact]
    public void GetBestStories_ReturnsServiceUnavailable_WhenCacheIsEmpty()
    {
        object? cachedValue = null;
        _cacheMock.Setup(x => x.TryGetValue(HackerNewsCacheRefresher.CacheKey, out cachedValue)).Returns(false);

        var result = _controller.GetBestStories(5);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, statusCodeResult.StatusCode);
    }
    
}