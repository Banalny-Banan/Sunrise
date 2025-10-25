using Sunrise.API.Backtracking;
using Sunrise.API.Visibility;
using Sunrise.Features.AntiWallhack;
using Sunrise.Features.DoorInteractionValidation;
using Sunrise.Features.ExploitPatches;
using Sunrise.Features.PickupValidation;
using Sunrise.Features.ServersideTeslaDamage;

namespace Sunrise.EntryPoint;

public class SunriseLoader : PluginModule
{
    protected override List<PluginModule> SubModules { get; } =
    [
        // API
        new BacktrackingModule(),
        new VisibilityModule(),

        // Features
        new AntiWallhackModule(),
        new PickupValidationModule(),
        new ServersideTeslaDamageModule(),
        new AntiDoorManipulatorModule(),
        new ExploitPatchesLoader(),
        // new PhantomPickupsModule(), // BUG: Flickering
    ];
}