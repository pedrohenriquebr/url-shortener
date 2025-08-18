using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infra.Context;

public class UrlShortenerDbContext : DbContext
{
    // O construtor é necessário para a injeção de dependência.
    public UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : base(options)
    {
    }

    // Cada DbSet<T> representa uma tabela que você quer acessar.
    // O nome da propriedade (ShortenedUrls) será o nome da tabela por padrão.
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    // public DbSet<ShortenedUrlStats> ShortenedUrlsStats { get; set; }
    
    // (Opcional, mas recomendado para controle total)
    // Sobrescreva este método para configurar o mapeamento via "Fluent API",
    // que é uma alternativa mais poderosa aos Data Annotations.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        // Etapa 1: Criar a SEQUENCE no banco de dados que guardará o valor "High"
        modelBuilder.HasSequence("ShortenedUrl_HiLoSequence")
            .IncrementsBy(10); // Opcional: quanto o valor da sequência incrementa
        
        // Configurações para a entidade ShortenedUrl
        modelBuilder.Entity<ShortenedUrl>(builder =>
        {
            // Mapeia explicitamente a entidade para a tabela "ShortenedUrls"
            builder.ToTable("ShortenedUrls");

            // Configura a chave primária
            builder.HasKey(e => e.Id);

            // Configura a propriedade LongUrl
            builder.Property(e => e.LongUrl).IsRequired();

            // Configura a propriedade ShortCode
            builder.Property(e => e.ShortCode)
                .IsRequired()
                .HasMaxLength(7);
            
            // Cria um índice único para a coluna ShortCode. Essencial para performance e unicidade.
            builder.HasIndex(e => e.ShortCode).IsUnique();

            builder.Property(d => d.Id)
                .UseHiLo("ShortenedUrl_HiLoSequence");
        });
    }
}