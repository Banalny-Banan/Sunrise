using MapGeneration;

namespace Sunrise.API.Visibility;

internal static class RoomVisibilityDataDebugVisualizer
{
    const float PrimitiveDuration = 15;
    static readonly Dictionary<Vector3Int, float> LastPrimitiveSpawnTimes = new();
    static readonly Dictionary<Vector3Int, Color> RoomColors = new();

    public static void DrawDebugPrimitives(VisibilityData data, Vector3Int coords)
    {
        if (Time.time < LastPrimitiveSpawnTimes.GetValueOrDefault(coords) + PrimitiveDuration)
            return;

        LastPrimitiveSpawnTimes[coords] = Time.time;
        Color color = RoomColors.TryGetValue(coords, out Color roomColor) ? roomColor : RoomColors[coords] = GenerateRandomColor();
        Vector3 origin = GetDebugOrigin(coords);

        VisualizeRoomConnections(data, origin, color);
        VisualizeRoomAxes(data, origin);
    }

    static Color GenerateRandomColor() =>
        Random.ColorHSV(0, 1, 0.7f, 1, 0.7f, 1) * 50;

    static Vector3 GetDebugOrigin(Vector3Int coords) =>
        RoomIdUtils.CoordsToCenterPos(coords) +
        (Random.insideUnitSphere * 0.5f) with { y = Random.Range(0, 0.2f) } +
        Vector3.up * 0.3f;

    static void VisualizeRoomConnections(VisibilityData data, Vector3 origin, Color color)
    {
        Debug.DrawPoint(origin, color, PrimitiveDuration);

        foreach (Vector3Int coord in data.VisibleCoords)
        {
            Debug.DrawLine(origin, RoomIdUtils.CoordsToCenterPos(coord) + origin.normalized * 0.1f, color, PrimitiveDuration);
        }
    }

    static void VisualizeRoomAxes(VisibilityData data, Vector3 origin)
    {
        Transform transform = data.Room.transform;
        origin += Vector3.up * 0.05f;

        Debug.DrawLine(origin, origin + transform.right * 0.3f, Colors.Red * 50, PrimitiveDuration);
        Debug.DrawLine(origin, origin + transform.forward * 0.3f, Colors.Blue * 50, PrimitiveDuration);
        Debug.DrawLine(origin, origin + transform.up * 0.3f, Colors.Green * 50, PrimitiveDuration);
    }
}