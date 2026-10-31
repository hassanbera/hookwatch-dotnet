using Microsoft.EntityFrameworkCore;
using App.Model;
namespace App.Repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
  public DbSet<Endpoint> Endpoints => Set<Endpoint>();
  public DbSet<Delivery> Deliveries => Set<Delivery>();
  public DbSet<Attempt> Attempts => Set<Attempt>();
  protected override void OnModelCreating(ModelBuilder b)
  {
    b.Entity<Endpoint>(e => { e.ToTable("endpoints"); e.HasKey(x => x.Id); e.Property(x => x.Id).HasColumnName("id"); e.Property(x => x.CreatedAt).HasColumnName("created_at"); e.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired(); e.Property(x => x.Url).HasColumnName("url").HasMaxLength(500).IsRequired(); });
    b.Entity<Delivery>(e => { e.ToTable("deliveries"); e.HasKey(x => x.Id); e.Property(x => x.Id).HasColumnName("id"); e.Property(x => x.CreatedAt).HasColumnName("created_at"); e.Property(x => x.EndpointId).HasColumnName("endpoint_id"); e.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(80).IsRequired(); e.Property(x => x.Payload).HasColumnName("payload").IsRequired(); e.Property(x => x.Status).HasColumnName("status").HasMaxLength(20).IsRequired(); e.Property(x => x.AttemptCount).HasColumnName("attempt_count"); e.Property(x => x.CycleAttempts).HasColumnName("cycle_attempts"); e.Property(x => x.NextAttemptAt).HasColumnName("next_attempt_at"); });
    b.Entity<Attempt>(e => { e.ToTable("attempts"); e.HasKey(x => x.Id); e.Property(x => x.Id).HasColumnName("id"); e.Property(x => x.CreatedAt).HasColumnName("created_at"); e.Property(x => x.DeliveryId).HasColumnName("delivery_id"); e.Property(x => x.Number).HasColumnName("number"); e.Property(x => x.StatusCode).HasColumnName("status_code"); e.Property(x => x.DurationMs).HasColumnName("duration_ms"); e.Property(x => x.Error).HasColumnName("error").HasMaxLength(500).IsRequired(); });
    b.Entity<Delivery>().HasOne<Endpoint>().WithMany().HasForeignKey(x => x.EndpointId).OnDelete(DeleteBehavior.Restrict);
    b.Entity<Attempt>().HasOne<Delivery>().WithMany().HasForeignKey(x => x.DeliveryId).OnDelete(DeleteBehavior.Restrict);
  }
}
