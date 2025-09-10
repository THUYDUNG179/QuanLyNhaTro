using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NhaTro.Infrastructure;

namespace NhaTro.Infrastructure.Data;

public partial class QLyNhaTroDbContext : DbContext
{
    public QLyNhaTroDbContext(DbContextOptions<QLyNhaTroDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Contract> Contracts { get; set; }

    public virtual DbSet<Incident> Incidents { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Motel> Motels { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomStatus> RoomStatuses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Utility> Utilities { get; set; }

    public virtual DbSet<UtilityReading> UtilityReadings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.HasKey(e => e.ContractId).HasName("PK__Contract__C90D346982749CC4");

            entity.HasIndex(e => e.RoomId, "IX_Contract_Room");

            entity.HasIndex(e => e.TenantId, "IX_Contract_Tenant");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DepositAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.FileContractPath).HasMaxLength(255);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Room).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__RoomI__5FB337D6");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Contracts)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Contracts__Tenan__60A75C0F");
        });

        modelBuilder.Entity<Incident>(entity =>
        {
            entity.HasKey(e => e.IncidentId).HasName("PK__Incident__3D8053B2D98F66C2");

            entity.HasIndex(e => e.RoomId, "IX_Incident_Room");

            entity.HasIndex(e => e.Status, "IX_Incident_Status");

            entity.HasIndex(e => e.TenantId, "IX_Incident_Tenant");

            entity.Property(e => e.AttachedImagePath).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Priority)
                .HasMaxLength(20)
                .HasDefaultValue("Normal");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Reported");
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Room).WithMany(p => p.Incidents)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Incidents__RoomI__04E4BC85");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Incidents)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Incidents__Tenan__05D8E0BE");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("PK__Invoices__D796AAB55DA0CD81");

            entity.HasIndex(e => new { e.ContractId, e.BillingPeriod }, "UQ_Invoice").IsUnique();

            entity.Property(e => e.BillingPeriod)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(300);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Unpaid");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Contract).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.ContractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Invoices__Contra__6E01572D");
        });

        modelBuilder.Entity<InvoiceDetail>(entity =>
        {
            entity.HasKey(e => e.InvoiceDetailId).HasName("PK__InvoiceD__1F157811F1588D1B");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__InvoiceDe__Invoi__72C60C4A");

            entity.HasOne(d => d.UtilityReading).WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.UtilityReadingId)
                .HasConstraintName("FK__InvoiceDe__Utili__73BA3083");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Logs__5E5486488C1D1C82");

            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "IX_Logs_UserTime");

            entity.Property(e => e.Action).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("IPAddress");
            entity.Property(e => e.TableName).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Logs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Logs__UserId__0B91BA14");
        });

        modelBuilder.Entity<Motel>(entity =>
        {
            entity.HasKey(e => e.MotelId).HasName("PK__Motels__2F579A240942FD1E");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.MotelName).HasMaxLength(200);

            entity.HasOne(d => d.Owner).WithMany(p => p.Motels)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Motels__OwnerId__52593CB8");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E129B1AB54B");

            entity.HasIndex(e => new { e.ReceiverUserId, e.IsRead }, "IX_Notification_Receiver");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasDefaultValue("General");

            entity.HasOne(d => d.ReceiverUser).WithMany(p => p.NotificationReceiverUsers)
                .HasForeignKey(d => d.ReceiverUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Recei__7C4F7684");

            entity.HasOne(d => d.RelatedContract).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RelatedContractId)
                .HasConstraintName("FK__Notificat__Relat__7F2BE32F");

            entity.HasOne(d => d.RelatedMotel).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RelatedMotelId)
                .HasConstraintName("FK__Notificat__Relat__7D439ABD");

            entity.HasOne(d => d.RelatedRoom).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RelatedRoomId)
                .HasConstraintName("FK__Notificat__Relat__7E37BEF6");

            entity.HasOne(d => d.SenderUser).WithMany(p => p.NotificationSenderUsers)
                .HasForeignKey(d => d.SenderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__Sende__7B5B524B");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payments__9B556A38406BC678");

            entity.HasIndex(e => e.InvoiceId, "IX_Payment_Invoice");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Method).HasMaxLength(50);
            entity.Property(e => e.PaymentDate).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Success");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Payments)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payments__Invoic__76969D2E");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A56A1BC3A");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160BA0E0F67").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Rooms__32863939D2375A81");

            entity.HasIndex(e => new { e.MotelId, e.RoomName }, "UQ_Room").IsUnique();

            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.RentalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RoomName).HasMaxLength(100);
            entity.Property(e => e.RoomStatusId).HasDefaultValue(1);

            entity.HasOne(d => d.Motel).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.MotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rooms__MotelId__59FA5E80");

            entity.HasOne(d => d.RoomStatus).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.RoomStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Rooms__RoomStatu__5AEE82B9");
        });

        modelBuilder.Entity<RoomStatus>(entity =>
        {
            entity.HasKey(e => e.RoomStatusId).HasName("PK__RoomStat__D29DF516DA147C31");

            entity.HasIndex(e => e.StatusName, "UQ__RoomStat__05E7698AD30B4E0A").IsUnique();

            entity.Property(e => e.RoomStatusId).ValueGeneratedNever();
            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CF683A40F");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534FA3A59C4").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__4D94879B");
        });

        modelBuilder.Entity<Utility>(entity =>
        {
            entity.HasKey(e => e.UtilityId).HasName("PK__Utilitie__8B7E2E1FBA846FD5");

            entity.Property(e => e.Unit).HasMaxLength(50);
            entity.Property(e => e.UtilityName).HasMaxLength(100);
        });

        modelBuilder.Entity<UtilityReading>(entity =>
        {
            entity.HasKey(e => e.ReadingId).HasName("PK__UtilityR__C80F9C4E19E61775");

            entity.HasIndex(e => new { e.RoomId, e.UtilityId, e.ReadingDate }, "UQ_UtilityReadings").IsUnique();

            entity.Property(e => e.ReadingValue).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Room).WithMany(p => p.UtilityReadings)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UtilityRe__RoomI__693CA210");

            entity.HasOne(d => d.Utility).WithMany(p => p.UtilityReadings)
                .HasForeignKey(d => d.UtilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UtilityRe__Utili__6A30C649");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
