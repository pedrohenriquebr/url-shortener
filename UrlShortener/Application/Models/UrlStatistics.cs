namespace UrlShortener.Application.Models;

public record UrlStatistics(
    string Id,
    string Url,
    string ShortCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long AccessCount
);