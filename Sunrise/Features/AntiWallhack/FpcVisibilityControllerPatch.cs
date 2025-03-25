using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using CustomPlayerEffects;
using HarmonyLib;
using JetBrains.Annotations;
using MapGeneration;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps;
using PlayerRoles.PlayableScps.Scp049;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Visibility;
using Sunrise.API.Visibility;

namespace Sunrise.Features.AntiWallhack;

/*
InvisibilityFlags GetActiveFlags(ReferenceHub observer)
.locals init
[0] valuetype PlayerRoles.Visibility.InvisibilityFlags activeFlags,
[1] class PlayerRoles.FirstPersonControl.IFpcRole currentRole1,
[2] class PlayerRoles.FirstPersonControl.IFpcRole currentRole2,
[3] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 position2,
[4] float32 num,
[5] valuetype [UnityEngine.CoreModule]UnityEngine.Vector3 V_5
*/

/// <summary>
///     Wallhack nerf.
///     Works by only sending data about players in rooms that can be seen from the room the observer is currently in.
///     Reduces wallhack effective distance to around 12m (from 36m in base game)
/// </summary>
[HarmonyPatch(typeof(FpcVisibilityController), nameof(FpcVisibilityController.GetActiveFlags)), UsedImplicitly]
internal static class FpcVisibilityControllerPatch
{
    [UsedImplicitly]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = instructions.ToList();

        newInstructions.InsertRange(newInstructions.Count - 1,
        [
            // activeFlags are already on the stack ready to be returned
            new(OpCodes.Ldloc_1), // currentRole1 (observer)
            new(OpCodes.Ldloc_2), // currentRole2 (target)

            new(OpCodes.Ldarg_0),
            new(OpCodes.Ldfld, Field(typeof(FpcVisibilityController), nameof(FpcVisibilityController._scp1344Effect))), // this._scp1344Effect

            new(OpCodes.Ldloc_S, 5), // V_5 (position difference)

            new(OpCodes.Call, Method(typeof(FpcVisibilityControllerPatch), nameof(AddCustomVisibility))),
        ]);

        return newInstructions;
    }

    // Some roles' footstep sounds can be heard from a distance higher than human 12m forced visibility distance
    static float GetForcedVisibilitySqrDistance(IFpcRole targetRole, Scp1344 scp1344Effect)
    {
        int targetForcedVisibility = targetRole.FpcModule.Role.RoleTypeId switch
        {
            RoleTypeId.Scp939 or RoleTypeId.Scp173 => 5000,
            RoleTypeId.Scp106 => 29 * 29,
            _ when scp1344Effect.IsEnabled => 24 * 24,
            _ => 12 * 12,
        };

        return targetForcedVisibility;
    }

    /// <summary>
    ///     This method limits visibility diagonally when players are inside the facility.
    /// </summary>
    [SuppressMessage("ReSharper", "BitwiseOperatorOnEnumWithoutFlags")]
    static InvisibilityFlags AddCustomVisibility(InvisibilityFlags flags, IFpcRole observerRole, IFpcRole targetRole, Scp1344 scp1344Effect, Vector3 positionDifference)
    {
        // Players are out of range
        if (!Config.Instance.AntiWallhack || (flags & InvisibilityFlags.OutOfRange) != 0)
            return flags;

        if (IsExceptionalCase(observerRole, targetRole))
            return flags;

        float sqrDistance = positionDifference.sqrMagnitude;

        if (sqrDistance < GetForcedVisibilitySqrDistance(targetRole, scp1344Effect))
            return flags;

        Vector3Int observerCoords = RoomIdUtils.PositionToCoords(observerRole.FpcModule.Position);
        Vector3Int targetCoords = RoomIdUtils.PositionToCoords(targetRole.FpcModule.Position);

        if (VisibilityData.Get(observerCoords) is VisibilityData observerVisibility && !observerVisibility.IsVisible(targetCoords))
            return flags | InvisibilityFlags.OutOfRange;

        if (sqrDistance < RaycastVisibilityChecker.SqrRange && !RaycastVisibilityChecker.IsVisible(observerRole, Player.Get(targetRole.FpcModule.Hub)))
            return flags | InvisibilityFlags.OutOfRange;

        return flags;
    }

    static bool IsExceptionalCase(IFpcRole observerRole, IFpcRole targetRole)
    {
        if (observerRole.FpcModule.Noclip.IsActive)
            return true;

        // Scp049's sense ability allows to see the target through walls
        if (observerRole is Scp049Role scp049Role && scp049Role.SubroutineModule.TryGetSubroutine<Scp049SenseAbility>(out Scp049SenseAbility? sense) && sense.HasTarget && sense.Target == targetRole.FpcModule.Hub)
            return true;

        // Scp939 can hear players through walls and has its own surprisingly robust VisibilityController
        if (observerRole is Scp939Role)
            return true;

        // Remarks:
        // Scp096 ignores OutOfRange flag when enraged

        return false;
    }
}

internal static class RaycastVisibilityChecker
{
    public const float SqrRange = 400f; // 20 * 20
    const float WidthMultiplier = 3f;
    const float ForecastAmount = 0.3f;
    
    public static readonly Vector2[] VisibilityReferencePoints = 
    [
        
    ];
    
    public static bool IsVisible(IFpcRole observerRole, Player target)
    {
        Player observer = Player.Get(observerRole.FpcModule.Hub);
        Vector3 originPosition = observerRole.FpcModule.Position + observer.Velocity * ForecastAmount;
        Vector3 cameraPosition = observer.CameraTransform.position;
        Vector3 cameraRightDirection = target.CameraTransform.TransformDirection(Vector3.right);

        if (!CanPlayerPoints(originPosition, cameraPosition, cameraRightDirection))
        {
            originPosition = observerRole.FpcModule.Position + observer.Velocity * ForecastAmount;
            return CanPlayerPoints(originPosition, cameraPosition, cameraRightDirection);
        }
        
        return true;
    }

    static bool CanPlayerPoints(Vector3 originPosition, Vector3 cameraPosition, Vector3 cameraRightDirection)
    {
        foreach (Vector2 point in VisibilityReferencePoints)
        {
            if (!Physics.Linecast(originPosition + point.x * WidthMultiplier * cameraRightDirection + Vector3.up * point.y, cameraPosition, VisionInformation.VisionLayerMask))
            {
                Debug.DrawLine(originPosition + point.x * WidthMultiplier * cameraRightDirection + Vector3.up * point.y, cameraPosition, Colors.Green * 50, 10f);
                return true;
            }
        }
        return false;
    }
}