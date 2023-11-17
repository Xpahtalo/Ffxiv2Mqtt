using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class DarkKnightGauge : Topic, IDisposable, IConfigurable
{
    private byte   blood;
    private ushort darksideTimeRemaining;
    private bool   hasDarkArts;
    private ushort shadowTimeRemaining;
    private ushort syncTimer;

    protected override string TopicPath => "Player/JobGauge/DRK";
    protected override bool   Retained  => false;


    [PluginService] public Configuration? Configuration { get; set; }

    public DarkKnightGauge()
    {
        Configure();
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Configure()
    {
        if (Configuration is not null) syncTimer = (ushort)Configuration.Interval;
    }

    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.DarkKnight)
            return;
        var gauge = Service.JobGauges.Get<DRKGauge>();
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

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
