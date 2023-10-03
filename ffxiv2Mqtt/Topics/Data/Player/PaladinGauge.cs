using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class PaladinGauge : Topic, IDisposable
{
    private byte oathGauge;

    protected override string TopicPath => "Player/JobGauge/PLD";
    protected override bool   Retained  => false;

    public PaladinGauge() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Paladin)
            return;
        var gauge = Service.JobGauges.Get<PLDGauge>();

        var shouldPublish = false;
        TestValue(gauge.OathGauge, ref oathGauge, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.OathGauge,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
