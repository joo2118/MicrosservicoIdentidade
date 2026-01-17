using Microsoft.AspNetCore.Mvc;
using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog;

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

        protected async Task<IActionResult> ExecuteAsync<T>(Func<Task<T>> action, string operationName, Dictionary<string, string> properties = null)
        {
            var stopwatch = Stopwatch.StartNew();
            properties ??= new Dictionary<string, string>();

            try
            {
                var result = await action();
                
                _telemetryClient.TrackEvent($"{operationName}_Success", properties);
                
                if (result is IActionResult actionResult)
                    return actionResult;
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex, properties);
                _logger.Error(ex, $"Error executing {operationName}");
                
                _telemetryClient.TrackEvent($"{operationName}_Failed", properties);
                
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetryClient.TrackMetric($"{operationName}_Duration", stopwatch.ElapsedMilliseconds);
            }
        }

        protected async Task<IActionResult> ExecuteAsync(Func<Task<IActionResult>> action, string operationName, Dictionary<string, string> properties = null)
        {
            var stopwatch = Stopwatch.StartNew();
            properties ??= new Dictionary<string, string>();

            try
            {
                var result = await action();
                
                _telemetryClient.TrackEvent($"{operationName}_Success", properties);
                
                return result;
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex, properties);
                _logger.Error(ex, $"Error executing {operationName}");
                
                _telemetryClient.TrackEvent($"{operationName}_Failed", properties);
                
                throw;
            }
            finally
            {
                stopwatch.Stop();
                _telemetryClient.TrackMetric($"{operationName}_Duration", stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
