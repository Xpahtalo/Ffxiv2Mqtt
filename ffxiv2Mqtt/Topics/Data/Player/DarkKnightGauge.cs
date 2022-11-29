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

internal class DarkKnightGauge : Topic, IDisposable, IConfigurable
{
    private byte   blood;
    private ushort darksideTimeRemaining;
    private bool   hasDarkArts;
    private ushort shadowTimeRemaining;
    private int    syncTimer;

    protected override string TopicPath => "Player/JobGauge/DRK";
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
        if ((Job)localPlayer.ClassJob.Id != Job.DarkKnight)
            return;
        var gauge = JobGauges?.Get<DRKGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.Blood,       ref blood,       ref shouldPublish);
        TestValue(gauge.HasDarkArts, ref hasDarkArts, ref shouldPublish);
        TestCountDown(gauge.ShadowTimeRemaining,   ref shadowTimeRemaining,   syncTimer, ref shouldPublish);
        TestCountDown(gauge.DarksideTimeRemaining, ref darksideTimeRemaining, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Blood,
                        DarkArts = gauge.HasDarkArts,
                        gauge.DarksideTimeRemaining,
                        gauge.ShadowTimeRemaining,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
