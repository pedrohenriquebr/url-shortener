using Carter;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Application.Features;

public class DeleteShortUrl : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/{shortCode}",
            async (string shortCode,
                IShortenedUrlRepository repository) =>
            {
                var found = await repository.FirstOrDefaultAsync(shortCode);

                if (found is null)
                    return Results.NotFound();

                repository.Remove(found);
                return Results.NoContent();
            });
    }
}