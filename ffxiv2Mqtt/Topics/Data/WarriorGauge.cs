using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data;

internal class WarriorGauge : Topic, IDisposable
{
    private byte beastGauge;

    protected override string TopicPath => "Player/JobGauge/WAR";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents? PlayerEvents { get; set; }
    [PluginService] public JobGauges?    JobGauges    { get; set; }
    [PluginService] public ClientState?  ClientState  { get; set; }

    public override void Initialize()
    {
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Warrior)
            return;
        var gauge = JobGauges?.Get<WARGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.BeastGauge, ref beastGauge, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.BeastGauge,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
