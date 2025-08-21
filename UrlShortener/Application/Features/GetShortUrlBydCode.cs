using Carter;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Application.Models;
using UrlShortener.Domain.Entities;
using UrlShortener.Infra.Context;

namespace UrlShortener.Application.UseCases;

public class GetShortUrlBydCode : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("{shortCode}",
                async (string shortCode, UrlShortenerDbContext dbContext, IConnectionMultiplexer redis) =>
                {
                    ShortenedUrl? found =
                        await dbContext.ShortenedUrls.FirstOrDefaultAsync(d => d.ShortCode == shortCode);

                    if (found is null)
                        return Results.NotFound();


                    var redisDb = redis.GetDatabase();
                    var redisKey = $"access_count:{shortCode}";


                    _ = redisDb.StringIncrementAsync(redisKey);

                    return Results.Ok(new UrlResponse(
                        Id: found.Id.ToString(),
                        Url: found.LongUrl,
                        ShortCode: found.ShortCode,
                        CreatedAt: found.CreatedAt,
                        UpdatedAt: found.UpdatedAt
                    ));
                })
            .WithName("GetUrlShortened");
    }
}