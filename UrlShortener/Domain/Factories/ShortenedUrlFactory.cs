using UrlShortener.Domain.Entities;

namespace UrlShortener.Domain.Factories;

public interface IShortenedFactory
{
    ShortenedUrl Create(string url);
}

public class ShortenedUrlFactory : IShortenedFactory
{
    public ShortenedUrl Create(string url)
    {
        var newUrl = new ShortenedUrl
        {
            LongUrl = url,
            ShortCode = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return newUrl;
    }
}