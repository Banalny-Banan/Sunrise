namespace Sunrise.Features.PickupValidation
{
    using System.Collections.Generic;
    using System.Reflection.Emit;

    using Exiled.API.Features;
    using Exiled.API.Features.Pickups;
    using Exiled.API.Features.Pools;

    using HarmonyLib;

    using InventorySystem.Items.Pickups;
    using InventorySystem.Searching;

    using Sunrise.Features.ServersideBacktrack;

    using static HarmonyLib.AccessTools;

    public class PickUpSearchEventArgs
    {
        public bool IsAllowed { get; set; } = true;
        public Player Player { get; set; }
        public ItemPickupBase ItemPickupBase { get; set; }

        public PickUpSearchEventArgs(Player player, ItemPickupBase itemPickupBase)
        {
            Player = player;
            ItemPickupBase = itemPickupBase;
        }
    }

    [HarmonyPatch(typeof(SearchCompletor), nameof(SearchCompletor.FromPickup))]
    internal class PickUpSearch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Pool.Get(instructions);

            Label ret = generator.DefineLabel();
            int retIndex = newInstructions.FindIndex(x => x.opcode == OpCodes.Ret);
            newInstructions[retIndex].labels.Add(ret);

            LocalBuilder ev = generator.DeclareLocal(typeof(PickUpSearchEventArgs));

            newInstructions.InsertRange(14, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0), // ReferenceHub
                new CodeInstruction(OpCodes.Call, Method(typeof(Player), nameof(Player.Get), new[] { typeof(ReferenceHub) })), // Get Player from ReferenceHub

                new CodeInstruction(OpCodes.Ldarg_1), // ItemPickupBase

                new CodeInstruction(OpCodes.Newobj, GetDeclaredConstructors(typeof(PickUpSearchEventArgs))[0]), // Сфывцф

                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Stloc_S, ev.LocalIndex),

                new CodeInstruction(OpCodes.Call, Method(typeof(PickUpSearch), nameof(PickUpSearch.OnSearchingPickUp))),
                new CodeInstruction(OpCodes.Ldloc_S, ev.LocalIndex),

                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(PickUpSearchEventArgs), nameof(PickUpSearchEventArgs.IsAllowed))),
                new CodeInstruction(OpCodes.Brfalse_S, ret),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Pool.Return(newInstructions);
        }

        public static void OnSearchingPickUp(PickUpSearchEventArgs ev)
        {
            if (!Config.Instance.PickupValidation)
                return;

            if (PickupValidator.TemporaryPlayerBypass.TryGetValue(ev.Player, out float time) && time > Time.time)
                return;

            BacktrackEntry history = BacktrackHistory.Get(ev.Player).Entries.Front();

            if (TryGetOldRaycast(history, ev.Player.CameraTransform.position, out RaycastHit hit))
            {
                Debug.DrawLine(history.Position, hit.point);

                if (Pickup.Get(hit.transform.gameObject).Base == ev.ItemPickupBase)
                    return;
            }

            ev.IsAllowed = false;
        }

        internal static bool TryGetOldRaycast(BacktrackEntry backtrack, Vector3 cameraPosition, out RaycastHit raycastHit)
        {
            Vector3 forward = backtrack.Rotation * Vector3.forward;
            return Physics.Raycast(new Ray(cameraPosition + (forward * 0.3f), forward), out raycastHit, 5, (1 << 0) | (1 << 9) | (1 << 14) | (1 << 27));
        }
    }
}