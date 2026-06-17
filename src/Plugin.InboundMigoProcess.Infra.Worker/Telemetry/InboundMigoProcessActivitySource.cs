using System.Diagnostics;

namespace Plugin.InboundMigoProcess.Infra.Worker.Telemetry;

public static class InboundMigoProcessActivitySource
{
    public static readonly ActivitySource Source = new("Plugin.InboundMigoProcess.Infra.Worker", "1.0.0");
}
