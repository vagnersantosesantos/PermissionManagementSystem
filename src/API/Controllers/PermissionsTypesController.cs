using MediatR;
using Microsoft.AspNetCore.Mvc;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Queries;

namespace PermissionManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PermissionsTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("types")]
        public async Task<ActionResult<IEnumerable<PermissionTypeDto>>> GetPermissionTypes()
        {
            var query = new GetPermissionTypesQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

    }
}
