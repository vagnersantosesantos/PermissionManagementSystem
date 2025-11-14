namespace PermissionManagement.Domain.Entities;

public class Permission
{
    public int Id { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public DateTime FechaPermiso { get; set; }
    
    // Navigation property
    public virtual PermissionType? PermissionType { get; set; }
}
