using MediatR;
using PermissionManagement.Application.DTOs;

namespace PermissionManagement.Application.Queries
{
    public class GetPermissionTypesQuery : IRequest<IEnumerable<PermissionTypeDto>>
    {
    }
}
