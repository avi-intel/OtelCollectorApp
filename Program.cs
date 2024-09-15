using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OtelCollectorApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add OpenTelemetry services
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("OtelCollectorApp"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:6000"); // Update this to match your application's port
        }))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:6000"); // Update this to match your application's port
        }));

builder.Services.AddSingleton<TelemetryDataService>();

// Configure logging
builder.Logging.AddOpenTelemetry(options =>
{
    options.AddOtlpExporter(options =>
    {
        options.Endpoint = new Uri("http://localhost:6000"); // Update this to match your application's port
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();