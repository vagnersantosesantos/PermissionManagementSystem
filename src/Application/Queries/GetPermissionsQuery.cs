using MediatR;
using PermissionManagement.Application.DTOs;

namespace PermissionManagement.Application.Queries;

public class GetPermissionsQuery : IRequest<IEnumerable<PermissionDto>>
{
}
