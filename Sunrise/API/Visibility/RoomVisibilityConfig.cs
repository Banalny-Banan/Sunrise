using Exiled.API.Enums;

namespace Sunrise.API.Visibility;

internal static class RoomVisibilityConfig
{
    public static readonly Dictionary<RoomType, Vector3Int[]> KnownDirectionsRooms = new()
    {
        [RoomType.HczCornerDeep] = [Vector3Int.back, Vector3Int.right],
        [RoomType.HczNuke] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomType.HczCrossRoomWater] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left, Vector3Int.right],
        [RoomType.HczArmory] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomType.HczIntersectionJunk] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],

        [RoomType.Hcz079] = [Vector3Int.left],
        [RoomType.HczIntersection] = [Vector3Int.forward, Vector3Int.back, Vector3Int.left],
        [RoomType.HczHid] = [Vector3Int.left, Vector3Int.right],

        [RoomType.EzSmallrooms] = [Vector3Int.forward, Vector3Int.back],
        [RoomType.EzIntercom] = [Vector3Int.left, Vector3Int.back],
    };

    public static readonly HashSet<RoomType> DiagonalVisibilityRooms =
    [
        RoomType.HczCornerDeep,
        RoomType.HczNuke,
        RoomType.HczCrossRoomWater,
        RoomType.HczArmory,
        RoomType.HczIntersectionJunk,
    ];

    public static readonly Vector3Int[] DefaultDirections =
    [
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
    ];
}