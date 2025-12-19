using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SSCV.Domain.Entities;

namespace SSCV.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<TelemetryRecord> TelemetryRecords { get; set; }
    public DbSet<AlertSystem> AlertSystems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TelemetryRecord>(entity =>
        {
            entity.HasKey(e => e.RecordId); 
            entity.Property(e => e.RecordId).UseIdentityColumn(); 

            entity.HasIndex(e => new { e.VehicleId, e.Timestamp });
        });
    }
}
