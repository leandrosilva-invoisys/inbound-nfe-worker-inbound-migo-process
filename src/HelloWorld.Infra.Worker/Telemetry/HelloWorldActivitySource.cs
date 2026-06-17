using System.Diagnostics;

namespace HelloWorld.Infra.Worker.Telemetry;

/// <summary>
/// ActivitySource compartilhado para o worker e consumer, usado pelo OpenTelemetry para gerar TraceId/SpanId.
/// </summary>
public static class HelloWorldActivitySource
{
    public static readonly ActivitySource Source = new("HelloWorld.Infra.Worker", "1.0.0");
}
