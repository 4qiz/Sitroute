using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Sitronics.Models;

namespace Sitronics.Data;

public partial class SitrouteDataContext : DbContext
{
    public SitrouteDataContext()
    {
    }

    public SitrouteDataContext(DbContextOptions<SitrouteDataContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Bus> Buses { get; set; }

    public virtual DbSet<BusStation> BusStations { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<Factor> Factors { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Route> Routes { get; set; }

    public virtual DbSet<RouteByBusStation> RouteByBusStations { get; set; }

    public virtual DbSet<Schedule> Schedules { get; set; }

    public virtual DbSet<Time> Times { get; set; }

    public virtual DbSet<TypeFactor> TypeFactors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=Sitronics;Integrated Security=True;User ID=DESKTOP-RKMQ39T\\vanya;Trust Server Certificate=True", x => x.UseNetTopologySuite());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.IdUser);

            entity.ToTable("Admin");

            entity.Property(e => e.IdUser).ValueGeneratedNever();
            entity.Property(e => e.Role).HasMaxLength(15);

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Admin_User");
        });

        modelBuilder.Entity<Bus>(entity =>
        {
            entity.HasKey(e => e.IdBus);

            entity.ToTable("Bus");

            entity.Property(e => e.Charge).HasDefaultValueSql("((100))");
            entity.Property(e => e.Number).HasMaxLength(6);

            entity.HasOne(d => d.IdRouteNavigation).WithMany(p => p.Buses)
                .HasForeignKey(d => d.IdRoute)
                .HasConstraintName("FK_Bus_Route");

            entity.HasMany(d => d.IdDrivers).WithMany(p => p.IdBus)
                .UsingEntity<Dictionary<string, object>>(
                    "DriverDriveBu",
                    r => r.HasOne<Driver>().WithMany()
                        .HasForeignKey("IdDriver")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DriverDriveBus_Driver"),
                    l => l.HasOne<Bus>().WithMany()
                        .HasForeignKey("IdBus")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_DriverDriveBus_Bus"),
                    j =>
                    {
                        j.HasKey("IdBus", "IdDriver");
                        j.ToTable("DriverDriveBus");
                    });
        });

        modelBuilder.Entity<BusStation>(entity =>
        {
            entity.HasKey(e => e.IdBusStation);

            entity.ToTable("BusStation");

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.PeopleCount).HasDefaultValueSql("((0))");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.IdDriver);

            entity.ToTable("Driver");

            entity.Property(e => e.IdDriver).ValueGeneratedNever();
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.StartTime).HasColumnType("datetime");

            entity.HasOne(d => d.IdDriverNavigation).WithOne(p => p.Driver)
                .HasForeignKey<Driver>(d => d.IdDriver)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Driver_User");
        });

        modelBuilder.Entity<Factor>(entity =>
        {
            entity.HasKey(e => e.IdFactor);

            entity.ToTable("Factor");

            entity.Property(e => e.Length).HasColumnType("decimal(6, 3)");
            entity.Property(e => e.SpeedСoefficient).HasColumnType("decimal(3, 2)");

            entity.HasOne(d => d.IdRouteNavigation).WithMany(p => p.Factors)
                .HasForeignKey(d => d.IdRoute)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factor_Route");

            entity.HasOne(d => d.IdTypeNavigation).WithMany(p => p.Factors)
                .HasForeignKey(d => d.IdType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Factor_TypeFactor");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.IdMessage).HasName("PK_Message_1");

            entity.ToTable("Message");

            entity.Property(e => e.Value).HasMaxLength(300);

            entity.HasOne(d => d.IdRecipientNavigation).WithMany(p => p.MessageIdRecipientNavigations)
                .HasForeignKey(d => d.IdRecipient)
                .HasConstraintName("FK_Message_User");

            entity.HasOne(d => d.IdSenderNavigation).WithMany(p => p.MessageIdSenderNavigations)
                .HasForeignKey(d => d.IdSender)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserInChat_User");
        });

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasKey(e => e.IdRoute);

            entity.ToTable("Route");

            entity.Property(e => e.Name).HasMaxLength(10);
        });

        modelBuilder.Entity<RouteByBusStation>(entity =>
        {
            entity.HasKey(e => new { e.IdRoute, e.IdBusStation }).HasName("PK_RouteByPoint");

            entity.HasOne(d => d.IdBusStationNavigation).WithMany(p => p.RouteByBusStations)
                .HasForeignKey(d => d.IdBusStation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RouteByBusStations_BusStation");

            entity.HasOne(d => d.IdRouteNavigation).WithMany(p => p.RouteByBusStations)
                .HasForeignKey(d => d.IdRoute)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RouteByPoint_Route");
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => new { e.IdBus, e.IdBusStation, e.IdTime });

            entity.ToTable("Schedule");

            entity.HasOne(d => d.IdBusNavigation).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.IdBus)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Bus");

            entity.HasOne(d => d.IdBusStationNavigation).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.IdBusStation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_BusStation1");

            entity.HasOne(d => d.IdTimeNavigation).WithMany(p => p.Schedules)
                .HasForeignKey(d => d.IdTime)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Schedule_Time");
        });

        modelBuilder.Entity<Time>(entity =>
        {
            entity.HasKey(e => e.IdTime);

            entity.ToTable("Time");

            entity.Property(e => e.Time1)
                .HasColumnType("datetime")
                .HasColumnName("Time");
        });

        modelBuilder.Entity<TypeFactor>(entity =>
        {
            entity.HasKey(e => e.IdType);

            entity.ToTable("TypeFactor");

            entity.Property(e => e.Description).HasMaxLength(300);
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser);

            entity.ToTable("User");

            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.Login)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Patronymic).HasMaxLength(50);
            entity.Property(e => e.SecondName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
