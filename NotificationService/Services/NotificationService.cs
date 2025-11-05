using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using NotificationService.Events;
using NotificationService.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Services;

public class ConsultationNotificationService : BackgroundService
{
    private const string QueueName = "consultation_created";
    private readonly IHubContext<NotificationHub> _hubContext;

    public ConsultationNotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<ConsultationCreatedEvent>(message);

                if (evt == null)
                {
                    Console.WriteLine("[NotificationService] Отримано некоректну подію.");
                    return;
                }

                Console.WriteLine($"[NotificationService] Нова консультація: Id={evt.ConsultationId} {evt.DoctorId} {evt.PatientId}");

                await _hubContext.Clients.All.SendAsync("ReceiveConsultationNotification", evt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NotificationService] Помилка обробки події: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: true,
            consumer: consumer);

        Console.WriteLine("[NotificationService] Слухаємо події...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}