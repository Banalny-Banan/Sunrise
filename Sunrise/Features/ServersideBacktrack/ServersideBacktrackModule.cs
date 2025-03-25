namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     Serverside backtrack works by recording a precise history of player positions and rotations.
///     When a player shoots, the server finds the best values from the history instead of blindly trusting the client over their position and rotation values.
///     This prevents any cheats that include shooting in a different direction than the actual one.
/// </summary>
internal class ServersideBacktrackModule : PluginModule
{
    protected override void OnEnabled()
    {
        Handlers.Server.ReloadedConfigs += OnReset;
    }

    protected override void OnDisabled()
    {
        Handlers.Server.ReloadedConfigs -= OnReset;
    }

    protected override void OnReset()
    {
        BacktrackHistory.Dictionary.Clear();
    }
}