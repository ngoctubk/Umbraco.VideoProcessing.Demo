using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Commons;

public class Video
{
    public Guid Id { get; set; }
    public required string VideoPath { get; set; }
    public bool IsExtracted { get; set; }
    public bool IsCut { get; set; }
    public string? RawMetadata { get; set; }
    public Guid? SuccessEventId { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}

public class VideoConfiguration : IEntityTypeConfiguration<Video>
{
    public void Configure(EntityTypeBuilder<Video> builder)
    {
        builder.ToTable("Videos");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedOnAdd();

        builder.Property(a => a.VideoPath).IsRequired();
        builder.Property(a => a.VideoPath).HasMaxLength(250);
        builder.HasIndex(a => a.VideoPath);

        builder.HasIndex(a => a.IsExtracted);

        builder.HasIndex(a => a.IsCut);

        builder.Property(a => a.CreatedDate).IsRequired();
        builder.HasIndex(a => a.CreatedDate);

        builder.Property(a => a.ModifiedDate).IsRequired();
        builder.HasIndex(a => a.ModifiedDate);
    }
}
