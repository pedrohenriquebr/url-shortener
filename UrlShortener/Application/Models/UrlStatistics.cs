using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Models;

public record UrlStatistics(
    string Id,
    string Url,
    string ShortCode,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    long AccessCount
)
{
    public static implicit operator UrlStatistics(ShortenedUrl found)
    {
        return new UrlStatistics(
            found.Id.ToString(),
            found.LongUrl,
            CreatedAt: found.CreatedAt,
            UpdatedAt: found.UpdatedAt,
            ShortCode: found.ShortCode,
            AccessCount: found.AccessCount
        );
    }
}