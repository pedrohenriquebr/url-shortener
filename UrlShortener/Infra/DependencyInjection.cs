using Carter;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Factories;
using UrlShortener.Infra.BackgroundJobs;
using UrlShortener.Infra.Context;
using UrlShortener.Infra.Repositories;

namespace UrlShortener.Infra;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddBackgroundJobs(this WebApplicationBuilder builder)
    {
        builder.Services.AddHostedService<RedisToSqlSyncService>();

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
            options.UseSqlServer(connectionString));

        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");

        if (redisConnectionString != null)
            builder.Services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString));


        return builder;
    }

    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Startup>());
        builder.Services.AddCarter();

        //factories
        builder.Services.AddScoped<IShortenedFactory, ShortenedUrlFactory>();
        builder.Services.AddScoped<IShortenedUrlRepository, ShortenedUrlRepository>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        return builder;
    }
}