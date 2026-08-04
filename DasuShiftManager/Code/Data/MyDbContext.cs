using System.Text.Json;
using DasuShiftManager.Code.Entities;
using Microsoft.EntityFrameworkCore;

namespace DasuShiftManager.Code.Data;

public class MyDbContext:DbContext
{
    public DbSet<Setting> Setting { get; set; }
    public DbSet<Staff> Staff { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Setting>()
            .Property(b => b.EveryHalfHourMinWorkers)
            .HasConversion(
                v=>JsonSerializer.Serialize(v,(JsonSerializerOptions)null!),
                v=>JsonSerializer.Deserialize<Dictionary<int,int>>(v,(JsonSerializerOptions)null!)??new Dictionary<int, int>());
        modelBuilder.Entity<Setting>()
            .Property(b => b.EveryHalfHourMinManagersOrPharmacist)
            .HasConversion(
                v=>JsonSerializer.Serialize(v,(JsonSerializerOptions)null!),
                v=>JsonSerializer.Deserialize<Dictionary<int,int>>(v,(JsonSerializerOptions)null!)??new Dictionary<int, int>());
    }
}