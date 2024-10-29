using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SmartFarmManager.DataAccessObject.Models;

public partial class FarmsContext : DbContext
{
    public FarmsContext()
    {
    }

    public FarmsContext(DbContextOptions<FarmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alert> Alerts { get; set; }

    public virtual DbSet<AlertType> AlertTypes { get; set; }

    public virtual DbSet<AlertUser> AlertUsers { get; set; }

    public virtual DbSet<CameraSurveillance> CameraSurveillances { get; set; }

    public virtual DbSet<DeviceReading> DeviceReadings { get; set; }

    public virtual DbSet<Farm> Farms { get; set; }

    public virtual DbSet<FarmStaffAssignment> FarmStaffAssignments { get; set; }

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    public virtual DbSet<IoTDevice> IoTDevices { get; set; }

    public virtual DbSet<Livestock> Livestocks { get; set; }

    public virtual DbSet<LivestockExpense> LivestockExpenses { get; set; }

    public virtual DbSet<LivestockSale> LivestockSales { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<RevenueAndProfitReport> RevenueAndProfitReports { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TaskHistory> TaskHistories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPermission> UserPermissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=db.fjourney.site;Initial Catalog=FARMS;User ID=SA;Password=<YourStrong@Passw0rda>;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Alerts__EBB16A8D70B82B60");

            entity.HasOne(d => d.AlertType).WithMany(p => p.Alerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alerts__AlertTyp__06CD04F7");

            entity.HasOne(d => d.Device).WithMany(p => p.Alerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alerts__DeviceId__05D8E0BE");

            entity.HasOne(d => d.Farm).WithMany(p => p.Alerts)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Alerts__FarmId__07C12930");
        });

        modelBuilder.Entity<AlertType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AlertTyp__016D41BD47CEE0B1");
        });

        modelBuilder.Entity<AlertUser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AlertUse__9E868E4382438AE9");

            entity.HasOne(d => d.Alert).WithMany(p => p.AlertUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AlertUser__Alert__0A9D95DB");

            entity.HasOne(d => d.User).WithMany(p => p.AlertUsers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__AlertUser__UserI__0B91BA14");
        });

        modelBuilder.Entity<CameraSurveillance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__CameraSu__F971E0C87134DF29");

            entity.Property(e => e.InstallDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.Farm).WithMany(p => p.CameraSurveillances)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CameraSur__FarmI__5DCAEF64");
        });

        modelBuilder.Entity<DeviceReading>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__DeviceRe__C80F9C4ECED516C4");

            entity.Property(e => e.Timestamp).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceReadings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DeviceRea__Devic__59063A47");
        });

        modelBuilder.Entity<Farm>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Farms__ED7BBAB981AFFB50");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Owner).WithMany(p => p.Farms)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Farms__OwnerId__4CA06362");
        });

        modelBuilder.Entity<FarmStaffAssignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__FarmStaf__32499E77E4556627");

            entity.HasOne(d => d.Farm).WithMany(p => p.FarmStaffAssignments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FarmStaff__FarmI__4F7CD00D");

            entity.HasOne(d => d.FarmStaff).WithMany(p => p.FarmStaffAssignments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__FarmStaff__FarmS__5070F446");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__F5FDE6B3D8FDE6F2");

            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Farm).WithMany(p => p.Inventories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__FarmI__787EE5A0");
        });

        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Inventor__55433A6B3D631B6C");

            entity.Property(e => e.Date).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Inventory).WithMany(p => p.InventoryTransactions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inventory__Inven__7C4F7684");
        });

        modelBuilder.Entity<IoTDevice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__IoTDevic__49E12311DF9DC36E");

            entity.Property(e => e.InstallDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Active");

            entity.HasOne(d => d.Farm).WithMany(p => p.IoTdevices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__IoTDevice__FarmI__5535A963");
        });

        modelBuilder.Entity<Livestock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Livestoc__8228ABF08B90916C");

            entity.Property(e => e.Expenses).HasDefaultValue(0.0);
            entity.Property(e => e.Status).HasDefaultValue("Active");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Farm).WithMany(p => p.Livestocks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Livestock__FarmI__6D0D32F4");
        });

        modelBuilder.Entity<LivestockExpense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Livestoc__1445CFD3B68EF710");

            entity.Property(e => e.ExpenseDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Livestock).WithMany(p => p.LivestockExpenses)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Livestock__Lives__70DDC3D8");
        });

        modelBuilder.Entity<LivestockSale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Livestoc__1EE3C3FF9E0C9548");

            entity.Property(e => e.SaleDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Livestock).WithMany(p => p.LivestockSales)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Livestock__Lives__74AE54BC");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__20CF2E12B1571953");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsRead).HasDefaultValue(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__10566F31");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__EFA6FB2F137B7BF6");
        });

        modelBuilder.Entity<RevenueAndProfitReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RevenueA__D5BD4805D2AAFD61");

            entity.Property(e => e.ReportGeneratedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.TotalProfit).HasComputedColumnSql("([TotalRevenue]-[TotalExpenses])", true);

            entity.HasOne(d => d.Farm).WithMany(p => p.RevenueAndProfitReports)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RevenueAn__FarmI__00200768");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__8AFACE1A6F649AA8");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tasks__7C6949B1048346FE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status).HasDefaultValue("Pending");

            entity.HasOne(d => d.AssignedToUser).WithMany(p => p.Tasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__AssignedT__628FA481");

            entity.HasOne(d => d.Farm).WithMany(p => p.Tasks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tasks__FarmId__6383C8BA");
        });

        modelBuilder.Entity<TaskHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TaskHist__2F15B73C3FA789A4");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskHistories)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TaskHisto__TaskI__6754599E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__1788CC4C736F545B");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__RoleI__412EB0B6"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__UserRoles__UserI__403A8C7D"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__UserRole__AF2760AD0C3A26E3");
                        j.ToTable("UserRoles");
                    });
        });

        modelBuilder.Entity<UserPermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserPerm__A90F88B2DD6A3730");

            entity.Property(e => e.GrantedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Permission).WithMany(p => p.UserPermissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserPermi__Permi__48CFD27E");

            entity.HasOne(d => d.User).WithMany(p => p.UserPermissions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserPermi__UserI__47DBAE45");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
