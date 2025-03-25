using System;
using Exiled.API.Interfaces;
using MapGeneration;

namespace Sunrise.API.Visibility;

public class VisibilityData
{
    static readonly Dictionary<Vector3Int, VisibilityData> Cache = new();

    VisibilityData(Room room)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        Room = room;
        InitializeVisibilityData();
    }

    public Room Room { get; }
    public HashSet<Vector3Int> VisibleCoords { get; } = [];

    public bool IsVisible(IPosition positionObject) => IsVisible(positionObject.Position);
    public bool IsVisible(Vector3 position) => IsVisible(RoomIdUtils.PositionToCoords(position));
    public bool IsVisible(Vector3Int coords) => VisibleCoords.Contains(coords);

    void InitializeVisibilityData()
    {
        VisibilityGenerator.AddRoomAndNeighbors(VisibleCoords, Room);
        Debug.Log($"Generated visibility data for room {Room.Type}. Total visible coords: {VisibleCoords.Count}");
    }

    public static VisibilityData Get(Room room, bool allowDebug = true)
    {
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        Vector3Int coords = RoomIdUtils.PositionToCoords(room.Position);
        
        if (!Cache.TryGetValue(coords, out VisibilityData? data))
        {
            data = new(room);

            foreach (Vector3Int occupiedCoord in room.Identifier.OccupiedCoords)
                Cache[occupiedCoord] = data;
        }
        
        if (Config.Instance.DebugPrimitives && allowDebug)
            RoomVisibilityDataDebugVisualizer.DrawDebugPrimitives(data, coords);
        
        return data;
    }

    public static VisibilityData Get(Vector3Int coords, bool allowDebug = true)
    {
        if (!Cache.TryGetValue(coords, out VisibilityData? data))
        {
            Room? room = Room.Get(coords);
            data = new(room);

            foreach (Vector3Int occupiedCoord in room.Identifier.OccupiedCoords)
                Cache[occupiedCoord] = data;
        }

        if (Config.Instance.DebugPrimitives && allowDebug)
            RoomVisibilityDataDebugVisualizer.DrawDebugPrimitives(data, coords);

        return data;
    }
}