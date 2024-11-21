using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Extensions;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class SageGauge : Topic, IDisposable, IConfigurable
{
    private byte  addersgall;
    private short addersgallTimer;
    private byte  addersting;
    private bool  eukrasia;
    private short syncTimer;

    protected override string TopicPath => "Player/JobGauge/SGE";
    protected override bool   Retained  => false;


    [PluginService] public IJobGauges?    JobGauges     { get; set; }
    [PluginService] public IClientState?  ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }

    public SageGauge()
    {
        Configure();
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = (short)Configuration.Interval;
    }

    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if (!localPlayer.IsJob(Job.Sage))
            return;
        var gauge = Service.JobGauges.Get<SGEGauge>();

        var shouldPublish = false;
        TestValue(gauge.Addersgall, ref addersgall, ref shouldPublish);
        TestValue(gauge.Addersting, ref addersting, ref shouldPublish);
        TestValue(gauge.Eukrasia,   ref eukrasia,   ref shouldPublish);
        TestCountUp(gauge.AddersgallTimer, ref addersgallTimer, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Addersgall,
                        gauge.AddersgallTimer,
                        gauge.Addersting,
                        gauge.Eukrasia,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
