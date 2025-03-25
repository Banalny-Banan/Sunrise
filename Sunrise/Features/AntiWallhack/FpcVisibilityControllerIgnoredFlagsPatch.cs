using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using PlayerRoles.FirstPersonControl;

namespace Sunrise.Features.AntiWallhack;

[HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.IgnoredFlags), MethodType.Getter)] [UsedImplicitly]
internal static class FpcVisibilityControllerIgnoredFlagsPatch
{
    [UsedImplicitly]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        CodeMatcher matcher = new(instructions);

        //replace ldc.i4.7 with ldc.i4.6, removing OutOfRange flag immunity from SCP-1344
        matcher
            .MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_7))
            .ThrowIfInvalid("Failed to find Ldc_I4_7")
            .RemoveInstruction()
            .Insert(new CodeInstruction(OpCodes.Ldc_I4_6));
        
        return matcher.Instructions();
    }
}