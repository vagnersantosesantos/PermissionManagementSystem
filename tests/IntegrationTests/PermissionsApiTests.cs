using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Infrastructure.Data;
using PermissionManagementSystem;
using Xunit;

namespace PermissionManagement.Tests.IntegrationTests;

public class PermissionsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public PermissionsApiTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<DbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        });

        _client = customFactory.CreateClient();

        // Seed data
        using var scope = customFactory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PermissionDbContext>();

        if (!db.PermissionTypes.Any())
        {
            db.PermissionTypes.AddRange(
                new Domain.Entities.PermissionType { Id = 1, Description = "Consultor" },
                new Domain.Entities.PermissionType { Id = 2, Description = "Líder" },
                new Domain.Entities.PermissionType { Id = 3, Description = "Gerente" },
                new Domain.Entities.PermissionType { Id = 4, Description = "Admin" }
            );
            db.SaveChanges();
        }
    }

    [Fact]
    public async Task GetPermissions_ReturnsSuccessStatusCode()
    {
        var response = await _client.GetAsync("/api/permissions");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RequestPermission_ReturnsCreatedPermission()
    {
        var command = new
        {
            nombreEmpleado = "João",
            apellidoEmpleado = "Silva",
            tipoPermiso = 1
        };

        var response = await _client.PostAsJsonAsync("/api/permissions/request", command);
        response.EnsureSuccessStatusCode();

        var permission = await response.Content.ReadFromJsonAsync<PermissionDto>();
        Assert.NotNull(permission);
        Assert.Equal("João", permission.NombreEmpleado);
    }

    [Fact]
    public async Task ModifyPermission_ReturnsUpdatedPermission()
    {
        var createCommand = new
        {
            nombreEmpleado = "Maria",
            apellidoEmpleado = "Santos",
            tipoPermiso = 2
        };

        var createResponse = await _client.PostAsJsonAsync("/api/permissions/request", createCommand);
        var created = await createResponse.Content.ReadFromJsonAsync<PermissionDto>();

        var modifyCommand = new
        {
            id = created!.Id,
            nombreEmpleado = "Maria Modificada",
            apellidoEmpleado = "Santos Silva",
            tipoPermiso = 3
        };

        var response = await _client.PutAsJsonAsync($"/api/permissions/modify/{modifyCommand.id}", modifyCommand);
        response.EnsureSuccessStatusCode();

        var modified = await response.Content.ReadFromJsonAsync<PermissionDto>();
        Assert.Equal("Maria Modificada", modified!.NombreEmpleado);
    }
}
