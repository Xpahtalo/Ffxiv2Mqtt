using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data;

internal class NinjaGauge : Topic, IDisposable, IConfigurable
{
    private byte hutonManualCasts;
    private int  hutonTimer;
    private byte ninki;
    private int  syncTimer;

    protected override string TopicPath => "Player/JobGauge/NIN";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents?  PlayerEvents  { get; set; }
    [PluginService] public JobGauges?     JobGauges     { get; set; }
    [PluginService] public ClientState?   ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }

    public override void Initialize()
    {
        Configure();
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Ninja)
            return;
        var gauge = JobGauges?.Get<NINGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.HutonManualCasts, ref hutonManualCasts, ref shouldPublish);
        TestCountDown(gauge.HutonTimer, ref hutonTimer, syncTimer, ref shouldPublish);
        TestValue(gauge.Ninki, ref ninki, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Ninki,
                        gauge.HutonTimer,
                        gauge.HutonManualCasts,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
