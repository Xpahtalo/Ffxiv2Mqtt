using System;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Enums;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data.Player;

internal class DancerGauge : Topic, IDisposable
{
    private          bool   isDancing;
    private readonly uint[] steps = new uint[4];
    private          uint   nextStep;
    private          byte   completedSteps;
    private          byte   esprit;
    private          byte   feathers;

    protected override string TopicPath => "Player/JobGauge/DNC";
    protected override bool   Retained  => false;

    public DancerGauge() { Service.PlayerEvents.LocalPlayerUpdated += PlayerUpdated; }

    private void PlayerUpdated(IPlayerCharacter localPlayer)
    {
        if (Service.ClientState.IsPvP)
            return;
        if ((Job)localPlayer.ClassJob.Id != Job.Dancer)
            return;
        var gauge = Service.JobGauges.Get<DNCGauge>();

        var shouldPublish = false;
        TestValue(gauge.CompletedSteps, ref completedSteps, ref shouldPublish);
        TestValue(gauge.Esprit,         ref esprit,         ref shouldPublish);
        TestValue(gauge.Feathers,       ref feathers,       ref shouldPublish);
        TestValue(gauge.IsDancing,      ref isDancing,      ref shouldPublish);
        TestValue(gauge.NextStep,       ref nextStep,       ref shouldPublish);
        for (var i = 0; i < steps.Length; i++) TestValue(gauge.Steps[i], ref steps[i], ref shouldPublish);

        if (shouldPublish)
            Publish(new
                    {
                        Dancing = gauge.IsDancing,
                        gauge.Steps,
                        gauge.NextStep,
                        gauge.CompletedSteps,
                        gauge.Esprit,
                        gauge.Feathers,
                    });
    }

    public void Dispose() { Service.PlayerEvents.LocalPlayerUpdated -= PlayerUpdated; }
}
