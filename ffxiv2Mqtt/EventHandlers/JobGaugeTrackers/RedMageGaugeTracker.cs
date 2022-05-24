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
            if (redMageGauge.ManaStacks != manaStacks)
            {
                mqttManager.PublishMessage("JobGauge/RDM/ManaStacks", redMageGauge.ManaStacks);
                manaStacks = redMageGauge.ManaStacks;
            }

            if (redMageGauge.BlackMana != blackMana)
            {
                mqttManager.PublishMessage("JobGauge/RDM/BlackMana", redMageGauge.BlackMana);
                blackMana = redMageGauge.BlackMana;
            }

            if (redMageGauge.WhiteMana != whiteMana)
            {
                mqttManager.PublishMessage("JobGauge/RDM/WhiteMana", redMageGauge.WhiteMana);
                whiteMana = redMageGauge.WhiteMana;
            }
        }
    }
}
