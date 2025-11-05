using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Hubs;
using NotificationService.Services;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.SetIsOriginAllowed(origin => true) 
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); 
    });
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<ConsultationNotificationService>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource
        .AddService(
            serviceName: "NotificationService",
            serviceVersion: "1.0.0",
            serviceInstanceId: Environment.MachineName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
            options.Filter = (httpContext) => { return !httpContext.Request.Path.StartsWithSegments("/health"); };
        })
        .AddHttpClientInstrumentation(options => { options.RecordException = true; })
        .AddSource("OnlineClinic.*")
        .AddZipkinExporter(options => { options.Endpoint = new Uri("http://localhost:9411/api/v2/spans"); })
    )
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());


var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors();

app.MapHub<NotificationHub>("/notifications");

app.Run();