using PermissionManagement.Application.DTOs;

namespace PermissionManagement.Application.Interfaces;

public interface IElasticsearchService
{
    Task IndexPermissionAsync(PermissionDto permission);
    Task<IEnumerable<PermissionDto>> SearchPermissionsAsync(string searchTerm);
}

public interface IKafkaProducerService
{
    Task SendOperationAsync(OperationDto operation);
}
