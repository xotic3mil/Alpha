using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Data.Entities;
using Microsoft.AspNetCore.Identity;


namespace Data.Contexts;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<UserEntity, RoleEntity, Guid>(options)
{
    public DbSet<ProjectEntity> Projects { get; set; }
    public DbSet<StatusTypesEntity> StatusTypes { get; set; }
    public DbSet<CustomerEntity> Customers { get; set; }
    public DbSet<ServiceEntity> Services { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Use static GUID values for seeding roles
        var adminRoleId = new Guid("e57c9438-1b01-4944-a5a5-db46e76fdae8");
        var userRoleId = new Guid("62507571-ab9a-4860-b424-38992e129bd3");

        modelBuilder.Entity<RoleEntity>().HasData(
            new RoleEntity { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
            new RoleEntity { Id = userRoleId, Name = "User", NormalizedName = "USER" }
        );

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.Property(u => u.AvatarUrl)
                  .HasColumnType("varchar(2083)")
                  .IsRequired(false);
        });

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Status)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Service)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => p.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ProjectEntity>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserEntity>()
       .HasMany(u => u.Projects)
       .WithMany(p => p.Users)
       .UsingEntity<Dictionary<string, object>>(
           "UserProjects", 
           j => j.HasOne<ProjectEntity>()
                 .WithMany()
                 .HasForeignKey("ProjectId")
                 .OnDelete(DeleteBehavior.Cascade),
           j => j.HasOne<UserEntity>()
                 .WithMany()
                 .HasForeignKey("UserId")
                 .OnDelete(DeleteBehavior.Cascade));



        modelBuilder.Entity<IdentityUserLogin<Guid>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
        modelBuilder.Entity<IdentityUserRole<Guid>>().HasKey(r => new { r.UserId, r.RoleId });
        modelBuilder.Entity<IdentityUserToken<Guid>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

        modelBuilder.Entity<UserEntity>(entity => {
            entity.ToTable(name: "Users");
        });

        modelBuilder.Entity<RoleEntity>(entity => {
            entity.ToTable(name: "Roles");
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(entity => {
            entity.ToTable("UserRoles");
        });

        modelBuilder.Entity<IdentityUserClaim<Guid>>(entity => {
            entity.ToTable("UserClaims");
        });

        modelBuilder.Entity<IdentityUserLogin<Guid>>(entity => {
            entity.ToTable("UserLogins");
        });

        modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity => {
            entity.ToTable("RoleClaims");
        });

        modelBuilder.Entity<IdentityUserToken<Guid>>(entity => {
            entity.ToTable("UserTokens");
        });
    }


}




