using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using StackExchange.Redis;
using UrlShortener.Application.Models;
using UrlShortener.Domain.Entities;
using UrlShortener.Infra.Configurations;
using UrlShortener.Infra.Context;

namespace UrlShortener.Application;

public static class Extensions
{
    public static WebApplication UseObservabilityServices(this WebApplication app)
    {
        app.UseHttpMetrics();
        app.MapMetrics();
        return app;
    }

    public static WebApplication UseEntryPoint(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            var appSettings = app.Services.GetRequiredService<ApplicationSettings>();

            if (appSettings.SeedDatabase)
            {
                app.SeedDatabase();
            }
        }

        return app;
    }


    public static WebApplication SeedDatabase(this WebApplication builder)
    {
        var services = builder.Services;
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UrlShortenerDbContext>();

        if (context.ShortenedUrls.Any())
            return builder;

        var generateShortenedUrls = DataBaseSeeder.GenerateShortenedUrls();
        context.ShortenedUrls.AddRange(generateShortenedUrls);
        generateShortenedUrls.ForEach(d => d.ShortCode = Base62Converter.Encode(d.Id));
        context.SaveChanges();

        return builder;
    }


    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var shortenApi = app.MapGroup("/shorten");

        shortenApi.MapPost("", async ([FromBody] UrlRequest request, UrlShortenerDbContext dbContext) =>
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


        shortenApi.MapGet("{shortCode}",
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

        shortenApi.MapPut("{shortCode}",
            async (string shortCode, [FromBody] UrlUpdateRequest request, UrlShortenerDbContext dbContext) =>
            {
                ShortenedUrl? found = await dbContext.ShortenedUrls.FirstOrDefaultAsync(d => d.ShortCode == shortCode);

                if (found is null)
                    return Results.NotFound();

                found.LongUrl = request.Url;
                found.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                return Results.Ok(new UrlResponse(
                    Id: found.Id.ToString(),
                    Url: found.LongUrl,
                    CreatedAt: found.CreatedAt,
                    UpdatedAt: found.UpdatedAt,
                    ShortCode: found.ShortCode
                ));
            });

        shortenApi.MapDelete("{shortCode}", async (string shortCode, UrlShortenerDbContext dbContext) =>
        {
            ShortenedUrl? found = await dbContext.ShortenedUrls.FirstOrDefaultAsync(d => d.ShortCode == shortCode);

            if (found is null)
                return Results.NotFound();

            dbContext.Remove(found);
            return Results.NoContent();
        });


        shortenApi.MapGet("{shortCode}/stats", async (string shortCode, UrlShortenerDbContext dbContext) =>
        {
            ShortenedUrl? found = await dbContext.ShortenedUrls.FirstOrDefaultAsync(d => d.ShortCode == shortCode);

            if (found is null)
                return Results.NotFound();

            return Results.Ok(new UrlStatistics(
                Id: found.Id.ToString(),
                Url: found.LongUrl,
                CreatedAt: found.CreatedAt,
                UpdatedAt: found.UpdatedAt,
                ShortCode: found.ShortCode,
                AccessCount: found.AccessCount
            ));
        });

        return app;
    }
}