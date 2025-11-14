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
            Console.WriteLine("Elasticsearch client não configurado.");
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
                    Console.WriteLine($"Elasticsearch: Falha ao criar índice. Erro: {createIndexResponse.ServerError?.Error?.Reason}");
                    return false;
                }

                Console.WriteLine($"Elasticsearch: Índice '{IndexName}' criado com sucesso!");
            }
            else
            {
                Console.WriteLine($"Elasticsearch: Índice '{IndexName}' já existe.");
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Elasticsearch não disponível: {ex.Message}");
            return false;
        }
    }

    public async Task IndexPermissionAsync(PermissionDto permission)
    {
        if (!_isAvailable || _elasticClient == null)
        {
            Console.WriteLine($"Elasticsearch não disponível. Permissão não indexada: {permission.NombreEmpleado} {permission.ApellidoEmpleado}");
            return;
        }

        try
        {
            var response = await _elasticClient.IndexDocumentAsync(permission);

            if (response.IsValid)
            {
                Console.WriteLine($"Permissão indexada no Elasticsearch: {permission.NombreEmpleado} {permission.ApellidoEmpleado}");
            }
            else
            {
                Console.WriteLine($"Falha ao indexar no Elasticsearch: {response.ServerError?.Error?.Reason}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao indexar no Elasticsearch (operação continua): {ex.Message}");
        }
    }

    public async Task<IEnumerable<PermissionDto>> SearchPermissionsAsync(string searchTerm)
    {
        if (!_isAvailable || _elasticClient == null)
        {
            Console.WriteLine("Elasticsearch não disponível para busca.");
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

            return searchResponse.Documents;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar no Elasticsearch: {ex.Message}");
            return Enumerable.Empty<PermissionDto>();
        }
    }
}