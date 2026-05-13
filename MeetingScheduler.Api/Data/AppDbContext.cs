using Microsoft.EntityFrameworkCore;
using MeetingScheduler.Api.Models;

namespace MeetingScheduler.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<MeetingGroup> MeetingGroups => Set<MeetingGroup>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Availability> Availabilities => Set<Availability>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeetingGroup>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ShareCode).IsUnique();
        });

        modelBuilder.Entity<Participant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.MeetingGroup)
             .WithMany(x => x.Participants)
             .HasForeignKey(x => x.MeetingGroupId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Availability>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Participant)
             .WithMany(x => x.Availabilities)
             .HasForeignKey(x => x.ParticipantId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
