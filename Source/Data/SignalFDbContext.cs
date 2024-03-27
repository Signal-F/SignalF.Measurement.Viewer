using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SignalF.Measurement.Viewer.Models.SignalFDb;

namespace SignalF.Measurement.Viewer.Data
{
    public partial class SignalFDbContext : DbContext
    {
        public SignalFDbContext()
        {
        }

        public SignalFDbContext(DbContextOptions<SignalFDbContext> options) : base(options)
        {
        }

        partial void OnModelBuilding(ModelBuilder builder);

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<SignalF.Measurement.Viewer.Models.SignalFDb.Device>()
              .HasOne(i => i.Room)
              .WithMany(i => i.Devices)
              .HasForeignKey(i => i.RoomId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement>()
              .HasOne(i => i.Device)
              .WithMany(i => i.Measurements)
              .HasForeignKey(i => i.DeviceId)
              .HasPrincipalKey(i => i.Id);

            builder.Entity<SignalF.Measurement.Viewer.Models.SignalFDb.Room>()
              .HasOne(i => i.Building)
              .WithMany(i => i.Rooms)
              .HasForeignKey(i => i.BuildingId)
              .HasPrincipalKey(i => i.Id);
            this.OnModelBuilding(builder);
        }

        public DbSet<SignalF.Measurement.Viewer.Models.SignalFDb.Building> Buildings { get; set; }

        public DbSet<SignalF.Measurement.Viewer.Models.SignalFDb.Device> Devices { get; set; }

        public DbSet<SignalF.Measurement.Viewer.Models.SignalFDb.Measurement> Measurements { get; set; }

        public DbSet<SignalF.Measurement.Viewer.Models.SignalFDb.Room> Rooms { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }
    
    }
}