using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infra.Context;

public class UrlShortenerDbContext : DbContext
{
    
    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : base(options)
    {
    }

   
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    // public DbSet<ShortenedUrlStats> ShortenedUrlsStats { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
       
        modelBuilder.HasSequence("ShortenedUrl_HiLoSequence")
            .IncrementsBy(10); 
        
        modelBuilder.Entity<ShortenedUrl>(builder =>
        {
           
            builder.ToTable("ShortenedUrls");

           
            builder.HasKey(e => e.Id);

            
            builder.Property(e => e.LongUrl).IsRequired();

            
            builder.Property(e => e.ShortCode)
                .IsRequired()
                .HasMaxLength(7);
            
           
            builder.HasIndex(e => e.ShortCode).IsUnique();

            builder.Property(d => d.Id)
                .UseHiLo("ShortenedUrl_HiLoSequence");
        });
    }
}
