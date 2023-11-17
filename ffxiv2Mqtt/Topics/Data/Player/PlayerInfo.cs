using System;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

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

    public PlayerInfo()
    {
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
        Service.PlayerEvents.OnJobChange        += JobChanged;
    }

    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        var shouldPublish = false;
        TestValue(localPlayer.ClassJob.Id, ref classJobId, ref shouldPublish);
        TestValue(localPlayer.Level,       ref level,      ref shouldPublish);
        TestValue(localPlayer.MaxHp,       ref maxHp,      ref shouldPublish);
        TestValue(localPlayer.MaxMp,       ref maxMp,      ref shouldPublish);
        TestValue(localPlayer.MaxCp,       ref maxCp,      ref shouldPublish);
        TestValue(localPlayer.MaxGp,       ref maxGp,      ref shouldPublish);

        if (!shouldPublish && !forcePublish)
            return;
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
    }

    private void JobChanged(Job previousJob, Job currentJob) { forcePublish = true; }

    public void Dispose()
    {
        Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
        Service.PlayerEvents.OnJobChange        -= JobChanged;
    }
}
