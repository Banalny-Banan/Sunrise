using HarmonyLib;
using JetBrains.Annotations;
using PlayerRoles.FirstPersonControl;

namespace Sunrise.Features.ServersideBacktrack;

[HarmonyPatch(typeof(FpcMouseLook), nameof(FpcMouseLook.ApplySyncValues))] [UsedImplicitly]
internal static class FpcMouseLookPatch
{
    [UsedImplicitly]
    static void Prefix(ushort horizontal, ushort vertical, FpcMouseLook __instance)
    {
        Quaternion curRotation = ConvertToQuaternion(horizontal, vertical);
        BacktrackHistory.Get(__instance._hub).RecordEntry(__instance._hub.transform.position, curRotation);
    }

    static Quaternion ConvertToQuaternion(ushort horizontal, ushort vertical)
    {
        const float ushortToFloat = 1f / ushort.MaxValue;

        float hAngle = Mathf.Lerp(0f, 360f, horizontal * ushortToFloat);
        float vAngle = Mathf.Lerp(88f, -88f, vertical * ushortToFloat);

        return Quaternion.Euler(vAngle, hAngle, 0f);
    }
}