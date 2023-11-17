using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using CSNinjaGauge = FFXIVClientStructs.FFXIV.Client.Game.Gauge.NinjaGauge;


namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class NinjaGauge : Topic, IDisposable 
{
    // private byte hutonManualCasts;
    // private int  hutonTimer;
    private byte ninki;
    private byte kazematoi;

    protected override string TopicPath => "Player/JobGauge/NIN";
    protected override bool   Retained  => false;


    [PluginService] public Configuration? Configuration { get; set; }

    public NinjaGauge() {
        Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated;
    }

    private unsafe void PlayerUpdated(IPlayerCharacter localPlayer) {
        if (Service.ClientState.IsPvP) {
            return;
        }

        if ((Job)localPlayer.ClassJob.Id != Job.Ninja) {
            return;
        }

        var gauge = Service.JobGauges.Get<NINGauge>();

        var shouldPublish = false;
        TestValue(gauge.Ninki, ref ninki, ref shouldPublish);
        var gaugeKazematoi = ((CSNinjaGauge*)gauge.Address)->Kazematoi;
        TestValue(gaugeKazematoi, ref kazematoi, ref shouldPublish);


        if (shouldPublish) {
            Publish(new {
                gauge.Ninki,
                gaugeKazematoi,
            });
        }
    }

    public void Dispose() {
        Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated;
    }
}