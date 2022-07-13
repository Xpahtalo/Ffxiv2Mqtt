using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data;

internal class PlayerInfo : Topic, IDisposable
{
    private bool forcePublish;
    private uint classJobId;
    private byte level;
    private uint maxHp;
    private uint maxMp;
    private uint maxGp;
    private uint maxCp;

    protected override     string        TopicPath    => "Player/Info";
    protected override     bool          Retained     => true;
    [PluginService] public PlayerEvents? PlayerEvents { get; set; }

    public override void Initialize()
    {
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
        PlayerEvents!.OnJobChange        += JobChanged;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        var shouldPublish = false;
        TestValue(localPlayer.ClassJob.Id, ref classJobId, ref shouldPublish);
        TestValue(localPlayer.Level,       ref level,      ref shouldPublish);
        TestValue(localPlayer.MaxHp,       ref maxHp,      ref shouldPublish);
        TestValue(localPlayer.MaxMp,       ref maxMp,      ref shouldPublish);
        TestValue(localPlayer.MaxCp,       ref maxCp,      ref shouldPublish);
        TestValue(localPlayer.MaxGp,       ref maxGp,      ref shouldPublish);

        if (shouldPublish || forcePublish) {
            forcePublish = false;
            var payload =
                $"{{\"Class\":\"{localPlayer.ClassJob.GameData?.Abbreviation ?? ""}\"," +
                $"\"ClassId\":{localPlayer.ClassJob.Id},"                               +
                $"\"Level\":{localPlayer.Level},"                                       +
                $"\"MaxHP\":{localPlayer.MaxHp},"                                       +
                $"\"MaxMP\":{localPlayer.MaxMp},"                                       +
                $"\"MaxCP\":{localPlayer.MaxCp},"                                       +
                $"\"MaxGP\":{localPlayer.MaxGp}}}";
            Publish(payload);
            // Publish(new
            //         {
            //             Class   = localPlayer.ClassJob.GameData?.Abbreviation ?? "",
            //             ClassId = localPlayer.ClassJob.Id,
            //             localPlayer.Level,
            //             MaxHP   = localPlayer.MaxHp,
            //             MaxMP   = localPlayer.MaxMp,
            //             MaxCP   = localPlayer.MaxCp,
            //             MaxGP   = localPlayer.MaxGp,
            //         });
        }
    }

    private void JobChanged(Job previousJob, Job currentJob)
    {
        forcePublish = true;
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
        PlayerEvents!.OnJobChange        -= JobChanged;
    }
}
