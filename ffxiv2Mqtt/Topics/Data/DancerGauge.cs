﻿using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.JobGauge;
using Ffxiv2Mqtt.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.IoC;
using Ffxiv2Mqtt.Services;

namespace Ffxiv2Mqtt.Topics.Data
{
    internal class DancerGauge : Topic, IDisposable
    {
        [PluginService] public PlayerEvents? PlayerEvents { get; set; }
        [PluginService] public JobGauges?    JobGauges    { get; set; }
        [PluginService] public ClientState?  ClientState  { get; set; }

        protected override string TopicPath => "Player/JobGauge/DNC";
        protected override bool   Retained  => false;

        private bool   isDancing;
        private uint[] steps;
        private uint   nextStep;
        private byte   completedSteps;
        private byte   esprit;
        private byte   feathers;

        public DancerGauge()
        {
            steps = new uint[4];
        }

        public override void Initialize()
        {
            PlayerEvents!.LocalPlayerUpdated += PlayerUpdated;
        }

        public void PlayerUpdated(PlayerCharacter localPlayer)
        {
            if (ClientState!.IsPvP)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.Bard)
                return;
            var gauge = JobGauges?.Get<DNCGauge>();
            if (gauge is null)
                return;

            var shouldPublish = false;
            TestValue(gauge.CompletedSteps, ref completedSteps, ref shouldPublish);
            TestValue(gauge.Esprit,         ref esprit,         ref shouldPublish);
            TestValue(gauge.Feathers,       ref feathers,       ref shouldPublish);
            TestValue(gauge.IsDancing,      ref isDancing,      ref shouldPublish);
            TestValue(gauge.NextStep,       ref nextStep,       ref shouldPublish);
            for (var i = 0; i < steps.Length; i++) {
                TestValue(gauge.Steps[i], ref steps[i], ref shouldPublish);
            }

            if (shouldPublish) {
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
        }

        public void Dispose()
        {
            PlayerEvents!.LocalPlayerUpdated -= PlayerUpdated;
        }
    }
}
