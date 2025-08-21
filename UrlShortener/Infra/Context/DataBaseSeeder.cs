using Bogus;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infra.Context;

public static class DataBaseSeeder
{
    public static List<ShortenedUrl> GenerateShortenedUrls()
    {
        return new Faker<ShortenedUrl>()
            .RuleFor(x => x.LongUrl, f => f.Internet.Url())
            .RuleFor(x => x.CreatedAt, f => f.Date.Recent())
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent())
            .Generate(2_000_000);
    }
}