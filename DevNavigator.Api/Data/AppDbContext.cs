using DevNavigator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevNavigator.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<FileMetadata> Files => Set<FileMetadata>();

    public DbSet<Repository> Repositories => Set<Repository>();

    public DbSet<CodeContent> CodeContents => Set<CodeContent>();
    public DbSet<CodeSymbol> CodeSymbols => Set<CodeSymbol>();
    public DbSet<CodeSymbolRelationship> CodeSymbolRelationships
    => Set<CodeSymbolRelationship>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CodeContent>()
            .HasOne(x => x.File)
            .WithOne(x => x.CodeContent)
            .HasForeignKey<CodeContent>(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CodeSymbol>()
            .HasOne(x => x.File)
            .WithMany()
            .HasForeignKey(x => x.FileId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CodeSymbolRelationship>()
            .HasOne(x => x.FromSymbol)
            .WithMany()
            .HasForeignKey(x => x.FromSymbolId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CodeSymbolRelationship>()
            .HasOne(x => x.ToSymbol)
            .WithMany()
            .HasForeignKey(x => x.ToSymbolId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}