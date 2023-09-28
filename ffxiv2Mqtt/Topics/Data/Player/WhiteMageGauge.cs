using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class WhiteMageGauge : Topic, IDisposable, IConfigurable
{
    private byte  lily;
    private byte  bloodLily;
    private short lilyTimer;
    private short syncTimer;

    protected override string TopicPath => "Player/JobGauge/WHM";
    protected override bool   Retained  => false;


    [PluginService] public Configuration? Configuration { get; set; }

    public WhiteMageGauge()
    {
        Configure();
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = (short)Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.WhiteMage)
            return;
        var gauge = Service.JobGauges.Get<WHMGauge>();

        var shouldPublish = false;
        TestValue(gauge.Lily,      ref lily,      ref shouldPublish);
        TestValue(gauge.BloodLily, ref bloodLily, ref shouldPublish);
        TestCountUp(gauge.LilyTimer, ref lilyTimer, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Lily,
                        gauge.BloodLily,
                        gauge.LilyTimer,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
