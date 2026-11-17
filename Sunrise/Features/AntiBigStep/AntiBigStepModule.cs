using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using Sunrise.API.Detections;

namespace Sunrise.Features.AntiBigStep;

internal class AntiBigStepModule : PluginModule
{
    static readonly Dictionary<ReferenceHub, MovementData> Players = [];

    protected override void OnEnabled()
    {
        if (!Config.Instance.AntiBigStep)
        {
            Debug.Log("AntiBigStep module disabled according to the config!");
            return;
        }

        SunrisePlugin.Harmony.CreateClassProcessor(typeof(FpcFromClientMessagePatch)).Patch();
        SunrisePlugin.Harmony.CreateClassProcessor(typeof(ServerOverridePositionPatch)).Patch();

        Handlers.PlayerEvents.ChangedRole += OnChangedRole;
        Handlers.PlayerEvents.Left += OnLeft;
    }

    protected override void OnDisabled()
    {
        if (!Config.Instance.AntiBigStep)
            return;

        Handlers.PlayerEvents.ChangedRole -= OnChangedRole;
        Handlers.PlayerEvents.Left -= OnLeft;
    }

    protected override void OnReset() => Players.Clear();

    static void OnChangedRole(PlayerChangedRoleEventArgs ev) => Players.Remove(ev.Player.ReferenceHub);
    static void OnLeft(PlayerLeftEventArgs ev) => Players.Remove(ev.Player.ReferenceHub);

    internal static void OnPositionOverridden(ReferenceHub hub)
    {
        if (!Players.TryGetValue(hub, out MovementData data))
            Players[hub] = data = new();

        data.HasPosition = false;
        data.IgnoreUntil = Time.time + 0.5f;
    }

    internal static bool OnMovement(ReferenceHub hub, FpcSyncData sync)
    {
        if (!sync._bitPosition)
            return true;

        if (hub.roleManager.CurrentRole is not IFpcRole role || !role.FpcModule.ModuleReady)
            return true;

        FirstPersonMovementModule module = role.FpcModule;

        if (hub.roleManager.CurrentRole.ActiveTime < 3f || module.Noclip.IsActive || module.Noclip.RecentlyActive)
        {
            Players.Remove(hub);
            return true;
        }

        if (!Players.TryGetValue(hub, out MovementData data))
            Players[hub] = data = new();

        Vector3 position = sync._position.Position;

        float now = Time.time;
        float rise = position.y - data.Position.y;
        float elapsed = now - data.Time;
        bool hasPrevious = data.HasPosition && data.Waypoint == sync._position.WaypointId;

        data.Position = position;
        data.Time = now;
        data.Waypoint = sync._position.WaypointId;
        data.HasPosition = true;

        if (!hasPrevious || now < data.IgnoreUntil)
            return true;

        float upwardSpeed = Mathf.Max(module.JumpSpeed * module.Motor.JumpController.JumpMultiplier, module.Motor.MoveDirection.y);
        float serverDifference = position.y - module.Position.y;

        if (!IsSuspicious(rise, elapsed, upwardSpeed, serverDifference))
            return true;

        foreach (Elevator elevator in Elevator.List)
        {
            Bounds bounds = elevator.WorldSpaceRelativeBounds.Bounds;

            if (bounds.Contains(module.Position) || bounds.Contains(position))
                return true;
        }

        Player player = Player.Get(hub);

        Room.TryGetRoomAtPosition(module.Position, out Room? oldPosRoom);
        Room.TryGetRoomAtPosition(position, out Room? newPosRoom);

        int ping = LiteNetLib4MirrorServer.GetPing(player.Connection.connectionId) * 2;

        string details = $"Old position: {module.Position} Room: {oldPosRoom?.Name ?? MapGeneration.RoomName.Unnamed} {oldPosRoom?.Transform.InverseTransformPoint(module.Position) ?? Vector3.zero}\nNew position: {position} Room: {newPosRoom?.Name ?? MapGeneration.RoomName.Unnamed} {newPosRoom?.Transform.InverseTransformPoint(position) ?? Vector3.zero}\nrise: {rise:F2}, interval: {elapsed:F3}s, server difference: {serverDifference:F2}, step offset: {module.CharController.stepOffset:F3}, ping {ping}.";

        if (serverDifference > 1f && module.CharController.stepOffset > 0.225f && ping < 200)
        {
            DetectionHandler.Handle(player, "BigStep", details, Config.Instance.DetectAction);
        }
        else
        {
            DetectionHandler.Handle(player, "Possible BigStep", details, ActionType.None);

            Vector3 serverPosition = module.Position;
            player.Position = serverPosition;

            data.Position = serverPosition;
            data.HasPosition = true;
            data.IgnoreUntil = 0f;

            if (now >= data.NextWarning)
            {
                data.NextWarning = now + 1f;
                Log.Warn($"[AntiBigStep] Possible BigStep detected: Player {player.Nickname} ({player.UserId})\n{details}");
            }

            return false;
        }

        return true;
    }

    internal static bool IsSuspicious(float rise, float elapsed, float upwardSpeed, float serverDifference)
        => elapsed >= 0f && rise >= 0.75f && serverDifference >= 0.75f
            && rise > Mathf.Max(0f, upwardSpeed) * elapsed + 0.35f;

    class MovementData
    {
        public Vector3 Position;
        public float Time;
        public byte Waypoint;
        public bool HasPosition;
        public float IgnoreUntil;
        public float NextWarning;
    }
}
