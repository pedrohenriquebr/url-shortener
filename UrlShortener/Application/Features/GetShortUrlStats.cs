using Carter;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Models;

namespace UrlShortener.Application.Features;

public class GetShortUrlStats : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{shortCode}/stats", async (string shortCode, IShortenedUrlRepository repository) =>
        {
            var found = await repository.FirstOrDefaultAsync(shortCode);

            if (found is null)
                return Results.NotFound();

            return Results.Ok((UrlStatistics)found);
        });
    }
}