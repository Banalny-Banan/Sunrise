using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Exiled.API.Enums;
using Sunrise.API.Visibility;

namespace Sunrise.Features.PickupEspClutter;

internal static class PhantomPickupSynchronizer
{
    static readonly HashSet<RoomType> ExcludedRooms =
    [
        RoomType.Surface,
        
        RoomType.Hcz106,
        RoomType.HczCrossRoomWater,
        RoomType.HczStraightPipeRoom,
        RoomType.HczNuke,
        RoomType.HczHid,

        RoomType.Lcz173,
    ];

    [field: AllowNull, MaybeNull]
    static List<Room> Rooms => field ??= Room.List.Where(r => !ExcludedRooms.Contains(r.Type)).ToList();

    static int index;

    internal static void GetNextPosition(out Vector3 position, out VisibilityData visibilityData)
    {
        Room room = Rooms[index];

        const float RandomOffset = 1.5f; //todo increase
        position = room.Position + (Random.insideUnitSphere * RandomOffset) with { y = Random.Range(10, 15) };
        visibilityData = VisibilityData.Get(room, false);

        index = (index + 1) % Rooms.Count;
    }
}