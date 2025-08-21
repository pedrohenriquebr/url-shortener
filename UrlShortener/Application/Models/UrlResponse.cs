namespace UrlShortener.Application.Models;

public record UrlResponse(
    string Id,
    string Url,
    string ShortCode,
    DateTime CreatedAt,
    DateTime UpdatedAt);