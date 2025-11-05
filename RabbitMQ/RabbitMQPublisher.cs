using System.Diagnostics;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace RabbitMQ;

public class RabbitMQPublisher
{
    private readonly string _hostname;
    private readonly ActivitySource? _activitySource;

    public RabbitMQPublisher(string hostname = "localhost", ActivitySource? activitySource = null)
    {
        _hostname = hostname;
        _activitySource = activitySource;
    }

    public async Task PublishAsync<T>(string queueName, T evt)
    {
        using var activity = _activitySource?.StartActivity("RabbitMQ Publish", ActivityKind.Producer);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", queueName);
        activity?.SetTag("messaging.operation", "publish");
        activity?.SetTag("messaging.message_type", typeof(T).Name);

        var factory = new ConnectionFactory() { HostName = _hostname };

        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        string message = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(message);

        var properties = new BasicProperties();
        properties.Headers = new Dictionary<string, object?>();
        
        if (activity != null)
        {
            properties.Headers["traceparent"] = activity.Id;
            if (!string.IsNullOrEmpty(activity.TraceStateString))
            {
                properties.Headers["tracestate"] = activity.TraceStateString;
            }
        }

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            mandatory: false,
            basicProperties: properties,
            body: body);

        activity?.AddEvent(new ActivityEvent("Message published"));
        Console.WriteLine($"[RabbitMQPublisher] Event published to queue '{queueName}': {typeof(T).Name}");
    }
}