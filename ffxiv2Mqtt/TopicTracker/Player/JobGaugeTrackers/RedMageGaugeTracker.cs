using Dalamud.Game.ClientState.JobGauge.Types;

namespace Ffxiv2Mqtt.TopicTracker
{
    internal class RedMageGaugeTracker : BaseTopicTracker, IUpdatable
    {
        public byte BlackMana { get => blackMana; }
        public byte WhiteMana { get => whiteMana; }
        public byte ManaStacks { get => manaStacks; }

        private byte blackMana;
        private byte whiteMana;
        private byte manaStacks;

        private const uint RedMageId = 35;

        public RedMageGaugeTracker(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/RDM";
        }

        public void Update()
        {
            if (Dalamud.ClientState.IsPvP)
                return;
            var localPlayer = Dalamud.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if (localPlayer.ClassJob.Id != RedMageId)
                return;
            var gauge = Dalamud.JobGauges.Get<RDMGauge>();

            TestValue(gauge.ManaStacks, ref manaStacks);
            TestValue(gauge.BlackMana, ref blackMana);
            TestValue(gauge.WhiteMana, ref whiteMana);

            PublishIfNeeded();
        }
    }
}
