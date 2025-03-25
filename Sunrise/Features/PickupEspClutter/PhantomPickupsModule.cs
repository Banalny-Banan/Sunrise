using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;

namespace Sunrise.Features.PickupEspClutter;

internal class PhantomPickupsModule : PluginModule
{
    public static readonly List<ItemType> PhantomItemTypes =
    [
        ItemType.SCP500,
        ItemType.Adrenaline,
        ItemType.KeycardO5,
        ItemType.KeycardFacilityManager,
        ItemType.KeycardChaosInsurgency,
    ];

    protected override void OnEnabled()
    {
        Handlers.Server.RoundStarted += OnRoundStarted;
        Handlers.Server.RoundEnded += OnRoundEnded;
        Handlers.Player.PickingUpItem += OnPickingUpItem;
    }

    protected override void OnDisabled()
    {
        Handlers.Server.RoundStarted -= OnRoundStarted;
        Handlers.Server.RoundEnded -= OnRoundEnded;
        Handlers.Player.PickingUpItem -= OnPickingUpItem;
    }

    protected override void OnReset()
    {
        PhantomPickup.Pickups.Clear();
    }

    static void OnRoundStarted()
    {
        PhantomItemSpawner.Start();
    }

    static void OnRoundEnded(RoundEndedEventArgs ev)
    {
        PhantomItemSpawner.Stop();
    }

    static void OnPickingUpItem(PickingUpItemEventArgs ev)
    {
        if (PhantomPickup.Pickups.Contains(ev.Pickup))
            ev.IsAllowed = false;
    }
}