using MediatR;
using PermissionManagement.Application.Commands;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Interfaces;
using PermissionManagement.Domain.Entities;
using PermissionManagement.Domain.Interfaces;

namespace PermissionManagement.Application.Handlers;

public class ModifyPermissionHandler : IRequestHandler<ModifyPermissionCommand, PermissionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IKafkaProducerService _kafkaProducerService;

    public ModifyPermissionHandler(
        IUnitOfWork unitOfWork,
        IElasticsearchService elasticsearchService,
        IKafkaProducerService kafkaProducerService)
    {
        _unitOfWork = unitOfWork;
        _elasticsearchService = elasticsearchService;
        _kafkaProducerService = kafkaProducerService;
    }

    public async Task<PermissionDto> Handle(ModifyPermissionCommand request, CancellationToken cancellationToken)
    {
        request.Validate();

        if (!request.IsValid)
        {
            var errors = string.Join("; ", request.Notifications.Select(n => n.Message));
            throw new ArgumentException(errors);
        }

        var permission = await _unitOfWork.Permissions.GetByIdAsync(request.Id);
        if (permission == null)
            throw new KeyNotFoundException($"Permiso con ID {request.Id} no encontrado.");

        var permissionType = await _unitOfWork.Permissions.GetByIdAsync(request.TipoPermiso);
        if (permissionType == null)
            throw new ArgumentException($"El tipo de permiso {request.TipoPermiso} no existe en el sistema.");


        permission.NombreEmpleado = request.NombreEmpleado;
        permission.ApellidoEmpleado = request.ApellidoEmpleado;
        permission.TipoPermiso = request.TipoPermiso;
        permission.FechaPermiso = request.FechaPermiso;

        await _unitOfWork.Permissions.UpdateAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        var dto = new PermissionDto
        {
            Id = permission.Id,
            NombreEmpleado = permission.NombreEmpleado,
            ApellidoEmpleado = permission.ApellidoEmpleado,
            TipoPermiso = permission.TipoPermiso,
            FechaPermiso = permission.FechaPermiso
        };

        await SendToExternalServicesAsync(permission);

        return dto;
    }

    private async Task SendToExternalServicesAsync(Permission? permission)
    {
        try
        {
            var permissionDto = new PermissionDto
            {
                Id = permission.Id,
                NombreEmpleado = permission.NombreEmpleado,
                ApellidoEmpleado = permission.ApellidoEmpleado,
                TipoPermiso = permission.TipoPermiso,
                FechaPermiso = permission.FechaPermiso
            };

            await _elasticsearchService.IndexPermissionAsync(permissionDto);

            var operation = new OperationDto
            {
                Id = Guid.NewGuid(),
                OperationName = "modify"
            };
            await _kafkaProducerService.SendOperationAsync(operation);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar a Kafka/Elasticsearch: {ex.Message}");

        }
    }
}
