using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IShortenedUrlRepository
{
    Task<bool> AlreadyExistsAsync(string longUrl, CancellationToken cancellationToken = default);
    Task<ShortenedUrl?> FirstOrDefaultAsync(string shortCode);
    Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default);
    void Remove(ShortenedUrl shortenedUrl);
}