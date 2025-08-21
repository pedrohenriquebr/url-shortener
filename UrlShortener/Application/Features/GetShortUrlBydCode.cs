using Carter;
using StackExchange.Redis;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Models;

namespace UrlShortener.Application.Features;

public class GetShortUrlByCode : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{shortCode}",
                async (string shortCode, IShortenedUrlRepository repository, IConnectionMultiplexer redis) =>
                {
                    var found =
                        await repository.FirstOrDefaultAsync(shortCode);

                    if (found is null)
                        return Results.NotFound();


                    var redisDb = redis.GetDatabase();
                    var redisKey = $"access_count:{shortCode}";


                    _ = redisDb.StringIncrementAsync(redisKey);

                    return Results.Ok((UrlResponse)found);
                })
            .WithName("GetUrlShortened");
    }
}