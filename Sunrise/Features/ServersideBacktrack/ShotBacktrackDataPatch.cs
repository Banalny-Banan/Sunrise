using System;
using System.Diagnostics;
using Exiled.Events.EventArgs.Player;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules.Misc;
using JetBrains.Annotations;
using RelativePositioning;
using BaseFirearm = InventorySystem.Items.Firearms.Firearm;

namespace Sunrise.Features.ServersideBacktrack;

[HarmonyPatch(typeof(ShotBacktrackData), nameof(ShotBacktrackData.ProcessShot))] [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
internal static class BacktrackOverridePatch
{
    static Stopwatch sw = new();
    static double totalElapsed = 0;
    static int count = 0;
    static bool first = true;

    static bool Prefix(BaseFirearm firearm, Action<ReferenceHub> processingMethod, ShotBacktrackData __instance)
    {
        if (!Config.Instance.ServersideBacktrack)
            return true;

        if (Config.Instance.Debug)
            sw.Restart();

        ProcessShot(firearm, processingMethod, __instance);
        return false;
    }

    static void ProcessShot(BaseFirearm firearm, Action<ReferenceHub> processingMethod, ShotBacktrackData backtrackData)
    {
        if (!WaypointBase.TryGetWaypoint(backtrackData.RelativeOwnerPosition.WaypointId, out WaypointBase wp))
            return;

        Player player = Player.Get(firearm.Owner);
        Vector3 worldspacePosition = wp.GetWorldspacePosition(backtrackData.RelativeOwnerPosition.Relative);
        Quaternion worldspaceRotation = wp.GetWorldspaceRotation(backtrackData.RelativeOwnerRotation);
        BacktrackEntry ownerClaimed = new(worldspacePosition, worldspaceRotation);

        if (Config.Instance.Debug) // The red line shows the claimed position
        {
            BacktrackEntry prev = new(player);
            ownerClaimed.Restore(player);
            Debug.DrawLine(player.CameraTransform.position, player.CameraTransform.position + player.CameraTransform.forward * 100f, Colors.Red * 50, 15);
            prev.Restore(player);
        }

        using BacktrackProcessor attackerProcessor = new(player, ownerClaimed, true);

        // The green line shows the found position. For normal players they should match most of the time.
        Debug.DrawLine(player.CameraTransform.position, player.CameraTransform.position + player.CameraTransform.forward * 100f, Colors.Green * 50, 15);

        if (backtrackData.HasPrimaryTarget)
        {
            Player target = Player.Get(backtrackData.PrimaryTargetHub);
            BacktrackEntry targetClaimed = new(backtrackData.PrimaryTargetRelativePosition.Position, Quaternion.identity);

            using BacktrackProcessor targetProcessor = new(target, targetClaimed, false);

            ShootingEventArgs args = new(firearm, ref backtrackData);
            Handlers.Player.Shooting.InvokeSafely(args);

            if (args.IsAllowed)
                processingMethod(backtrackData.PrimaryTargetHub);
        }
        else
        {
            ShootingEventArgs args = new(firearm, ref backtrackData);
            Handlers.Player.Shooting.InvokeSafely(args);

            if (args.IsAllowed)
                processingMethod(null!);
        }
    }

#if DEBUG
    public static void Postfix(BaseFirearm firearm, Action<ReferenceHub> processingMethod)
    {
        if (!Config.Instance.Debug)
            return;

        if (first)
        {
            first = false;
            return;
        }

        sw.Stop();
        count++;
        totalElapsed += sw.Elapsed.TotalMilliseconds;

        Debug.Log($"Backtrack took {sw.Elapsed.TotalMilliseconds:F3}ms. Total for {count} shots: {totalElapsed:F5}ms. Average: {totalElapsed / count:F7}ms. Per 1000 shots: {totalElapsed / count * 1000:F5}ms.");

        // [ServersideBacktrack = true] Total for 100 shots: 56.80070ms. Average: 0.5680070ms. Per 1000 shots: 568.00700ms.
        // [ServersideBacktrack = false] Total for 100 shots: 119.02560ms. Average: 1.1902560ms. Per 1000 shots: 1190.25600ms.
        // Sunrise backtrack results in 2x performance increase (idk how lol nw code is baaaaaaaaad). Ratios reproduced on 3 different machines.
    }
#endif
}