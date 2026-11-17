using FitnessClubIS.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessClubIS.Data;

public class FitnessContext : DbContext
{
    // Строку подключения можно переопределить переменной окружения FITNESS_DB
    public static string ConnectionString =>
        Environment.GetEnvironmentVariable("FITNESS_DB")
        ?? "Host=SQLstudv2;Port=5432;Database=24ipp12;Username=24ipp12;Password=24ipp12;Search Path=fitness";

    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<MembershipType> MembershipTypes => Set<MembershipType>();
    public DbSet<ClientMembership> ClientMemberships => Set<ClientMembership>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<WorkoutType> WorkoutTypes => Set<WorkoutType>();
    public DbSet<Trainer> Trainers => Set<Trainer>();
    public DbSet<TrainingSession> Sessions => Set<TrainingSession>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseNpgsql(ConnectionString);

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Booking>().HasIndex(x => new { x.SessionId, x.ClientId }).IsUnique();
        b.Entity<TrainingSession>().HasMany(s => s.Bookings).WithOne().HasForeignKey(x => x.SessionId);
        b.Entity<Client>().HasMany(c => c.Memberships).WithOne(m => m.Client).HasForeignKey(m => m.ClientId);
        b.Entity<ClientMembership>().HasMany(m => m.Payments).WithOne(p => p.ClientMembership).HasForeignKey(p => p.ClientMembershipId);
    }
}
