using Exiled.API.Features.Pickups;
using Exiled.API.Features.Roles;
using Exiled.Events.EventArgs.Player;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;

namespace Sunrise.Features.PickupValidation;

internal static class PickupValidator
{
    internal static readonly Dictionary<Player, float> TemporaryPlayerBypass = new();
    internal static readonly Dictionary<LockerChamber, float> LockerLastInteraction = new();
    internal static readonly Dictionary<DoorVariant, float> DoorLastInteraction = new();

    internal static bool ValidateSearchStart(Player player, ItemPickupBase pickupBase)
    {
        if (!Config.Instance.PickupValidation || !pickupBase || player.Role is FpcRole { IsNoclipEnabled: true })
            return true;

        if (TemporaryPlayerBypass.TryGetValue(player, out float time) && time > Time.time)
            return true;

        Pickup? pickup = Pickup.Get(pickupBase);

        // Exiled bug - armor pickups become permanently locked if the event is denied
        if (pickup is BodyArmorPickup)
            return true;

        if (!CanPickUp(player, pickup))
            return false;
        
        return true;
    }

    static bool CanPickUp(Player player, Pickup pickup)
    {
        float bypassTime = pickup.PickupTimeForPlayer(player) + 1f;

        if (!IsObstructed(player.CameraTransform.position, pickup.Position, out _, bypassTime))
            return true;

        Bounds bounds = pickup.Base.GetComponentInChildren<Renderer>().bounds;
        Vector3 eyePos = player.CameraTransform.position;
        Vector3 direction = player.CameraTransform.forward;

        if (CanPickUpDirect(eyePos, direction, pickup))
            return true;

        bool result = CanPickupBounds(eyePos, bounds, bypassTime);

        if (!result)
        {
            Vector3 jumpEyePos = GetJumpEyePos(eyePos);

            result = CanPickupBounds(jumpEyePos, bounds, bypassTime);

            if (!result && Config.Instance.DebugPrimitives)
            {
                foreach (Vector3 point in GetCorners(bounds))
                {
                    Debug.DrawLine(eyePos, point, Colors.Red * 30, 10f);
                }

                foreach (Vector3 point in GetCorners(bounds))
                {
                    Debug.DrawLine(jumpEyePos, point, Colors.Red * 30, 10f);
                }
            }
        }

        Debug.DrawCube(bounds.center, bounds.size, result ? Colors.Green * 30 : Colors.Red * 30, 10f);

        return result;
    }

    static bool CanPickUpDirect(Vector3 eyePos, Vector3 direction, Pickup pickup)
    {
        var ray = new Ray(eyePos, direction);

        if (!Physics.Raycast(ray, out RaycastHit pickupHit, 3, (int)(Mask.HitregObstacles | Mask.Pickups)))
            return false;

        if (pickupHit.collider.gameObject.layer != (int)Layer.Pickups)
            return false;

        if (pickupHit.collider.GetComponentInParent<ItemPickupBase>() != pickup.Base)
            return false;

        Debug.DrawLine(ray.origin, pickupHit.point, Colors.Blue * 50, 15);
        return true;
    }

    static bool CanPickupBounds(Vector3 position, Bounds bounds, float bypassTime)
    {
        foreach (Vector3 corner in GetCorners(bounds))
        {
            if (!IsObstructed(position, corner, out RaycastHit hit, bypassTime))
            {
                Debug.DrawLine(position, corner, Colors.Green * 50, 10f);
                return true;
            }
            else
            {
                Debug.Log($"Hit {hit.collider?.gameObject?.name} ({hit.collider?.gameObject?.layer:G}) at {hit.point}");
            }
        }

        return false;
    }

    static Vector3 GetJumpEyePos(Vector3 eyePos)
    {
        var upRay = new Ray(eyePos, Vector3.up);
        var jumpHeight = 0.75f;

        if (Physics.Raycast(upRay, out RaycastHit hit, jumpHeight, (int)Mask.PlayerObstacles))
            jumpHeight = hit.distance - 0.05f;

        return upRay.GetPoint(jumpHeight);
    }

    static bool IsObstructed(Vector3 a, Vector3 b, out RaycastHit hit, float bypassTime) => Physics.Linecast(a, b, out hit, (int)Mask.HitregObstacles) && !CanIgnoreHit(hit, bypassTime);

    static bool CanIgnoreHit(RaycastHit hit, float bypassTime)
    {
        int layer = hit.collider.gameObject.layer;

        return (Layer)layer switch
        {
            Layer.Doors => hit.collider.gameObject.GetComponentInParent<DoorVariant>() is DoorVariant door && DoorLastInteraction.TryGetValue(door, out float time) && time + bypassTime > Time.time,
            Layer.Glass => hit.collider.GetComponentInParent<LockerChamber>() is LockerChamber locker && LockerLastInteraction.TryGetValue(locker, out float time) && time + bypassTime > Time.time,
            _ => false,
        };
    }

    static IEnumerable<Vector3> GetCorners(Bounds bounds)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        yield return center;

        yield return center + new Vector3(extents.x, extents.y, extents.z);
        yield return center + new Vector3(extents.x, extents.y, -extents.z);
        yield return center + new Vector3(extents.x, -extents.y, extents.z);
        yield return center + new Vector3(extents.x, -extents.y, -extents.z);

        yield return center + new Vector3(-extents.x, extents.y, extents.z);
        yield return center + new Vector3(-extents.x, extents.y, -extents.z);
        yield return center + new Vector3(-extents.x, -extents.y, extents.z);
        yield return center + new Vector3(-extents.x, -extents.y, -extents.z);
    }
}