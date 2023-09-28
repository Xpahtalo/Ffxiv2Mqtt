using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class ReaperGauge : Topic, IDisposable, IConfigurable
{
    private byte   soul;
    private byte   lemureShroud;
    private byte   voidShroud;
    private ushort enshroudedTimeRemaining;
    private ushort syncTimer;

    protected override string TopicPath => "Player/JobGauge/RPR";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents?  PlayerEvents  { get; set; }
    [PluginService] public IJobGauges?     JobGauges     { get; set; }
    [PluginService] public IClientState?   ClientState   { get; set; }
    [PluginService] public Configuration? Configuration { get; set; }


    public override void Initialize()
    {
        Configure();
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = (ushort)Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Reaper)
            return;
        var gauge = JobGauges?.Get<RPRGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.Soul,         ref soul,         ref shouldPublish);
        TestValue(gauge.LemureShroud, ref lemureShroud, ref shouldPublish);
        TestValue(gauge.VoidShroud,   ref voidShroud,   ref shouldPublish);
        TestCountDown(gauge.EnshroudedTimeRemaining, ref enshroudedTimeRemaining, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Soul,
                        gauge.LemureShroud,
                        gauge.VoidShroud,
                        gauge.EnshroudedTimeRemaining,
                    });
    }

    public void Dispose() { PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated; }
}
