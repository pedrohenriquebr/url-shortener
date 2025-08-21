using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Infra.BackgroundJobs;
using UrlShortener.Infra.Context;

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
        return builder;
    }
}