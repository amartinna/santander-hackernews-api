using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Santander.HackerNews.Api.Controllers;
using Santander.HackerNews.Api.Models;
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
    public void GetBestStories_ReturnsBadRequest_WhenParamIsZeroOrNegative()
    {
        var result = _controller.GetBestStories(0);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("El parámetro de búsqueda 'n' debe ser un número entero mayor que cero.", badRequestResult.Value);
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

    [Fact]
    public void GetBestStories_ReturnsCorrectCount_WhenDataExistsInCache()
    {
        var fakeStories = new List<StoryDto>
        {
            new() { Title = "Story 1", Score = 100 },
            new() { Title = "Story 2", Score = 90 },
            new() { Title = "Story 3", Score = 80 }
        };

        object cachedValue = fakeStories;
        _cacheMock.Setup(x => x.TryGetValue(HackerNewsCacheRefresher.CacheKey, out cachedValue)).Returns(true);

        var result = _controller.GetBestStories(2);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedStories = Assert.IsAssignableFrom<IEnumerable<StoryDto>>(okResult.Value);
        Assert.Equal(2, returnedStories.Count());
    }
    
}