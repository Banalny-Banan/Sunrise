using Exiled.API.Features.Pickups;
using JetBrains.Annotations;
using MEC;
using Mirror;
using NorthwoodLib.Pools;
using Sunrise.API.Visibility;

namespace Sunrise.Features.PickupEspClutter;

internal class PhantomPickup : MonoBehaviour
{
    [UsedImplicitly] public static bool DebugMode;

    readonly HashSet<Player> _hiddenFor = HashSetPool<Player>.Shared.Rent();
    NetworkIdentity _netIdentity = null!;

    LinkedListNode<PhantomPickup> _node = null!;
    Pickup _pickup = null!;

    internal static LinkedList<PhantomPickup> List { get; } = [];
    internal static HashSet<Pickup> Pickups { get; } = [];

    void Start()
    {
        _node = List.AddLast(this);
        _pickup = Pickup.Get(gameObject);
        _netIdentity = _pickup.Base.netIdentity;

        Pickups.Add(_pickup);

        Timing.RunCoroutine(Coroutine());
    }

    void OnDestroy()
    {
        List.Remove(_node);
        _pickup.Destroy();
        HashSetPool<Player>.Shared.Return(_hiddenFor);
    }

    public void Destroy()
    {
        _pickup.Destroy();
    }

    IEnumerator<float> Coroutine()
    {
        while (true)
        {
            // Choose a new position for the item
            PhantomPickupSynchronizer.GetNextPosition(out Vector3 position, out VisibilityData roomVisibilityData);
            _pickup.Position = position;

            // Wait for the item to change position to one where it wont be noticed by legit players immediately, so we can safely update visibility
            yield return Timing.WaitForSeconds(Random.Range(0.2f, 0.3f));

            // Update visibility for all players
            foreach (Player player in Player.List)
                SetVisibility(player, !IsObserving(roomVisibilityData, player));

            // Lay on the ground for some time
            yield return Timing.WaitForSeconds(Random.Range(1f, 2f));
        }
    }

    void SetVisibility(Player player, bool visibility)
    {
        switch (visibility)
        {
            case true when _hiddenFor.Remove(player):
                NetworkServer.ShowForConnection(_netIdentity, player.Connection);
                break;
            case false when _hiddenFor.Add(player):
                NetworkServer.HideForConnection(_netIdentity, player.Connection);
                break;
        }
    }

    // Whether the player will be able to see the item
    static bool IsObserving(VisibilityData visibilityData, Player player)
    {
        if (DebugMode)
            return MathExtensions.SqrDistance(player.Position, visibilityData.Room.Position) < 1.5f * 1.5f;

        return visibilityData.IsVisible(player);
    }

    internal static void DestroyOldest()
    {
        if (List.Count > 0)
            List.First.Value.Destroy();
    }

    internal static void Create()
    {
        PhantomPickupSynchronizer.GetNextPosition(out Vector3 position, out _);
        ItemType type = PhantomPickupsModule.PhantomItemTypes.RandomItem();
        var pickup = Pickup.CreateAndSpawn(type, position, Random.rotation);
        pickup.GameObject.AddComponent<PhantomPickup>();
    }
}