using Microsoft.EntityFrameworkCore;

namespace SimulationContextLib;


public class SimulationContext: DbContext
{
    public DbSet<ForkUpdate> ForkUpdates { get; set; }
    public DbSet<PhilosopherUpdate> PhilosopherUpdates { get; set; }

    public SimulationContext(DbContextOptions<SimulationContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ForkUpdate>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ForkId)
                  .IsRequired();

            entity.Property(e => e.ForkOwner)
                  .IsRequired();

            entity.Property(e => e.UpdateTime)
                  .IsRequired();

            entity.Property(e => e.UpdateTime)
                  .HasColumnType("time");
        });

        modelBuilder.Entity<PhilosopherUpdate>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired();

            entity.Property(e => e.State)
                  .IsRequired();

            entity.Property(e => e.UpdateTime)
                  .IsRequired();

            entity.Property(e => e.UpdateTime)
                  .HasColumnType("time");
        });
    }
}