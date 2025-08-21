namespace UrlShortener.Application.Interfaces;

public interface IUnitOfWork
{
    public Task SaveChangesAsync();
}