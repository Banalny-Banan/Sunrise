using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Features.Wrappers;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sunrise.API.Detections;

internal class DetectionHandler : PluginModule
{
    static readonly Dictionary<Player, float> ReportCoolDown = [];
    static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(10) };

    protected override void OnEnabled() => Handlers.PlayerEvents.Left += OnLeft;
    protected override void OnDisabled() => Handlers.PlayerEvents.Left -= OnLeft;
    protected override void OnReset() => ReportCoolDown.Clear();

    static void OnLeft(PlayerLeftEventArgs ev) => ReportCoolDown.Remove(ev.Player);

    internal static void Handle(Player player, string detection, string details, ActionType action)
    {
        if (ReportCoolDown.TryGetValue(player, out float nextReport) && Time.time < nextReport)
            return;

        ReportCoolDown[player] = Time.time + 10f;

        string reason = $"[Sunrise] {detection} detected!";
        string report = $"{reason}\nPlayer [{player.PlayerId}] {player.Nickname} ({player.UserId})\n{details}";

        Log.Warn(report);
        
        if (!string.IsNullOrWhiteSpace(Config.Instance.DetectWebhook))
            _ = SendWebhook(Config.Instance.DetectWebhook, report);

        if (Config.Instance.DetectReport)
            SendReport(player, $"{reason}\n{details}");

        switch (action)
        {
            case ActionType.Kick:
                if (!player.Kick(reason))
                    Log.Warn($"[DetectionHandler] Kick was rejected for {player.UserId}.");

                break;

            case ActionType.Ban:
                if (Config.Instance.DetectBanDuration <= 0)
                {
                    Log.Warn($"[DetectionHandler] DetectBanDuration must be between 1 and {uint.MaxValue} seconds. No punishment applied.");
                    break;
                }

                if (!player.Ban(reason, Config.Instance.DetectBanDuration))
                    Log.Warn($"[DetectionHandler] Ban was rejected for {player.UserId}.");

                break;

            case ActionType.Kill:
                if (!player.Kill(reason))
                    Log.Warn($"[DetectionHandler] Kill was rejected for {player.UserId}.");

                break;
        }
    }

    static void SendReport(Player player, string reason)
    {
        if (Server.Host is not Player host)
            return;

        PlayerReportingCheaterEventArgs ev = new(host.ReferenceHub, player.ReferenceHub, reason);
        Handlers.PlayerEvents.OnReportingCheater(ev);

        if (!ev.IsAllowed)
            return;

        host.ReferenceHub.GetComponent<CheaterReport>().SendStaffChatNotification(host.UserId, player.UserId, ev.Reason, "Sunrise Anti-Cheat", player.Nickname);
        Handlers.PlayerEvents.OnReportedCheater(new PlayerReportedCheaterEventArgs(host.ReferenceHub, player.ReferenceHub, ev.Reason));
    }

    static async Task SendWebhook(string webhook, string report)
    {
        if (string.IsNullOrWhiteSpace(webhook))
            return;

        try
        {
            string json = Utf8Json.JsonSerializer.ToJsonString(new
            {
                content = report.Length > 2000 ? report.Substring(0, 2000) : report,
                allowed_mentions = new { parse = Array.Empty<string>() },
            });

            using StringContent content = new(json, Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await Client.PostAsync(webhook, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                Log.Warn($"[DetectionHandler] Webhook returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception exception)
        {
            Log.Warn($"[DetectionHandler] Webhook failed ({exception.GetType().Name}).");
        }
    }
}

public enum ActionType
{
    None,
    Kill,
    Kick,
    Ban
}