using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Infra.Context;

namespace UrlShortener.Infra.Repositories;

public class ShortenedUrlRepository : IShortenedUrlRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public ShortenedUrlRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> AlreadyExistsAsync(string longUrl, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ShortenedUrls.AnyAsync(d => d.LongUrl == longUrl, cancellationToken);
    }

    public async Task<ShortenedUrl?> FirstOrDefaultAsync(string shortCode)
    {
        return await _dbContext.ShortenedUrls.FirstOrDefaultAsync(d => d.ShortCode == shortCode);
    }

    public async Task AddAsync(ShortenedUrl shortenedUrl, CancellationToken cancellationToken = default)
    {
        await _dbContext.ShortenedUrls.AddAsync(shortenedUrl, cancellationToken);
        shortenedUrl.ShortCode = Base62Converter.Encode(shortenedUrl.Id);
    }

    public void Remove(ShortenedUrl shortenedUrl)
    {
        _dbContext.Remove(shortenedUrl);
    }
}