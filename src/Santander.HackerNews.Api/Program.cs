using Santander.HackerNews.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddHostedService<HackerNewsCacheRefresher>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();

app.Run();