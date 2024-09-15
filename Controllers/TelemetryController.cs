using Microsoft.AspNetCore.Mvc;
using OtelCollectorApp.Services;
using System.Text.Json;

namespace OtelCollectorApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TelemetryController : ControllerBase
    {
        private readonly TelemetryDataService _telemetryDataService;
        private readonly ILogger<TelemetryController> _logger;

        public TelemetryController(TelemetryDataService telemetryDataService, ILogger<TelemetryController> logger)
        {
            _telemetryDataService = telemetryDataService;
            _logger = logger;
        }

        [HttpPost("trace")]
        public IActionResult ReceiveTrace([FromBody] JsonElement traceData)
        {
            _telemetryDataService.StoreTelemetryData("trace", traceData);
            _logger.LogInformation("Received trace data");
            return Ok();
        }

        [HttpPost("metric")]
        public IActionResult ReceiveMetric([FromBody] JsonElement metricData)
        {
            _telemetryDataService.StoreTelemetryData("metric", metricData);
            _logger.LogInformation("Received metric data");
            return Ok();
        }

        [HttpPost("log")]
        public IActionResult ReceiveLog([FromBody] JsonElement logData)
        {
            _telemetryDataService.StoreTelemetryData("log", logData);
            _logger.LogInformation("Received log data");
            return Ok();
        }

        [HttpGet("{type}")]
        public IActionResult GetTelemetryData(string type, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var data = _telemetryDataService.GetTelemetryData(type, from, to);
            return Ok(data);
        }

        [HttpGet("grafana/{type}")]
        public IActionResult GetDataForGrafana(string type, [FromQuery] long? from, [FromQuery] long? to)
        {
            var fromDate = from.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(from.Value).UtcDateTime : (DateTime?)null;
            var toDate = to.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(to.Value).UtcDateTime : (DateTime?)null;

            var data = _telemetryDataService.GetTelemetryData(type, fromDate, toDate);

            // Transform data into a format Grafana can understand
            var transformedData = data.Select(d => new
            {
                time = ((DateTimeOffset)d.Timestamp).ToUnixTimeMilliseconds(),
                value = JsonSerializer.Deserialize<JsonElement>(d.Data)
            }).ToList();

            return Ok(transformedData);
        }
    }
}