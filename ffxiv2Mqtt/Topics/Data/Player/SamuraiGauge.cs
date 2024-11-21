using System;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Extensions;

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

    public SamuraiGauge() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if (!localPlayer.IsJob(Job.Samurai))
            return;
        var gauge = Service.JobGauges.Get<SAMGauge>();

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

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
