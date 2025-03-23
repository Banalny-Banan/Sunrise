using Sunrise.Events;
using Sunrise.Utility;

namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     Serverside backtrack works by recording a precise history of player positions and rotations.
///     When a player shoots, the server finds the best values from the history instead of blindly trusting the client over their position and rotation values.
///     This prevents any cheats that include shooting in a different direction than the actual one.
/// </summary>
public class ServersideBacktrackModule : PluginModule
{
    public static readonly Dictionary<Player, BacktrackHistory> BacktrackHistories = new();
    public static readonly Dictionary<Player, RelativeRotation> LastClientsRotation = new Dictionary<Player, RelativeRotation>();

    protected override void OnEnabled()
    {
        StaticUnityMethods.OnUpdate += OnUpdate;
        Handlers.Server.ReloadedConfigs += OnReset;
        RotationReceivedPatch.OnRotationReceived += OnRotationReceived;
    }

    protected override void OnDisabled()
    {
        StaticUnityMethods.OnUpdate -= OnUpdate;
        Handlers.Server.ReloadedConfigs -= OnReset;
        RotationReceivedPatch.OnRotationReceived -= OnRotationReceived;
    }

    protected override void OnReset()
    {
        BacktrackHistories.Clear();
    }

    static void OnUpdate()
    {
        if (!Config.Instance.ServersideBacktrack)
            return;

        foreach (Player player in Player.Dictionary.Values)
        {
            BacktrackHistory history = BacktrackHistories.GetOrAdd(player, () => new(player));
            history.RecordEntry();
        }
    }

    void OnRotationReceived(RotationReceivedPatch.EventArgs ev)
    {
        if (!Player.TryGet(ev.Instance?._hub, out var player) || player.GameObject == null)
            return;

        LastClientsRotation[player] = new RelativeRotation(ev.CurrentRelativeRotation, ev.WaypointId);
    }
}