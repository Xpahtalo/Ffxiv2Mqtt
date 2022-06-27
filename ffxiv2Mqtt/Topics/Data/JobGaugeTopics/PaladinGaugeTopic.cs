using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.TopicTracker.Interfaces;

namespace Ffxiv2Mqtt.TopicTracker.Data
{
    internal class PaladinGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte OathGauge { get => oathGauge; }
        private byte oathGauge;

        private const uint PaladinId = 19;

        public PaladinGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/PLD";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != PaladinId)
                return;
            var gauge = Dalamud.JobGauges.Get<PLDGauge>();

            TestValue(gauge.OathGauge, ref oathGauge);

            PublishIfNeeded();
        }
    }
}
