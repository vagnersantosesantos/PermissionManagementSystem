using MediatR;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Interfaces;
using PermissionManagement.Application.Queries;
using PermissionManagement.Domain.Interfaces;

namespace PermissionManagement.Application.Handlers;

public class GetPermissionsHandler : IRequestHandler<GetPermissionsQuery, IEnumerable<PermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IElasticsearchService _elasticsearchService;
    private readonly IKafkaProducerService _kafkaProducerService;

    public GetPermissionsHandler(
        IUnitOfWork unitOfWork,
        IElasticsearchService elasticsearchService,
        IKafkaProducerService kafkaProducerService)
    {
        _unitOfWork = unitOfWork;
        _elasticsearchService = elasticsearchService;
        _kafkaProducerService = kafkaProducerService;
    }

    public async Task<IEnumerable<PermissionDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {

        var permissions = await _unitOfWork.Permissions.GetAllAsync();

        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            NombreEmpleado = p.NombreEmpleado,
            ApellidoEmpleado = p.ApellidoEmpleado,
            TipoPermiso = p.TipoPermiso,
            FechaPermiso = p.FechaPermiso,
            PermissionTypeDescription = p.PermissionType?.Description
        }).ToList();

        await SendToExternalServicesAsync(permissionDtos);

        return permissionDtos;

    }

    private async Task SendToExternalServicesAsync(List<PermissionDto> permissionDtos)
    {
        try
        {
            foreach (var dto in permissionDtos)
            {
                await _elasticsearchService.IndexPermissionAsync(dto);
            }

            var operation = new OperationDto
            {
                Id = Guid.NewGuid(),
                OperationName = "get"
            };
            await _kafkaProducerService.SendOperationAsync(operation);

        } catch (Exception ex)
        {
            Console.WriteLine($" Error al enviar a Kafka/Elasticsearch: {ex.Message}");
        }
    }
}
