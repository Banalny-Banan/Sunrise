using System.Linq;
using MapGeneration;

namespace Sunrise.API.Visibility;

internal static class RoomConnectionChecker
{
    static readonly float[] ConnectionCheckingOffsets = [0, -1, 1];

    public static bool AreConnected(Vector3Int coordsA, Vector3Int coordsB)
    {
        if (Vector3Int.Distance(coordsA, coordsB) > 1)
            return false;

        Vector3 posA = GetCenterPosition(coordsA);
        Vector3 posB = GetCenterPosition(coordsB);
        Vector3 direction = (posB - posA).normalized;

        return ConnectionCheckingOffsets.Any(offset => CheckOffsetConnection(posA, posB, direction, offset));
    }

    static Vector3 GetCenterPosition(Vector3Int coords) =>
        RoomIdUtils.CoordsToCenterPos(coords) + Vector3.up;

    static bool CheckOffsetConnection(Vector3 posA, Vector3 posB, Vector3 direction, float offset)
    {
        Vector3 right = Vector3.Cross(direction, Vector3.up);
        Vector3 offsetVector = right * offset;

        return CheckUnobstructedPath(posA, posA + offsetVector, posB + offsetVector - direction * 7.4f)
            || CheckUnobstructedPath(posB, posB + offsetVector, posA + offsetVector + direction * 7.4f);
    }

    static bool CheckUnobstructedPath(Vector3 start, Vector3 intermediate, Vector3 end) => !Physics.Linecast(start, intermediate, (int)Mask.DefaultColliders)
        && !Physics.Linecast(intermediate, end, (int)Mask.DefaultColliders);
}