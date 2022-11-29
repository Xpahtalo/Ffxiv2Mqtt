using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class SamuraiGauge : Topic, IDisposable
{
    private byte   kenki;
    private byte   meditationStacks;
    private bool   hasGetsu;
    private bool   hasKa;
    private bool   hasSetsu;
    private Kaeshi kaeshi;

    protected override string TopicPath => "Player/JobGauge/SAM";
    protected override bool   Retained  => false;

    [PluginService] public PlayerEvents? PlayerEvents { get; set; }
    [PluginService] public JobGauges?    JobGauges    { get; set; }
    [PluginService] public ClientState?  ClientState  { get; set; }

    public override void Initialize()
    {
        PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
    }

    private void PlayerUpdated(PlayerCharacter localPlayer)
    {
        if (ClientState!.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Samurai)
            return;
        var gauge = JobGauges?.Get<SAMGauge>();
        if (gauge is null)
            return;

        var shouldPublish = false;
        TestValue(gauge.Kenki,            ref kenki,            ref shouldPublish);
        TestValue(gauge.MeditationStacks, ref meditationStacks, ref shouldPublish);
        TestValue(gauge.HasGetsu,         ref hasGetsu,         ref shouldPublish);
        TestValue(gauge.HasKa,            ref hasKa,            ref shouldPublish);
        TestValue(gauge.HasSetsu,         ref hasSetsu,         ref shouldPublish);
        TestValue(gauge.Kaeshi,           ref kaeshi,           ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        gauge.Kenki,
                        gauge.MeditationStacks,
                        Getsu = gauge.HasGetsu,
                        Ka    = gauge.HasKa,
                        Setsu = gauge.HasSetsu,
                        gauge.Kaeshi,
                    });
    }

    public void Dispose()
    {
        PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
    }
}
