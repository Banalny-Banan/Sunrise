using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.Events.Features;
using HarmonyLib;
using PlayerRoles.FirstPersonControl;
using RelativePositioning;

namespace Sunrise.Events
{
    [HarmonyPatch(typeof(FpcMouseLook), nameof(FpcMouseLook.ApplySyncValues))]
    class RotationReceivedPatch
    {
        public static Event<EventArgs> OnRotationReceived { get; set; } = new Event<EventArgs>();

        [HarmonyPrefix]
        private static bool Prefix(FpcMouseLook __instance, ushort horizontal, ushort vertical)
        {
            if (__instance._prevSyncH == horizontal && __instance._prevSyncV == vertical)
            {
                __instance._fpmm.Motor.RotationDetected = false;
                return false;
            }

            var curRotation = ConvertToQuaternion(horizontal, vertical);
            var prevRotation = ConvertToQuaternion(__instance._prevSyncH, __instance._prevSyncV);

            OnRotationReceived.InvokeSafely(new EventArgs(__instance, prevRotation, curRotation, __instance._fpmm.Motor.ReceivedPosition.WaypointId));

            return true;
        }

        public static Quaternion ConvertToQuaternion(ushort horizontal, ushort vertical)
        {
            float hAngle = Mathf.Lerp(0f, 360f, horizontal / 65535f);
            float vAngle = Mathf.Lerp(-88f, 88f, vertical / 65535f);

            var hRot = Quaternion.Euler(Vector3.up * hAngle).normalized;
            var vRot = Quaternion.Euler(Vector3.left * vAngle).normalized;

            return (hRot * vRot).normalized;
        }

        public class EventArgs
        {
            public FpcMouseLook Instance { get; }
            public Quaternion PreviousRelativeRotation { get; }
            public Quaternion CurrentRelativeRotation { get; }
            public byte WaypointId { get; }

            public EventArgs(FpcMouseLook instance, Quaternion previousRelativeRotation, Quaternion currentRelativeRotation, byte waypointId)
            {
                Instance = instance;
                PreviousRelativeRotation = previousRelativeRotation;
                CurrentRelativeRotation = currentRelativeRotation;
                WaypointId = waypointId;
            }
        }
    }
}
