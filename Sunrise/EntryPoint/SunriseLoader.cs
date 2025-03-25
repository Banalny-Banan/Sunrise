using Sunrise.Features.AntiWallhack;
using Sunrise.Features.DoorInteractionValidation;
using Sunrise.Features.PickupEspClutter;
using Sunrise.Features.PickupValidation;
using Sunrise.Features.ServersideBacktrack;
using Sunrise.Features.ServersideTeslaDamage;

namespace Sunrise.EntryPoint;

public class SunriseLoader : PluginModule
{
    protected override List<PluginModule> SubModules { get; } =
    [
        new AntiWallhackModule(),
        new PickupValidationModule(),
        new ServersideBacktrackModule(),
        new ServersideTeslaDamageModule(),
        new AntiDoorManipulatorModule(),
        new PhantomPickupsModule(),
    ];
}