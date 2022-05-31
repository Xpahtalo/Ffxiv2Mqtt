using Dalamud.Game.ClientState.JobGauge.Types;
namespace Ffxiv2Mqtt.EventHandlers.JobGaugeTrackers
{
    internal class RedMageGaugeTracker : BaseGaugeTracker
    {
        private byte manaStacks;
        private byte blackMana;
        private byte whiteMana;

        public RedMageGaugeTracker(MqttManager m) : base(m) { }

        public void Update(RDMGauge redMageGauge)
        {
            TestValue(redMageGauge.ManaStacks, ref manaStacks, "JobGauge/RDM/ManaStacks");
            TestValue(redMageGauge.BlackMana, ref blackMana, "JobGauge/RDM/BlackMana");
            TestValue(redMageGauge.WhiteMana, ref whiteMana, "JobGauge/RDM/WhiteMana");
        }
    }
}
