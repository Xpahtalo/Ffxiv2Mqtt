using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
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
        if (Configuration is not null) syncTimer = (short)Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Sage)
            return;
        var gauge = JobGauges?.Get<SGEGauge>();
        if (gauge is null)
            return;

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

    public void Dispose() { PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated; }
}
