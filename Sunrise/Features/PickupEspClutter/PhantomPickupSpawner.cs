using System.Diagnostics;
using MEC;

namespace Sunrise.Features.PickupEspClutter;

internal static class PhantomItemSpawner
{
    static CoroutineHandle coroutineHandle;

    internal static void Start()
    {
        Timing.KillCoroutines(coroutineHandle);
        coroutineHandle = Timing.RunCoroutine(PhantomItemSpawnerCoroutine());
    }

    internal static void Stop()
    {
        Timing.KillCoroutines(coroutineHandle);
    }

    static IEnumerator<float> PhantomItemSpawnerCoroutine()
    {
        const int Count = 100;
        var sw = Stopwatch.StartNew();

        foreach (PhantomPickup phantomPickup in PhantomPickup.List)
        {
            phantomPickup.Destroy();
        }

        for (var i = 0; i < Count; i++)
        {
            sw.Start();
            PhantomPickup.Create();
            sw.Stop();
            yield return Timing.WaitForOneFrame;
        }

        sw.Stop();
        Debug.Log($"Spawned {Count} phantom pickups in {sw.Elapsed.TotalMilliseconds:F5}ms");

        while (true)
        {
            PhantomPickup.DestroyOldest();
            yield return Timing.WaitForSeconds(Random.Range(1, 2f));
            PhantomPickup.Create();
            yield return Timing.WaitForSeconds(Random.Range(1, 2f));
        }
    }
}