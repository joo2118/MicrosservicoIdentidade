using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;
using Microsoft.AspNetCore.Http;

namespace Identidade.RESTAPI.Controladores
{
    public abstract class BaseController : ControllerBase
    {
        protected readonly TelemetryClient _telemetryClient;
        protected readonly ILogger _logger;

        protected BaseController(TelemetryClient telemetryClient, ILogger logger)
        {
            _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private Dictionary<string, string> EnrichPropertiesWithEndpoint(Dictionary<string, string> properties)
        {
            properties ??= [];

            var httpContext = HttpContext;
            if (httpContext is null)
                return properties;
            var request = httpContext.Request;

            if (!properties.ContainsKey("http.method"))
                properties["http.method"] = request?.Method ?? string.Empty;

            var endpoint = httpContext.GetEndpoint();
            if (!properties.ContainsKey("httpGroute") && endpoint is not null)
                properties["http.route"] = endpoint.DisplayName ?? string.Empty;

            if (!properties.ContainsKey("http.path"))
                properties["http.path"] = request?.Path.Value ?? string.Empty;

            return properties;
        }

        private void TrackEndpointDuration(string operationName, long elapsedMilliseconds, Dictionary<string, string> properties)
        {
            var enriched = EnrichPropertiesWithEndpoint(properties);
            _telemetryClient.TrackMetric($"{operationName}_Duration", elapsedMilliseconds);

            var evento = new EventTelemetry($"{operationName}_Duration")
            {
                Timestamp = DateTimeOffset.UtcNow
            };

            foreach (var kvp in enriched)
                evento.Properties[kvp.Key] = kvp.Value;

            evento.Metrics["duration_ms"] = elapsedMilliseconds;

            _telemetryClient.TrackEvent(evento);
        }

        protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action, string operationName, Dictionary<string, string> properties = null)
        {
            var stopwatch = Stopwatch.StartNew();
            properties ??= new Dictionary<string, string>();

            try
            {
                var result = await action();

                _telemetryClient.TrackEvent($"{operationName}_Success", EnrichPropertiesWithEndpoint(properties));

                if (result is IActionResult actionResult)
                    return actionResult;

                return Ok(result);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex, EnrichPropertiesWithEndpoint(properties));
                _logger.Error(ex, $"Error executing {operationName}");

                _telemetryClient.TrackEvent($"{operationName}_Failed", EnrichPropertiesWithEndpoint(properties));

                throw;
            }
            finally
            {
                stopwatch.Stop();
                TrackEndpointDuration(operationName, stopwatch.ElapsedMilliseconds, properties);
            }
        }

        protected async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action, string operationName, Dictionary<string, string> properties = null)
        {
            var stopwatch = Stopwatch.StartNew();
            properties ??= new Dictionary<string, string>();

            try
            {
                var result = await action();

                _telemetryClient.TrackEvent($"{operationName}_Success", EnrichPropertiesWithEndpoint(properties));

                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex, EnrichPropertiesWithEndpoint(properties));
                _logger.Error(ex, $"Error executing {operationName}");

                _telemetryClient.TrackEvent($"{operationName}_Failed", EnrichPropertiesWithEndpoint(properties));

                throw;
            }
            finally
            {
                stopwatch.Stop();
                TrackEndpointDuration(operationName, stopwatch.ElapsedMilliseconds, properties);
            }
        }
    }
}
