using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Scp914;

namespace Sunrise.Features.PickupValidation;

internal class PickupValidationModule : PluginModule
{
    protected override void OnEnabled()
    {
        Handlers.Scp914.UpgradingInventoryItem += OnScp914UpgradingInventoryItem;
        Handlers.Player.InteractingLocker += OnInteractingLocker;
        Handlers.Player.InteractingDoor += OnInteractingDoor;
    }

    protected override void OnDisabled()
    {
        Handlers.Scp914.UpgradingInventoryItem -= OnScp914UpgradingInventoryItem;
        Handlers.Player.InteractingLocker -= OnInteractingLocker;
        Handlers.Player.InteractingDoor -= OnInteractingDoor;
    }

    protected override void OnReset()
    {
        PickupValidator.TemporaryPlayerBypass.Clear();
        PickupValidator.LockerLastInteraction.Clear();
        PickupValidator.DoorLastInteraction.Clear();
    }

    static void OnScp914UpgradingInventoryItem(UpgradingInventoryItemEventArgs ev)
    {
        PickupValidator.TemporaryPlayerBypass[ev.Player] = Time.time + 0.01f;
    }

    static void OnInteractingLocker(InteractingLockerEventArgs ev)
    {
        if (ev.IsAllowed) 
            PickupValidator.LockerLastInteraction[ev.InteractingChamber.Base] = Time.time;
    }

    static void OnInteractingDoor(InteractingDoorEventArgs ev)
    {
        if (ev.IsAllowed) 
            PickupValidator.DoorLastInteraction[ev.Door.Base] = Time.time;
    }
}