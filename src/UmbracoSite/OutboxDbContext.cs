using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace UmbracoSite;

public class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
