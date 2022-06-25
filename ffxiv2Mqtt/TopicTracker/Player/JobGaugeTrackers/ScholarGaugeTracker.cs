using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.JobGauge.Enums;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class ScholarGaugeTracker : BaseTopicTracker, IUpdatable
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
            TestCountDown(gauge.SeraphTimer, ref seraphTimer, 1000);

            PublishIfNeeded();
        }
    }
}
