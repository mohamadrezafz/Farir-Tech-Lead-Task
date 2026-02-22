using Microsoft.EntityFrameworkCore;
using Solution.Data.Entities;

namespace Solution.Data;

public class SolutionDbContext : DbContext
{
    public SolutionDbContext(DbContextOptions<SolutionDbContext> options) : base(options) { }

    public DbSet<Car> Cars => Set<Car>();
    public DbSet<SpeedReading> SpeedReadings => Set<SpeedReading>();
    public DbSet<Ticket> Tickets => Set<Ticket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Car>(e =>
        {
            e.HasIndex(x => x.CarId).IsUnique();
            e.Property(x => x.CarId).HasMaxLength(64);
            e.Property(x => x.PlateNumber).HasMaxLength(32);
        });

        modelBuilder.Entity<SpeedReading>(e =>
        {
            e.HasOne(x => x.Car).WithMany(c => c.SpeedReadings).HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.CameraId).HasMaxLength(64);
            e.HasIndex(x => new { x.CarId, x.RecordedAtUtc });
        });

        modelBuilder.Entity<Ticket>(e =>
        {
            e.HasOne(x => x.Car).WithMany(c => c.Tickets).HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SpeedReading).WithOne(r => r.SpeedTicket).HasForeignKey<Ticket>(x => x.SpeedReadingId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.ExternalTicketId).IsUnique().HasFilter("[ExternalTicketId] IS NOT NULL");
            e.Property(x => x.ExternalTicketId).HasMaxLength(64);
            e.Property(x => x.Reason).HasMaxLength(256);
            e.HasIndex(x => x.RecordedAtUtc);
        });
    }
}
