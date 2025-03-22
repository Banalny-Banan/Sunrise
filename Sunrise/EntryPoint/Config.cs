using System.ComponentModel;
using Exiled.API.Interfaces;

namespace Sunrise.EntryPoint;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;

    [Description("Enables some visualizations for debugging")]
    public bool DebugPrimitives { get; set; } = false;

    [Description(
        """
        The maximum latency for which the server has to account for.
        Higher values give more authority to clients, lower values may decrease gameplay quality for high latency players.
        """
    )]
    public float AllowedLatencySeconds { get; set; } = 0.3f;

    [Description("Toggle features separately")]
    public bool CustomVisibility { get; set; } = true;

    public bool ServersideBacktrack { get; set; } = true;
    public bool ServersideTeslaDamage { get; set; } = true;

    public static Config Instance => Config.Instance;
}