using System;
using Exiled.API.Enums;
using Exiled.Events.EventArgs.Player;

namespace Sunrise.Features.ServersideTeslaDamage;

/// <summary>
///     Makes tesla gates deal damage on server side after a short delay to account for latency.
///     In base game, clients are expected to report themselves getting damaged, which cheaters can exploit.
/// </summary>
internal class ServersideTeslaDamageModule : PluginModule
{
    protected override void OnEnabled()
    {
        TeslaGate.OnBursted += OnTeslaGateBursted;
        Handlers.Player.Hurt += OnPlayerHurt;
    }

    protected override void OnDisabled()
    {
        TeslaGate.OnBursted -= OnTeslaGateBursted;
        Handlers.Player.Hurt -= OnPlayerHurt;
    }

    protected override void OnReset()
    {
        ServersideTeslaHitreg.Dictionary.Clear();
        ServersideTeslaHitreg.ShockedPlayers.Clear();
    }

    static void OnTeslaGateBursted(TeslaGate tesla)
    {
        if (!Config.Instance.ServersideTeslaDamage)
            return;

        try
        {
            ServersideTeslaHitreg.Get(tesla).Burst();
        }
        catch (Exception e)
        {
            Log.Error($"Error in {nameof(ServersideTeslaDamageModule)}.{nameof(OnTeslaGateBursted)}: {e}");
        }
    }

    static void OnPlayerHurt(HurtEventArgs ev)
    {
        if (ev.DamageHandler.Type == DamageType.Tesla) ServersideTeslaHitreg.ShockedPlayers[ev.Player] = Time.time;
    }
}