using System;
using System.Reflection.Emit;
using Exiled.API.Features.Pickups;
using HarmonyLib;
using InventorySystem.Items.Pickups;
using InventorySystem.Searching;
using JetBrains.Annotations;
using NorthwoodLib.Pools;
using Sunrise.Features.ServersideBacktrack;

namespace Sunrise.Features.PickupValidation;

[HarmonyPatch(typeof(SearchCompletor), nameof(SearchCompletor.FromPickup)), UsedImplicitly]
internal class StartingPickUpSearch
{
    [UsedImplicitly]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        Label ret = generator.DefineLabel();
        int retIndex = newInstructions.FindIndex(x => x.opcode == OpCodes.Ret);
        newInstructions[retIndex].labels.Add(ret);

        int offset = 1;
        int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Isinst && x.operand is Type type && type == typeof(InventorySystem.Items.ICustomSearchCompletorItem)) + offset;

        newInstructions.InsertRange(index, 
        [
            new(OpCodes.Ldarg_0), // referenceHub
            new(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), [typeof(ReferenceHub)])), // var player = Player.Get(ReferenceHub)

            new(OpCodes.Ldarg_1), // itemPickupBase

            new(OpCodes.Call, Method(typeof(StartingPickUpSearch), nameof(OnSearchingPickUp))), // StartingPickUpSearch.OnSearchingPickUp(ev)

            new(OpCodes.Brfalse_S, ret), // if (!ev.IsAllowed) return;
        ]);

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    public static bool OnSearchingPickUp(Player player, ItemPickupBase itemPickupBase)
    {
        if (!Config.Instance.PickupValidation)
            return true;

        if (PickupValidator.TemporaryPlayerBypass.TryGetValue(player, out float time) && time > Time.time)
            return true;

        BacktrackEntry history = BacktrackHistory.Get(player).Entries.Front();

        if (TryGetOldRaycast(history, player.CameraTransform.position, out RaycastHit hit))
        {
            Debug.DrawLine(history.Position, hit.point);

            if (Pickup.Get(hit.transform.gameObject).Base == itemPickupBase)
                return true;
        }

        return false;
    }

    internal static bool TryGetOldRaycast(BacktrackEntry backtrack, Vector3 cameraPosition, out RaycastHit raycastHit)
    {
        Vector3 forward = backtrack.Rotation * Vector3.forward;
        return Physics.Raycast(new Ray(cameraPosition + forward * 0.3f, forward), out raycastHit, 5, (1 << 0) | (1 << 9) | (1 << 14) | (1 << 27));
    }
}