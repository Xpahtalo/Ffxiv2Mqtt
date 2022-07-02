using Ffxiv2Mqtt.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Ffxiv2Mqtt.Topic.Interfaces;

namespace Ffxiv2Mqtt.Topic.Data
{
    internal class RedMageGaugeTopic : JobGaugeTopic, IUpdatable
    {
        public byte BlackMana { get => blackMana; }
        public byte WhiteMana { get => whiteMana; }
        public byte ManaStacks { get => manaStacks; }

        private byte blackMana;
        private byte whiteMana;
        private byte manaStacks;


        public RedMageGaugeTopic(MqttManager m) : base(m)
        {
            topic = "Player/JobGauge/RDM";
        }

        public void Update()
        {
            if (DalamudServices.ClientState.IsPvP)
                return;
            var localPlayer = DalamudServices.ClientState.LocalPlayer;
            if (localPlayer is null)
                return;
            if ((Job)localPlayer.ClassJob.Id != Job.RedMage)
                return;
            var gauge = DalamudServices.JobGauges.Get<RDMGauge>();

            TestValue(gauge.ManaStacks, ref manaStacks);
            TestValue(gauge.BlackMana, ref blackMana);
            TestValue(gauge.WhiteMana, ref whiteMana);

            PublishIfNeeded();
        }
    }
}
