using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class NinjaGauge : Topic, IDisposable, IConfigurable
{
    private byte hutonManualCasts;
    private int  hutonTimer;
    private byte ninki;
    private int  syncTimer;

    protected override string TopicPath => "Player/JobGauge/NIN";
    protected override bool   Retained  => false;


    [PluginService] public Configuration? Configuration { get; set; }

    public NinjaGauge()
    {
        Configure();
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Ninja)
            return;
        var gauge = Service.JobGauges.Get<NINGauge>();

        var shouldPublish = false;
        TestValue(gauge.HutonManualCasts, ref hutonManualCasts, ref shouldPublish);
        TestValue(gauge.Ninki,            ref ninki,            ref shouldPublish);
        TestCountDown(gauge.HutonTimer, ref hutonTimer, syncTimer, ref shouldPublish);


        if (shouldPublish)
            Publish(new
                    {
                        gauge.Ninki,
                        gauge.HutonTimer,
                        gauge.HutonManualCasts,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
