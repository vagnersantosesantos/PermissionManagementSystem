using Confluent.Kafka;
using PermissionManagement.Application.DTOs;
using PermissionManagement.Application.Interfaces;
using System.Text.Json;

namespace PermissionManagement.Infrastructure.Services;

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string>? _producer;
    private const string TopicName = "permission-operations";

    public KafkaProducerService(IProducer<string, string>? producer)
    {
        _producer = producer;
    }

    public async Task SendOperationAsync(OperationDto operation)
    {
        if (_producer == null)
        {
            Console.WriteLine($"Kafka no disponible. Operación no enviada: {operation.OperationName} (ID: {operation.Id})");
            return;
        }

        try
        {
            var message = new Message<string, string>
            {
                Key = operation.Id.ToString(),
                Value = JsonSerializer.Serialize(operation)
            };

            var result = await _producer.ProduceAsync(TopicName, message);

            if (result.Status == PersistenceStatus.Persisted)
            {
                Console.WriteLine($"Mensaje enviado a Kafka: {operation.OperationName} (ID: {operation.Id})");
            }
            else
            {
                Console.WriteLine($"Kafka - Estado no persistido: {result.Status}");
            }
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine($"Error al enviar a Kafka (la operación continúa): {ex.Error.Reason}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar a Kafka (la operación continúa): {ex.Message}");
        }
    }
}