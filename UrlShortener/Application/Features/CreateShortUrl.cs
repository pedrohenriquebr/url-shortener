using Carter;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Models;
using UrlShortener.Domain.Factories;

namespace UrlShortener.Application.Features;

public record CreateShortRequest(string Url);

public class CreateShortUrlModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            [FromBody] CreateShortRequest request,
            IShortenedFactory factory,
            IShortenedUrlRepository repository,
            IUnitOfWork uow
        ) =>
        {
            if (await repository.AlreadyExistsAsync(request.Url)) return Results.BadRequest("URL already shortened.");

            Console.WriteLine($"Received URL: {request.Url}");
            var newUrl = factory.Create(request.Url);
            await repository.AddAsync(newUrl);
            await uow.SaveChangesAsync();


            return Results.CreatedAtRoute("GetUrlShortened",
                new { shortCode = newUrl.ShortCode },
                (UrlResponse)newUrl
            );
        });
    }
}