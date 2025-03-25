using System;
using Exiled.API.Enums;
using HarmonyLib;
using JetBrains.Annotations;

namespace Sunrise.EntryPoint;

[UsedImplicitly]
public class SunrisePlugin : Plugin<Config>
{
    public override string Name { get; } = "Sunrise";
    public override string Author { get; } = "BanalnyBanan";
    public override Version RequiredExiledVersion { get; } = new(9, 5, 1);
    public override Version Version { get; } = new(2025, 3, 1);
    public override PluginPriority Priority { get; } = PluginPriority.Highest;

    public SunriseLoader Loader { get; } = new();
    public Harmony Harmony { get; } = new("Sunrise");

    public override void OnEnabled()
    {
        Loader.Enable();
        Harmony.PatchAll();
        base.OnEnabled();
    }

    public override void OnDisabled()
    {
        Loader.Disable();
        Harmony.UnpatchAll();
        base.OnDisabled();
    }
}