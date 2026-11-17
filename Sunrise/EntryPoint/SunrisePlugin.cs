using HarmonyLib;
using LabApi.Loader.Features.Plugins;
using System;

namespace Sunrise.EntryPoint;

public sealed class SunrisePlugin : Plugin<Config>
{
    public override string Name { get; } = "Sunrise";
    public override string Author { get; } = "ui_2506 (The author of the first Exiled versions is BanalnyBanan)";
    public override Version Version { get; } = new(2, 2, 2);
    public override string Description { get; } = "AntiCheat";
    public override Version RequiredApiVersion { get; } = LabApi.Features.LabApiProperties.CurrentVersion;

    public SunriseLoader Loader { get; } = new();
    public static Harmony Harmony { get; } = new("Sunrise");

    public override void Enable()
    {
        Loader.Enable();
    }

    public override void Disable()
    {
        Loader.Disable();
        Harmony.UnpatchAll(Harmony.Id);
    }
}
