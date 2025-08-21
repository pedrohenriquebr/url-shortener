using Carter;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Models;

namespace UrlShortener.Application.Features;

public class UpdateShortUrl : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/{shortCode}",
            async (string shortCode, [FromBody] UpdateShortUrlRequest request,
                IShortenedUrlRepository repository,
                IUnitOfWork uow) =>
            {
                var found = await repository.FirstOrDefaultAsync(shortCode);

                if (found is null)
                    return Results.NotFound();

                found.LongUrl = request.Url;
                found.UpdatedAt = DateTime.UtcNow;
                await uow.SaveChangesAsync();

                return Results.Ok((UrlResponse)found);
            });
    }
}