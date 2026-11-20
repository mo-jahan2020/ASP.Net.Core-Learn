using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SsoServer.Domain.Entities;

namespace SsoServer.Infrastructure.Persistence;

/// <summary>
/// Single database context: ASP.NET Core Identity stores, OpenIddict stores
/// (applications, authorizations, scopes, tokens - registered via <c>UseOpenIddict()</c>
/// on the options builder) and the SSO app entities.
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ClientProfile> ClientProfiles => Set<ClientProfile>();

    public DbSet<ClientUserAssignment> ClientUserAssignments => Set<ClientUserAssignment>();

    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.DisplayName).HasMaxLength(200);
            entity.HasIndex(u => u.IsActive);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(r => r.PersianDescription).HasMaxLength(200);
        });

        builder.Entity<ClientProfile>(entity =>
        {
            entity.HasKey(c => c.ClientId);
            entity.Property(c => c.ClientId).HasMaxLength(100);
            entity.Property(c => c.PersianDisplayName).HasMaxLength(200).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.Owner).HasMaxLength(200);
            entity.HasIndex(c => c.Enabled);
        });

        builder.Entity<ClientUserAssignment>(entity =>
        {
            // CRITICAL: foreign key columns must be defined with EXACTLY the same length
            // as the referenced principal columns, otherwise SQL Server rejects the FK:
            //   "Column 'AspNetUsers.Id' is not the same length or scale as referencing
            //    column 'ClientUserAssignments.UserId' ..."
            // AspNetUsers.Id (Identity string key)  = nvarchar(450)
            // ClientProfiles.ClientId (primary key) = nvarchar(100)
            entity.Property(a => a.UserId).HasMaxLength(450);
            entity.Property(a => a.ClientId).HasMaxLength(100);
            entity.Property(a => a.AssignedBy).HasMaxLength(100);

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ClientProfile>()
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(a => new { a.UserId, a.ClientId }).IsUnique();
            entity.HasIndex(a => a.ClientId);
        });

        builder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(s => s.Key);
            entity.Property(s => s.Key).HasMaxLength(200);
            entity.Property(s => s.Value).HasMaxLength(2000);
            entity.Property(s => s.UpdatedBy).HasMaxLength(100);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.Property(a => a.ActorUserId).HasMaxLength(100);
            entity.Property(a => a.ActorUserName).HasMaxLength(200);
            entity.Property(a => a.ClientId).HasMaxLength(100);
            entity.Property(a => a.Ip).HasMaxLength(100);
            entity.Property(a => a.DataJson).HasMaxLength(4000);
            entity.HasIndex(a => a.CreatedAt);
            entity.HasIndex(a => a.ActorUserId);
            entity.HasIndex(a => a.Action);
        });
    }
}
