using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Topics.Interfaces;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class DragoonGauge : Topic, IDisposable, IConfigurable
{
    private byte  eyeCount;
    private byte  firstmindsFocusCount;
    private bool  isLotdActive;
    private short lotdTimer;
    private short syncTimer;

    protected override string TopicPath => "Player/JobGauge/DRG";
    protected override bool   Retained  => false;


    [PluginService] public Configuration? Configuration { get; set; }


    public DragoonGauge()
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
        if ((Job)localPlayer.ClassJob.Id != Job.Dragoon)
            return;
        var gauge = Service.JobGauges.Get<DRGGauge>();

        var shouldPublish = false;
        TestValue(gauge.EyeCount,             ref eyeCount,             ref shouldPublish);
        TestValue(gauge.FirstmindsFocusCount, ref firstmindsFocusCount, ref shouldPublish);
        TestValue(gauge.IsLOTDActive,         ref isLotdActive,         ref shouldPublish);
        TestCountDown(gauge.LOTDTimer, ref lotdTimer, syncTimer, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.EyeCount,
                        gauge.FirstmindsFocusCount,
                        LOTDActive = gauge.IsLOTDActive,
                        gauge.LOTDTimer,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
