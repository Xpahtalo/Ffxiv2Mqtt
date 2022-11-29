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

internal class WhiteMageGauge : Topic, IDisposable, IConfigurable
{
    private byte  lily;
    private byte  bloodLily;
    private short lilyTimer;
    private int   syncTimer;

    protected override string TopicPath => "Player/JobGauge/WHM";
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
        if ((Job)localPlayer.ClassJob.Id != Job.WhiteMage)
            return;
        var gauge = JobGauges?.Get<WHMGauge>();
        if (gauge is null)
            return;

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

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
