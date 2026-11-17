using LabApi.Features.Wrappers;
using MEC;

namespace Sunrise.Features.ServersideTeslaDamage;

internal class ServersideTeslaHitreg(TeslaGate tesla)
{
    const float ShockDuration = 0.5f;
    internal const string CoroutineTag = "Sunrise.Tesla";

    internal static readonly Dictionary<TeslaGate, ServersideTeslaHitreg> Dictionary = [];
    internal static readonly Dictionary<Player, float> ShockedPlayers = [];

    readonly Bounds _bounds = new(tesla.transform.position + Vector3.up * (tesla.sizeOfKiller.y / 2f), (tesla.transform.rotation * new Vector3(4f, 0.7f, 5f)).Abs());
    readonly Collider[] _hitBuffer = new Collider[100];
    readonly List<ReferenceHub> _hitPlayers = [];

    public void Burst() => Timing.RunCoroutine(BurstCoroutine(), CoroutineTag);

    IEnumerator<float> BurstCoroutine()
    {
        yield return Timing.WaitForSeconds(Config.Instance.AccountedLatencySeconds);

        float remainingTime = ShockDuration - Config.Instance.AccountedLatencySeconds;

        Debug.DrawCube(_bounds.center, _bounds.size, Colors.Yellow * 30, remainingTime);

        while (remainingTime > 0)
        {
            UpdateBurst();
            yield return Timing.WaitForOneFrame;
            remainingTime -= Time.deltaTime;
        }

        // Here we give clients time to report the damage themselves to prevent dealing the damage twice
        yield return Timing.WaitForSeconds(Config.Instance.AccountedLatencySeconds);

        ProcessHits();
    }

    void UpdateBurst()
    {
        int count = Physics.OverlapBoxNonAlloc(_bounds.center, _bounds.extents, _hitBuffer, Quaternion.identity, (int)Mask.PlayerHitbox);

        for (var i = 0; i < count; i++)
        {
            Collider collider = _hitBuffer[i];

            if (collider.transform.TryGetComponentInParent(out ReferenceHub hub) && !_hitPlayers.Contains(hub))
                _hitPlayers.Add(hub);
        }
    }

    void ProcessHits()
    {
        foreach (ReferenceHub hub in _hitPlayers)
        {
            Player player = Player.Get(hub);

            if (player.IsDestroyed)
                continue;

            // If the player have reported the damage themselves we don't want to deal it twice
            if (ShockedPlayers.TryGetValue(player, out float shockTime) && Time.time - shockTime < ShockDuration)
                continue;

            TeslaGateController.ServerReceiveMessage(player.Connection, new(tesla));
        }

        _hitPlayers.Clear();
    }

    internal static ServersideTeslaHitreg Get(TeslaGate tesla) => Dictionary.GetOrAdd(tesla, () => new(tesla));
}
