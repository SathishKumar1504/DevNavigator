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
}