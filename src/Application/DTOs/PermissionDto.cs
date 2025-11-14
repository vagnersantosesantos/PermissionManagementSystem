namespace PermissionManagement.Application.DTOs;

public class PermissionDto
{
    public int Id { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public DateTime FechaPermiso { get; set; }
    public string? PermissionTypeDescription { get; set; }
}

public class CreatePermissionDto
{
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public DateTime FechaPermiso { get; set; }
}

public class UpdatePermissionDto
{
    public int Id { get; set; }
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public DateTime FechaPermiso { get; set; }
}
