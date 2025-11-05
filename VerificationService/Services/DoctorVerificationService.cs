using System.Diagnostics;
using System.Text;
using System.Text.Json;
using OpenTelemetry.Trace;
using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VerificationService.Event;
using VerificationService.Model;

namespace VerificationService.Services;

public class DoctorVerificationService
{
    private readonly RabbitMQPublisher _publisher;
    private readonly List<DoctorVerification> _verifications = new();
    private const string RegistrationQueue = "doctor_registration";
    private readonly string _hostname = "localhost";
    private static readonly ActivitySource ActivitySource = new("VerificationService.RabbitMQ");

    public DoctorVerificationService(RabbitMQPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task StartListeningAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory() { HostName = _hostname };
        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queue: RegistrationQueue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            string? parentTraceId = null;
            if (ea.BasicProperties?.Headers != null && ea.BasicProperties.Headers.ContainsKey("traceparent"))
            {
                parentTraceId = Encoding.UTF8.GetString((byte[])ea.BasicProperties.Headers["traceparent"]);
            }

            using var activity = ActivitySource.StartActivity(
                "RabbitMQ Consume",
                ActivityKind.Consumer,
                parentTraceId);

            activity?.SetTag("messaging.system", "rabbitmq");
            activity?.SetTag("messaging.source", RegistrationQueue);
            activity?.SetTag("messaging.operation", "receive");

            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                activity?.SetTag("messaging.message_size", message.Length);

                var evt = JsonSerializer.Deserialize<DoctorRegistrationEvent>(message);

                if (evt != null)
                {
                    activity?.SetTag("doctor.id", evt.DoctorId);
                    activity?.SetTag("doctor.name", evt.FullName);

                    var verification = new DoctorVerification
                    {
                        DoctorId = evt.DoctorId,
                        FullName = evt.FullName,
                        Certificate = evt.Certificate,
                        RegisteredAt = evt.RegistrationDate,
                        IsVerified = false
                    };

                    _verifications.Add(verification);

                    activity?.AddEvent(new ActivityEvent("Doctor verification created"));
                    Console.WriteLine($"[VerificationService] Doctor received: {evt.FullName}");
                }
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                Console.WriteLine($"[VerificationService] Error processing registration: {ex.Message}");
            }
        };

        await channel.BasicConsumeAsync(queue: RegistrationQueue, autoAck: true, consumer: consumer);

        Console.WriteLine("[VerificationService] Listening for doctor registrations...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    public async Task VerifyDoctorAsync(int doctorId, bool isVerified)
    {
        using var activity = ActivitySource.StartActivity("Verify Doctor", ActivityKind.Internal);
        activity?.SetTag("doctor.id", doctorId);
        activity?.SetTag("verification.result", isVerified);

        var verification = _verifications.FirstOrDefault(v => v.DoctorId == doctorId);

        if (verification != null)
        {
            verification.IsVerified = isVerified;
            verification.VerifiedAt = DateTime.UtcNow;

            var verifiedEvent = new DoctorVerifiedEvent
            {
                DoctorId = verification.DoctorId,
                IsVerified = verification.IsVerified,
                VerificationDate = verification.VerifiedAt.Value
            };

            await _publisher.PublishAsync("doctor_verified", verifiedEvent);

            activity?.AddEvent(new ActivityEvent("Verification event published"));
            Console.WriteLine($"[VerificationService] Doctor {verification.FullName} verified: {isVerified}");
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Doctor not found");
        }
    }

    public List<DoctorVerification> GetPendingVerifications()
    {
        return _verifications.Where(v => !v.IsVerified).ToList();
    }

    public async Task DenyDoctorsAsync(int doctorId)
    {
        using var activity = ActivitySource.StartActivity("Deny Doctor", ActivityKind.Internal);
        activity?.SetTag("doctor.id", doctorId);

        var verification = _verifications.FirstOrDefault(v => v.DoctorId == doctorId);

        if (verification != null)
        {
            _verifications.Remove(verification);
            activity?.AddEvent(new ActivityEvent("Doctor verification removed"));
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Doctor not found");
        }
    }
}