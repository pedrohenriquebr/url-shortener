using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Models;

public record UrlResponse(
    string Id,
    string Url,
    string ShortCode,
    DateTime CreatedAt,
    DateTime UpdatedAt)
{
    public static implicit operator UrlResponse(ShortenedUrl newUrl)
    {
        return new UrlResponse(
            newUrl.Id.ToString(),
            newUrl.LongUrl,
            newUrl.ShortCode,
            newUrl.CreatedAt,
            newUrl.UpdatedAt
        );
    }
}