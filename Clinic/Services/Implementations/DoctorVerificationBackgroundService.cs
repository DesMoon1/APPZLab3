using System.Text;
using System.Text.Json;
using Clinic.Data;
using Clinic.Events;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Clinic.Services.Implementations;

public class DoctorVerificationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _hostname = "localhost";
    private const string VerifiedQueue = "doctor_verified";

    public DoctorVerificationBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(
            queue: VerifiedQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var evt = JsonSerializer.Deserialize<DoctorVerifiedEvent>(json);

                if (evt != null)
                {
                    await HandleDoctorVerifiedAsync(evt);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DoctorVerificationBackgroundService] Error: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(queue: VerifiedQueue, autoAck: true, consumer: consumer);

        Console.WriteLine("[DoctorVerificationBackgroundService] Listening for 'doctor_verified' events...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleDoctorVerifiedAsync(DoctorVerifiedEvent evt)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();

        var doctor = await dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == evt.DoctorId);
        if (doctor != null)
        {
            doctor.Verified = evt.IsVerified;

            await dbContext.SaveChangesAsync();

            Console.WriteLine($"[DoctorVerificationBackgroundService] ✅ Doctor '{doctor.FullName}' verification updated: {evt.IsVerified}");
        }
        else
        {
            Console.WriteLine($"[DoctorVerificationBackgroundService] ⚠️ Doctor ID {evt.DoctorId} not found.");
        }
    }
}