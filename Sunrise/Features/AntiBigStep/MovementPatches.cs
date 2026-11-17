using HarmonyLib;
using Mirror;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;

namespace Sunrise.Features.AntiBigStep;

[HarmonyPatch(typeof(FpcFromClientMessage), nameof(FpcFromClientMessage.ProcessMessage))]
internal static class FpcFromClientMessagePatch
{
    static bool Prefix(FpcFromClientMessage __instance, NetworkConnection sender)
    {
        if (sender.identity == null || sender.identity.isLocalPlayer)
            return true;

        return !ReferenceHub.TryGetHubNetID(sender.identity.netId, out ReferenceHub hub)
            || AntiBigStepModule.OnMovement(hub, __instance._data);
    }
}

[HarmonyPatch(typeof(FirstPersonMovementModule), nameof(FirstPersonMovementModule.ServerOverridePosition))]
internal static class ServerOverridePositionPatch
{
    static void Postfix(FirstPersonMovementModule __instance)
    {
        AntiBigStepModule.OnPositionOverridden(__instance.Hub);
    }
}
