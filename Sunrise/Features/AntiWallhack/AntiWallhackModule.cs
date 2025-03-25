using System;
using MapGeneration;
using Sunrise.API.Visibility;

namespace Sunrise.Features.AntiWallhack;

/// <summary>
///     Wallhack nerf.
///     Works by only sending data about players in rooms that can be seen from the room the observer is currently in.
///     Reduces wallhack effective distance to around 12m (from 36m in base game)
/// </summary>
internal class AntiWallhackModule : PluginModule
{
    protected override void OnEnabled()
    {
        SeedSynchronizer.OnGenerationStage += OnMapGenerationStage;
    }

    protected override void OnDisabled()
    {
        SeedSynchronizer.OnGenerationStage -= OnMapGenerationStage;
    }

    void OnMapGenerationStage(MapGenerationPhase mapGenerationStage)
    {
        if (mapGenerationStage == MapGenerationPhase.RelativePositioningWaypoints)
        {
            foreach (Room room in Room.List)
            {
                try
                {
                    VisibilityData.Get(room);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to Get visibility data for room {room.Type} during map generation: {e}");
                }
            }
        }
    }
}