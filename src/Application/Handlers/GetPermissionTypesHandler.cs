using MediatR;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Queries;
using PermissionManagement.Domain.Interfaces;

namespace PermissionManagement.Application.Handlers
{
    public class GetPermissionTypesHandler : IRequestHandler<GetPermissionTypesQuery, IEnumerable<PermissionTypeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPermissionTypesHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PermissionTypeDto>> Handle(GetPermissionTypesQuery request, CancellationToken cancellationToken)
        {
            var permissionTypes = await _unitOfWork.PermissionTypes.GetAllAsync();

            var dtos = permissionTypes.Select(pt => new PermissionTypeDto
            {
                Id = pt.Id,
                Description = pt.Description
            }).ToList();

            return dtos;
        }
    }
}
