using System.ComponentModel;
using Exiled.API.Interfaces;
using JetBrains.Annotations;

namespace Sunrise.EntryPoint;

[UsedImplicitly]
public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public bool Debug { get; set; } = false;

    [Description("Enables some visual debugging features.")]
    public bool DebugPrimitives { get; set; } = false;

    [Description("The maximum latency for which the server has to account. Higher values give more authority to clients, lower values may decrease gameplay quality for players with higher latency.")]
    public float AccountedLatencySeconds { get; set; } = 0.3f;

    [Description("Toggle features separately:\nSignificantly reduces wallhack usefulness. Performance impact negligible.")]
    public bool AntiWallhack { get; set; } = true;

    [Description("Completely cuts off wallhack past 12m range using precise line-of-sight checks. Performance impact minimal: ~1.3% of server time consumed with 30 players in worst case positions.")]
    public bool RaycastVisibilityValidation { get; set; } = true;

    [Description("Prevents picking up items through walls. Performance impact negligible.")]
    public bool PickupValidation { get; set; } = true;

    [Description("Prevents silent-aim and spinbots. Actually improves performance.")]
    public bool ServersideBacktrack { get; set; } = true;

    [Description("Prevents cheaters from ignoring tesla damage. Performance impact negligible.")]
    public bool ServersideTeslaDamage { get; set; } = true;

    [Description("Prevents cheaters from interacting with doors they're not looking at. Performance impact negligible.")]
    public bool DoorInteractionValidation { get; set; } = true;

    // TODO fix
    /*[Description("Clutters ESP cheats with phantom pickups that disappear when players get close. Performance impact minimal.")]
    public bool PhantomPickups { get; set; } = true;*/

    #region Singleton

    public Config() => Instance = this;
    public static Config Instance { get; private set; } = null!;

    #endregion
}