using UrlShortener.Application.Interfaces;
using UrlShortener.Infra.Context;

namespace UrlShortener.Infra;

public class UnitOfWork : IUnitOfWork
{
    private readonly UrlShortenerDbContext _dbContext;

    public UnitOfWork(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}