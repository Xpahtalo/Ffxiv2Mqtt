using System;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Extensions;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class ViperGauge : Topic, IDisposable {
    private byte         rattlingCoilStacks;
    private byte         serpentOffering;
    private byte         anguineTribute;
    private DreadCombo   dreadCombo;
    private SerpentCombo serpentCombo;

    protected override string TopicPath => "Player/JobGauge/VPR";
    protected override bool   Retained  => false;

    public ViperGauge() {
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    public void Dispose() {
        Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
    }

    private void PlayerUpdated(IPlayerCharacter localPlayer) {
        if (Service.ClientState.IsPvP) {
            return;
        }

        if (!localPlayer.IsJob(Job.Viper)) {
            return;
        }

        var gauge = Service.JobGauges.Get<VPRGauge>();

        var shouldPublish = false;
        TestValue(gauge.RattlingCoilStacks, ref rattlingCoilStacks, ref shouldPublish);
        TestValue(gauge.SerpentOffering,    ref serpentOffering,    ref shouldPublish);
        TestValue(gauge.AnguineTribute,     ref anguineTribute,     ref shouldPublish);
        TestValue(gauge.DreadCombo,         ref dreadCombo,         ref shouldPublish);
        TestValue(gauge.SerpentCombo,       ref serpentCombo,       ref shouldPublish);

        if (shouldPublish) {
            var dreadCombo = gauge.DreadCombo switch {
                DreadCombo.Dreadwinder    => "Dreadwinder",
                DreadCombo.HuntersCoil    => "HuntersCoil",
                DreadCombo.SwiftskinsCoil => "SwiftskinsCoil",
                DreadCombo.PitOfDread     => "PitOfDread",
                DreadCombo.HuntersDen     => "HuntersDen",
                DreadCombo.SwiftskinsDen  => "SwiftskinsDen",
                _                         => "none",
            };

            var serpentCombo = gauge.SerpentCombo switch {
                SerpentCombo.DEATHRATTLE  => "DeathRattle",
                SerpentCombo.LASTLASH     => "LastLash",
                SerpentCombo.FIRSTLEGACY  => "FirstLegacy",
                SerpentCombo.SECONDLEGACY => "SecondLegacy",
                SerpentCombo.THIRDLEGACY  => "ThirdLegacy",
                SerpentCombo.FOURTHLEGACY => "FourthLegacy",
                _                         => "none",
            };

            Publish(new {
                gauge.RattlingCoilStacks,
                gauge.SerpentOffering,
                gauge.AnguineTribute,
                DreadCombo   = dreadCombo,
                SerpentCombo = serpentCombo,
            });
        }
    }
}