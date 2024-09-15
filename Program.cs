using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OtelCollectorApp.Services;
using OpenTelemetry.Proto.Collector.Trace.V1;
using OpenTelemetry.Proto.Collector.Metrics.V1;
using OpenTelemetry.Proto.Collector.Logs.V1;
using Google.Protobuf;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TelemetryDataService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Add OTLP receiver endpoints
app.MapPost("/v1/traces", async (HttpContext context) =>
{
    var telemetryDataService = context.RequestServices.GetRequiredService<TelemetryDataService>();
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var request = ExportTraceServiceRequest.Parser.ParseFrom(ByteString.CopyFromUtf8(body));

        foreach (var resourceSpans in request.ResourceSpans)
        {
            telemetryDataService.StoreTelemetryData("traces", JsonSerializer.Serialize(resourceSpans));
        }
        logger.LogInformation("Received and stored trace data");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing trace data");
        return Results.StatusCode(500);
    }
});

app.MapPost("/v1/metrics", async (HttpContext context) =>
{
    var telemetryDataService = context.RequestServices.GetRequiredService<TelemetryDataService>();
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var request = ExportMetricsServiceRequest.Parser.ParseFrom(ByteString.CopyFromUtf8(body));

        foreach (var resourceMetrics in request.ResourceMetrics)
        {
            telemetryDataService.StoreTelemetryData("metrics", JsonSerializer.Serialize(resourceMetrics));
        }
        logger.LogInformation("Received and stored metric data");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing metric data");
        return Results.StatusCode(500);
    }
});

app.MapPost("/v1/logs", async (HttpContext context) =>
{
    var telemetryDataService = context.RequestServices.GetRequiredService<TelemetryDataService>();
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    try
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var request = ExportLogsServiceRequest.Parser.ParseFrom(ByteString.CopyFromUtf8(body));

        foreach (var resourceLogs in request.ResourceLogs)
        {
            telemetryDataService.StoreTelemetryData("logs", JsonSerializer.Serialize(resourceLogs));
        }
        logger.LogInformation("Received and stored log data");
        return Results.Ok();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error processing log data");
        return Results.StatusCode(500);
    }
});

app.Run();