using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using VerificationService.Services;
using RabbitMQ;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var rabbitMQActivitySource = new ActivitySource("VerificationService.RabbitMQ");

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Doctor Verification Service API",
        Version = "v1"
    });
});

builder.Services.AddSingleton(rabbitMQActivitySource);
builder.Services.AddSingleton<RabbitMQPublisher>(sp => 
    new RabbitMQPublisher("localhost", rabbitMQActivitySource));

builder.Services.AddSingleton<DoctorVerificationService>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "VerificationService",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = (httpContext) => { return !httpContext.Request.Path.StartsWithSegments("/health"); };
        })
        .AddHttpClientInstrumentation(options => { options.RecordException = true; })
        .AddSource("VerificationService.*")
        .AddSource("VerificationService.RabbitMQ") // Додали RabbitMQ source
        .AddZipkinExporter(options => { options.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); })
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

const string AllowFrontend = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowFrontend, policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy
                .WithOrigins(
                    "http://localhost:4200",
                    "http://127.0.0.1:4200"
                )
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Doctor Verification Service API v1"); });
}

app.UseRouting();

app.UseCors(AllowFrontend);

app.UseAuthorization();

app.MapControllers();

var verificationService = app.Services.GetRequiredService<DoctorVerificationService>();
_ = verificationService.StartListeningAsync(new CancellationToken());

app.Run();