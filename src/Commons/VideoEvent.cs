using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commons;

public class VideoEvent
{
    public Guid Id { get; set; }
    public required string VideoPath { get; set; }
    public required string Event { get; set; }
    public string? EventMessage { get; set; }
    public DateTime EventDate { get; set; }
}

public class VideoEventConfiguration : IEntityTypeConfiguration<VideoEvent>
{
    public void Configure(EntityTypeBuilder<VideoEvent> builder)
    {
        builder.ToTable("VideoEvents");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.VideoPath).IsRequired();
        builder.Property(a => a.VideoPath).HasMaxLength(250);
        builder.HasIndex(a => a.VideoPath);

        builder.Property(a => a.Event).IsRequired();
        builder.Property(a => a.Event).HasMaxLength(100);
        builder.HasIndex(a => a.Event);

        builder.Property(a => a.EventMessage).HasMaxLength(400);

        builder.Property(a => a.EventDate).IsRequired();
        builder.HasIndex(a => a.EventDate);
    }
}