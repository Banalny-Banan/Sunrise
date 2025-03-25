using MathExtensions = Sunrise.Utility.MathExtensions;

namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     Records a history of player's positions and rotations for later use.
/// </summary>
public class BacktrackHistory(Player player)
{
    public const float AcceptedDistance = 0.02f;
    public const float AcceptedAngle = 0.1f;

    internal static readonly Dictionary<ReferenceHub, BacktrackHistory> Dictionary = new();

    internal readonly CircularBuffer<BacktrackEntry> Entries = new((int)(Config.Instance.AccountedLatencySeconds * 60));

    public void RecordEntry(Vector3 position, Quaternion rotation)
    {
        Entries.PushFront(new(position, rotation));
    }

    /// <summary>
    ///     When players move quickly, their position history may not contain the latest data because of latency.
    ///     This method forecasts the player's position to account for this.
    /// </summary>
    void ForecastEntry()
    {
        float speed = player.Velocity.magnitude;
        Vector3 direction = player.Velocity / speed;
        float forecastDistance = speed * Config.Instance.AccountedLatencySeconds;

        if (forecastDistance < AcceptedDistance)
            return;

        if (forecastDistance > 0.4f && Physics.Raycast(player.Position, direction, out RaycastHit hit, forecastDistance, (int)Mask.PlayerObstacles))
            forecastDistance = hit.distance - 0.4f;

        forecastDistance = Mathf.Clamp01(forecastDistance);

        Vector3 forecastPosition = player.Position + direction * forecastDistance;

        BacktrackEntry forecastEntry = new(forecastPosition, player.Rotation)
        {
            Timestamp = Time.time + Config.Instance.AccountedLatencySeconds,
        };
        Entries.PushFront(forecastEntry);

        Debug.DrawPoint(forecastPosition, Color.yellow * 50);
    }

    /// <summary>
    ///     Finds the closest position and rotation to the claimed entry that were present in history.
    ///     This prevents clients from being able to shoot in directions they are not looking at.
    /// </summary>
    public void RestoreClosestEntry(BacktrackEntry claimed, bool forecast)
    {
        if (forecast)
            ForecastEntry();

        BacktrackEntry best = default;
        var minSqrDistance = float.MaxValue;
        var minAngle = float.MaxValue;
        var matchFlags = EntryMatchFlags.None;

        using IEnumerator<BacktrackEntry> enumerator = Entries.GetEnumerator(); // Entries are ordered from newest to oldest
        enumerator.MoveNext();
        BacktrackEntry newest = enumerator.Current;

        while (enumerator.MoveNext())
        {
            BacktrackEntry oldest = enumerator.Current;

            Debug.DrawLine(oldest.Position, newest.Position, Colors.Blue * 50);
            Debug.DrawLine(newest.Position, newest.Position + newest.Rotation * Vector3.forward, Colors.Blue * 50);

            if ((matchFlags & EntryMatchFlags.Position) == 0) // If we haven't found a suitable position yet
            {
                float t = MathExtensions.GetClosestT(newest.Position, oldest.Position, claimed.Position);

                Vector3 lerpedPosition = t switch
                {
                    0f => newest.Position,
                    1f => oldest.Position,
                    _ => Vector3.Lerp(newest.Position, oldest.Position, t),
                };

                float sqrDistance = MathExtensions.SqrDistance(claimed.Position, lerpedPosition);

                if (sqrDistance < minSqrDistance)
                {
                    minSqrDistance = sqrDistance;

                    // Add a little bit of client authority.
                    // Increases the accuracy without meaningfully harming the security
                    best.Position = Vector3.MoveTowards(lerpedPosition, claimed.Position, 0.1f);
                    best.Timestamp = Mathf.Lerp(newest.Timestamp, oldest.Timestamp, t);

                    if (minSqrDistance < AcceptedDistance)
                    {
                        matchFlags |= EntryMatchFlags.Position;

                        if (matchFlags == EntryMatchFlags.All)
                            break;
                    }
                }
            }

            if ((matchFlags & EntryMatchFlags.Rotation) == 0) // If we haven't found a suitable rotation yet
            {
                float angle = Quaternion.Angle(newest.Rotation, claimed.Rotation);

                if (angle < minAngle)
                {
                    minAngle = angle;

                    best.Rotation = Quaternion.RotateTowards(newest.Rotation, claimed.Rotation, 1);

                    if (minAngle < AcceptedAngle)
                    {
                        matchFlags |= EntryMatchFlags.Rotation;

                        if (matchFlags == EntryMatchFlags.All)
                            break;
                    }
                }
            }

            newest = oldest;
        }

        if (best.Timestamp != 0) // Null check struct edition
        {
            best.Restore(player);

            if (Config.Instance.Debug)
            {
                Debug.Log($"Best entry found for {player.Nickname} " +
                    $"Difference: A:{Quaternion.Angle(best.Rotation, claimed.Rotation):F5}, P:{Vector3.Distance(best.Position, claimed.Position):F} " +
                    $"Age: {best.Age * 1000:F0}ms");
            }
        }
        else
        {
            Debug.Log($"No suitable entry found for {player.Nickname}");
        }
    }

    public static BacktrackHistory Get(Player player)
    {
        return Dictionary.GetOrAdd(player.ReferenceHub, () => new(player));
    }

    public static BacktrackHistory Get(ReferenceHub hub)
    {
        return Dictionary.GetOrAdd(hub, () => new(Player.Get(hub)));
    }
}