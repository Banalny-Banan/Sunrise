using HarmonyLib;
using JetBrains.Annotations;
using MEC;
using Mirror;
using PlayerRoles.PlayableScps.Scp939;
using RelativePositioning;
using Utils.Networking;

namespace Sunrise.Features.CommonPatches;

// void ServerProcessCmd(NetworkReader reader)
[HarmonyPatch(typeof(Scp939LungeAbility), nameof(Scp939LungeAbility.ServerProcessCmd))] [UsedImplicitly]
public static class Scp939LungePatch
{
    [UsedImplicitly]
    public static bool Prefix(NetworkReader reader, Scp939LungeAbility __instance)
    {
        if (__instance.State != Scp939LungeState.Triggered)
        {
            ConsumeSubroutineData(reader);
            return false;
        }

        return true;
    }

    static void ConsumeSubroutineData(NetworkReader reader)
    {
        reader.ReadRelativePosition();
        reader.ReadReferenceHub();
        reader.ReadRelativePosition();
    }
}