using Nest;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Interfaces;

namespace PermissionManagement.Infrastructure.Services;

public class ElasticsearchService : IElasticsearchService
{
    private readonly IElasticClient? _elasticClient;
    private readonly bool _isAvailable;
    private const string IndexName = "permissions";

    public ElasticsearchService(IElasticClient? elasticClient)
    {
        _elasticClient = elasticClient;
        _isAvailable = TryCreateIndexIfNotExists();
    }

    private bool TryCreateIndexIfNotExists()
    {
        if (_elasticClient == null)
        {
            Console.WriteLine("Cliente de Elasticsearch no configurado.");
            return false;
        }

        try
        {
            var existsResponse = _elasticClient.Indices.Exists(IndexName);

            if (!existsResponse.Exists)
            {
                var createIndexResponse = _elasticClient.Indices.Create(IndexName, c => c
                    .Map<PermissionDto>(m => m
                        .AutoMap()
                    )
                );

                if (!createIndexResponse.IsValid)
                {
                    Console.WriteLine($" Elasticsearch: Error al crear índice. Error: {createIndexResponse.ServerError?.Error?.Reason}");
                    return false;
                }

                Console.WriteLine($"Elasticsearch: Índice '{IndexName}' creado con éxito.");
            }
            else
            {
                Console.WriteLine($"Elasticsearch: Índice '{IndexName}' ya existe.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Elasticsearch no disponible: {ex.Message}");
            return false;
        }
    }

    public async Task IndexPermissionAsync(PermissionDto permission)
    {
        if (!_isAvailable || _elasticClient == null)
        {
            Console.WriteLine($"Elasticsearch no disponible. Permiso no indexado: {permission.NombreEmpleado} {permission.ApellidoEmpleado}");
            return;
        }

        try
        {
            var response = await _elasticClient.IndexDocumentAsync(permission);

            if (response.IsValid)
            {
                Console.WriteLine($"Permiso indexado en Elasticsearch: {permission.NombreEmpleado} {permission.ApellidoEmpleado}");
            }
            else
            {
                Console.WriteLine($"Error al indexar en Elasticsearch: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al indexar en Elasticsearch (la operación continúa): {ex.Message}");
        }
    }

    public async Task<IEnumerable<PermissionDto>> SearchPermissionsAsync(string searchTerm)
    {
        if (!_isAvailable || _elasticClient == null)
        {
            Console.WriteLine("Elasticsearch no disponible para búsqueda.");
            return Enumerable.Empty<PermissionDto>();
        }

        try
        {
            var searchResponse = await _elasticClient.SearchAsync<PermissionDto>(s => s
                .Index(IndexName)
                .Query(q => q
                    .MultiMatch(m => m
                        .Fields(f => f
                            .Field(p => p.NombreEmpleado)
                            .Field(p => p.ApellidoEmpleado)
                        )
                        .Query(searchTerm)
                    )
                )
            );

            if (searchResponse.IsValid)
            {
                Console.WriteLine($"Búsqueda en Elasticsearch exitosa. Resultados: {searchResponse.Documents.Count}");
            }

            return searchResponse.Documents;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar en Elasticsearch: {ex.Message}");
            return Enumerable.Empty<PermissionDto>();
        }
    }
}