using Moq;
using PermissionManagement.Application.Commands;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Handlers;
using PermissionManagement.Application.Interfaces;
using PermissionManagement.Domain.Entities;
using PermissionManagement.Domain.Interfaces;
using Xunit;

namespace PermissionManagement.Tests.UnitTests;

public class RequestPermissionHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IElasticsearchService> _mockElasticsearch;
    private readonly Mock<IKafkaProducerService> _mockKafka;
    private readonly Mock<IRepository<Permission>> _mockPermissionRepo;
    private readonly RequestPermissionHandler _handler;

    public RequestPermissionHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockElasticsearch = new Mock<IElasticsearchService>();
        _mockKafka = new Mock<IKafkaProducerService>();
        _mockPermissionRepo = new Mock<IRepository<Permission>>();

        _mockUnitOfWork.Setup(u => u.Permissions).Returns(_mockPermissionRepo.Object);

        _handler = new RequestPermissionHandler(
            _mockUnitOfWork.Object,
            _mockElasticsearch.Object,
            _mockKafka.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsPermissionDto()
    {
        // Arrange
        var command = new RequestPermissionCommand
        {
            NombreEmpleado = "Juan",
            ApellidoEmpleado = "Perez",
            TipoPermiso = 1,
            FechaPermiso = DateTime.Now
        };

        var permission = new Permission
        {
            Id = 1,
            NombreEmpleado = command.NombreEmpleado,
            ApellidoEmpleado = command.ApellidoEmpleado,
            TipoPermiso = command.TipoPermiso,
            FechaPermiso = command.FechaPermiso
        };

        _mockPermissionRepo.Setup(r => r.AddAsync(It.IsAny<Permission>()))
            .ReturnsAsync(permission);

        _mockUnitOfWork.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(permission.Id, result.Id);
        Assert.Equal(command.NombreEmpleado, result.NombreEmpleado);
        Assert.Equal(command.ApellidoEmpleado, result.ApellidoEmpleado);

        _mockPermissionRepo.Verify(r => r.AddAsync(It.IsAny<Permission>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        _mockElasticsearch.Verify(e => e.IndexPermissionAsync(It.IsAny<PermissionDto>()), Times.Once);
        _mockKafka.Verify(k => k.SendOperationAsync(It.Is<OperationDto>(o => o.OperationName == "request")), Times.Once);
    }
}
