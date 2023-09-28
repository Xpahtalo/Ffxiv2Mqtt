using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;
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
        if (Configuration is not null) syncTimer = (short)Configuration.Interval;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Dragoon)
            return;
        var gauge = JobGauges?.Get<DRGGauge>();
        if (gauge is null)
            return;

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

    public void Dispose() { PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated; }
}
