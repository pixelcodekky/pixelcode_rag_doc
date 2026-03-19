using Microsoft.EntityFrameworkCore;
using RagBot.Domain.Entities;

namespace RagBot.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<DocumentChunk> DocumentChunks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");

        modelBuilder.Entity<DocumentChunk>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Embedding)
                .HasColumnType("vector(1536)"); // Default for OpenAI text-embedding-ada-002 or text-embedding-3-small
        });

        base.OnModelCreating(modelBuilder);
    }
}
