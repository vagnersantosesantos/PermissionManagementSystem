using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PermissionManagement.Domain.Entities;

namespace PermissionManagement.Infrastructure.Mappings
{
    public class PermissionMap : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id).ValueGeneratedOnAdd();
            builder.Property(e => e.NombreEmpleado).IsRequired().HasMaxLength(100);
            builder.Property(e => e.ApellidoEmpleado).IsRequired().HasMaxLength(100);
            builder.Property(e => e.TipoPermiso).IsRequired();
            builder.Property(e => e.FechaPermiso).IsRequired();

            builder.HasOne(p => p.PermissionType)
                .WithMany(pt => pt.Permissions)
                .HasForeignKey(p => p.TipoPermiso)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
