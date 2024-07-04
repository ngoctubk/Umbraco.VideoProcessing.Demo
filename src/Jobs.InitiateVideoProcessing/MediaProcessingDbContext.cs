using System.Reflection;
using Commons;
using Microsoft.EntityFrameworkCore;

namespace Jobs.InitiateVideoProcessing;

public class MediaProcessingDbContext(DbContextOptions<MediaProcessingDbContext> options) : DbContext(options)
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<VideoEvent> VideoEvents { get; set; }
    public DbSet<ProcessingTask> ProcessingTasks { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new VideoConfiguration());
        modelBuilder.ApplyConfiguration(new VideoEventConfiguration());
        modelBuilder.ApplyConfiguration(new ProcessingTaskConfiguration());
    }
}
