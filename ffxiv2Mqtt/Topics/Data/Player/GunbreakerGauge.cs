using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Dalamud.Plugin.Services;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class GunbreakerGauge : Topic, IDisposable
{
    private short ammo;
    private byte  ammoComboStep;
    private short maxTimerDuration;

    protected override string TopicPath => "Player/JobGauge/GNB";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents? PlayerEvents { get; set; }
    [PluginService] public IJobGauges?    JobGauges    { get; set; }
    [PluginService] public IClientState?  ClientState  { get; set; }

    public override void Initialize()
    {
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }


    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Gunbreaker)
            return;
        var gauge = JobGauges?.Get<GNBGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.Ammo,             ref ammo,             ref shouldPublish);
        TestValue(gauge.AmmoComboStep,    ref ammoComboStep,    ref shouldPublish);
        TestValue(gauge.MaxTimerDuration, ref maxTimerDuration, ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Ammo,
                        gauge.AmmoComboStep,
                        gauge.MaxTimerDuration,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
