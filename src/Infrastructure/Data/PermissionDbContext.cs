using Microsoft.EntityFrameworkCore;
using PermissionManagement.Domain.Entities;
using PermissionManagement.Infrastructure.Mappings;
using System.ComponentModel.DataAnnotations;

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

        modelBuilder.ApplyConfiguration(new PermissionMap());
        modelBuilder.ApplyConfiguration(new PermissionTypeMap());
    }
}
