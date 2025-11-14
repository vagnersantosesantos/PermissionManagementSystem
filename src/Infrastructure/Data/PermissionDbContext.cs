using Microsoft.EntityFrameworkCore;
using PermissionManagement.Domain.Entities;

namespace PermissionManagement.Infrastructure.Data;

public class PermissionDbContext : DbContext
{
    public PermissionDbContext(DbContextOptions<PermissionDbContext> options)
        : base(options)
    {
    }

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<PermissionType> PermissionTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.NombreEmpleado).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ApellidoEmpleado).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TipoPermiso).IsRequired();
            entity.Property(e => e.FechaPermiso).IsRequired();

            entity.HasOne(p => p.PermissionType)
                .WithMany(pt => pt.Permissions)
                .HasForeignKey(p => p.TipoPermiso)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PermissionType configuration
        modelBuilder.Entity<PermissionType>(entity =>
        {
            entity.ToTable("PermissionTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
        });

        // Seed data
        modelBuilder.Entity<PermissionType>().HasData(
            new PermissionType { Id = 1, Description = "Vacaciones" },
            new PermissionType { Id = 2, Description = "Enfermedad" },
            new PermissionType { Id = 3, Description = "Permiso Personal" },
            new PermissionType { Id = 4, Description = "Capacitación" },
            new PermissionType { Id = 5, Description = "Licencia Maternidad/Paternidad" }
        );
    }
}
