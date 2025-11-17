using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PermissionManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace PermissionManagement.Infrastructure.Mappings
{
    public class PermissionTypeMap : IEntityTypeConfiguration<PermissionType>
    {
        public void Configure(EntityTypeBuilder<PermissionType> builder)
        {
                builder.ToTable("PermissionTypes");
                builder.HasKey(e => e.Id);
                builder.Property(e => e.Id).ValueGeneratedOnAdd();
                builder.Property(e => e.Description).IsRequired().HasMaxLength(200);

            // Seed data
            builder.HasData(
                new PermissionType { Id = 1, Description = "Vacaciones" },
                new PermissionType { Id = 2, Description = "Enfermedad" },
                new PermissionType { Id = 3, Description = "Permiso Personal" },
                new PermissionType { Id = 4, Description = "Capacitación" },
                new PermissionType { Id = 5, Description = "Licencia Maternidad/Paternidad" }
            );
        }
    }
}
