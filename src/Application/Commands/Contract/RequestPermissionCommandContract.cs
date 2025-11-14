using Flunt.Validations;

namespace PermissionManagement.Application.Commands.Contract
{
    public class RequestPermissionCommandContract : Contract<RequestPermissionCommand>
    {
        public RequestPermissionCommandContract(RequestPermissionCommand command)
        {
            var today = DateTime.Today;

            Requires()
                        // Tipo Permiso
                        .IsGreaterThan(command.TipoPermiso, 0, "TipoPermiso", "Se requiere algún tipo de permiso.")
                        .IsLowerOrEqualsThan(command.TipoPermiso, 4, "TipoPermiso", "Tipo de permiso debe estar entre 1 y 4.")

                        // Nombre Empleado
                        .IsNotNullOrWhiteSpace(command.NombreEmpleado, "NombreEmpleado", "Se requiere algún nombre de empleado.")
                        .IsGreaterOrEqualsThan(command.NombreEmpleado, 3, "NombreEmpleado", "Nombre debe tener al menos 2 caracteres.")
                        .IsLowerOrEqualsThan(command.NombreEmpleado, 100, "NombreEmpleado", "Nombre no puede exceder 100 caracteres.")

                        // Apellido Empleado
                        .IsNotNullOrWhiteSpace(command.ApellidoEmpleado, "ApellidoEmpleado", "Se requiere algún apellido de empleado.")
                        .IsGreaterOrEqualsThan(command.ApellidoEmpleado, 3, "ApellidoEmpleado", "Apellido debe tener al menos 2 caracteres.")
                        .IsLowerOrEqualsThan(command.ApellidoEmpleado, 100, "ApellidoEmpleado", "Apellido no puede exceder 100 caracteres.")

                        // Fecha Permiso
                        .IsNotNull(command.FechaPermiso, "FechaPermiso", "Se requiere una fecha de permiso.")
                        .IsFalse(command.FechaPermiso == default, "FechaPermiso", "La fecha no puede ser el valor por defecto.")
                        .IsGreaterOrEqualsThan(command.FechaPermiso.Date, today, "FechaPermiso", "La fecha no puede ser en el pasado. Debe ser de hoy en adelante.");


        }
    }
}