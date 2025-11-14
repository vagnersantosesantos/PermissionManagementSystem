using Moq;
using PermissionManagement.Application.Commands;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Handlers;
using PermissionManagement.Application.Interfaces;
using PermissionManagement.Domain.Entities;
using PermissionManagement.Domain.Interfaces;
using Xunit;

namespace PermissionManagement.Tests.Handlers
{
    public class ModifyPermissionHandlerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRepository<Permission>> _permissionRepoMock;
        private readonly Mock<IElasticsearchService> _elasticsearchMock;
        private readonly Mock<IKafkaProducerService> _kafkaMock;
        private readonly ModifyPermissionHandler _handler;

        public ModifyPermissionHandlerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _permissionRepoMock = new Mock<IRepository<Permission>>();
            _elasticsearchMock = new Mock<IElasticsearchService>();
            _kafkaMock = new Mock<IKafkaProducerService>();

            _unitOfWorkMock.SetupGet(u => u.Permissions).Returns(_permissionRepoMock.Object);

            _handler = new ModifyPermissionHandler(
                _unitOfWorkMock.Object,
                _elasticsearchMock.Object,
                _kafkaMock.Object
            );
        }

        [Fact]
        public async Task Handle_Should_Update_Existing_Permission_And_Return_Dto()
        {
            // Arrange
            var existingPermission = new Permission
            {
                Id = 1,
                NombreEmpleado = "João",
                ApellidoEmpleado = "Silva",
                TipoPermiso = 1,
                FechaPermiso = DateTime.UtcNow.AddDays(-2)
            };

            var command = new ModifyPermissionCommand
            {
                Id = 1,
                NombreEmpleado = "Maria",
                ApellidoEmpleado = "Santos",
                TipoPermiso = 1,
                FechaPermiso = DateTime.UtcNow
            };

            _permissionRepoMock.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existingPermission);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(command.Id, result.Id);
            Assert.Equal("Maria", result.NombreEmpleado);
            Assert.Equal("Santos", result.ApellidoEmpleado);
            Assert.Equal(1, result.TipoPermiso);

            _permissionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Permission>()), Times.Once);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
            _elasticsearchMock.Verify(e => e.IndexPermissionAsync(It.IsAny<PermissionDto>()), Times.Once);
            _kafkaMock.Verify(k => k.SendOperationAsync(It.Is<OperationDto>(o => o.OperationName == "modify")), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_Throw_When_Permission_Not_Found()
        {
            // Arrange
            _permissionRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Permission)null!);

            var command = new ModifyPermissionCommand
            {
                Id = 99,
                NombreEmpleado = "Inexistente",
                ApellidoEmpleado = "Teste",
                FechaPermiso = DateTime.Now,
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _permissionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Permission>()), Times.Never);
            _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
            _elasticsearchMock.Verify(e => e.IndexPermissionAsync(It.IsAny<PermissionDto>()), Times.Never);
            _kafkaMock.Verify(k => k.SendOperationAsync(It.IsAny<OperationDto>()), Times.Never);
        }
    }
}
