using MediatR;
using Microsoft.AspNetCore.Mvc;
using PermissionManagement.Application.Commands;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Queries;

namespace PermissionManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PermissionsController> _logger;

    public PermissionsController(IMediator mediator, ILogger<PermissionsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Request a new permission
    /// </summary>
    [HttpPost("request")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PermissionDto>> RequestPermission([FromBody] CreatePermissionDto dto)
    {
        try
        {
            var command = new RequestPermissionCommand
            {
                NombreEmpleado = dto.NombreEmpleado,
                ApellidoEmpleado = dto.ApellidoEmpleado,
                TipoPermiso = dto.TipoPermiso,
                FechaPermiso = dto.FechaPermiso
            };

            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetPermissions), new { id = result.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting permission");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Modify an existing permission
    /// </summary>
    [HttpPut("modify/{id}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PermissionDto>> ModifyPermission(int id, [FromBody] UpdatePermissionDto dto)
    {
        try
        {
            var command = new ModifyPermissionCommand
            {
                Id = id,
                NombreEmpleado = dto.NombreEmpleado,
                ApellidoEmpleado = dto.ApellidoEmpleado,
                TipoPermiso = dto.TipoPermiso,
                FechaPermiso = dto.FechaPermiso
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Permission not found");
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error modifying permission");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all permissions
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PermissionDto>>> GetPermissions()
    {
        try
        {
            var query = new GetPermissionsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions");
            return BadRequest(new { error = ex.Message });
        }
    }
}
