using Sunrise.Features.AntiWallhack;
using Sunrise.Features.PickupValidation;
using Sunrise.Features.ServersideBacktrack;
using Sunrise.Features.ServersideTeslaDamage;
using Sunrise.Utility;

namespace Sunrise.EntryPoint;

public class SunriseLoader : PluginModule
{
    public override List<PluginModule> SubModules { get; } =
    [
        new AntiWallhackModule(),
        new PickupValidationModule(),
        new ServersideBacktrackModule(),
        new ServersideTeslaDamageModule(),
    ];
}