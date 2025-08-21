using Carter;
using Prometheus;
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

            if (appSettings.SeedDatabase) app.SeedDatabase();
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
        app.MapGroup("/shorten")
            .MapCarter();

        return app;
    }
}