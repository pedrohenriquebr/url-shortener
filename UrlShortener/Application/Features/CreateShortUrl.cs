using MediatR;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Models;
using UrlShortener.Domain.Entities;
using UrlShortener.Infra.Context;

using Carter;
using Microsoft.AspNetCore.Mvc;

namespace UrlShortener.Application.UseCases;


public record CreateShortRequest(string Url);
public class CreateShortUrlModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/shorten", async ([FromBody] CreateShortRequest request, UrlShortenerDbContext dbContext) =>
        {
            if (await dbContext.ShortenedUrls.AnyAsync(d => d.LongUrl == request.Url))
            {
                return Results.BadRequest("URL already shortened.");
            }

            Console.WriteLine($"URL Recebida: {request.Url}");
            var newUrl = new ShortenedUrl()
            {
                LongUrl = request.Url,
                ShortCode = "",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await dbContext.ShortenedUrls.AddAsync(newUrl);
            newUrl.ShortCode = Base62Converter.Encode(newUrl.Id);

            await dbContext.SaveChangesAsync();


            return Results.CreatedAtRoute("GetUrlShortened",
                new { shortCode = newUrl.ShortCode },
                new UrlResponse(
                    newUrl.Id.ToString(),
                    newUrl.LongUrl,
                    newUrl.ShortCode,
                    newUrl.CreatedAt,
                    newUrl.UpdatedAt
                )
            );
        });
    }
}