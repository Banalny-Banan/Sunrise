using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Enums;
using PlayerRoles.FirstPersonControl;
using LabApi.Features.Wrappers;
using Interactables;
using Sunrise.API.Backtracking;

namespace Sunrise.Features.DoorInteractionValidation;

internal class AntiDoorManipulatorModule : PluginModule
{
    protected override void OnEnabled()
    {
        if (!Config.Instance.DoorInteractionValidation)
        {
            Debug.Log("DoorInteractionValidation module disabled according to the config!");
            return;
        }

        Handlers.PlayerEvents.InteractingDoor += OnInteractingDoor;
    }

    protected override void OnDisabled()
    {
        Handlers.PlayerEvents.InteractingDoor -= OnInteractingDoor;
    }

    static void OnInteractingDoor(PlayerInteractingDoorEventArgs ev)
    {
        if (ev.Player.RoleBase is not IFpcRole || ev.Player.IsNoclipEnabled || ev.Door.DoorName == DoorName.Lcz330Chamber)
            return;

        if (!CanInteract(ev.Player, ev))
        {
            if (Config.Instance.Debug)
                ev.CanOpen = false;
            else
                ev.IsAllowed = false;
        }
    }

    static bool CanInteract(Player player, PlayerInteractingDoorEventArgs ev)
    {
        Vector3 forward = BacktrackHistory.Get(player).LatestForward;

        if (InteractableCollider.AllInstances.TryGetValue(ev.Door.Base, out Dictionary<byte, InteractableCollider> buttons))
        {
            foreach (InteractableCollider button in buttons.Values)
            {
                if (LooksAtCollider(player, forward, button.transform.position + button.transform.TransformDirection(button.VerificationOffset)))
                    return true;
            }
        }

        foreach (BoxCollider collider in ev.Door.Base.AllColliders)
        {
            if (LooksAtCollider(player, forward, collider.transform.TransformPoint(collider.center)))
                return true;
        }

        Ray ray = new(player.Camera.position, forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 3, (int)(Mask.Doors | Mask.DoorButtons | Mask.Glass)))
        {
            Debug.DrawLine(ray.origin, hit.point, Colors.Yellow * 50, 15);
            return true;
        }

        Debug.Log($"Door interaction blocked. Player: {player.Nickname}, Door: {ev.Door.Position}");
        return false;
    }

    static bool LooksAtCollider(Player player, Vector3 forward, Vector3 colliderPos)
    {
        const float AllowedAngle = 30;

        Vector3 direction = (colliderPos - player.Camera.position).normalized;
        float angle = Vector3.Angle(forward with { y = direction.y }, direction);

        if (angle < AllowedAngle)
            return true;

        return false;
    }
}
