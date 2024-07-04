using Commons;
using Microsoft.EntityFrameworkCore;

namespace Jobs.MonitorProcessing;

public class MediaProcessingDbContext(DbContextOptions<MediaProcessingDbContext> options) : DbContext(options)
{
    public DbSet<Video> Videos { get; set; }
    public DbSet<VideoEvent> VideoEvents { get; set; }
    public DbSet<ProcessingTask> ProcessingTasks { get; set; }
}
