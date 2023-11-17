using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class WarriorGauge : Topic, IDisposable
{
    private byte beastGauge;

    protected override string TopicPath => "Player/JobGauge/WAR";
    protected override bool   Retained  => false;

    public WarriorGauge() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Warrior)
            return;
        var gauge = Service.JobGauges.Get<WARGauge>();

        var shouldPublish = false;
        TestValue(gauge.BeastGauge, ref beastGauge, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.BeastGauge,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
