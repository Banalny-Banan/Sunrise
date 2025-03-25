using System;
using System.Linq;
using MapGeneration;

namespace Sunrise.API.Visibility;

public static class RoomDirectionHelper
{
    public static Vector3Int[] GetSearchDirections(Room room, out bool known)
    {
        if (RoomVisibilityConfig.KnownDirectionsRooms.TryGetValue(room.Type, out Vector3Int[]? directions))
        {
            known = true;
            return ApplyRoomRotation(directions, room.Rotation);
        }

        known = false;
        return RoomVisibilityConfig.DefaultDirections;
    }

    static Vector3Int[] ApplyRoomRotation(Vector3Int[] directions, Quaternion rotation)
    {
        return directions.Select(d =>
        {
            Vector3 rotated = rotation * d;

            return new Vector3Int(
                Mathf.RoundToInt(rotated.x),
                Mathf.RoundToInt(rotated.y),
                Mathf.RoundToInt(rotated.z)
            );
        }).ToArray();
    }

    public static void IncludeDirection(Vector3Int startCoords, Vector3Int direction, bool known, HashSet<Vector3Int> visibleCoords)
    {
        if (direction.sqrMagnitude != 1)
            throw new ArgumentException("Direction must be normalized");

        Vector3Int currentCoords = startCoords + direction;
        Vector3Int previousCoords = startCoords;

        while (RoomIdentifier.RoomsByCoordinates.TryGetValue(currentCoords, out RoomIdentifier? identifier)
            && Room.Get(identifier) is Room room
            && (RoomConnectionChecker.AreConnected(previousCoords, currentCoords) || known))
        {
            visibleCoords.UnionWith(room.Identifier.OccupiedCoords);
            previousCoords = currentCoords;
            currentCoords += direction;
            known = false; // Only first room is guaranteed when direction is known
        }
    }
}