namespace UrlShortener.Domain.Entities;

public class ShortenedUrl
{
    public long Id { get; set; }
    public string LongUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public long AccessCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}