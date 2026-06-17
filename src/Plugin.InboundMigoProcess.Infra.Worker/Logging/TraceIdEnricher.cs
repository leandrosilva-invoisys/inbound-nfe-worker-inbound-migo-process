using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Plugin.InboundMigoProcess.Infra.Worker.Logging;

/// <summary>
/// Enricher Serilog que adiciona a propriedade TraceId (X-Trace-Id) aos logs a partir do Activity atual do OpenTelemetry.
/// </summary>
public class TraceIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var traceId = Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrEmpty(traceId))
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));
    }
}
