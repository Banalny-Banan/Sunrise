namespace Sunrise.Features.ServersideBacktrack;

/// <summary>
///     A single entry in the serverside backtrack history.
/// </summary>
public struct BacktrackEntry
{
    public BacktrackEntry(Player player)
    {
        Position = player.Position;
        Rotation = player.CameraTransform.rotation;
    }

    /// <summary>
    ///     A single entry in the serverside backtrack history.
    /// </summary>
    public BacktrackEntry(Vector3 position, Quaternion rotation)
    {
        Rotation = rotation;
        Position = position;
    }

    public Quaternion Rotation { get; set; }
    public Vector3 Position { get; set; }
    public float Timestamp { get; set; } = Time.time;

    public readonly float Age => Time.time - Timestamp;

    public readonly void Restore(Player player)
    {
        player.Transform.position = Position;
        player.CameraTransform.rotation = Rotation;
    }
}