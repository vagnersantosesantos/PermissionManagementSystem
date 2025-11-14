using MediatR;
using PermissionManagement.Application.Commands;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Interfaces;
using PermissionManagement.Domain.Entities;
using PermissionManagement.Domain.Interfaces;

namespace PermissionManagement.Application.Handlers;

public class RequestPermissionHandler : IRequestHandler<RequestPermissionCommand, PermissionDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IKafkaProducerService _kafkaProducerService;

    public RequestPermissionHandler(
        IUnitOfWork unitOfWork,
        IElasticsearchService elasticsearchService,
        IKafkaProducerService kafkaProducerService)
    {
        _unitOfWork = unitOfWork;
        _elasticsearchService = elasticsearchService;
        _kafkaProducerService = kafkaProducerService;
    }

    public async Task<PermissionDto> Handle(RequestPermissionCommand request, CancellationToken cancellationToken)
    {
        request.Validate();

        if (!request.IsValid)
        {
            var errors = string.Join("; ", request.Notifications.Select(n => n.Message));
            throw new ArgumentException(errors);
        }

        var permission = new Permission
        {
            NombreEmpleado = request.NombreEmpleado,
            ApellidoEmpleado = request.ApellidoEmpleado,
            TipoPermiso = request.TipoPermiso,
            FechaPermiso = request.FechaPermiso
        };

        var createdPermission = await _unitOfWork.Permissions.AddAsync(permission);
        await _unitOfWork.SaveChangesAsync();

        await SendToExternalServicesAsync(createdPermission);

        var dto = new PermissionDto
        {
            Id = createdPermission.Id,
            NombreEmpleado = createdPermission.NombreEmpleado,
            ApellidoEmpleado = createdPermission.ApellidoEmpleado,
            TipoPermiso = createdPermission.TipoPermiso,
            FechaPermiso = createdPermission.FechaPermiso
        };

        return dto;
    }

    private async Task SendToExternalServicesAsync(Permission createdPermission)
    {
        try
        {
            var permissionDto = new PermissionDto
            {
                Id = createdPermission.Id,
                NombreEmpleado = createdPermission.NombreEmpleado,
                ApellidoEmpleado = createdPermission.ApellidoEmpleado,
                TipoPermiso = createdPermission.TipoPermiso,
                FechaPermiso = createdPermission.FechaPermiso
            };

            await _elasticsearchService.IndexPermissionAsync(permissionDto);

            var operation = new OperationDto
            {
                Id = Guid.NewGuid(),
                OperationName = "request"
            };
            await _kafkaProducerService.SendOperationAsync(operation);

        } catch(Exception ex)
        {
            Console.WriteLine($"Error al enviar a Kafka/Elasticsearch: {ex.Message}");
        }
    }
}
