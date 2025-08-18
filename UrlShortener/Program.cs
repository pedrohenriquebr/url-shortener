using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.CompilerServices;
using Prometheus;
using StackExchange.Redis;
using UrlShortener.Domain.Entities;
using UrlShortener.Infra.BackgroundJobs;
using UrlShortener.Infra.Context;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<UrlShortenerDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// --- Configuração do Redis ---
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
// Registra a conexão com o Redis como um Singleton
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnectionString));

// --- Registra o Background Service ---
builder.Services.AddHostedService<RedisToSqlSyncService>();
var app = builder.Build();
app.UseHttpMetrics(); // Captura automaticamente as métricas dos 4 Sinais de Ouro para HTTP
app.MapMetrics();     // Expõe o endpoint /metrics que o Prometheus vai ler

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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


shortenApi.MapGet("{shortCode}", async (string shortCode, UrlShortenerDbContext dbContext, IConnectionMultiplexer redis) =>
    {
        ShortenedUrl? found = await dbContext.ShortenedUrls.FirstOrDefaultAsync(d => d.ShortCode == shortCode);

        if (found is null)
            return Results.NotFound();
        
        // --- Lógica do Redis ---
        var redisDb = redis.GetDatabase();
        var redisKey = $"access_count:{shortCode}";
        
        // Dispara o comando de incremento e não espera por ele ("fire and forget")
        // A contagem não é crítica para o sucesso do redirecionamento.
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

shortenApi.MapPut("{shortCode}", async (string shortCode, [FromBody] UrlUpdateRequest request, UrlShortenerDbContext dbContext) =>
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


shortenApi.MapGet("{shortCode}/stats", async (string shortCode,  UrlShortenerDbContext dbContext) =>
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


app.Run();

public record UrlRequest(string Url);

public record UrlUpdateRequest(string Url);

public record UrlResponse(
    string Id,
    string Url,
    string ShortCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record UrlStatistics(
    string Id,
    string Url,
    string ShortCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long AccessCount
);