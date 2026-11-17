using LabApi.Events.Arguments.PlayerEvents;
using PlayerStatsSystem;
using System;
using MEC;

namespace Sunrise.Features.ServersideTeslaDamage;

/// <summary>
///     Makes tesla gates deal damage on server side after a short delay to account for latency.
///     In base game, clients are expected to report themselves getting damaged, which cheaters can exploit.
/// </summary>
internal class ServersideTeslaDamageModule : PluginModule
{
    protected override void OnEnabled()
    {
        if (!Config.Instance.ServersideTeslaDamage)
        {
            Debug.Log("ServersideTeslaDamage module disabled according to the config!");
            return;
        }

        TeslaGate.OnBursted += OnTeslaGateBursted;
        Handlers.PlayerEvents.Hurt += OnPlayerHurt;
    }

    protected override void OnDisabled()
    {
        TeslaGate.OnBursted -= OnTeslaGateBursted;
        Handlers.PlayerEvents.Hurt -= OnPlayerHurt;
    }

    protected override void OnReset()
    {
        Timing.KillCoroutines(ServersideTeslaHitreg.CoroutineTag);
        ServersideTeslaHitreg.Dictionary.Clear();
        ServersideTeslaHitreg.ShockedPlayers.Clear();
    }

    static void OnTeslaGateBursted(TeslaGate tesla)
    {
        if (tesla == null)
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

    static void OnPlayerHurt(PlayerHurtEventArgs ev)
    {
        if (ev.DamageHandler is UniversalDamageHandler uni && uni.TranslationId == DeathTranslations.Tesla.Id)
            ServersideTeslaHitreg.ShockedPlayers[ev.Player] = Time.time;
    }
}
