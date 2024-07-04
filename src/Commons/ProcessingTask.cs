using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commons;

public class ProcessingTask
{
    public Guid Id { get; set; }
    public required string VideoPath { get; set; }
    public required string MediaPartPath { get; set; }
    public required int Resolution { get; set; }    
    public required string TaskName { get; set; }
    public required string TaskContent { get; set; }
    public bool IsDone { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public class ProcessingTaskConfiguration : IEntityTypeConfiguration<ProcessingTask>
{
    public void Configure(EntityTypeBuilder<ProcessingTask> builder)
    {
        builder.ToTable("ProcessingTasks");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.VideoPath).IsRequired();
        builder.Property(a => a.VideoPath).HasMaxLength(250);
        builder.HasIndex(a => a.VideoPath);


        builder.Property(a => a.MediaPartPath).IsRequired();
        builder.Property(a => a.MediaPartPath).HasMaxLength(250);
        builder.HasIndex(a => a.MediaPartPath);
        
        builder.Property(a => a.TaskName).IsRequired();
        builder.Property(a => a.TaskName).HasMaxLength(100);
        builder.HasIndex(a => a.TaskName);

        builder.Property(a => a.TaskContent).HasMaxLength(400);

        builder.Property(a => a.CreatedDate).IsRequired();
        builder.HasIndex(a => a.CreatedDate);

        builder.Property(a => a.ModifiedDate).IsRequired();
        builder.HasIndex(a => a.ModifiedDate);
    }
}
