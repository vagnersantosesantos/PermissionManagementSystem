using Flunt.Notifications;
using MediatR;
using PermissionManagement.Application.Commands.Contract;
using PermissionManagement.Application.DTOs;

namespace PermissionManagement.Application.Commands;

public class RequestPermissionCommand : Notifiable<Notification>, IRequest<PermissionDto>
{
    public string NombreEmpleado { get; set; } = string.Empty;
    public string ApellidoEmpleado { get; set; } = string.Empty;
    public int TipoPermiso { get; set; }
    public DateTime FechaPermiso { get; set; }

    public void Validate()
    {
        AddNotifications(new RequestPermissionCommandContract(this));
    }
}
