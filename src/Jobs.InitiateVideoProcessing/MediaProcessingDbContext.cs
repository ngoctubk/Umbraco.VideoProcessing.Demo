using System.Reflection;
using Commons;
using Microsoft.EntityFrameworkCore;

namespace Jobs.InitiateVideoProcessing;

public class MediaProcessingDbContext(DbContextOptions<MediaProcessingDbContext> options) : DbContext(options)
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<VideoEvent> VideoEvents { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
