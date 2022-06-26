﻿using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class ScholarGaugeTracker : BaseGaugeTracker, IUpdatable
    {
        public byte Aetherflow { get => aetherflow; }
        public DismissedFairy DismissedFairy { get => dismissedFairy; }
        public byte FairyGauge { get => fairyGauge; }
        public short SeraphTimer { get => seraphTimer; }

        private byte aetherflow;
        private DismissedFairy dismissedFairy;
        private byte fairyGauge;
        private short seraphTimer;

        private const uint ScholarId = 28;

        public ScholarGaugeTracker(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/SCH";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != ScholarId)
                return;
            var gauge = Dalamud.JobGauges.Get<SCHGauge>();

            TestValue(gauge.Aetherflow, ref aetherflow);
            TestValue(gauge.DismissedFairy, ref dismissedFairy);
            TestValue(gauge.FairyGauge, ref fairyGauge);
            TestCountDown(gauge.SeraphTimer, ref seraphTimer, syncTimer);

            PublishIfNeeded();
        }
    }
}
