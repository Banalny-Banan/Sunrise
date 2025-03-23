using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using InventorySystem.Items.Firearms.Modules;
using InventorySystem.Items.Firearms.Modules.Misc;
using Mirror;
using RelativePositioning;
using Sunrise.Utility;
using static InventorySystem.Items.Firearms.Modules.AutomaticActionModule;
using static PlayerList;

namespace Sunrise.Features.ServersideBacktrack
{
    [HarmonyPatch]
    public static class ShotMessagePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(AutomaticActionModule), nameof(AutomaticActionModule.ServerProcessCmd))]
        private static bool Automatic_ProcessCmd(NetworkReader reader, AutomaticActionModule __instance)
        {
            if (reader.ReadByte() == 0)
            {
                var shotRequest = new AutomaticActionModule.ShotRequest(reader);
                var player = Player.Get(__instance.Firearm.Owner);

                if (IsValidRotation(player, shotRequest.BacktrackData))
                    __instance._serverQueuedRequests.Enqueue(shotRequest);
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(DisruptorActionModule), nameof(DisruptorActionModule.ServerProcessCmd))]
        private static bool Disruptor_ProcessCmd(NetworkReader reader, DisruptorActionModule __instance)
        {
            DisruptorActionModule.MessageType messageType = (DisruptorActionModule.MessageType)reader.ReadByte();
            if (messageType == DisruptorActionModule.MessageType.CmdRequestStartFiring)
            {
                __instance.ServerProcessStartCmd(reader.ReadBool());
                return false;
            }

            if (messageType != DisruptorActionModule.MessageType.CmdConfirmDischarge)
                return false;

            var backtrack = new ShotBacktrackData(reader);
            var player = Player.Get(__instance.Firearm.Owner);

            if (IsValidRotation(player, backtrack))
            {
                __instance._receivedShots.Enqueue(backtrack);
                __instance.ServerUpdateShotRequests();
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(MethodType.Normal)]
        [HarmonyPatch(typeof(PumpActionModule), nameof(PumpActionModule.ServerProcessCmd))]
        private static bool Pump_ProcessCmd(NetworkReader reader, PumpActionModule __instance)
        {
            var backtrack = new ShotBacktrackData(reader);
            var player = Player.Get(__instance.Firearm.Owner);

            if (IsValidRotation(player, backtrack))
                __instance._serverQueuedShots.Enqueue(backtrack);

            return false;
        }

        private static bool IsValidRotation(Player shooter, ShotBacktrackData backtrack)
        {
            var curClientRot = shooter.GetClientRotation();
            if (curClientRot == null)
                return false;

            var claimedRotation = WaypointBase.GetWorldRotation(backtrack.RelativeOwnerPosition.WaypointId, backtrack.RelativeOwnerRotation).normalized;
            var realAngle = claimedRotation.GetDeltaRotationTo(curClientRot.ToWorldRotation()).AsMouseMove().magnitude;
            Log.Debug($"real angle is {realAngle}");

            return realAngle <= 0.012f;
        }
    }
}
